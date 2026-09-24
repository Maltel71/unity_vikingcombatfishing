using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerScript playerScript;
    private FishingRod fishingRod;

    private const string IDLE = "IdleAnimRagnar";
    private const string WALK = "Ragnar_WalkAnimate";
    private const string ATTACK = "Ragnar_AttackAnimate";
    private const string REELING = "Ragnar_ReelingAnimate";
    private const string CATCH = "Ragnar_Catch_Animate";
    private const string DEATH = "DeathAnimeRagnar";
    private const string DEATH_IDLE = "Ragnar_DeathIdleAnimate";
    private const string DANCE = "danceanimragnar";

    [Tooltip("State name for the berserk swing. Leave empty to reuse the normal attack.")]
    public string berserkState = "Berserk_Attack";

    private string currentState;
    private bool isPlayingAction = false;

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

        if (!playerScript.isAlive)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

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

        if (isPlayingAction)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f)
            {
                isPlayingAction = false;
            }
            else
            {
                return;
            }
        }

        if (fishingRod != null && fishingRod.isReelingIn)
        {
            ChangeAnimationState(REELING);
            return;
        }

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

    public void PlayAttack()
    {
        ChangeAnimationState(ATTACK);
        isPlayingAction = true;
    }

    public void PlayBerserk()
    {
        if (string.IsNullOrEmpty(berserkState))
        {
            PlayAttack();
            return;
        }

        if (!animator.HasState(0, Animator.StringToHash(berserkState)))
        {
            Debug.LogWarning("Ragnar has no animator state called \"" + berserkState + "\". Using the normal attack.");
            PlayAttack();
            return;
        }

        ChangeAnimationState(berserkState);
        isPlayingAction = true;
    }

    public void PlayCatch()
    {
        ChangeAnimationState(CATCH);
        isPlayingAction = true;
    }

    public void StartDancing()
    {
        ChangeAnimationState(DANCE);
        isPlayingAction = true;
    }

    public void StopDancing()
    {
        isPlayingAction = false;

    }
}
