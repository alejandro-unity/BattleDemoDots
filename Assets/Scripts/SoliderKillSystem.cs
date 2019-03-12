using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class SoliderKillSystem : JobComponentSystem
{
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
                    CommandBuffer.DestroyEntity(index, entity);
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
