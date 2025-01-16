#region
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
#endregion

public class NetworkSlider : NetworkBehaviour
{
	[SerializeField] Slider healthSlider;

	public void UpdateSlider(float value)
	{
		if (IsServer) UpdateSliderClientRpc(value);
	}

	[ClientRpc]
	void UpdateSliderClientRpc(float value) => healthSlider.value = value;
}
