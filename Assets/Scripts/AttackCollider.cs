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
        // Kopiera listan innan vi slar. TakeDamage kan doda fienden, vilket
        // stanger av dess collider -> OnTriggerExit2D -> listan andras mitt i
        // loopen och kastar "Collection was modified".
        hitBuffer.Clear();
        hitBuffer.AddRange(enemiesInRange);

        foreach (EnemyScript enemy in hitBuffer)
        {
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage);
            }
        }

        hitBuffer.Clear();
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