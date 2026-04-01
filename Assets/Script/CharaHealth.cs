using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] AudioClip[] deathSound;
    [SerializeField] AudioSource myAudioSource;

    [Header("Vie")]
    [SerializeField] int maxHealth = 3;

    int currentHealth;
    bool isDead;

    CharaController chara;

    void Awake()
    {
        chara = GetComponent<CharaController>();
        currentHealth = maxHealth;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDeadZone(other.gameObject)) Die();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (IsDeadZone(other.gameObject)) Die();
    }

    void SoundDeath()
    {
        int monRandom = Random.Range(0, 4);
        myAudioSource.clip = deathSound[monRandom];
        myAudioSource.Play();
    }

    bool IsDeadZone(GameObject go)
    {
        return go.CompareTag("dead_zone") ||
               go.layer == LayerMask.NameToLayer("dead_zone");
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        chara.IsInputLocked = true;
        chara.FreezePhysics(true);

        SoundDeath();

        SpawnManager.Instance.Respawn(this);

        if (DeathScreenManager.Instance != null)
        {
            DeathScreenManager.Instance.ShowDeathScreen(null);
        }
    }

    public void Revive(Vector3 spawnPosition)
    {
        chara.Teleport(spawnPosition);
        chara.FreezePhysics(false);
        chara.IsInputLocked = false;
        currentHealth = maxHealth;
        isDead = false;
    }
}