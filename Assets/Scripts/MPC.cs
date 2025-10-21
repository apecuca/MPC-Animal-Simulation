using UnityEngine;
struct SimulatedStatus
{
    public float health;
    public float sleep;
    public float hunger;
    public readonly float max;

    public SimulatedStatus(AIStatus real)
    {
        if (real != null)
        {
            health = real.health;
            sleep = real.sleep;
            hunger = real.hunger;
            max = real.maxStatusValue;
        }
        else
        {
            max = real.maxStatusValue;
            health = max;
            sleep = max;
            hunger = max;
        }
    }
}

public class MPC : AIHead
{
    [Header("Parâmetros MPC")]
    [SerializeField] private int horizon;         // Horizonte de predição (passos)
    [SerializeField] private float gamma;         // Fator de desconto (peso do futuro)

    [Header("Pesos de Custo")]
    [SerializeField] private float wHunger;
    [SerializeField] private float wSleep;
    [SerializeField] private float wHealth;
    [SerializeField] private float wEnemy;
    [SerializeField] private float wSwitch;

    // Tomada de decisões da mpc
    override protected AIAction DecideNewAction()
    {
        AIAction newAction = AIAction.NONE;

        if (currentAction == AIAction.DEAD || ai_status.health <= 0f)
            return AIAction.DEAD;

        AIAction bestAction = AIAction.SEARCHINGFOOD;
        float bestCost = float.MaxValue;

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

        Debug.Log($"[MPC] hunger={ai_status.hunger:F1}, sleep={ai_status.sleep:F1}, health={ai_status.health:F1} \nEscolheu {bestAction} (custo={bestCost:F2})");

        return newAction;
    }

    private float SimulateFuture(AIAction testAction)
    {
        SimulatedStatus sim = new SimulatedStatus(ai_status);
        float totalCost = 0f;

        for (int t = 0; t < horizon; t++)
        {
            // Simular decay
            sim.hunger -= ai_status.hungerDecay * Time.deltaTime;
            sim.sleep -= ai_status.sleepDecay * Time.deltaTime;
            if (sim.hunger <= 0.0f)
                sim.health -= ai_status.starvedDamage * Time.deltaTime;
            if (sim.hunger >= ai_status.minHungerToHPRecovery && sim.sleep > 0)
                sim.health = Mathf.Clamp(sim.health + ai_status.healthRecovery * Time.deltaTime, 0.0f, sim.max);

            // Adicionar ao custo
            totalCost += StepCost(ref sim, testAction);

            // Horizon causa a morte
            if (sim.health <= 0.0f)
            {
                totalCost += 99999.0f;
                break;
            }
        }

        return totalCost;
    }

    private float StepCost(ref SimulatedStatus sim, AIAction testAction)
    {
        float c = 0.0f;
        float max = sim.max;

        c += wHunger * Mathf.Pow((max - sim.hunger) / max, 2);
        c += wSleep * Mathf.Pow((max - sim.sleep) / max, 2);
        c += wHealth * Mathf.Pow((max - sim.health) / max, 2);
        c += wEnemy * (nearestEnemy != null ? 1.0f : 0.0f);

        // Extra costs
        c += testAction == AIAction.EATING ? 1 : 2;

        if (testAction != currentAction)
            c += wSwitch;

        return c;
    }

    /*
    public float StepCost(in PetState s, IAction current, IAction prev)
    {
        float hungerCost = quadraticPenalties ? Mathf.Pow(s.hunger/100f,2) : s.hunger/100f;
        float energyCost = quadraticPenalties ? Mathf.Pow((100f - s.energy)/100f,2) : (100f - s.energy)/100f;
        float hygieneCost= quadraticPenalties ? Mathf.Pow((100f - s.hygiene)/100f,2) : (100f - s.hygiene)/100f;
        float ratsCost   = s.activeRats; // já inteiro
        float healthCost = quadraticPenalties ? Mathf.Pow((100f - s.health)/100f,2) : (100f - s.health)/100f;

        float switchCost = (prev != null && current != null && current.Name != prev.Name) ? 1f : 0f;

        return wHunger*hungerCost + wEnergy*energyCost + wHygiene*hygieneCost
               + wRats*ratsCost + wHealth*healthCost + wActionSwitch*switchCost;
    } 
    */

