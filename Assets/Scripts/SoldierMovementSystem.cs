using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SoldierMovementSystem : JobComponentSystem
{
    [BurstCompile]
    public struct SoldierOrientationJob : IJobForEach<Translation, Target, SoldierOrientation>
    {
        [ReadOnly] public ComponentDataFromEntity<Translation> allPositions;
        public void Execute([ReadOnly]ref Translation translation, [ReadOnly]ref Target target, ref SoldierOrientation soldierOrientation)
        {
            if (allPositions.Exists(target.Value))
            {
                var src = translation.Value;
                var dst = allPositions[target.Value].Value;
                // just store the orientation
                soldierOrientation.Value = math.normalizesafe(dst - src);
            }
            else
            {
                // prevent to keep moving if the target is destroyed
                soldierOrientation.Value = float3.zero;
            }
        }
    }
    [BurstCompile]
    public struct SoldierMovSystemJob : IJobForEach<Translation, SoldierOrientation>
    {
        public float dt;
        public void Execute(ref Translation translation, [ReadOnly]ref SoldierOrientation soldierOrientation)
        {
            translation.Value += soldierOrientation.Value * dt*2;
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle handle )
    {
        handle = new SoldierOrientationJob
        {
            // this needs to be readOnly
            allPositions = GetComponentDataFromEntity<Translation>(),
        }.Schedule(this, handle);
        
        handle = new SoldierMovSystemJob
        {
            // this needs to be readOnly
            dt = Time.deltaTime
        }.Schedule(this, handle);
        
        return handle;
    }
}
