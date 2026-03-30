using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Points de spawn")]
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] int activeSpawnIndex = 0;  // index du spawn actif

    [Header("Respawn")]
    [SerializeField] float respawnDelay = 1.5f; // délai avant réapparition

    void Awake()
    {
        // Singleton simple
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }


    public void Respawn(CharaController player)
    {
        StartCoroutine(RespawnRoutine(player));
    }

    IEnumerator RespawnRoutine(CharaController player)
    {
        yield return new WaitForSeconds(respawnDelay);

        Vector3 target = GetActiveSpawnPoint();
        player.Revive(target);
    }

    Vector3 GetActiveSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("SpawnManager : aucun spawn point assigné !");
            return Vector3.zero;
        }

        activeSpawnIndex = Mathf.Clamp(activeSpawnIndex, 0, spawnPoints.Length - 1);
        return spawnPoints[activeSpawnIndex].position;
    }


    public void SetSpawnPoint(int index)
    {
        activeSpawnIndex = Mathf.Clamp(index, 0, spawnPoints.Length - 1);
        Debug.Log($"Checkpoint activé : spawn {activeSpawnIndex}");
    }
}