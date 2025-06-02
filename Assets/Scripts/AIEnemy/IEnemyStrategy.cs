
// Assets/Scripts/AI/AIContracts.cs
using UnityEngine;

namespace AIEnemy
{
    /// 敌人有限状态机可取值
    public enum EnemyState { Patrol, Chase, Attack, Dead }

    /// 所有行为策略都实现这个接口
    public interface IEnemyStrategy
    {
        /// 返回 true  ⇒ 需要跳出当前状态
        bool Execute(AIEnemyManager ctx, float dt);
    }
}
