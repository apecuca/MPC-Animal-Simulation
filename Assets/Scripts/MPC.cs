using UnityEngine;

/// @struct SimulatedStatus
/// @brief Estrutura que representa o estado simulado do agente durante a predição do MPC.
///
/// Essa estrutura mantém uma cópia temporária dos atributos do agente para simular
/// o efeito das ações futuras sem modificar o estado real.
///
/// @note O valor máximo (`max`) é definido com base em `AIStatus.maxStatusValue`.
struct SimulatedStatus
{
    /// @brief Valor atual de saúde.
    public float health;

    /// @brief Valor atual de sono.
    public float sleep;

    /// @brief Valor atual de fome.
    public float hunger;

    /// @brief Valor máximo possível para os atributos.
    public readonly float max;

    /// @brief Constrói um estado simulado a partir do estado real do agente.
    ///
    /// @param[in] real Estado real do agente (`AIStatus`) usado como base para simulação.
    /// 
    /// @note Se `real` for nulo, o estado simulado será inicializado com valores máximos.
    public SimulatedStatus(AIStatus real)
    {
        if (real != null)
        {
            health = real.health;
            sleep = real.sleep;
            hunger = real.hunger;
            max = AIStatus.maxStatusValue;
        }
        else
        {
            max = AIStatus.maxStatusValue;
            health = max;
            sleep = max;
            hunger = max;
        }
    }
}

/// @class MPC
/// @brief Implementação do controlador de decisão baseado em Modelo Preditivo (Model Predictive Control).
///
/// Essa classe herda de `AIHead` e aplica um algoritmo MPC para prever e selecionar
/// a melhor ação possível com base em custos simulados de curto prazo.
///
/// @details O controlador avalia as ações possíveis simulando o comportamento futuro
/// do agente em um horizonte de tempo, aplicando pesos a fatores como fome, sono,
/// saúde e presença de inimigos.
public class MPC : AIHead
{
    // =======================
    // Parâmetros de Controle
    // =======================

    [Header("Parâmetros MPC")]
    [SerializeField] private int horizon;         ///< Horizonte de predição (número de passos simulados).
    [SerializeField] private float gamma;         ///< Fator de desconto aplicado a custos futuros (0–1).

    [Header("Pesos de Custo")]
    [SerializeField] private float wHunger;       ///< Peso aplicado à penalidade por fome.
    [SerializeField] private float wSleep;        ///< Peso aplicado à penalidade por sono.
    [SerializeField] private float wHealth;       ///< Peso aplicado à penalidade por saúde baixa.
    [SerializeField] private float wEnemy;        ///< Peso aplicado à penalidade por inimigos próximos.

    /// @brief Determina a próxima ação do agente com base no custo preditivo.
    ///
    /// @details Avalia todas as ações possíveis (exceto `NONE` e `DEAD`), simulando o futuro
    /// para cada uma e escolhendo aquela que resulta no menor custo acumulado.
    ///
    /// @retval AIAction Retorna a ação com menor custo preditivo.
    /// @warning Caso o agente esteja morto (`health <= 0`), a função retorna `AIAction.DEAD`.
    override protected AIAction DecideNewAction()
    {
        if (currentAction == AIAction.DEAD || ai_status.health <= 0f)
            return AIAction.DEAD;

        AIAction originalAction = currentAction;
        AIAction bestAction = AIAction.SEARCHINGFOOD;
        float bestCost = float.MaxValue;

        // Testa todas as ações e escolhe a que possui o menor custo total
        foreach (AIAction a in System.Enum.GetValues(typeof(AIAction)))
        {
            if (a == AIAction.NONE || a == AIAction.DEAD)
                continue;

            float cost = SimulateFuture(a);
            if (cost < bestCost)
            {
                bestCost = cost;
                bestAction = a;
            }
        }

        return bestAction;
    }

