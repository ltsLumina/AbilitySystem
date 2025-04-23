using UnityEngine;
using UnityEngine.EventSystems;

public class ItemButton : MonoBehaviour, ISelectHandler, ISubmitHandler, IPointerClickHandler
{
	[SerializeField] SceneItem item; // The item this button represents

	public void OnSelect(BaseEventData eventData)
	{
		Debug.Log($"Selected by: {eventData.selectedObject.name}");
	}

	public void OnSubmit(BaseEventData eventData)
	{
		Debug.Log($"Submitted by: {eventData.selectedObject.name}");
	}

	public void Test()
	{
		Debug.Log($"Test method called for {item.name}");
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log("pressed by: " + eventData.pointerId);
	}
}
