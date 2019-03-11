using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
public struct BattleConfigData : IComponentData
{
    public Entity prefabRed;
    public Entity prefabBlue;
}

public class BattleConfig : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject prefabRed;
    public GameObject prefabBlue;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity , new BattleConfigData
        {
            prefabRed  = conversionSystem.GetPrimaryEntity(prefabRed),
            prefabBlue = conversionSystem.GetPrimaryEntity(prefabBlue)
        });
    }    

    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(prefabRed);
        gameObjects.Add(prefabBlue);
    }
}
