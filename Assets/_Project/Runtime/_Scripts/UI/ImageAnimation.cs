#region
using UnityEngine;
using UnityEngine.UI;
#endregion

[RequireComponent(typeof(Image))]
public class ImageAnimation : MonoBehaviour
{
	public Sprite[] sprites;
	public int framesPerSprite = 20;
	public bool loop = true;
	public bool destroyOnEnd;

	int index;
	Image image;
	int frame;

	void Awake() => image = GetComponent<Image>();

	void FixedUpdate()
	{
		if (!loop && index == sprites.Length) return;
		frame++;
		if (frame < framesPerSprite) return;
		image.sprite = sprites[index];
		frame = 0;
		index++;

		if (index >= sprites.Length)
		{
			if (loop) index = 0;
			if (destroyOnEnd) Destroy(gameObject);
		}
	}
}
