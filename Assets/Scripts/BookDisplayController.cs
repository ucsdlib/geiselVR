using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookDisplayController : MonoBehaviour
{
    public float ScrollSpeed;
    /*
     * Assumption is made that text is anchored and positioned in
     * in its left most extreme. Text is scrolled to the left
     */
    public Text SpineText;

    private volatile bool lerping;

    private void Update()
    {
        // scroll if text exceeds bounds
        if (SpineText.preferredWidth > SpineText.rectTransform.rect.width)
        {
            
        }
    }
    
    
}
