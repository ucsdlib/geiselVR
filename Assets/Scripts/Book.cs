using System;
using UnityEngine;

public class Book : MonoBehaviour
{
    
    public TextRenderer TitleRenderer;

    public string CallNumber
    {
        get { return _meta.CallNumber; }
    }
    
    public float Width
    {
        get { return _meta.Width; }
    }

    private MetaBook _meta;
    
    void Awake()
    {
        TitleRenderer = transform
            .Find("BookWithTitle").Find("Title").GetComponent<TextRenderer>();
        if (!TitleRenderer)
        {
            Debug.Log("Could not find title renderer");
        }
    }

    public void SetMeta(MetaBook meta)
    {
        _meta = meta;
    }
    
    public void LoadData()
    {
        if (_meta == null) return;
//        TitleRenderer.GenerateText(_meta.CallNumber);
    }
}