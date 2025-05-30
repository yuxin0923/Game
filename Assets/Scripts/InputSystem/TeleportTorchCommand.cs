using InputSystem;
using Player;

public class TeleportTorchCommand : ICommand
{
    readonly Player.Player player;
    public TeleportTorchCommand(Player.Player p) => player = p;

    public void Execute(float _ = 0) => player.TeleportToNearestBurningTorch();
}
