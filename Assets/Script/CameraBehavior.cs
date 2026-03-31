using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // Pour récupérer l'état du jeu (pause ou non)
    [Header("Pause")]
    public UIController uiController;

    [Header("Follow")]
    [SerializeField] Transform target;       // glisse le joueur ici
    [SerializeField] float smoothSpeed = 5f; // plus c'est élevé, plus c'est réactif

    [Header("Shake")]
    [SerializeField] float magnitude = 0.03f;
    [SerializeField] float speed = 0.8f;

    float seed;

    void Start()
    {
        seed = Random.Range(0f, 100f);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Définit un bool basé sur l'état du jeu (pause ou non)
        bool curPause = uiController.Pause;
        // Vérifie que le jeu n'est pas en pause avant d'éxecuter le code
        if (!curPause) {
            // 1. Smooth follow vers le joueur (on garde le Z de la caméra)
            Vector3 desiredPos = new Vector3(target.position.x, target.position.y, transform.position.z);
            Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

            // 2. Shake par-dessus
            float t = Time.time * speed;
            float offsetX = (Mathf.PerlinNoise(seed + t, 0f) * 2f - 1f) * magnitude;
            float offsetY = (Mathf.PerlinNoise(0f, seed + t) * 2f - 1f) * magnitude;

            transform.position = smoothedPos + new Vector3(offsetX, offsetY, 0f);
        }
    }
}