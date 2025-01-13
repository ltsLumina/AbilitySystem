#region
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
#endregion

public class DamagePopupManager : NetworkBehaviour
{
	[SerializeField] GameObject damageNumberPrefab;

	// This method is called on the server and triggers a ClientRpc
	public void SpawnPopUpText(float damage, Vector3 position) => SpawnPopUpTextClientRpc(damage, position);

	[Rpc(SendTo.Everyone)]
	void SpawnPopUpTextClientRpc(float damage, Vector3 position)
	{
		var canvas = GameObject.FindGameObjectWithTag("Finish");

		// Instantiate the damage popup prefab
		TextMeshProUGUI damageNumber = Instantiate(damageNumberPrefab.GetComponent<TextMeshProUGUI>(), position, Quaternion.identity);

		// Apply random offset and rotation
		Vector2 randomPosition = Random.insideUnitCircle * 2f;
		int randomRotation = Random.Range(-25, 25);
		damageNumber.transform.position = position + new Vector3(randomPosition.x, randomPosition.y);

		// Animate the popup
		Sequence sequence = DOTween.Sequence();
		sequence.Append(damageNumber.transform.DOMove(position + new Vector3(Random.insideUnitCircle.x * 3, Random.insideUnitCircle.y * 3), 2f).SetEase(Ease.OutQuad));
		sequence.Join(damageNumber.transform.DORotate(new (0, 0, randomRotation), 2f).SetEase(Ease.OutQuad));
		sequence.AppendInterval(1f);
		sequence.Append(damageNumber.DOFade(0, 1f).SetEase(Ease.OutQuad));
		sequence.OnComplete(() => Destroy(damageNumber.gameObject));

		// Set text and parent
		damageNumber.text = damage.ToString("F0");
		damageNumber.transform.SetParent(canvas.transform, false);
	}
}
