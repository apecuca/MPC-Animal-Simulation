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
                if (nearestEnemy && ai_combat.IsInAtkRange(parent, nearestEnemy.transform))
                    newAction = AIAction.ATTACKING;
                else if (ai_status.sleep < ai_status.hunger)
                    newAction = AIAction.SLEEPING;
                else
                    newAction = AIAction.SEARCHINGFOOD;
                break;

            case AIAction.ATTACKING:
                if (nearestEnemy && !ai_combat.IsInAtkRange(parent, nearestEnemy.transform))
                    newAction = AIAction.WALKTOATTACK;
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

    protected override bool ShouldStartChasingEnemy()
    {
        if (!nearestEnemy) return false;
        if (currentAction == AIAction.ATTACKING) return false;
        if (currentAction == AIAction.WALKTOATTACK) return false;
        if (!ai_combat.IsInAtkRange(parent, nearestEnemy.transform)) return true;

        return true;
    }
}
