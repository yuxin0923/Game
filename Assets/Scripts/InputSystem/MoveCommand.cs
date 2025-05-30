using InputSystem;

public class MoveCommand : ICommand
{
    readonly Player.Player player;
    public MoveCommand(Player.Player p) => player = p;

    public void Execute(float value = 0) => player.Move(value);
}