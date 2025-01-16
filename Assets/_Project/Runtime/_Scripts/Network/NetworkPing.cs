#region
using Unity.Netcode;
using UnityEngine;
#endregion

public class NetworkPing : NetworkBehaviour
{
	[SerializeField] GameObject prefabToSpawn;

	public void RequestSpawnPrefab(Vector3 position)
	{
		if (IsOwner) SpawnPrefabServerRpc(position);
	}

	[ServerRpc(RequireOwnership = false)]
	void SpawnPrefabServerRpc(Vector3 position)
	{
		GameObject spawnedObject = Instantiate(prefabToSpawn, new Vector2(position.x, position.y + 2), Quaternion.identity);
		spawnedObject.GetComponent<NetworkObject>().Spawn();
		GameObject canvas = GameObject.Find("Ping Canvas"); // NOTE: I AM WELL AWARE THIS CODE IS DOG WATER BUT IT IS WHAT IT IS
		spawnedObject.transform.SetParent(canvas.transform);
		Destroy(spawnedObject, 5f);
	}
}
