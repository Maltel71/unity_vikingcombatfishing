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
            if (stateInfo.normalizedTime >= 1.0f)
            {
                isAttacking = false;
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

    public void PlayAttack()
    {
        if (!isDead && !isAttacking)
        {
            isAttacking = true;
            ChangeAnimationState(attackState);
        }
    }

    public void OnAttackHit()
    {
        if (enemyScript != null)
        {
            enemyScript.DealDamage();
        }
    }
}
