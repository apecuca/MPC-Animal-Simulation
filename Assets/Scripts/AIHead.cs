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

    // Variáveis de controle das ações:
    // Geral
    protected Transform lockedObject;

    // SEARCHINGFOOD
    protected bool goingBackIn = true;

    // EATING
    protected float eatingDist = 0.75f;
    protected float eatDuration = 2.0f;
    protected float eatingTimer = 0.0f;

    [Header("Assignables")]
    [SerializeField] protected ConeCollider visionCone;
    protected Transform parent;

    virtual protected void Awake()
    {
        ai_movement = GetComponentInChildren<AIMovement>();
        ai_status = GetComponentInChildren<AIStatus>();
        ai_combat = GetComponentInChildren<AICombat>();

        parent = transform;
    }

    private void Start()
    {
        ForceNewAction(AIAction.SEARCHINGFOOD);
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

    protected void OnActionEnded()
    {
        AIAction newAction = DecideNewAction();
        ForceNewAction(newAction);
    }

    protected void ForceNewAction(AIAction newAction)
    {
        // Limpar remanescentes da ação anterior
        ClearLastAction();

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

        currentAction = newAction;
    }

    private void ClearLastAction()
    {
        // Parar movimento
        ai_movement.SetMoveDir(Vector2.zero);

        // Limpeza da ação atual, caso precise
        switch (currentAction)
        {
            case AIAction.SEARCHINGFOOD:
                goingBackIn = false;
                break;

            case AIAction.EATING: 
                lockedObject = null;
                eatingTimer = 0.0f;
                break;

            case AIAction.SLEEPING: break;

            case AIAction.WALKTOATTACK: break;

            case AIAction.ATTACKING: break;

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
        Vector2 newDir = Vector2.zero;

        if (IsOutOfMap())
        {
            newDir = -ai_movement.lastRegisteredDir;

            // Evitar voltar para o lugar original
            if (parent.position.x < -GameManager.mapSize.x ||
                parent.position.x > GameManager.mapSize.x) 
                newDir.y = Random.Range(-1.0f, 1.0f);

            if (parent.position.y < -GameManager.mapSize.y ||
                parent.position.y > GameManager.mapSize.y)
                newDir.x = Random.Range(-1.0f, 1.0f);

            newDir.Normalize();
        }
        else
            newDir = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;

        // Aplicar mudanças
        if (IsOutOfMap())
            goingBackIn = true;

        ai_movement.SetMoveDir(newDir);
    }

    private void Behaviour_searchingfood()
    {
        if (IsOutOfMap())
        {
            // Finalizar ação, saiu e não achou comida
            if (!goingBackIn)
            {
                //OnActionEnded();
                ForceNewAction(AIAction.SEARCHINGFOOD);
            }
        }
        else
            goingBackIn = false;

        // Found food
        if (visionCone.nearestFoodSource)
        {
            if (Vector3.Distance(parent.position, visionCone.nearestFoodSource.position) > eatingDist)
                ai_movement.SetMoveDir((visionCone.nearestFoodSource.position - parent.position).normalized);
            else
                ForceNewAction(AIAction.EATING);
                //OnActionEnded();
        }
    }

    // Comer
    private void NewAction_eating()
    {
        lockedObject = visionCone.nearestFoodSource;
        eatingTimer = eatDuration;
    }

    private void Behaviour_eating()
    {
        if (eatingTimer <= 0.0f)
        {
            ai_status.IncrementHunger(ai_status.hungerPerFood);
            Destroy(lockedObject.gameObject);
            ForceNewAction(AIAction.SEARCHINGFOOD);
            //OnActionEnded();
            return;
        }

        eatingTimer -= Time.deltaTime * Time.timeScale;
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

    #region UTIls

    private bool IsOutOfMap()
    {
        if (parent.position.x > GameManager.mapSize.x ||
            parent.position.x < -GameManager.mapSize.x ||
            parent.position.y > GameManager.mapSize.y ||
            parent.position.y < -GameManager.mapSize.y)
            return true;

        return false;
    }

    #endregion
}
