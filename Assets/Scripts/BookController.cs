using UnityEngine;

public class BookController : MonoBehaviour
{

    public BookDisplayController Display;

    public float Width
    {
        get { return Meta.Width; }
    }

	public Book Meta { get; set; }

    public void SetMeta(Book meta)
    {
        Meta = meta;
    }
    
    public void LoadData()
    {
        if (Meta == null) return;
        Display.SpineText.text = Meta.Title;
    }
}