    protected override bool ShouldStopSleeping()
    {
        return false;
    }

    protected override bool ShouldStartChasingEnemy(Enemy source, bool tookDamage)
    {
        return false;
    }
}


/* OLD MPC
 * 
 * using UnityEngine;

/// <summary>
/// Controlador de decisão baseado em Modelo Preditivo (MPC)
/// que considera o decaimento passivo e aplica efeitos simbólicos
/// apenas dentro da simulação preditiva.
/// </summary>
public class MPC : AIHead
{
    [Header("Parâmetros MPC")]
    public int horizon;            // Horizonte de predição (passos)
    public int numCandidates;      // Número de sequências testadas
    public float gamma;         // Fator de desconto (peso do futuro)

    [Header("Pesos de Custo")]
    public float wHunger;
    public float wSleep;
    public float wHealth;
    public float wEnemy;
    public float wSwitch;

    private bool isDeciding = false;

    // =====================================================
    // ================ MÉTODOS PRINCIPAIS =================
    // =====================================================

    protected override AIAction DecideNewAction()
    {
        // Evita reentrância (recursão infinita)
        if (isDeciding)
            return currentAction != AIAction.NONE ? currentAction : AIAction.SEARCHINGFOOD;

        if (ai_status == null)
            return AIAction.SEARCHINGFOOD;

        if (currentAction == AIAction.DEAD || ai_status.health <= 0f)
            return AIAction.DEAD;

        isDeciding = true;

        AIAction bestAction = AIAction.SEARCHINGFOOD;
        float bestCost = float.MaxValue;

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

        //Debug.Log($"[MPC] hunger={ai_status.hunger:F1}, sleep={ai_status.sleep:F1}, health={ai_status.health:F1} -> Escolheu {bestAction} (custo={bestCost:F2})");

        isDeciding = false;
        return bestAction;
    }

    // =====================================================
    // =============== SIMULAÇÃO PREDITIVA =================
    // =====================================================

    private float SimulateFuture(AIAction firstAction)
    {
        SimulatedStatus sim = new SimulatedStatus(ai_status);
        float totalCost = 0f;
        AIAction prevAction = currentAction;
        AIAction current = firstAction;

        for (int t = 0; t < horizon; t++)
        {
            // aplica decaimento passivo e efeitos simbólicos
            ApplyPassiveDecay(ref sim);
            ApplySymbolicEffect(ref sim, current);

            // custo do passo
            totalCost += Mathf.Pow(gamma, t) * ComputeCost(sim, current, prevAction);

            prevAction = current;
            current = ChooseHeuristicNext(sim);

            if (sim.health <= 0f)
            {
                totalCost += 10000f; // penalidade grande para morte
                break;
            }
        }

        return totalCost;
    }

    // =====================================================
    // ============= SIMULAÇÃO DO DECAY PASSIVO ============
    // =====================================================

    private void ApplyPassiveDecay(ref SimulatedStatus s)
    {
        s.hunger = Mathf.Clamp(s.hunger - ai_status.hungerPerSecond * Time.deltaTime, 0f, s.max);
        s.sleep = Mathf.Clamp(s.sleep - ai_status.sleepPerSecond * Time.deltaTime, 0f, s.max);

        // dano por fome extrema
        if (s.hunger <= 0f)
            s.health = Mathf.Clamp(s.health - ai_status.hungerPerSecond * Time.deltaTime, 0f, s.max);

        // recuperação se bem alimentado e descansado
        if (s.hunger >= ai_status.minHungerToHPRecovery && s.sleep >= 0.0f)
            s.health = Mathf.Clamp(s.health + ai_status.healthRecovery * Time.deltaTime, 0f, s.max);
    }

    // =====================================================
    // ============= EFEITOS SIMBÓLICOS POR AÇÃO ============
    // =====================================================

    private void ApplySymbolicEffect(ref SimulatedStatus s, AIAction a)
    {
        // Pequenas alterações internas para que o MPC perceba diferenças.
        switch (a)
        {
            case AIAction.EATING:
                s.hunger = Mathf.Clamp(s.hunger + 15f, 0f, s.max);
                break;
            case AIAction.SLEEPING:
                s.sleep = Mathf.Clamp(s.sleep + 20f, 0f, s.max);
                break;
            case AIAction.ATTACKING:
                s.health = Mathf.Clamp(s.health - 5f, 0f, s.max);
                break;
        }
    }

    // =====================================================
    // ============== FUNÇÃO DE CUSTO CONTEXTUAL ============
    // =====================================================

    private float ComputeCost(SimulatedStatus s, AIAction current, AIAction previous)
    {
        float max = s.max;
        float hungerNorm = (max - s.hunger) / max;
        float sleepNorm = (max - s.sleep) / max;
        float healthNorm = (max - s.health) / max;

        float c = 0f;
        c += wHunger * Mathf.Pow(hungerNorm, 2);
        c += wSleep * Mathf.Pow(sleepNorm, 2);
        c += wHealth * Mathf.Pow(healthNorm, 2);
        c += wEnemy * (nearestEnemy != null ? 1f : 0f);

        // penaliza incoerências de contexto
        if (current == AIAction.SLEEPING && s.sleep > 80f) c += 2f;
        if (current == AIAction.EATING && s.hunger > 80f) c += 2f;
        if (current == AIAction.WALKTOATTACK && nearestEnemy == null) c += 3f;

        // pequena recompensa simbólica para coerência
        if (current == AIAction.SEARCHINGFOOD && s.hunger < 50f) c -= 0.5f;

        if (current != previous)
            c += wSwitch;

        // penaliza continuar comendo em combate
        if (current == AIAction.EATING && nearestEnemy != null)
            c += 10f;

        // penaliza ignorar fome crítica
        if (s.hunger < 30f && current != AIAction.EATING)
            c += 5f;

        // penaliza continuar dormindo com inimigo por perto
        if (current == AIAction.SLEEPING && nearestEnemy != null)
            c += 8f;


        return c;
    }

    // =====================================================
    // ================= HEURÍSTICA AUXILIAR ===============
    // =====================================================

    private AIAction ChooseHeuristicNext(SimulatedStatus s)
    {
        if (s.hunger < 30f) return AIAction.EATING;
        if (s.sleep < 25f) return AIAction.SLEEPING;
        if (nearestEnemy != null) return AIAction.WALKTOATTACK;
        return AIAction.SEARCHINGFOOD;
    }

    // =====================================================
    // ================ MÉTODOS HERDADOS ===================
    // =====================================================

    protected override bool ShouldStopSleeping()
    {
        if (ai_status == null) return true;
        return ai_status.sleep >= satisfyingSleep;
    }

    protected override bool ShouldStartChasingEnemy(Enemy source, bool tookDamage)
    {
        if (source == null) return false;
        if (tookDamage) return true;

        float dist = Vector3.Distance(parent.position, source.transform.position);
        return dist < 6f;
    }

    // =====================================================
    // ============= ESTRUTURA DE ESTADO LOCAL =============
    // =====================================================

    private struct SimulatedStatus
    {
        public float health;
        public float sleep;
        public float hunger;
        public readonly float max;

        public SimulatedStatus(AIStatus real)
        {
            if (real != null)
            {
                health = real.health;
                sleep = real.sleep;
                hunger = real.hunger;
                max = real.maxStatusValue;
            }
            else
            {
                max = 100f;
                health = max;
                sleep = max * 0.6f;
                hunger = max * 0.6f;
            }
        }
    }
}
*/