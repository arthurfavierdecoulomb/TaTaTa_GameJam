using UnityEngine;

public class Battery : MonoBehaviour
{
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FlashlightStamina stamina = other.GetComponentInChildren<FlashlightStamina>();

            if (stamina != null)
            {
                stamina.RechargeStamina();
                Destroy(gameObject); // Fait disparaître la pile
            }
        }
    }
}