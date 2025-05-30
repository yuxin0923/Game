using InputSystem;
using Player;
using World;
using UnityEngine;

public class TorchCommand : ICommand
{
    readonly Player.Player player;
    const float RADIUS = 1.0f;         // 圆形交互半径
    const float XRANGE = 0.3f;         // 下方判定：X 误差
    const float HEIGHT = 1.0f;         // 火把到玩家头顶的最大垂直距离

    public TorchCommand(Player.Player p) => player = p;

    public void Execute(float _ = 0)
    {
        // 方案 1：圆形范围
        Torch t = Torch.GetNearestInRadius(player.transform.position, RADIUS);

        // 方案 2：只能对脚下火把
        // Torch t = Torch.GetTorchBelow(
        //              player.transform.position,
        //              XRANGE,
        //              player.body.halfSize.y + HEIGHT);

        if (t != null) t.Switch();
    }
}

