using UnityEngine;
using static UnityEngine.Audio.GeneratorInstance;

public class ArmRotation : MonoBehaviour
{
    // Pour récupérer l'état du jeu (pause ou non)
    [Header("Pause")]
    public UIController uiController;

    [Header("Limites de rotation")]
    [SerializeField] float minAngle = -80f;
    [SerializeField] float maxAngle = 80f;

    [Header("Référence joueur (pour le flip)")]
    [SerializeField] Transform playerTransform;

    void Update()
    {
        // Définit un bool basé sur l'état du jeu (pause ou non)
        bool curPause = uiController.Pause;
        // Vérifie que le jeu n'est pas en pause avant d'éxecuter le code
        if (!curPause) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = mousePos - transform.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Si le joueur regarde à gauche, on inverse l'angle
            bool facingLeft = playerTransform.localScale.x < 0f;
            if (facingLeft) angle = 180f - angle;

            angle = Mathf.Clamp(angle, minAngle, maxAngle);

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
