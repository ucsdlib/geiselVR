using System.ComponentModel;
using UnityEngine;

public class UnitFactory : MonoBehaviour
{
    public BookshelfController TemplateShelf;

    private BookshelfController bookshelf;
    private Unit unit;

    private void Start()
    {
//        bookshelf = Instantiate(TemplateShelf, gameObject.transform);
//        bookshelf.transform.position += Vector3.down * 100;
//        unit = bookshelf.GetComponent<Unit>();
//        if (unit == null)
//        {
//            Debug.LogError("UnitFactory could not find Unit component on TemplateShelf");
//        }
    }

    public Unit UnitStartCallNum(string start)
    {
//        bookshelf.SetStartEndCallNum(start, "");
        return unit;
    }
}