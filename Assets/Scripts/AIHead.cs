using UnityEngine;

public enum AIAction
{
    NONE,
    SEARCHINGFOOD,
    EATING,
    SLEEPING,
    WALKTOATTACK,
    ATTACKING,
    DEAD
}

public class AIHead : MonoBehaviour
{
    protected AIAction currentAction = AIAction.NONE;

    protected AIMovement ai_movement;
    protected AIStatus ai_status;
    protected AICombat ai_combat;

    virtual protected void Awake()
    {
        ai_movement = GetComponent<AIMovement>();
        ai_status = GetComponent<AIStatus>();
        ai_combat = GetComponent<AICombat>();
    }

    virtual protected void Update()
    {
        ManageBehaviour();
    }

    private void ManageBehaviour()
    {
        // Ações sem comportamento
        if (currentAction == AIAction.NONE ||
            currentAction == AIAction.DEAD)
            return;

        // Comportamento por frame de cada ação
        switch (currentAction)
        {
            case AIAction.SEARCHINGFOOD:    Behaviour_searchingfood(); break;
            case AIAction.EATING:           Behaviour_eating(); break;
            case AIAction.SLEEPING:         Behaviour_sleeping(); break;
            case AIAction.WALKTOATTACK:     Behaviour_walkToAttack(); break;
            case AIAction.ATTACKING:        Behaviour_attacking(); break;
            default: break;
        }
    }

    virtual protected void OnActionEnded()
    {
        AIAction newAction = DecideNewAction();

        // Limpeza da ação atual, caso precise
        switch (currentAction)
        {
            default:
                break;
        }

        // Preparar nova ação
        switch (newAction)
        {
            case AIAction.SEARCHINGFOOD:    NewAction_searchingfood(); break;
            case AIAction.EATING:           NewAction_eating(); break;
            case AIAction.SLEEPING:         NewAction_sleeping(); break;
            case AIAction.WALKTOATTACK:     NewAction_walkToAttack(); break;
            case AIAction.ATTACKING:        NewAction_attacking(); break;
            default: break;
        }
    }

    // Finalizar a simulação
    public void OnLifeEnded()
    {

    }

    // O objetivo é que as classes derivadas tenham a própria
    // tomada de decisões, que começa e termina aqui
    virtual protected AIAction DecideNewAction() { return AIAction.NONE; }

    #region BEHAVIOURS/NEW ACTIONS

    // Procurar por comida
    private void NewAction_searchingfood()
    {

    }

    private void Behaviour_searchingfood()
    {

    }

    // Comer
    private void NewAction_eating()
    {

    }

    private void Behaviour_eating()
    {

    }

    // Dormir
    private void NewAction_sleeping()
    {

    }

    private void Behaviour_sleeping()
    {

    }

    // Andar até o ataque
    private void NewAction_walkToAttack()
    {

    }

    private void Behaviour_walkToAttack()
    {

    }

    // Atacar
    private void NewAction_attacking()
    {

    }

    private void Behaviour_attacking()
    {

    }

    #endregion
}
