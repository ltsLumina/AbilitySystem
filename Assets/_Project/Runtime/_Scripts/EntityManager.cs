using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class EntityManager : Singleton<EntityManager>
{
    [SerializeField] EntityList<Entity> entities = new ();
    [SerializeField] SerializedDictionary<GameObject, Entity.ActiveState> entityStates = new ();

    public EntityList<Entity> Entities
    {
        get => entities;
        set => entities = value;
    }

    void Start()
    {
        Refresh();
    }

    void Update()
    {
        foreach (Entity entity in entities) entityStates[entity.gameObject] = entity.State;
    }

    /// <summary>
    ///    <para> Refreshes the entity list with all entities in the scene. </para>
    /// </summary>
    public void Refresh()
    {
        entities.Clear();
        Entity[] allEntities = FindObjectsByType<Entity>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        foreach (Entity entity in allEntities) entities.Add(entity);
        
        RefreshEvents();
    }

    void RefreshEvents()
    {
        foreach (Entity entity in entities)
        {
            entity.OnEntityEnable += e =>
            {
                Logger.Log($"{e.name} has been enabled."); 
                entityStates[e.gameObject] = Entity.ActiveState.Enabled;
            };
            entity.OnEntityDisable += e =>
            {
                Logger.Log($"{e.name} has been disabled.");
                entityStates[e.gameObject] = Entity.ActiveState.Disabled;
            };
            entity.OnEntityDestroy += e =>
            {
                Logger.Log($"{e.name} has been destroyed.");
                entities -= e;
            };
        }
    }
}
