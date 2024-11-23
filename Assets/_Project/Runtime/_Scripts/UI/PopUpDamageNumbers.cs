#region
using DG.Tweening;
using TMPro;
using UnityEngine;
#endregion

public static class PopUpDamageNumbers
{
    public static void ShowDamage(float damage, Vector3 position)
    {
        var canvas = GameObject.FindGameObjectWithTag("Finish");

        var damageNumberPrefab = Resources.Load<TextMeshProUGUI>("PREFABS/UI/Damage Number");
        Vector2 randomPosition = Random.insideUnitCircle * 2f;
        int randomRotation = Random.Range(-25, 25);
        TextMeshProUGUI damageNumber = Object.Instantiate(damageNumberPrefab, position + new Vector3(randomPosition.x, randomPosition.y), Quaternion.identity);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(damageNumber.transform.DOMove(position + new Vector3(3, 3), 2f).SetEase(Ease.OutQuad));
        sequence.Join(damageNumber.transform.DORotate(new (0, 0, randomRotation), 2f).SetEase(Ease.OutQuad));
        sequence.AppendInterval(1f);
        sequence.Append(damageNumber.DOFade(0, 1f).SetEase(Ease.OutQuad));
        sequence.OnComplete(() => Object.Destroy(damageNumber.gameObject));
        damageNumber.text = damage.ToString("F0");
        damageNumber.transform.SetParent(canvas.transform);
    }
}
