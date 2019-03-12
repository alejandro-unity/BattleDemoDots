using System.ComponentModel;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public struct Target : IComponentData
{
    public Entity Value;
}

public class AssignTargetSystem : JobComponentSystem
{
    
    ComponentGroup m_AllSoldiers;
    ComponentGroup m_SoldiersWithoutTarget;
    EndSimulationEntityCommandBufferSystem endSimCommandBuffer;
    struct AssignTargetJob:IJobProcessComponentDataWithEntity<SoldierTag>
    {
        //public EntityCommandBuffer commandBuffer;
        public EntityCommandBuffer.Concurrent commandBuffer;
        [Unity.Collections.ReadOnly, DeallocateOnJobCompletion] public NativeArray<Entity> potentialTargets;
        public int randomNumber;
        
        // add Target componentData
        public void Execute(Entity entity, int index, [Unity.Collections.ReadOnly]ref SoldierTag soldierTag)
        {
            Target target = new Target();
            // using the old random which is not great
            target.Value = potentialTargets[randomNumber];
            commandBuffer.AddComponent(index, entity , target );
        }
    }

    protected override void OnCreateManager()
    {
        m_AllSoldiers = GetComponentGroup(
            ComponentType.ReadOnly<SoldierTag>(),
            ComponentType.ReadOnly<RenderMesh>() // renderMesh ? 
        );
        m_SoldiersWithoutTarget = GetComponentGroup(
            ComponentType.ReadOnly<SoldierTag>(),
            ComponentType.ReadOnly<RenderMesh>(),
            ComponentType.Exclude<Target>());

        endSimCommandBuffer = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        // filter by SharedComponentData
        // We can also filter using a componentData for each team 
        var redRenderMesh  = EntityManager.GetSharedComponentData<RenderMesh>(GetSingleton<BattleConfigData>().prefabRed);
        var blueRenderMesh = EntityManager.GetSharedComponentData<RenderMesh>(GetSingleton<BattleConfigData>().prefabBlue);
        // get red soldiers
        m_AllSoldiers.SetFilter(redRenderMesh);
        NativeArray<Entity> redSoldierEntities = m_AllSoldiers.ToEntityArray(Allocator.TempJob);
        // get blue soldiers
        m_AllSoldiers.SetFilter(blueRenderMesh);
        NativeArray<Entity> blueSoldierEntities = m_AllSoldiers.ToEntityArray(Allocator.TempJob);
        
        //CONFIGURE RED SOLDIERS
        // Schedule Job only on the Group of m_SoldiersWithoutTarget
        // Red soldiers without target 
        if (blueSoldierEntities.Length > 0)
        {
            m_SoldiersWithoutTarget.SetFilter(redRenderMesh);
            handle = new AssignTargetJob
            {
                // when to use ToConcurrent ? 
                commandBuffer = endSimCommandBuffer.CreateCommandBuffer().ToConcurrent(),
                potentialTargets = blueSoldierEntities,
                // use the old style random which doesnt work correctly.
                randomNumber =  UnityEngine.Random.Range(0, blueSoldierEntities.Length )
            }.ScheduleGroup(m_SoldiersWithoutTarget, handle);

            endSimCommandBuffer.AddJobHandleForProducer(handle);
        }
        else
        {
            blueSoldierEntities.Dispose();
            
        }

        
        //CONFIGURE BLUE SOLDIERS
        if (redSoldierEntities.Length > 0)
        {
        }
        else
        {
            redSoldierEntities.Dispose();
        }

        // dispose
        return handle;
    }
}
