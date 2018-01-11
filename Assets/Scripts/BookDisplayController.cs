using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class BookDisplayController : MonoBehaviour
{
    public float ScrollTimeFactor;
    public Text SpineText;

    private volatile bool lerping;

    private void Update()
    {
        // scroll if text exceeds bounds
        if (!lerping && SpineText.preferredWidth > SpineText.rectTransform.rect.width)
        {
            lerping = true;
            var distance = SpineText.preferredWidth - SpineText.rectTransform.rect.width;
            StartCoroutine(ShiftSpine(ScrollTimeFactor, distance));
        }
        SpineText.rectTransform.anchoredPosition += Vector2.left * 0.1f;
    }
    
    private IEnumerator ShiftSpine(float timeFactor, float distance)
    {
        var start = SpineText.rectTransform.anchoredPosition;
        var end = SpineText.rectTransform.anchoredPosition + distance * Vector2.left;
        var t = 0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime / timeFactor;
            SpineText.rectTransform.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        // TODO make a smoother transition back to start
        SpineText.rectTransform.anchoredPosition = start;
        lerping = false;
    }
}
