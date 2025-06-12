using UnityEngine;
using InputSystem;
using UInput = UnityEngine.Input;          // ← 给 Unity 的旧输入系统起别名


public class InputHandler : MonoBehaviour
{
    [SerializeField] Player.Player player;

    ICommand moveCmd, jumpCmd, torchCmd, tpCmd;

    void Awake()
    {
        moveCmd = new MoveCommand(player);
        jumpCmd = new JumpCommand(player);
        torchCmd = new TorchCommand(player);          // L 键熄 / 点火把
        tpCmd = new TeleportTorchCommand(player);  // M 键瞬移
        // torchCmd = new TorchCommand(player);
    }

    void Update()
    {
        // 水平移动
        float h = UInput.GetAxisRaw("Horizontal");
        if (Mathf.Abs(h) > 0.01f) moveCmd.Execute(h);
        else moveCmd.Execute(0);   // 松开时清零

        // 跳跃
        if (UInput.GetButtonDown("Jump"))
            jumpCmd.Execute();

        if (Input.GetKeyDown(KeyCode.L))
            torchCmd.Execute();          // 已有：熄 / 点火把

        // 瞬移到最近的燃烧火把（默认 M 键）
        if (UInput.GetKeyDown(KeyCode.M))
            tpCmd.Execute();
            
        // if (GameCore.GameManager.I.State == GameCore.GameState.Died &&
        // Input.GetKeyDown(KeyCode.R))
        // {
        //     GameCore.GameManager.I.RestartLevel();
        // }


        // 手电（默认 F 键）
        //     if (UInput.GetKeyDown(KeyCode.F))
        //         torchCmd.Execute();
    }
}
