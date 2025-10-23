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
        if (currentAction == AIAction.DEAD || ai_status.health <= 0f)
            return AIAction.DEAD;

        AIAction originalAction = currentAction;
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

        //Debug.Log($"Escolheu {bestAction} (custo={bestCost})");

        return bestAction;
    }

    private float SimulateFuture(AIAction testAction)
    {
        SimulatedStatus sim = new SimulatedStatus(ai_status);
        float totalCost = 0f;

        for (int t = 0; t < horizon; t++)
        {
            // Simular decay
            sim.hunger = AIStatus.ClampStatusValue(sim.hunger, ai_status.hungerDecay * Time.deltaTime);
            sim.sleep = AIStatus.ClampStatusValue(sim.sleep, ai_status.sleepDecay * Time.deltaTime);

            // Penalizar por fome
            if (sim.hunger <= 0.0f)
                sim.health = AIStatus.ClampStatusValue(sim.health, -(ai_status.starvedDamage * Time.deltaTime));

            // Regenerar se os status estiverem bons
            if (sim.hunger >= ai_status.minHungerToHPRecovery && sim.sleep > 0)
                sim.health = AIStatus.ClampStatusValue(sim.health, ai_status.healthRecovery * Time.deltaTime);

            // Simular situações específicas
            SimulateSituations(ref sim, testAction, t); 

            // Adicionar ao custo
            totalCost += Mathf.Pow(gamma, t) * StepCost(ref sim, testAction);

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
        c += wSleep * (sim.sleep >= (max - 1.0f) ? 0.0f : Mathf.Pow((max - sim.sleep) / max, 2));
        c += wHealth * Mathf.Pow((max - sim.health) / max, 2);
        c += wEnemy * (nearestEnemy == null ? 0.0f : 1.0f);

        if (testAction != currentAction)
            c += wSwitch;

        return c;
    }

    private void SimulateSituations(ref SimulatedStatus sim, AIAction action, int iteration)
    {
        // Simular dormir
        if (action == AIAction.SLEEPING)
            sim.sleep = AIStatus.ClampStatusValue(sim.sleep, (ai_status.sleepDecay + ai_status.sleepRecovery) * Time.deltaTime);

        // Simular combate
        if (nearestEnemy)
        {
            // Diminuir a vida se estiver perto e a ação não for de combate
            if (action != AIAction.ATTACKING &&
                Vector2.Distance(nearestEnemy.transform.position, parent.position) <= ai_combat.atkRange)
                sim.health = AIStatus.ClampStatusValue(sim.health, -(nearestEnemy.GetAtkDmg() / horizon));

            if (action == AIAction.WALKTOATTACK)
            {
                Vector2 dirToEnemy = (nearestEnemy.transform.position - parent.position).normalized;
                Vector2 currentSimPosition = (Vector2)parent.position + (dirToEnemy * ai_movement.GetMvSpeed() * (iteration + 1));
                if (Vector2.Distance(nearestEnemy.transform.position, parent.position) <= ai_combat.atkRange)
                    sim.health = AIStatus.ClampStatusValue(sim.health, -(nearestEnemy.GetAtkDmg() / horizon));
            }
        }

        // Simular comida
        if (nearestFoodSource)
        {
            if (action == AIAction.EATING &&
                Vector3.Distance(parent.position, nearestFoodSource.transform.position) <= eatingDist)
                sim.hunger = AIStatus.ClampStatusValue(sim.hunger, (ai_status.hungerDecay + ai_status.hungerPerFood) * Time.deltaTime);
        }
    }

    protected override bool ShouldStopSleeping()
    {
        if (DecideNewAction() == AIAction.SLEEPING)
            return false;

        return true;
    }
}