using UnityEngine;
using System.Collections.Generic;

public class AttackCollider : MonoBehaviour
{
    private List<EnemyScript> enemiesInRange = new List<EnemyScript>();
    private Collider2D attackCollider;

    void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        if (attackCollider != null)
        {
            attackCollider.isTrigger = true;
            attackCollider.enabled = false; // Start disabled
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyScript enemy = other.GetComponent<EnemyScript>();
            if (enemy != null && !enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Add(enemy);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyScript enemy = other.GetComponent<EnemyScript>();
            if (enemy != null && enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Remove(enemy);
            }
        }
    }

    public void ActivateAttack(float damage)
    {
        // Hit all enemies currently in range
        foreach (EnemyScript enemy in enemiesInRange)
        {
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage);
                Debug.Log($"Hit {enemy.gnomeName} for {damage} damage!");
            }
        }
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