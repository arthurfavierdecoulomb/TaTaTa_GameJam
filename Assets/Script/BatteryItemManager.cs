using UnityEngine;

public class Battery : MonoBehaviour
{
    [SerializeField] FlashlightStamina stamina; // glissez SpotLight2D ici

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            stamina.RechargeStamina();
            Destroy(gameObject);
        }
    }
}