#region
using Lumina.Essentials.Attributes;
using UnityEngine;
using static Lumina.Essentials.Modules.Helpers;
#endregion

public class Healthbar : MonoBehaviour
{
	[SerializeField] [ReadOnly] int hearts;

	Player player;

	void Start() => player = Find<Player>();

	void Update()
	{
		hearts = player.Health;

		for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(i < hearts);
	}
}
