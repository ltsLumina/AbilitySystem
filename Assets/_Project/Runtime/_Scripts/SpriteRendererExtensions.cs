using System.Collections;
using DG.Tweening;
using UnityEngine;

public static class SpriteRendererExtensions
{
    public static void FlashSprite(this SpriteRenderer spriteRenderer, Color color, float duration) { spriteRenderer.DOColor(color, duration).SetLoops(2, LoopType.Yoyo).SetEase(ease: Ease.InOutSine); }

    public static IEnumerator CreateAfterImages(this SpriteRenderer spriteRenderer, float interval, float duration, int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateAfterImage(spriteRenderer, duration);
            yield return new WaitForSeconds(interval);
        }
    }

    public static void CreateAfterImage(SpriteRenderer spriteRenderer, float duration)
    {
        GameObject afterImage = new GameObject("AfterImage");
        SpriteRenderer afterImageRenderer = afterImage.AddComponent<SpriteRenderer>();
        afterImageRenderer.sprite = spriteRenderer.sprite;
        afterImageRenderer.color = new (1, 1, 1, 0.5f); // Semi-transparent
        afterImage.transform.position = spriteRenderer.transform.position;
        afterImage.transform.rotation = spriteRenderer.transform.rotation;
        afterImage.transform.localScale = spriteRenderer.transform.localScale;

        afterImageRenderer.DOFade(0, duration).OnComplete(() => Object.Destroy(afterImage));
    }
}
