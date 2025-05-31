using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image healthBarFill; // 血条填充组件
    public Flashlight flashlight; // 玩家手电筒组件

    // 控制血条变化的速度，数值越高，变化越快
    public float fillSpeed = 5f;

    private void Update()
    {
        if (flashlight != null)
        {
            // 获取当前的电量百分比
            //float targetFill = flashlight.GetCurrentCharge() / flashlight.maxCharge;
            float targetFill = flashlight.ChargePercent;   // 已经 0~1


            // 使用 Lerp 函数来平滑变化血条
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFill, fillSpeed * Time.deltaTime);
        }
    }
}
