using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SoldierMovementSystem : JobComponentSystem
{
    public struct SoldierOrientationJob : IJobProcessComponentData<Translation, Target, SoldierOrientation>
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
        }
    }
    public struct SoldierMovSystemJob : IJobProcessComponentData<Translation, SoldierOrientation>
    {
        public float dt;
        public void Execute(ref Translation translation, [ReadOnly]ref SoldierOrientation soldierOrientation)
        {
            translation.Value += soldierOrientation.Value * dt;
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
