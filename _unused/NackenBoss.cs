using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyScript))]
public class NackenBoss : MonoBehaviour
{
    [Header("Animation States")]
    [Tooltip("The arm breaking the surface and reaching for Ragnar.")]
    public string reachState = "Näcken_Reaching";
    [Tooltip("The grab. Its DealDamage event is what actually hurts Ragnar.")]
    public string grabState = "Näcken_Sucess";
    [Tooltip("Played when the arm is struck and while it pulls back under.")]
    public string hurtState = "Näcken_Hurt";

    [Header("Movement")]
    [Tooltip("How far below its spawn point the arm hides. Set this so it clears the waterline.")]
    public float sinkDepth = 3.5f;
    [Tooltip("Seconds to rise out of the water.")]
    public float riseDuration = 0.6f;
    [Tooltip("Seconds to sink back down.")]
    public float sinkDuration = 0.7f;

    [Header("Rhythm")]
    [Tooltip("Quiet seconds before the arm rises the first time.")]
    public float firstRiseDelay = 2f;
    [Tooltip("How long the arm stays under between attempts.")]
    public float timeBelow = 3.5f;
    [Tooltip("Total time above water before the verdict. Includes the rise.")]
    public float reachDuration = 2f;
    [Tooltip("How long the grab runs before the arm withdraws.")]
    public float grabDuration = 1.6f;

    [Header("Reaction")]
    [Tooltip("Pull back under the moment Ragnar lands a hit.")]
    public bool retreatWhenHit = true;
    [Tooltip("Seconds it recoils above water before sinking.")]
    public float flinchTime = 0.4f;

    [Header("Vulnerability")]
    [Tooltip("Turn the collider off while below, so Ragnar cannot hit what is not there.")]
    public bool shieldWhileBelow = true;

    public bool IsAbove { get; private set; }

    private EnemyScript enemy;
    private Animator animator;
    private Collider2D body;
    private Vector3 abovePos;
    private Vector3 belowPos;
    private int healthMark;
    private bool struck;

    void Awake()
    {
        enemy = GetComponent<EnemyScript>();
        enemy.externalBehaviour = true;

        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        body = GetComponent<Collider2D>();
    }

    void Start()
    {
        abovePos = transform.position;
        belowPos = abovePos - Vector3.up * sinkDepth;

        transform.position = belowPos;
        SetExposed(false);

        StartCoroutine(Cycle());
    }

    IEnumerator Cycle()
    {
        yield return new WaitForSeconds(firstRiseDelay);

        while (enemy != null && enemy.health > 0)
        {
            if (PlayerScript.IsGameOver) yield break;

            healthMark = enemy.health;
            struck = false;

            SetExposed(true);
            Play(reachState);

            yield return Travel(belowPos, abovePos, riseDuration);

            if (!struck && Alive())
            {
                yield return Hold(Mathf.Max(0f, reachDuration - riseDuration));
            }

            if (Alive() && !struck && WithinReach())
            {
                Play(grabState);
                yield return new WaitForSeconds(grabDuration);
            }
            else if (struck)
            {
                Play(hurtState);
                yield return new WaitForSeconds(flinchTime);
            }

            SetExposed(false);

            yield return Travel(abovePos, belowPos, sinkDuration);

            if (!Alive()) yield break;

            yield return new WaitForSeconds(timeBelow);
        }
    }

    IEnumerator Travel(Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f)
        {
            transform.position = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.position = Vector3.Lerp(from, to, t * t * (3f - 2f * t));

            WatchForHits();

            yield return null;
        }

        transform.position = to;
    }

    IEnumerator Hold(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            WatchForHits();
            if (struck) yield break;

            yield return null;
        }
    }

    void WatchForHits()
    {
        if (enemy == null) return;

        if (enemy.health < healthMark)
        {
            healthMark = enemy.health;
            if (retreatWhenHit) struck = true;
        }
    }

    bool Alive()
    {
        return enemy != null && enemy.health > 0;
    }

    bool WithinReach()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        float distance = Mathf.Abs(player.transform.position.x - transform.position.x);
        return distance <= enemy.attackRange;
    }

    void SetExposed(bool exposed)
    {
        IsAbove = exposed;

        if (body != null && shieldWhileBelow) body.enabled = exposed;
    }

    void Play(string state)
    {
        if (animator == null || string.IsNullOrEmpty(state)) return;

        animator.Play(state, 0, 0f);
    }
}
