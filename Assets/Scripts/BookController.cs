using UnityEngine;

public class BookController : MonoBehaviour
{
    
    public TextRenderer TitleRenderer;

    public string CallNumber
    {
        get { return meta.CallNumber; }
    }
    
    public float Width
    {
        get { return meta.Width; }
    }

	public Book Meta 
	{
		get { return meta; }
	}

    private Book meta;
    
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
        this.meta = meta;
    }
    
    public void LoadData()
    {
        if (meta == null) return;
        TitleRenderer.GenerateText(meta.Title);
    }
}