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
    public Food nearestFoodSource { get; private set; } = null;
    public Enemy nearestEnemy { get; private set; } = null;

    // EATING
    protected float eatingDist = 0.75f;
    //protected float eatDuration = 10.0f;
    //protected float eatingTimer = 0.0f;
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
                //lockedObject = null;
                //eatingTimer = 0.0f;
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
        if (IsNewEnemyCloser(spottedEnemy))
            nearestEnemy = spottedEnemy;

        if (ShouldStartChasingEnemy(nearestEnemy, false))
            ForceNewAction(AIAction.WALKTOATTACK);
    }

    // Chamado por inimigos ao acertar um ataque
    public void TakeDamage(float dmg, Enemy source)
    {
        ai_status.DecrementHealth(dmg);

        if (IsNewEnemyCloser(source))
            nearestEnemy = source;

        if (ShouldStartChasingEnemy(source, true))
        {
            lockedObject = source.transform;
            ForceNewAction(AIAction.WALKTOATTACK);
        }
    }

    #region DECISIONS

    // O objetivo é que as classes derivadas tenham a própria
    // tomada de decisões, que começa e termina aqui
    virtual protected AIAction DecideNewAction() { return AIAction.NONE; }

    // Decidir se deveria ou não parar de dormir
    virtual protected bool ShouldStopSleeping() { return false; }

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
            if (Vector3.Distance(parent.position, nearestFoodSource.transform.position) > eatingDist)
                ai_movement.SetMoveDir((nearestFoodSource.transform.position - parent.position).normalized);
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

        lockedObject = nearestFoodSource.transform;
    }

    private void Behaviour_eating()
    {
        // Condições para erro
        if (!nearestFoodSource ||
            lockedObject != nearestFoodSource.transform)
        {
            OnActionEnded();
            return;
        }

        // Se voltar falso, terminou de comer
        if (!nearestFoodSource.Eat())
        {
            nearestFoodSource = null;
            lockedObject = null;
            OnActionEnded();
            return;
        }
        else
        {
            // Compensar pelo decay
            ai_status.IncrementHunger((ai_status.hungerDecay + ai_status.hungerPerFood) * Time.deltaTime);
        }
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
        if (nearestEnemy == null)
        {
            OnActionEnded();
            return;
        }

        lockedObject = nearestEnemy.transform; 

        if (ai_combat.IsInAtkRange(parent, lockedObject))
        {
            OnActionEnded();
            return;
        }
    }

    private void Behaviour_walkToAttack()
    {
        if (ai_combat.IsInAtkRange(parent, lockedObject))
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
        if (nearestEnemy == null)
        {
            OnActionEnded();
            return;
        }

        lockedObject = nearestEnemy.transform;

        if (!ai_combat.IsInAtkRange(parent, lockedObject))
        {
            OnActionEnded();
            return;
        }

        ai_combat.StartAttackTimer();
    }

    private void Behaviour_attacking()
    {
        if (!lockedObject)
        {
            OnActionEnded();
            return;
        }

        if (lockedObject != nearestEnemy.transform)
        {
            OnActionEnded();
            return;
        }

        if (ai_combat.Attack(parent, nearestEnemy))
        {
            OnActionEnded();
            return;
        }
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
            float ogDist = Vector2.Distance(nearestFoodSource.transform.position, parent.position);
            if (ogDist > Vector2.Distance(source.position, parent.position))
                nearestFoodSource = source.GetComponent<Food>();
        }
        else
            nearestFoodSource = source.GetComponent<Food>();
    }

    public bool IsNewEnemyCloser(Enemy newEnemy)
    {
        if (nearestEnemy == null)
            return true;

        float curDist = Vector2.Distance(nearestEnemy.transform.position, parent.transform.position);
        float newDist = Vector2.Distance(newEnemy.transform.position, parent.transform.position);

        return newDist < curDist;
    }

    // Decidir se deveria PARAR O QUE ESTA FAZENDO para ir até um ataque
    protected bool ShouldStartChasingEnemy(Enemy source, bool tookDamage)
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
            if ((currentAction == AIAction.ATTACKING || currentAction == AIAction.WALKTOATTACK) &&
                nearestEnemy == source)
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
            if (ai_status.hunger <= dangerousHunger)
                return false;
        }

        return true;
    }

    #endregion
}
