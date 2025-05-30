// Assets/Scripts/Player/Flashlight.cs
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public float maxCharge          = 100f;   // 最大电量
    public float chargeDepletionRate = 5f;    // 每秒掉电量
    public float rechargeRate        = 25f;   // 每秒充电量

    [Header("Optional Visual")]
    public UnityEngine.Rendering.Universal.Light2D light2D; // 若有 2D 光源，可拖进来

    public float CurrentCharge { get; private set; }

    /* 如果 UI 想走事件流，可订阅这个回调，参数 0~1 */
    public System.Action<float> onChargeChanged;

    bool isRecharging;

    void Awake()
    {
        CurrentCharge = maxCharge;
        if (!light2D) light2D = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // ↓↓↓ 电量更新 --------------------------------------------------
        if (isRecharging)
            CurrentCharge += rechargeRate * dt;
        else
            CurrentCharge -= chargeDepletionRate * dt;

        CurrentCharge = Mathf.Clamp(CurrentCharge, 0, maxCharge);

        // ↓↓↓ 可视/事件 --------------------------------------------------
        if (light2D)
            light2D.intensity = CurrentCharge / maxCharge;      // 灯光强度衰减

        onChargeChanged?.Invoke(CurrentCharge / maxCharge);      // 推送给 UI

        // ↓↓↓ 死亡判定 --------------------------------------------------
        if (CurrentCharge <= 0f)
            Die();
    }

    /* ============ API ============ */
    public void StartRecharge() => isRecharging = true;
    public void StopRecharge()  => isRecharging = false;

    public float GetCurrentCharge() => CurrentCharge;            // 兼容旧调用
    public float ChargePercent   => CurrentCharge / maxCharge;   // UI 更方便

    /* ============ 私有 ============ */
    void Die()
    {
        Debug.Log("Player died: flashlight out of charge");
        // TODO: 调 GameManager / UIManager 做死亡流程
        Destroy(gameObject);
    }
}
