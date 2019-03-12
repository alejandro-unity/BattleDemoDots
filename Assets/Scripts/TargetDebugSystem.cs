using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class TargetDebugSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        //you can also use: var allEnitities = GetComponentDataFromEntity<Translation>(true); and get the array element
        
        // All entities with target assigned
        ForEach((Entity entity, ref Target target) =>
        {
            // if the entity was killed
            if (target.Value != Entity.Null)
            {
                var entityTranlation = EntityManager.GetComponentData<Translation>(entity);
                var targetTranslation = EntityManager.GetComponentData<Translation>(target.Value);
                Debug.DrawLine(entityTranlation.Value, targetTranslation.Value);
            }
        });

    }
}
