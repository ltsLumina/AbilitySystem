#region
using Lumina.Essentials.Modules;
using UnityEngine;
#endregion

// Singleton class that persists through scenes.
[DefaultExecutionOrder(-2)]
public class Singleton<T> : MonoBehaviour
    where T : Component
{
    static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                // In Editor, the instance is found by searching for the object as it is normally set in playmode.
                if (!Application.isPlaying && Application.isEditor) {
                    instance = (T) Helpers.Find(typeof(T));
                    return instance;
                }
                Debug.LogError("No object of type " + typeof(T).FullName + " was found.");
                return null;
            }

            return instance;
        }
    }

    void Awake()
    {
        if (instance == null) instance = this as T;
        else Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}

/// <summary>
///     Allows you to create a singleton that persists through scenes by inheriting from this class.
/// </summary>
/// <typeparam name="T"> The class to turn into a singleton. </typeparam>
public class SingletonPersistent<T> : MonoBehaviour
    where T : Component
{
    static T instance;

    public static T Instance
    {
        get
        {
            if (Application.isEditor)
            {
                instance = (T) Helpers.Find(typeof(T));
                return instance;
            }
            
            if (instance == null)
            {
                Debug.LogError("No object of type " + typeof(T).FullName + " was found.");
                return null;
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;

            if (transform.parent == null) DontDestroyOnLoad(gameObject);
            else DontDestroyOnLoad(gameObject.transform.parent);
        }
        else { Destroy(gameObject); }
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
