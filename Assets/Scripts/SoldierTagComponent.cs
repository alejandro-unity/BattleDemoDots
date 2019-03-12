using Unity.Entities;
using UnityEngine;
public struct SoldierTag : IComponentData
{
}
public class SoldierTagComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new SoldierTag());
    }
}
