// Assets/Scripts/Physic/PhysicsMaterial.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewPhysicsMaterial", menuName = "Physic/Physics Material")]
public class PhysicsMaterial : ScriptableObject
{
    [Header("摩擦系数")]
    [Range(0, 1)] public float friction = 0.5f;
    
    [Header("表面类型")]
    public SurfaceType surfaceType = SurfaceType.Normal;
    
    public enum SurfaceType
    {
        Normal,  // 普通表面（草地等）
        Ice,     // 冰面
        Mud      // 泥泞表面
    }
}