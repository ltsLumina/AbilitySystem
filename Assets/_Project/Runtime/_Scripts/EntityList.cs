using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntityList<T> : IEnumerable<Entity> where T : Entity
{
    [SerializeField] List<T> collection = new ();
    
    public static EntityList<T> operator +(EntityList<T> list, T entity)
    {
        list.Add(entity);
        return list;
    }
    
    public static EntityList<T> operator -(EntityList<T> list, T entity)
    {
        list.Remove(entity);
        return list;
    }
    
    public Entity this[int i] => collection[i];
    public void Add(T entity) => collection.Add(entity);
    public void Remove(T entity) => collection.Remove(entity);
    public void Clear() => collection.Clear();
    public int Count => collection.Count;

    public bool Contains(T entity) => collection.Contains(entity);

    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => collection.GetEnumerator();

    public IEnumerator GetEnumerator() => collection.GetEnumerator();
}
