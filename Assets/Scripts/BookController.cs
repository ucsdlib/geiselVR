using UnityEngine;
using Random = System.Random;

/// <summary>
/// Controls a physical book. It is the view for a <see cref="Book"/> model. Designed to be used
/// with a pool.
/// </summary>
public class BookController : MonoBehaviour
{
    public BookDisplayController Display;
    public BookshelfController ParentBookshelf { get; set; }

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

    /// <summary>
    /// Replaces the internal data represented. Note that the object will not change visually until
    /// <see cref="LoadData()"/> is called.
    /// </summary>
    /// <param name="meta">New data to be displayed</param>
    public void SetMeta(Book meta)
    {
        Meta = meta;
    }

    /// <summary>
    /// Updates the object's appearances based on the internal data
    /// </summary>
    public void LoadData()
    {
        if (Meta == null) return;
        Display.SpineText.text = Meta.Title;
    }
}