// Assets/Scripts/Physic/PhysicsMaterial.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewPhysicsMaterial", menuName = "Physic/Physics Material")]
public class PhysicsMaterial : ScriptableObject
{
    [Header("coefficient of friction")]
    [Range(0, 1)] public float friction = 0.5f;

    [Header("Surface Type")]
    public SurfaceType surfaceType = SurfaceType.Normal;
    
    public enum SurfaceType
    {
        Normal,  // Normal surface (grass, etc.)
        Ice,     // Ice surface
        Mud      // Muddy surface
    }
}