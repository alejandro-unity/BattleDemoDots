using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public struct SoldierTag : IComponentData
{
}

public struct SoldierOrientation : IComponentData
{
    public float3 Value;
}
public class SoldierTagComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new SoldierTag());
        dstManager.AddComponentData(entity, new SoldierOrientation{Value = float3.zero});
    }
}
