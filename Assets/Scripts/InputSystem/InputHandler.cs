using UnityEngine;
using InputSystem;
using UInput = UnityEngine.Input;          // ← Alias Unity's old input system.

/*
 InputHandler.cs — Glue from Unity Input → Command Objects
 ---------------------------------------------------------
 • Role  
   Collects raw Unity-Input each frame and translates it into discrete
   ICommand executions (Move, Jump, ToggleTorch, Teleport).  
   Lives in the **InputSystem** package alongside the individual command
   classes, keeping all control-scheme code in one place.

 • Design pattern  
   Command Pattern  
   – Each player action is an object implementing `ICommand.Execute()`.  
   – InputHandler is merely the invoker; gameplay logic resides in the
     command classes and the Player façade.  
   Benefits:  
     • Easy to add/remap inputs (create a new command, wire a key).  
     • Commands can be queued, replayed, or network-serialized for
       replays and multiplayer.

 • Key interactions  
   UnityEngine.Input → InputHandler → ICommand → Player API  
   (no direct Player movement code here, keeping the class lightweight and
   agnostic of gameplay rules.)
*/

public class InputHandler : MonoBehaviour
{
    [SerializeField] Player.Player player;

    ICommand moveCmd, jumpCmd, torchCmd, tpCmd;

    void Awake()
    {
        moveCmd = new MoveCommand(player);
        jumpCmd = new JumpCommand(player);
        torchCmd = new TorchCommand(player);          // L key to extinguish / ignite torch
        tpCmd = new TeleportTorchCommand(player);  // M key to teleport
        // torchCmd = new TorchCommand(player);
    }

    void Update()
    {
        // Horizontal movement
        float h = UInput.GetAxisRaw("Horizontal");
        if (Mathf.Abs(h) > 0.01f) moveCmd.Execute(h);
        else moveCmd.Execute(0);   // Reset when released

        // Jump
        if (UInput.GetButtonDown("Jump"))
            jumpCmd.Execute();

        if (Input.GetKeyDown(KeyCode.L))
            torchCmd.Execute();          // L key to extinguish / ignite torch

        // M key to teleport to the nearest burning torch
        if (UInput.GetKeyDown(KeyCode.M))
            tpCmd.Execute();
            
    }
}
