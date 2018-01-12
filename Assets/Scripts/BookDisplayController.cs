using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class BookDisplayController : MonoBehaviour
{
    [Tooltip("Number of local units per second")]
    public float ScrollSpeed;
    public Text SpineText;

    private volatile bool lerping;

    private void Update()
    {
        // scroll if text exceeds bounds
        if (!lerping && SpineText.preferredWidth > SpineText.rectTransform.rect.width)
        {
            lerping = true;
            StartCoroutine(ShiftSpine());
        }
    }
    
    private IEnumerator ShiftSpine()
    {
        var width = SpineText.rectTransform.rect.width;
        var distance = SpineText.preferredWidth - width;
        var start = SpineText.rectTransform.anchoredPosition;
        var end = SpineText.rectTransform.anchoredPosition + (distance + width / 2) * Vector2.left;
        var timeFactor = distance / ScrollSpeed;

        yield return new WaitForSeconds(1.0f); // allow user to read start
        
        // initial normal reading shift
        var t = 0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime / timeFactor;
            SpineText.rectTransform.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        // reset shift
        t = 0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime / (timeFactor / 10);
            SpineText.rectTransform.anchoredPosition = Vector2.Lerp(end, start, t);
            yield return null;
        }
        
        SpineText.rectTransform.anchoredPosition = start;
        lerping = false;
    }
}
