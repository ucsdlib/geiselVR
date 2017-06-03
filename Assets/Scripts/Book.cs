using System;
using UnityEngine;

public class Book : MonoBehaviour
{
    
    public TextRenderer TitleRenderer;

    public int CallNumber
    {
        get { return _callNumber; }
    }
    
    public float Width
    {
        get { return _width; }
    }

    private int _callNumber;
    private float _width;
    private bool _loaded;
    
    void Awake()
    {
        TitleRenderer = transform
            .Find("BookWithTitle").Find("Title").GetComponent<TextRenderer>();
        if (!TitleRenderer)
        {
            Debug.Log("Could not find title renderer"); // DEBUG
        }
    }

    public void LoadMeta(int callNumber)
    {
        _loaded = true;
        _callNumber = callNumber;
        _width = 0.1f;
    }
    
    public void LoadData()
    {
        if (!_loaded) return;
        TitleRenderer.GenerateText(_callNumber.ToString());
    }
}