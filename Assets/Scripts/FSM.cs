using UnityEngine;

public class FSM : AIHead
{
    //

    // Tomada de decisões da FSM
    override protected AIAction DecideNewAction() 
    {
        AIAction newAction = AIAction.NONE;

        switch (currentAction)
        {
            case AIAction.NONE: 
                newAction = AIAction.SEARCHINGFOOD; 
                break;

            case AIAction.SEARCHINGFOOD: 
                newAction = nearestFoodSource ? AIAction.EATING : AIAction.SEARCHINGFOOD;
                break;

            case AIAction.EATING:
                if (ai_status.sleep < ai_status.hunger)
                    newAction = AIAction.SLEEPING;
                else
                    newAction = AIAction.SEARCHINGFOOD;
                break;

            case AIAction.SLEEPING:
                newAction = AIAction.SEARCHINGFOOD;
                break;

            case AIAction.WALKTOATTACK:
                if (nearestEnemy && (nearestEnemy.health > 0))
                {
                    if (ai_combat.IsInAtkRange(parent, nearestEnemy.transform))
                        newAction = AIAction.ATTACKING;
                    else
                        newAction = AIAction.WALKTOATTACK;
                }
                else if (ai_status.sleep < ai_status.hunger)
                    newAction = AIAction.SLEEPING;
                else
                    newAction = AIAction.SEARCHINGFOOD;
                break;

            case AIAction.ATTACKING:
                if (nearestEnemy && (nearestEnemy.health > 0))
                {
                    if (ai_combat.IsInAtkRange(parent, nearestEnemy.transform))
                        newAction = AIAction.ATTACKING;
                    else
                        newAction = AIAction.WALKTOATTACK;
                }
                else if (ai_status.sleep < ai_status.hunger)
                    newAction = AIAction.SLEEPING;
                else
                    newAction = AIAction.SEARCHINGFOOD;
                break;

            default: 
                break;
        }

        return newAction; 
    }

    protected override bool ShouldStopSleeping()
    {
        if (ai_status.hunger <= dangerousHunger ||
            ai_status.sleep >= ai_status.maxStatusValue - 1.0f)
            return true;

        return false;
    }

    protected override bool ShouldStartChasingEnemy(Enemy source, bool tookDamage)
    {
        // Dar os motivos para NÃO seguir
        // Se nenhum for verdadeiro, vai

        // Erro, nenhuma fonte
        if (!source) return false;

        // Caso já tenha um inimigo perto
        if (nearestEnemy != null)
        {
            float currentDist = Vector2.Distance(parent.position, nearestEnemy.transform.position);
            float newDist = Vector2.Distance(parent.position, source.transform.position);

            // Fonte antiga é mais favorável
            if (source.health >= nearestEnemy.health &&
                newDist > currentDist)
                return false;

            // Já está em combate
            if (currentAction == AIAction.ATTACKING || currentAction == AIAction.WALKTOATTACK)
                return false;
        }

        // Continuar dormindo se não tomou dano
        if (currentAction == AIAction.SLEEPING)
        {
            if (!tookDamage)
                return false;
        }

        // Continuar comendo se estiver quase acabando
        if (currentAction == AIAction.EATING)
        {
            if (eatingTimer <= timeLeftToContinueEating)
                return false;
        }

        return true;
    }
}
