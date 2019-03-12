using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SoldierMovementSystem : JobComponentSystem
{
    public struct SoldierMovSystemJob : IJobProcessComponentData<Translation, Target>
    {
        [ReadOnly] public ComponentDataFromEntity<Translation> allPositions;
        public float dt;
        public void Execute(ref Translation translation, ref Target target )
        {
            // can not be accessed in the Job
            //var t = EntityManager.GetComponentData<Translation>(target.Value);
            // it could be killed
            if (allPositions.Exists(target.Value))
            {
                var src = translation.Value;
                var dst = allPositions[target.Value].Value;
                var dir = math.normalizesafe(src - dst);
                translation.Value += dir * dt;
            }
        }
        // this job has this issue 
        /*The writable NativeArray SoldierMovSystemJob.Iterator is the same NativeArray as
         SoldierMovSystemJob.Data.allPositions,
         two NativeArrays may not be the same (aliasing).
         */
    }
    
    protected override JobHandle OnUpdate(JobHandle handle )
    {
        handle = new SoldierMovSystemJob
        {
            // this needs to be readOnly
            allPositions = GetComponentDataFromEntity<Translation>(),
            dt = Time.deltaTime
            
        }.Schedule(this, handle);
        return handle;
    }
}
