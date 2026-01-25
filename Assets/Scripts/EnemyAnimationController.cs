using UnityEngine;
using System.Collections;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;
    private EnemyScript enemyScript;

    private const string WALK = "Gnome_Villager_Walk";
    private const string ATTACK = "Gnome_Villager_Attack";
    private const string DEATH = "Gnome_Villager_Death";
    private const string DEATH_IDLE = "Gnome_Death_Idle";

    private string currentState;
    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        enemyScript = GetComponent<EnemyScript>();

        ChangeAnimationState(WALK);
    }

    void Update()
    {
        if (animator == null || enemyScript == null) return;

        if (enemyScript.health <= 0)
        {
            if (!isDead)
            {
                isDead = true;
                isAttacking = false;
                ChangeAnimationState(DEATH);
                StartCoroutine(HandleDeathAnimation());
            }
            return;
        }

        if (isAttacking)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f)
            {
                isAttacking = false;
                ChangeAnimationState(WALK);
            }
            return;
        }

        if (currentState != WALK)
        {
            ChangeAnimationState(WALK);
        }
    }

    IEnumerator HandleDeathAnimation()
    {
        yield return new WaitForSeconds(0.1f);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        ChangeAnimationState(DEATH_IDLE);
        enabled = false;
    }

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        animator.Play(newState, 0, 0f);
        currentState = newState;
    }

    public void PlayAttack()
    {
        if (!isDead && !isAttacking)
        {
            isAttacking = true;
            ChangeAnimationState(ATTACK);
        }
    }

    // Called by Animation Event
    public void OnAttackHit()
    {
        if (enemyScript != null)
        {
            enemyScript.DealDamage();
        }
    }
}