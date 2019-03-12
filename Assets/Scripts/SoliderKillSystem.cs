using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class SoliderKillSystem : JobComponentSystem
{
    /*Problems: 
        The system keeps moving the player with the orientation [fixed] 
        We need to remove the Target. [fixed but not correctly] 
        If the target.value do not exist we can set the orientation to zero [fixed]
        Problem with the TargetDebugSystem, disable for now        
        
        All entities passed to EntityManager must exist. One of the entities has already been destroyed or was never created.
     */ 

    private EndSimulationEntityCommandBufferSystem endSimCmd;
    
    public struct SoliderContactJob : IJobProcessComponentDataWithEntity<Translation, Target>
    {
        [ReadOnly] public ComponentDataFromEntity<Translation> allTranslation;
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute(Entity entity, int index, [ReadOnly]ref Translation translation, ref Target target)
        {
            if (allTranslation.Exists(target.Value))
            {
                if (math.distancesq(translation.Value, allTranslation[target.Value].Value) < 1)
                {
                    // Can we destroy the target here ?  
                    CommandBuffer.DestroyEntity(index, target.Value);
                    //We can not remove the Target since we are iterating over it
                    CommandBuffer.RemoveComponent<Target>( index, entity );
                }
            }
        }
    }

    protected override void OnCreateManager()
    {
        endSimCmd = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        ComponentDataFromEntity<Translation> allTranslation =  GetComponentDataFromEntity<Translation>();
        handle = new SoliderContactJob
        {
            allTranslation = allTranslation,
            CommandBuffer  = endSimCmd.CreateCommandBuffer().ToConcurrent()
            
        }.Schedule(this, handle);
        
        endSimCmd.AddJobHandleForProducer(handle);
        
        return handle;
    }
    
}
