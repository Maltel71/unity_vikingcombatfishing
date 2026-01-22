using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerScript playerScript;
    private FishingRod fishingRod;

    // Animation state names
    private const string IDLE = "IdleAnimRagnar";
    private const string WALK = "Ragnar_WalkAnimate";
    private const string ATTACK = "Ragnar_AttackAnimate";
    private const string REELING = "Ragnar_ReelingAnimate";
    private const string CATCH = "Ragnar_Catch_Animate";
    private const string DEATH = "DeathAnimeRagnar";
    private const string DEATH_IDLE = "Ragnar_DeathIdleAnimate";

    private string currentState;
    private bool isPlayingAction = false; // Track if an action animation is playing

    void Start()
    {
        Transform graphicsChild = transform.Find("Graphics");
        if (graphicsChild != null)
        {
            animator = graphicsChild.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("Graphics child object not found!");
        }

        playerScript = GetComponent<PlayerScript>();
        fishingRod = GetComponentInChildren<FishingRod>();
    }

    void Update()
    {
        if (animator == null) return;

        // Handle death states
        if (!playerScript.isAlive)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // If death animation is playing and finished, switch to death idle
            if (currentState == DEATH && stateInfo.normalizedTime >= 1.0f)
            {
                ChangeAnimationState(DEATH_IDLE);
            }
            else if (currentState != DEATH && currentState != DEATH_IDLE)
            {
                ChangeAnimationState(DEATH);
            }
            return;
        }

        // Check if action animation has finished
        if (isPlayingAction)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f)
            {
                isPlayingAction = false;
            }
            else
            {
                return; // Don't change animation while action is playing
            }
        }

        // Fishing takes priority
        if (fishingRod != null && fishingRod.isReelingIn)
        {
            ChangeAnimationState(REELING);
            return;
        }

        // Attack
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeAnimationState(ATTACK);
            isPlayingAction = true;
            return;
        }

        // Movement
        float moveInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            ChangeAnimationState(WALK);
        }
        else
        {
            ChangeAnimationState(IDLE);
        }
    }

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        animator.Play(newState);
        currentState = newState;
    }

    public void PlayCatch()
    {
        ChangeAnimationState(CATCH);
        isPlayingAction = true;
    }
}