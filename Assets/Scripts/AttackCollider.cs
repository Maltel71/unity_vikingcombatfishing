using UnityEngine;
using System.Collections.Generic;

public class AttackCollider : MonoBehaviour
{
    private List<EnemyScript> enemiesInRange = new List<EnemyScript>();
    private readonly List<EnemyScript> hitBuffer = new List<EnemyScript>();
    private Collider2D attackCollider;

    void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        if (attackCollider != null)
        {
            attackCollider.isTrigger = true;
            attackCollider.enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyScript enemy = other.GetComponentInParent<EnemyScript>();

        if (enemy != null && !enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        EnemyScript enemy = other.GetComponentInParent<EnemyScript>();

        if (enemy != null && enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Remove(enemy);
        }
    }

    public int ActivateAttack(float damage)
    {

        hitBuffer.Clear();
        hitBuffer.AddRange(enemiesInRange);

        int hits = 0;

        foreach (EnemyScript enemy in hitBuffer)
        {
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage);
                hits++;
            }
        }

        hitBuffer.Clear();
        return hits;
    }

    public void EnableCollider()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
            enemiesInRange.Clear();
        }
    }

    public void DisableCollider()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
            enemiesInRange.Clear();
        }
    }
}
