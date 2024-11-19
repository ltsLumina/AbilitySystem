using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TransitionsPlus {
    public class Misc {
            public static T FindObjectOfType<T>(bool includeInactive = false) where T : Object {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindAnyObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#else
        return Object.FindObjectOfType<T>(includeInactive);
#endif
        }

            public static Object[] FindObjectsOfType(Type type, bool includeInactive = false) {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindObjectsByType(type, includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        return Object.FindObjectsOfType(type, includeInactive);
#endif
        }

            public static T[] FindObjectsOfType<T>(bool includeInactive = false) where T : Object {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindObjectsByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        return Object.FindObjectsOfType<T>(includeInactive);
#endif
        }
    }

}