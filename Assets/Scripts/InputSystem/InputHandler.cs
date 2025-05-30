using UnityEngine;
using InputSystem;
using UInput = UnityEngine.Input;          // ← 给 Unity 的旧输入系统起别名

public class InputHandler : MonoBehaviour
{
    [SerializeField] Player.Player player;

    ICommand moveCmd, jumpCmd;

    void Awake()
    {
        moveCmd  = new MoveCommand(player);
        jumpCmd  = new JumpCommand(player);
        // torchCmd = new TorchCommand(player);
    }

    void Update()
    {
        // 水平移动
        float h = UInput.GetAxisRaw("Horizontal");
        if (Mathf.Abs(h) > 0.01f) moveCmd.Execute(h);
        else                      moveCmd.Execute(0);   // 松开时清零

        // 跳跃
        if (UInput.GetButtonDown("Jump"))
            jumpCmd.Execute();

        // 手电（默认 F 键）
    //     if (UInput.GetKeyDown(KeyCode.F))
    //         torchCmd.Execute();
     }
}
