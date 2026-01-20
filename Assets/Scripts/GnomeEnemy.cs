using UnityEngine;

public class GnomeEnemy : MonoBehaviour
{
    public float movementSpeed;

    private EndlessWaveManager waveManager;

    public void SetWaveManager(EndlessWaveManager manager)
    {
        waveManager = manager;
    }

    // Call this when the gnome dies
    public void Die()
    {
        if (waveManager != null)
        {
            waveManager.OnGnomeKilled();
        }
        Destroy(gameObject);
    }
}