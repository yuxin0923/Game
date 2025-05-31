// Assets/Scripts/Player/Flashlight.cs
using UnityEngine;
using UnityEngine.Rendering.Universal; // 若无需灯光可删

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public float maxCharge           = 100f;   // 最大电量
    public float chargeDepletionRate = 5f;     // 不充电时，每秒掉电量
    public float rechargeRate        = 25f;    // 充电时，每秒回血量

    [Header("Optional Visual")]
    public Light2D light2D;                   // 若有 2D 灯光，可拖进来

    public float CurrentCharge { get; private set; }

    bool isRecharging;
    bool hasDied;
    Player.Player owner;                     // 引用 Player 用于死亡

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

    // ——仅根据 isRecharging，增/减电量——
    void UpdateCharge(float deltaTime)
    {
        if (isRecharging)
            CurrentCharge += rechargeRate * deltaTime;
        else
            CurrentCharge -= chargeDepletionRate * deltaTime;

        CurrentCharge = Mathf.Clamp(CurrentCharge, 0f, maxCharge);
    }

    // ——灯光强度随电量明暗——
    void UpdateLightIntensity()
    {
        if (light2D)
            light2D.intensity = CurrentCharge / maxCharge;
    }

    // ——电量耗尽时只触发一次 Player.Die()——
    void CheckForDeath()
    {
        if (hasDied) return;

        if (CurrentCharge <= 0f && owner != null)
        {
            hasDied = true;
            owner.Die();
        }
    }

    // 供 Player 或外部调用
    public void StartRecharge() => isRecharging = true;
    public void StopRecharge()  => isRecharging = false;

    public float ChargePercent => CurrentCharge / maxCharge;
}
