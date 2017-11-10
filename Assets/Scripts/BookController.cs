using System;
using UnityEngine;

public class BookController : MonoBehaviour
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

    private Book _meta;
    
    void Awake()
    {
        TitleRenderer = GetComponentInChildren<TextRenderer>();
        if (!TitleRenderer)
        {
            Debug.Log("Could not find title renderer");
        }
    }

    public void SetMeta(Book meta)
    {
        _meta = meta;
    }
    
    public void LoadData()
    {
        if (_meta == null) return;
        TitleRenderer.GenerateText(_meta.CallNumber);
    }
}