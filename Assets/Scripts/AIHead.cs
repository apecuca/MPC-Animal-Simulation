using UnityEngine;

public enum AIAction
{
    NONE,
    SEARCHINGFOOD,
    EATING,
    SLEEPING,
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
        // Comportamento por frame de cada ação
        switch (currentAction)
        {
            default:
                break;
        }
    }

    virtual protected void OnActionEnded()
    {
        AIAction newAction = DecideNewAction();

        // Preparar ação atual
        switch (currentAction)
        {
            default:
                break;
        }

        // Preparar nova ação
        switch (newAction)
        {
            default:
                break;
        }
    }

    public void OnLifeEnded()
    {

    }

    // O objetivo é que as classes derivadas tenham a própria
    // tomada de decisões, que começa e termina aqui
    virtual protected AIAction DecideNewAction() { return AIAction.NONE; }
}
