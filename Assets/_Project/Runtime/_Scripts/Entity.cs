using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
///     <para>
///         An entity is a game object that can interact with other entities.
///         It is the base class for all entities in the game, and should be inherited from by all entities.
///         A game object is not an entity unless it inherits from this class.
///     </para>
/// </summary>
public abstract class Entity : MonoBehaviour, IEntity
{
    public enum ActiveState
    {
        Enabled,
        Disabled,
    }
    
    public ActiveState State { get; private set; }

    #region Events
    public event Action<Entity> OnEntityEnable;
    public event Action<Entity> OnEntityDisable;
    public event Action<Entity> OnEntityDestroy;
    #endregion
    
    void OnEnable() => OnEntityEnable?.Invoke(this);
    void OnDisable() => OnEntityDisable?.Invoke(this);
    void OnDestroy() => OnEntityDestroy?.Invoke(this);

    public override string ToString()
    {
        return base.ToString();
    }

    bool hasTicked;
    
    protected virtual IEnumerator Start()
    {
        Initialize();

        var tickTask = InitializeTick();

        yield return new WaitForSeconds(1); Debug.Assert(
            tickTask.Task.IsCompleted, $"{name} has not ticked after 1 second. " + 
                   "\nEnsure the TickManager is running and that your entity is calling base.Start() in its Start method.");

        yield break;
        TaskCompletionSource<bool> InitializeTick()
        {
            var tcs = new TaskCompletionSource<bool>();

            TickManager.OnMicroTick += () =>
            {
                OnMicroTick();
                tcs.TrySetResult(true);
            };
            
            TickManager.OnTick += OnTick;

            return tcs;
        }

        void Initialize()
        {
            // any other initialization code here
        }
    }

    protected abstract void OnMicroTick();

    protected abstract void OnTick();
    
    public void Destroy()
    {
        StartCoroutine(DestroyCoroutine(0));
    }
    
    public void Destroy(float delay)
    {
        StartCoroutine(DestroyCoroutine(delay));
    }

    IEnumerator DestroyCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            Debug.Log($"{name} collided with {other.gameObject.name}.");
        }
    }

}

public static class EntityExtensions
{
    public static void Destroy(this IEntity entity)
    {
        entity.Destroy();
    }
    
    public static void Destroy(this IEntity entity, float delay)
    {
        entity.Destroy(delay);
    }
}