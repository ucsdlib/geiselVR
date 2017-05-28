using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Book : MonoBehaviour
{
    public TextRenderer TitleRenderer;
    
    private int _callNumber;

    private void Awake()
    {
        TitleRenderer = transform.Find("Title").GetComponent<TextRenderer>();
        if (!TitleRenderer)
        {
            Debug.Log("Could not find title renderer"); // DEBUG
        }
    }

    public void LoadData(int callNumber)
    {
        Debug.Log("Loading data for: " + callNumber); // DEBUG
        _callNumber = callNumber;
        TitleRenderer.GenerateText("HELLO " + _callNumber);
    }
}
