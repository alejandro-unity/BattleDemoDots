using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class TargetDebugSystem : ComponentSystem
{
    protected override void OnCreateManager()
    {
        Enabled = false;
    }

    protected override void OnUpdate()
    {
        
        //changed to use the GetComponentDataFromEntity
        var allEnitities = GetComponentDataFromEntity<Translation>(true);
        ForEach((Entity entity, ref Target target) =>
        {
            // if the entity was killed
            if (allEnitities.Exists(target.Value))
            {
                var entityTranslation = allEnitities[entity].Value;
                var targetTranslation = allEnitities[target.Value].Value;
                Debug.DrawLine(entityTranslation, targetTranslation);
            }
        });

    }
}
