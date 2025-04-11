#region
using UnityEngine;
#endregion

public class Orb : MonoBehaviour
{
	void OnBecameInvisible() => Destroy(gameObject);
}
