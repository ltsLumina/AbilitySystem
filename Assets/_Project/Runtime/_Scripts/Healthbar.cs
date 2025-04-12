#region
using Lumina.Essentials.Attributes;
using UnityEngine;
#endregion

public class Healthbar : MonoBehaviour
{
	[SerializeField] [ReadOnly] int hearts;
	[SerializeField] [ReadOnly] Player owner;

	void Start() => PlayerManager.Instance.OnPlayerJoined += player => { owner = player; };

	void Update()
	{
		if (owner == null) return;

		hearts = owner.Health;

		for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(i < hearts);
	}
}
