using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] int spawnIndex; 

    bool activated = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        activated = true;
        SpawnManager.Instance.SetSpawnPoint(spawnIndex);
    }
}