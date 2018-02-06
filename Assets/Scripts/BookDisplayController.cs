using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BookDisplayController : MonoBehaviour
{
    [Tooltip("Number of local units per second")]
    public float ScrollSpeed;

    [Tooltip("How many seconds to pause for at beginning of text shift")]
    public float PauseTime;

    public bool Animate { get; set; }

    public Text SpineText;

    private volatile bool lerping;
    private Vector2 spineInitPos;

    private void Start()
    {
        spineInitPos = SpineText.rectTransform.anchoredPosition;
        Animate = false;
    }

    private void Update()
    {
        // scroll if text exceeds bounds
        if (Animate && !lerping &&
            SpineText.preferredWidth > SpineText.rectTransform.rect.width)
        {
            lerping = true;
//            SpineText.alignment = TextAnchor.MiddleLeft;
            StartCoroutine(ShiftSpine());
        }
        else
        {
//            SpineText.alignment = TextAnchor.MiddleCenter;
        }
    }

    private void OnEnable()
    {
        lerping = false; // disable may have interrupted coroutine, reset to init
        SpineText.rectTransform.anchoredPosition = spineInitPos;
    }

    private IEnumerator ShiftSpine()
    {
        var width = SpineText.rectTransform.rect.width;
        var distance = SpineText.preferredWidth - width;
        var start = SpineText.rectTransform.anchoredPosition;
        var end = SpineText.rectTransform.anchoredPosition + (distance + width / 2) * Vector2.left;
        var timeFactor = (distance + width / 2) / ScrollSpeed;

        yield return new WaitForSeconds(PauseTime);

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
            t += Time.deltaTime / (timeFactor / 8);
            SpineText.rectTransform.anchoredPosition = new Vector2(
                Mathf.SmoothStep(end.x, start.x, t), start.y);
            yield return null;
        }

        SpineText.rectTransform.anchoredPosition = start;
        lerping = false;
    }
}