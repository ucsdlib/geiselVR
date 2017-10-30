using System.ComponentModel;
using UnityEngine;

public class UnitFactory : MonoBehaviour
{
    public BookshelfController TemplateShelf;

    public Unit UnitStartCallNum(string start)
    {
        var bookshelf = Instantiate(TemplateShelf, gameObject.transform);
        bookshelf.transform.position += Vector3.down * 100; // move from visible space
        bookshelf.SetStartEndCallNum(start, "");
        var unit = bookshelf.GetComponent<Unit>();
        if (unit == null)
        {
            Debug.LogError("UnitFactory could not find Unit component on TemplateShelf");
            return null;
        }
        return unit;
    }
}