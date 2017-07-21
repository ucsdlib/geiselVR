using System;
using UnityEngine;

public class Book : MonoBehaviour
{
    
    public TextRenderer TitleRenderer;

    public string CallNumber
    {
        get { return _callNumber; }
    }
    
    public float Width
    {
        get { return _width; }
    }

    public static float ConvertWidthToLocalScale(double width)
    {
        return (float) width / 1000 + 0.01f; // convert cm -> m
    }

    private string _callNumber;
    private float _width;
    private string _title;
    private bool _loaded;
    
    void Awake()
    {
        TitleRenderer = transform
            .Find("BookWithTitle").Find("Title").GetComponent<TextRenderer>();
        if (!TitleRenderer)
        {
            Debug.Log("Could not find title renderer");
        }
    }

    public void LoadMeta(DataEntry entry)
    {
        _loaded = true;
        _callNumber = entry.CallNum;
        _title = entry.Title;
        _width = ConvertWidthToLocalScale(entry.Width);
    }
    
    public void LoadData()
    {
        if (!_loaded) return;
        TitleRenderer.GenerateText(_callNumber);
    }
}