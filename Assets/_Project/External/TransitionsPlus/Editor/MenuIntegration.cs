#region
using UnityEditor;
using UnityEngine;
#endregion

namespace TransitionsPlus
{
public static class MenuIntegration
{
    [MenuItem("GameObject/Effects/Transitions Plus")]
    public static void CreateTransitionRootMenu()
    {
        GameObject o = TransitionAnimator.CreateTransition();

        Undo.RegisterCreatedObjectUndo(o, "Create Transition");

        Selection.activeGameObject = o.GetComponentInChildren<TransitionAnimator>().gameObject;
    }
}
}