    /// @brief Simula o comportamento futuro do agente para uma ação específica.
    ///
    /// @details O método aplica um modelo de predição por horizonte,
    /// atualizando o estado simulado passo a passo e acumulando o custo
    /// ponderado pelo fator de desconto `gamma`.
    ///
    /// @param[in] testAction A ação que será testada na simulação.
    /// @retval float Retorna o custo total associado à ação simulada.
    /// @note Se a saúde simulada chegar a 0, um custo elevado é adicionado (morte).
    private float SimulateFuture(AIAction testAction)
    {
        SimulatedStatus sim = new SimulatedStatus(ai_status);
        float totalCost = 0f;

        for (int t = 0; t < horizon; t++)
        {
            // Simular decaimento natural dos status
            sim.hunger = AIStatus.ClampStatusValue(sim.hunger, -(ai_status.hungerDecay * Time.deltaTime));
            sim.sleep = AIStatus.ClampStatusValue(sim.sleep, -(ai_status.sleepDecay * Time.deltaTime));

            // Penalizar caso o agente esteja com fome extrema
            if (sim.hunger <= 0.0f)
                sim.health = AIStatus.ClampStatusValue(sim.health, -(ai_status.starvedDamage * Time.deltaTime));

            // Regenerar saúde se os status estiverem bons
            if (sim.hunger >= ai_status.minHungerToHPRecovery && sim.sleep > 0)
                sim.health = AIStatus.ClampStatusValue(sim.health, ai_status.healthRecovery * Time.deltaTime);

            // Simular interações contextuais (sono, combate, alimentação)
            SimulateSituations(ref sim, testAction, t);

            // Calcular e acumular o custo ponderado pelo fator de desconto
            totalCost += Mathf.Pow(gamma, t) * StepCost(ref sim, testAction);

            // Encerrar simulação se o agente morrer
            if (sim.health <= 0.0f)
            {
                totalCost += 99999.0f;
                break;
            }
        }

        return totalCost;
    }

    /// @brief Calcula o custo instantâneo de um estado simulado.
    ///
    /// @details O custo é obtido somando penalidades ponderadas pelos desvios
    /// dos atributos em relação ao máximo ideal.
    ///
    /// @param[in,out] sim Estrutura contendo o estado simulado atual.
    /// @param[in] testAction A ação sendo avaliada.
    /// @retval float Retorna o custo numérico do estado.
    /// @note Custo adicional é aplicado se o agente estiver com sono máximo (para evitar loop infinito).
    private float StepCost(ref SimulatedStatus sim, AIAction testAction)
    {
        float c = 0.0f;
        float max = sim.max;

        c += wHunger * Mathf.Pow((max - sim.hunger) / max, 2);
        c += wSleep * (sim.sleep >= (max - 1.0f) ? 99999.0f : Mathf.Pow((max - sim.sleep) / max, 2));
        c += wHealth * Mathf.Pow((max - sim.health) / max, 2);
        c += wEnemy * (nearestEnemy == null ? 0.0f : 1.0f);

        return c;
    }

    /// @brief Simula eventos específicos que afetam o estado do agente.
    ///
    /// @details Atualiza os valores de fome, sono e saúde com base na ação
    /// e no contexto ambiental, como combate, alimentação e descanso.
    ///
    /// @param[in,out] sim Referência ao estado simulado a ser modificado.
    /// @param[in] action Ação sendo simulada.
    /// @param[in] iteration Iteração atual dentro do horizonte de predição.
    /// @note Essa função não altera o estado real do NPC, apenas o simulado.
    private void SimulateSituations(ref SimulatedStatus sim, AIAction action, int iteration)
    {
        // --- Simular sono ---
        if (action == AIAction.SLEEPING)
            sim.sleep = AIStatus.ClampStatusValue(sim.sleep, (ai_status.sleepDecay + ai_status.sleepRecovery) * Time.deltaTime);

        // --- Simular combate ---
        if (nearestEnemy)
        {
            // Se não estiver atacando, sofrer dano se estiver próximo
            if (action != AIAction.ATTACKING &&
                Vector2.Distance(nearestEnemy.transform.position, parent.position) <= ai_combat.atkRange)
                sim.health = AIStatus.ClampStatusValue(sim.health, -(nearestEnemy.GetAtkDmg() / horizon));

            // Caso esteja indo atacar, aproximar e aplicar dano
            if (action == AIAction.WALKTOATTACK)
            {
                Vector2 dirToEnemy = (nearestEnemy.transform.position - parent.position).normalized;
                Vector2 currentSimPosition = (Vector2)parent.position + (dirToEnemy * ai_movement.GetMvSpeed() * (iteration + 1));
                if (Vector2.Distance(nearestEnemy.transform.position, parent.position) <= ai_combat.atkRange)
                    sim.health = AIStatus.ClampStatusValue(sim.health, -(nearestEnemy.GetAtkDmg() / horizon));
            }
        }

        // --- Simular alimentação ---
        if (nearestFoodSource)
        {
            if (action == AIAction.EATING &&
                Vector3.Distance(parent.position, nearestFoodSource.transform.position) <= eatingDist)
                sim.hunger = AIStatus.ClampStatusValue(sim.hunger, (ai_status.hungerDecay + ai_status.hungerPerFood) * Time.deltaTime);
        }
    }

    /// @brief Determina se o NPC deve interromper o sono atual.
    ///
    /// @details O agente continuará dormindo apenas se o MPC determinar
    /// que dormir ainda é a melhor ação no momento.
    ///
    /// @retval true Se o agente deve acordar.
    /// @retval false Se o agente deve continuar dormindo.
    protected override bool ShouldStopSleeping()
    {
        if (DecideNewAction() == AIAction.SLEEPING)
            return false;

        return true;
    }
}
