using UnityEngine;
using System.Collections;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;
    private EnemyScript enemyScript;

    [Header("State Names")]
    [Tooltip("Must match the state names in this enemy's Animator Controller.")]
    public string walkState = "Gnome_Villager_Walk";
    public string attackState = "Gnome_Villager_Attack";
    [Tooltip("Extra attack states. One of these or Attack State is picked at random for every swing.")]
    public string[] extraAttackStates;
    public string deathState = "Gnome_Villager_Death";
    public string deathIdleState = "Gnome_Death_Idle";
    [Tooltip("Played once when the enemy takes a hit. Leave empty for enemies that have no flinch clip.")]
    public string hurtState = "";

    [Header("Desperation Attack")]
    [Tooltip("Played instead of the normal attack when this enemy or Ragnar is badly hurt. Leave empty to switch it off.")]
    public string finisherState = "";
    [Range(0f, 1f)]
    [Tooltip("Trigger it once this enemy's own health drops below this share of its maximum.")]
    public float finisherWhenSelfBelow = 0.35f;
    [Range(0f, 1f)]
    [Tooltip("Trigger it once Ragnar's health drops below this share of his maximum.")]
    public float finisherWhenPlayerBelow = 0.35f;
    [Range(0f, 1f)]
    public float finisherChance = 0.6f;
    public float finisherCooldown = 10f;
    [Tooltip("How much harder the desperation attack hits than a normal one.")]
    public float finisherDamageMultiplier = 2f;

    private string currentState;
    private bool isDead = false;
    private bool isAttacking = false;
    private float attackStartedAt = 0f;
    private string activeAttackState;

    [Header("Debug")]
    [Tooltip("Print every animation state change to the console.")]
    public bool logAnimation = false;

    [Tooltip("Safety valve so a stuck animation cannot freeze the enemy forever. Clips longer than this get their own length plus a second, so a long clip is never cut short.")]
    public float attackTimeout = 4f;

    [Header("Pull Back")]
    [Tooltip("Clip played once after every attack, for an enemy whose attack ends away from its resting pose. Leave empty for enemies whose attack returns on its own.")]
    public string returnState = "";

    private int attacksFinished = 0;
    private bool warnedAboutEvent = false;
    private float nextFinisherTime = 0f;
    private bool isReacting = false;
    private float reactStartedAt = 0f;
    private bool isReturning = false;
    private float returnStartedAt = 0f;

    public bool SuppressDamage { get { return isReturning; } }

    public bool IsSwinging { get { return isAttacking; } }

    public float CurrentAttackMultiplier
    {
        get
        {
            bool finisher = !string.IsNullOrEmpty(finisherState) && activeAttackState == finisherState;
            return finisher ? finisherDamageMultiplier : 1f;
        }
    }
    private PlayerScript player;

    public int AttacksFinished { get { return attacksFinished; } }

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        enemyScript = GetComponent<EnemyScript>();
        activeAttackState = attackState;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponent<PlayerScript>();

        ChangeAnimationState(walkState);
    }

    void Update()
    {
        if (animator == null || enemyScript == null) return;

        if (PlayerScript.IsGameOver)
        {
            animator.speed = 0f;
            return;
        }

        if (enemyScript.health <= 0)
        {
            if (!isDead)
            {
                isDead = true;
                isAttacking = false;
                isReturning = false;
                ChangeAnimationState(deathState);
                StartCoroutine(HandleDeathAnimation());
            }
            return;
        }

        if (isReacting)
        {
            AnimatorStateInfo reactInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool onHurtClip = reactInfo.IsName(hurtState);
            bool reactDone = onHurtClip && reactInfo.normalizedTime >= 1f;

            if (reactDone || Time.time - reactStartedAt > TimeoutFor(reactInfo, onHurtClip))
            {
                isReacting = false;
                ChangeAnimationState(walkState);
            }
            return;
        }

        if (isReturning)
        {
            AnimatorStateInfo returnInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool onReturnClip = returnInfo.IsName(returnState);
            bool returnDone = onReturnClip && returnInfo.normalizedTime >= 1f;

            if (returnDone || Time.time - returnStartedAt > TimeoutFor(returnInfo, onReturnClip))
            {
                if (logAnimation) Debug.Log(name + "  return finished. onReturnClip=" + onReturnClip
                    + "  t=" + returnInfo.normalizedTime.ToString("F2"));
                isReturning = false;
                isAttacking = false;
                attacksFinished++;
                WarnIfEventMissing();
                ChangeAnimationState(walkState);
            }
            return;
        }

        if (isAttacking)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool onAttackClip = stateInfo.IsName(activeAttackState);
            bool finished = onAttackClip && stateInfo.normalizedTime >= 1f;
            bool timedOut = Time.time - attackStartedAt > TimeoutFor(stateInfo, onAttackClip);

            if (finished)
            {
                if (logAnimation) Debug.Log(name + "  attack \"" + activeAttackState
                    + "\" finished. returnState=\"" + returnState + "\"");

                if (!string.IsNullOrEmpty(returnState))
                {
                    isReturning = true;
                    returnStartedAt = Time.time;
                    ChangeAnimationState(returnState);
                    return;
                }
            }

            if (finished || timedOut)
            {
                isAttacking = false;
                attacksFinished++;
                WarnIfEventMissing();
                ChangeAnimationState(walkState);
            }
            return;
        }

        if (currentState != walkState)
        {
            ChangeAnimationState(walkState);
        }
    }

    IEnumerator HandleDeathAnimation()
    {
        yield return new WaitForSeconds(0.1f);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        ChangeAnimationState(deathIdleState);
        enabled = false;
    }

    float TimeoutFor(AnimatorStateInfo info, bool onExpectedClip)
    {
        if (!onExpectedClip || info.length <= 0f) return attackTimeout;

        return Mathf.Max(attackTimeout, info.length + 1f);
    }

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        if (logAnimation)
        {
            bool known = animator.HasState(0, Animator.StringToHash(newState));
            Debug.Log(name + "  play \"" + newState + "\"  exists in controller: " + known);
            if (!known) Debug.LogWarning(name + " has no state called \"" + newState + "\" in its Animator Controller.");
        }

        animator.Play(newState, 0, 0f);
        currentState = newState;
    }

    bool ShouldUseFinisher()
    {
        if (string.IsNullOrEmpty(finisherState)) return false;
        if (Time.time < nextFinisherTime) return false;

        bool selfHurt = false;
        if (enemyScript != null && enemyScript.MaxHealth > 0)
        {
            selfHurt = (float)enemyScript.health / enemyScript.MaxHealth <= finisherWhenSelfBelow;
        }

        bool playerHurt = false;
        if (player != null && player.maxHealth > 0)
        {
            playerHurt = (float)player.playerHealth / player.maxHealth <= finisherWhenPlayerBelow;
        }

        if (!selfHurt && !playerHurt) return false;

        return Random.value <= finisherChance;
    }

    string PickAttackState()
    {
        if (ShouldUseFinisher())
        {
            nextFinisherTime = Time.time + finisherCooldown;
            return finisherState;
        }

        if (extraAttackStates == null || extraAttackStates.Length == 0) return attackState;

        int roll = Random.Range(0, extraAttackStates.Length + 1);
        if (roll == 0) return attackState;

        string pick = extraAttackStates[roll - 1];
        return string.IsNullOrEmpty(pick) ? attackState : pick;
    }

    void WarnIfEventMissing()
    {
        if (warnedAboutEvent) return;
        if (attacksFinished < 2) return;
        if (enemyScript == null || enemyScript.DamageEventFired) return;

        warnedAboutEvent = true;
        Debug.LogWarning(name + " has swung twice without dealing damage. The clip \""
            + activeAttackState + "\" is missing its DealDamage animation event.");
    }

    public void PlayHurt()
    {
        if (isDead || isAttacking || isReacting) return;
        if (string.IsNullOrEmpty(hurtState)) return;

        isReacting = true;
        reactStartedAt = Time.time;
        ChangeAnimationState(hurtState);
    }

    public void PlayAttack()
    {
        if (isDead || isAttacking || isReacting || isReturning) return;

        isAttacking = true;
        attackStartedAt = Time.time;
        activeAttackState = PickAttackState();
        ChangeAnimationState(activeAttackState);
    }

    public void OnAttackHit()
    {
        if (enemyScript == null || isReturning) return;

        enemyScript.DealDamageScaled(CurrentAttackMultiplier);
    }
}
