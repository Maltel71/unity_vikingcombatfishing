using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyScript))]
public class NackenSurfacing : MonoBehaviour
{
    [Header("Height")]
    [Tooltip("Where the bottom of the arm sits when she is up. The spawn point only decides left and right.")]
    public float surfacedHeight = -2.2f;
    [Tooltip("Extra clearance below the bottom of the screen when she is under water.")]
    public float hideMargin = 0.5f;

    [Header("Rhythm")]
    public float firstSurfaceDelay = 0.5f;
    [Tooltip("Longest time she stays up if nothing else sends her down.")]
    public float surfaceTime = 7f;
    public float hiddenTime = 4f;
    [Tooltip("Dive once this many swings have finished. 0 means stay up for the full Surface Time.")]
    public int attacksBeforeDive = 1;

    [Header("Movement")]
    public float riseDuration = 0.9f;
    public float diveDuration = 0.7f;

    [Header("Reactions")]
    [Tooltip("How many hits she takes before retreating. 0 means she never retreats from damage.")]
    public int hitsBeforeDive = 3;
    [Tooltip("Hits this soon after surfacing do not count, so she cannot be chased off the moment she arrives.")]
    public float minTimeUpBeforeHitDive = 1f;
    [Tooltip("A retreat is slower than a planned dive so you can see her pull back.")]
    public float hitDiveDuration = 1.3f;

    [Header("While Hidden")]
    public bool disableCollidersWhenHidden = true;

    [Header("Debug")]
    public bool logSurfacing = false;

    private EnemyScript enemy;
    private EnemyAnimationController animController;
    private readonly List<Collider2D> bodyColliders = new List<Collider2D>();
    private float hiddenY;
    private int healthMark;
    private float surfacedAt;
    private bool hitWhileUp;
    private int hitsTaken;

    void Awake()
    {
        enemy = GetComponent<EnemyScript>();
        animController = GetComponent<EnemyAnimationController>();
    }

    void Start()
    {
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            if (col != null && col.enabled) bodyColliders.Add(col);
        }

        healthMark = enemy.health;
        hiddenY = surfacedHeight - 12f;

        SetPositionY(hiddenY);
        SetCollidersEnabled(false);
        enemy.externalBehaviour = true;

        StartCoroutine(Cycle());
    }

    void Update()
    {
        if (hitsBeforeDive <= 0) return;
        if (enemy.health >= healthMark) return;

        healthMark = enemy.health;

        if (Time.time - surfacedAt < minTimeUpBeforeHitDive) return;

        hitsTaken++;
        if (hitsTaken >= hitsBeforeDive) hitWhileUp = true;
    }

    IEnumerator Cycle()
    {
        yield return new WaitForSeconds(firstSurfaceDelay);

        MeasureHiddenDepth();
        SetPositionY(hiddenY);

        while (enemy.health > 0 && !PlayerScript.IsGameOver)
        {
            SetCollidersEnabled(true);
            yield return Travel(hiddenY, surfacedHeight, riseDuration);

            enemy.externalBehaviour = false;
            surfacedAt = Time.time;
            hitWhileUp = false;
            hitsTaken = 0;
            healthMark = enemy.health;

            int swingMark = animController != null ? animController.AttacksFinished : 0;
            float upUntil = Time.time + surfaceTime;

            while (Time.time < upUntil && !hitWhileUp)
            {
                if (enemy.health <= 0 || PlayerScript.IsGameOver) yield break;

                if (attacksBeforeDive > 0 && animController != null
                    && animController.AttacksFinished - swingMark >= attacksBeforeDive) break;

                yield return null;
            }

            enemy.externalBehaviour = true;
            yield return Travel(surfacedHeight, hiddenY, hitWhileUp ? hitDiveDuration : diveDuration);
            SetCollidersEnabled(false);

            float downUntil = Time.time + hiddenTime;
            while (Time.time < downUntil)
            {
                if (enemy.health <= 0 || PlayerScript.IsGameOver) yield break;
                yield return null;
            }
        }
    }

    void MeasureHiddenDepth()
    {
        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        Camera cam = FindViewCamera();

        if (sprite == null || cam == null)
        {
            Debug.LogWarning(name + " could not measure how deep to hide. Using a fixed drop instead.");
            hiddenY = surfacedHeight - 12f;
            return;
        }

        float topAbovePivot = sprite.bounds.max.y - transform.position.y;
        float camBottom = cam.transform.position.y - cam.orthographicSize;

        hiddenY = camBottom - hideMargin - topAbovePivot;

        if (logSurfacing)
        {
            Debug.Log("Nacken  surfaced=" + surfacedHeight.ToString("F2")
                + "  hidden=" + hiddenY.ToString("F2")
                + "  armHeight=" + topAbovePivot.ToString("F2")
                + "  scale=" + transform.localScale.y.ToString("F2"));
        }
    }

    Camera FindViewCamera()
    {
        Camera cam = Camera.main;
        if (cam != null && cam.orthographic) return cam;

        Camera best = null;

        foreach (Camera candidate in FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (candidate == null || !candidate.orthographic || !candidate.isActiveAndEnabled) continue;
            if (best == null || candidate.depth > best.depth) best = candidate;
        }

        return best;
    }

    IEnumerator Travel(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetPositionY(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            SetPositionY(Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetPositionY(to);
    }

    void SetPositionY(float y)
    {
        Vector3 pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }

    void SetCollidersEnabled(bool state)
    {
        if (!disableCollidersWhenHidden) return;

        foreach (Collider2D col in bodyColliders)
        {
            if (col != null) col.enabled = state;
        }
    }
}
