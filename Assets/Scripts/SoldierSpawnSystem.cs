﻿using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
public struct SoliderSpawnJob : IJob
{
    public Entity prefabSoldier;
    public float3 position;
    public EntityCommandBuffer CommandBuffer;
    public int count;
       
    public void Execute()
    {
        for (int i = 0; i < count; i++)
        {
            var instance = CommandBuffer.Instantiate(prefabSoldier);
            CommandBuffer.SetComponent(instance, new Translation {Value = position + new float3(i+0.5f,0,0)});
        }
    }
}
public class SoldierSpawnSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBufferSystem;
    protected override void OnCreateManager()
    {
        m_EndSimulationEntityCommandBufferSystem = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
    }
    protected override JobHandle OnUpdate(JobHandle handle)
    {
        if (Camera.main == null)
            return handle;
        Entity prefab = Entity.Null; 
        if (Input.GetMouseButtonDown(0))
        {
            prefab = GetSingleton<BattleConfigData>().prefabRed;
        }
        if (Input.GetMouseButtonDown(1))
        {
            prefab = GetSingleton<BattleConfigData>().prefabBlue;
        }

        if (prefab != Entity.Null)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, 0 );
            bool hit = plane.Raycast(ray, out var dist);
            if (hit)
            {
                SoliderSpawnJob job = new SoliderSpawnJob();
                job.position = ray.GetPoint(dist);
                job.prefabSoldier = prefab;
                job.count = 3;
                job.CommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer();
                handle = job.Schedule(handle);
                m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(handle);
            }
        }
        return handle;
    }
}
