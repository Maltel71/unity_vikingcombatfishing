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
    public string deathState = "Gnome_Villager_Death";
    public string deathIdleState = "Gnome_Death_Idle";

    private string currentState;
    private bool isDead = false;
    private bool isAttacking = false;
    private float attackStartedAt = 0f;

    [Tooltip("Safety valve so a stuck attack cannot freeze the enemy forever.")]
    public float attackTimeout = 4f;

    private int attacksFinished = 0;
    private bool warnedAboutEvent = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        enemyScript = GetComponent<EnemyScript>();

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
                ChangeAnimationState(deathState);
                StartCoroutine(HandleDeathAnimation());
            }
            return;
        }

        if (isAttacking)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool onAttackClip = stateInfo.IsName(attackState);
            bool finished = onAttackClip && stateInfo.normalizedTime >= 1f;
            bool timedOut = Time.time - attackStartedAt > attackTimeout;

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

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        animator.Play(newState, 0, 0f);
        currentState = newState;
    }

    void WarnIfEventMissing()
    {
        if (warnedAboutEvent) return;
        if (attacksFinished < 2) return;
        if (enemyScript == null || enemyScript.DamageEventFired) return;

        warnedAboutEvent = true;
        Debug.LogWarning(name + " has swung twice without dealing damage. The clip \""
            + attackState + "\" is missing its DealDamage animation event.");
    }

    public void PlayAttack()
    {
        if (isDead || isAttacking) return;

        isAttacking = true;
        attackStartedAt = Time.time;
        ChangeAnimationState(attackState);
    }

    public void OnAttackHit()
    {
        if (enemyScript != null)
        {
            enemyScript.DealDamage();
        }
    }
}
