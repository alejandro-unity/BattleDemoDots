using Unity.Burst;
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
    [BurstCompile]
    public struct SoliderContactJob : IJobProcessComponentDataWithEntity<Translation, Target>
    {
        [ReadOnly] public ComponentDataFromEntity<Translation> allTranslation;
        public EntityCommandBuffer.Concurrent CommandBuffer;
        // if other thread mark this variable as 0 is ok  
        [NativeDisableParallelForRestriction]public ComponentDataFromEntity<SoldierAlive> aliveFromEntity;
        public void Execute(Entity entity, int index, [ReadOnly]ref Translation translation, ref Target target)
        {
            if (allTranslation.Exists(target.Value))
            {
                if (math.distancesq(translation.Value, allTranslation[target.Value].Value) < 1)
                {
                    aliveFromEntity[target.Value] = new SoldierAlive { Value = 0 };
                }
            }
        }
    }
    
    // Remove targets.
    // Retrieve all entities with target and check if that entity still exist 
    struct RemoveInvalidTargets : IJobProcessComponentDataWithEntity<Target>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        [ReadOnly] public ComponentDataFromEntity<Translation> PositionFromEntity;
        public void Execute(Entity entity, int index, ref Target target)
        {
            if(!PositionFromEntity.Exists(target.Value))
                CommandBuffer.RemoveComponent<Target>(index, entity);
        }
    }
    
    // use the SoldierAlive value to Destroy the entities
    [BurstCompile]
    struct RemoveDeadSoldiers : IJobProcessComponentDataWithEntity<SoldierAlive>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute(Entity entity, int index, ref SoldierAlive alive)
        {
            if(alive.Value == 0)
                CommandBuffer.DestroyEntity(index, entity);
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
            allTranslation =  allTranslation,
            aliveFromEntity = GetComponentDataFromEntity<SoldierAlive>(),
            CommandBuffer  = endSimCmd.CreateCommandBuffer().ToConcurrent()
            
        }.Schedule(this, handle);
        
        // Job that remove the target  
        handle = new RemoveInvalidTargets
        {
            CommandBuffer = endSimCmd.CreateCommandBuffer().ToConcurrent(),
            PositionFromEntity = GetComponentDataFromEntity<Translation>()
        }.Schedule(this, handle);
        
        // Job that Destroy entities
        handle = new RemoveDeadSoldiers
        {
            CommandBuffer = endSimCmd.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, handle);
        
        
        // setup the endsimCMD
        endSimCmd.AddJobHandleForProducer(handle);
        
        return handle;
    }
    
}
