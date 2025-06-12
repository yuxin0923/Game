using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image healthBarFill; // bloodstain filler component
    public Flashlight flashlight; // flashlight component

    // Control the speed of health bar changes, the higher the value, the faster the change
    public float fillSpeed = 5f;

    private void Update()
    {
        if (flashlight != null)
        {
            // Get the current charge percentage
            //float targetFill = flashlight.GetCurrentCharge() / flashlight.maxCharge;
            float targetFill = flashlight.ChargePercent;   // Already 0~1


            // Use Lerp function to smooth the health bar changes
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFill, fillSpeed * Time.deltaTime);
        }
    }
}
