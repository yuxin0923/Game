// Assets/Scripts/Player/Flashlight.cs
using UnityEngine;
using UnityEngine.Rendering.Universal; 

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public float maxCharge           = 100f;   // Maximum charge
    public float chargeDepletionRate = 5f;     // Charge depletion rate per second when not recharging
    public float rechargeRate        = 25f;    // Charge recharge rate per second

    [Header("Optional Visual")]
    public Light2D light2D;                   // If there's a 2D light, you can drag it in

    public float CurrentCharge { get; private set; }

    bool isRecharging;
    bool hasDied;
    Player.Player owner;                     // Reference to Player for death

    void Awake()
    {
        CurrentCharge = maxCharge;
        owner = GetComponent<Player.Player>();
        if (!owner)
            owner = GetComponentInParent<Player.Player>();

        if (!light2D)
            light2D = GetComponent<Light2D>();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        UpdateCharge(dt);
        UpdateLightIntensity();
        CheckForDeath();
    }

    // ——Increase/decrease power only according to isRecharging——
    void UpdateCharge(float deltaTime)
    {
        if (isRecharging)
            CurrentCharge += rechargeRate * deltaTime;
        else
            CurrentCharge -= chargeDepletionRate * deltaTime;

        CurrentCharge = Mathf.Clamp(CurrentCharge, 0f, maxCharge);
    }

    // ——Light intensity changes with charge——
    void UpdateLightIntensity()
    {
        if (light2D)
            light2D.intensity = CurrentCharge / maxCharge;
    }

    // ——Only trigger Player.Die() once when out of power——
    void CheckForDeath()
    {
        if (hasDied) return;

        if (CurrentCharge <= 0f && owner != null)
        {
            hasDied = true;
            owner.Die();
        }
    }

    public void DrainAll()
    {
        CurrentCharge = 0f;
        // Immediately update light intensity to ensure visual extinguishing
        if (light2D)
            light2D.intensity = 0f;
    }

    public void ReduceCharge(float amount)
    {
        if (hasDied) return;

        CurrentCharge -= amount;
        CurrentCharge = Mathf.Max(CurrentCharge, 0f);
        if (light2D)
            light2D.intensity = CurrentCharge / maxCharge;

        // If depleted, call Player's Die()
        if (CurrentCharge <= 0f && owner != null)
        {
            hasDied = true;
            owner.Die();
        }
    }

    // For Player or external calls
    public void StartRecharge() => isRecharging = true;
    public void StopRecharge()  => isRecharging = false;

    public float ChargePercent => CurrentCharge / maxCharge;
}
