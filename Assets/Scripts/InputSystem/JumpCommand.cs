using InputSystem;
public class JumpCommand : ICommand
{
    readonly Player.Player player;
    public JumpCommand(Player.Player p) => player = p;

    public void Execute(float _ = 0) => player.Jump();
}