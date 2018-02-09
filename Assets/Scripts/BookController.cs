using UnityEngine;
using Random = System.Random;

public class BookController : MonoBehaviour
{
    public BookDisplayController Display;

    public float Width
    {
        get { return Meta.Width; }
    }

	public Book Meta { get; set; }
    
    private static Random random = new Random();

    private void Awake()
    {
        // set random material
        var materials = Manager.Instance.BookMaterials;
        if (materials.Count == 0) return;
        var index = random.Next(materials.Count);
        gameObject.GetComponent<Renderer>().material = materials[index];
    }

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