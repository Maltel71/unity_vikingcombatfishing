using UnityEngine;
using System.Collections;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;
    private EnemyScript enemyScript;

    // Animation state names - match your animator
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
        if (animator == null)
        {
            Debug.LogError("Gnome needs an Animator component!");
            return;
        }

        enemyScript = GetComponent<EnemyScript>();

        // Start with walk animation
        ChangeAnimationState(WALK);
    }

    void Update()
    {
        if (animator == null || enemyScript == null) return;

        // If dead, handle death animations ONCE then stop updating
        if (enemyScript.health <= 0)
        {
            if (!isDead)
            {
                isDead = true;
                ChangeAnimationState(DEATH);
                StartCoroutine(HandleDeathAnimation());
            }
            return; // STOP updating once dead
        }

        // Check if attack animation is playing
        if (isAttacking)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f)
            {
                isAttacking = false;
            }
            else
            {
                return; // Don't change animation while action is playing
            }
        }

        // Default to walk when alive and not attacking
        if (currentState != WALK)
        {
            ChangeAnimationState(WALK);
        }
    }

    IEnumerator HandleDeathAnimation()
    {
        // Wait for death animation to finish
        yield return new WaitForSeconds(0.1f); // Small delay to let animation start

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        // Play death idle
        ChangeAnimationState(DEATH_IDLE);

        // Disable this script so Update stops running
        enabled = false;
    }

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        animator.Play(newState);
        currentState = newState;
        Debug.Log($"Gnome changed animation to: {newState}");
    }

    public void PlayAttack()
    {
        if (!isDead && !isAttacking)
        {
            isAttacking = true;
            ChangeAnimationState(ATTACK);
            Debug.Log("Gnome playing attack animation");
        }
    }
}