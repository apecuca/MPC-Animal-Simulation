using UnityEngine;
using static Unity.VisualScripting.Member;

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
    public Transform nearestFoodSource { get; private set; } = null;
    public Enemy nearestEnemy { get; private set; } = null;

    // EATING
    protected float eatingDist = 0.75f;
    protected float eatDuration = 10.0f;
    protected float eatingTimer = 0.0f;
    protected float dangerousHunger = 60.0f;
    // No caso de dano
    protected float timeLeftToContinueEating = 1.0f;

    // SLEEPING
    protected float satisfyingSleep = 90.0f;

    // COMBAT
    protected float enemyDamage = 2.0f;

    [Header("Assignables")]
    protected Transform parent;

    public static AIHead instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(instance.gameObject);
        instance = this;

        ai_movement = GetComponentInChildren<AIMovement>();
        ai_status = GetComponentInChildren<AIStatus>();
        ai_combat = GetComponentInChildren<AICombat>();

        parent = transform;
    }

    private void Start()
    {
        OnActionEnded();
    }

    virtual protected void Update()
    {
        ManageBehaviour();
    }

    #region MANAGEMENT

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
        if (currentAction == AIAction.DEAD)
            return;

        // Limpar remanescentes da ação anterior
        if (newAction != currentAction)
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
        GameManager.instance.OnAIDied();
        ForceNewAction(AIAction.DEAD);
    }

    public void OnEnemySpotted(Enemy spottedEnemy)
    {
        if (ShouldStartChasingEnemy(spottedEnemy, false))
        {
            nearestEnemy = spottedEnemy;
            ForceNewAction(AIAction.WALKTOATTACK);
        }
    }

    // Chamado por inimigos ao acertar um ataque
    public void TakeDamage(float dmg, Enemy source)
    {
        ai_status.DecrementHealth(dmg);

        if (ShouldStartChasingEnemy(source, true))
        {
            nearestEnemy = source;
            ForceNewAction(AIAction.WALKTOATTACK);
        }
    }

    #region DECISIONS

    // O objetivo é que as classes derivadas tenham a própria
    // tomada de decisões, que começa e termina aqui
    virtual protected AIAction DecideNewAction() { return AIAction.NONE; }

    // Decidir se deveria ou não parar de dormir
    virtual protected bool ShouldStopSleeping() { return false; }

    // Decidir se deveria PARAR O QUE ESTA FAZENDO para ir até um ataque
    virtual protected bool ShouldStartChasingEnemy(Enemy source, bool tookDamage) { return false; }

    #endregion

    #endregion

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
                OnActionEnded();
        }
        else
            goingBackIn = false;

        // Found food
        if (nearestFoodSource)
        {
            if (Vector3.Distance(parent.position, nearestFoodSource.position) > eatingDist)
                ai_movement.SetMoveDir((nearestFoodSource.position - parent.position).normalized);
            else
                OnActionEnded();
        }
    }

    // Comer
    private void NewAction_eating()
    {
        if (nearestFoodSource == null)
        {
            OnActionEnded();
            return;
        }

        lockedObject = nearestFoodSource;
        eatingTimer = eatDuration;
    }

    private void Behaviour_eating()
    {
        if (eatingTimer <= 0.0f)
        {
            ai_status.IncrementHunger(ai_status.hungerPerFood);
            Destroy(lockedObject.gameObject);
            OnActionEnded();
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
        if (ShouldStopSleeping())
        {
            OnActionEnded();
            return;
        }

        ai_status.RecoverSleep();
    }

    // Andar até o ataque
    private void NewAction_walkToAttack()
    {
        if (!nearestEnemy)
        {
            OnActionEnded();
            return;
        }    

        if (ai_combat.IsInAtkRange(parent, nearestEnemy.transform))
        {
            OnActionEnded();
            return;
        }
    }

    private void Behaviour_walkToAttack()
    {
        if (ai_combat.IsInAtkRange(parent, nearestEnemy.transform))
        {
            OnActionEnded();
            return;
        }

        Vector2 newDir = (nearestEnemy.transform.position - parent.position).normalized;
        ai_movement.SetMoveDir(newDir);
    }

    // Atacar
    private void NewAction_attacking()
    {
        if (!nearestEnemy)
        {
            OnActionEnded();
            return;
        }

        if (!ai_combat.IsInAtkRange(parent, nearestEnemy.transform))
        {
            OnActionEnded();
            return;
        }

        ai_combat.StartAttackTimer();
    }

    private void Behaviour_attacking()
    {
        if (ai_combat.Attack(parent, nearestEnemy))
            OnActionEnded();
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

    public void OnFoodSourceDetected(Transform source)
    {
        if (nearestFoodSource)
        {
            // Substituir se a fonte nova for mais perto
            float ogDist = Vector2.Distance(nearestFoodSource.position, parent.position);
            if (ogDist > Vector2.Distance(source.position, parent.position))
                nearestFoodSource = source;
        }
        else
            nearestFoodSource = source;
    }

    #endregion
}
