using InputSystem;
using Player;
using World;
using UnityEngine;

public class TorchCommand : ICommand
{
    readonly Player.Player player;
    const float RADIUS = 1.0f;         // Circular Interaction Radius
    const float XRANGE = 0.3f;         // Lower Determination: X Error
    const float HEIGHT = 1.0f;         // Maximum Vertical Distance from Torch to Player's Head

    public TorchCommand(Player.Player p) => player = p;

    public void Execute(float _ = 0)
    {
        // 方案 1：圆形范围
        Torch t = Torch.GetNearestInRadius(player.transform.position, RADIUS);
        if (t != null) t.Switch();
    }
}

