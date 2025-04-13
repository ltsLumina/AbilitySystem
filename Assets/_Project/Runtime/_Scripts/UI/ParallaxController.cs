#region
using UnityEngine;
using static Lumina.Essentials.Modules.Helpers;
#endregion

public class ParallaxController : MonoBehaviour
{
	[SerializeField] float parallaxEffect;

	float length, startPos;

	void Start()
	{
		startPos = transform.position.x;
		length = GetComponent<SpriteRenderer>().bounds.size.x;
	}

	void Update()
	{
		float temp = CameraMain.transform.position.x * (1 - parallaxEffect);
		float dist = CameraMain.transform.position.x * parallaxEffect;
		transform.position = new (startPos + dist, transform.position.y, transform.position.z);

		if (temp > startPos + length) startPos += length;
		else if (temp < startPos - length) startPos -= length;
	}
}
