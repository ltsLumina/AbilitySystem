#region
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VInspector;
#endregion

public class EntityManager : Singleton<EntityManager>
{
    [SerializeField] List<Entity> entities = new ();
    [SerializeField] SerializedDictionary<GameObject, Entity.ActiveState> entityStates = new ();
    
    public List<Entity> Entities
    {
        get => entities;
        set => entities = value;
    }
    
    public SerializedDictionary<GameObject, Entity.ActiveState> EntityStates
    {
        get => entityStates;
        set => entityStates = value;
    }
}
