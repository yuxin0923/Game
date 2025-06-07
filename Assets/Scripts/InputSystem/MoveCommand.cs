// using InputSystem;

// public class MoveCommand : ICommand
// {
//     readonly Player.Player player;
//     public MoveCommand(Player.Player p) => player = p;

//     public void Execute(float value = 0) => player.Move(value);
// }
// Assets/Scripts/Player/MoveCommand.cs
using InputSystem;
using UnityEngine;

public class MoveCommand : ICommand
{
    readonly Player.Player player;
    public MoveCommand(Player.Player p) => player = p;

    public void Execute(float value = 0f)
    {
        // 只有 value != 0 时才把速度设给玩家
        if (Mathf.Abs(value) > 0.01f)
            player.Move(value);
        // 否则什么都不做，不会把 velocity.x 清 0
    }
}
