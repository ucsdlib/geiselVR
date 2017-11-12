using UnityEngine;

public class UnitFactory
{
    private BookshelfController template;

    public UnitFactory(GameObject templateUnit)
    {
        template = templateUnit.GetComponent<BookshelfController>();
        Debug.Log("UnitFactory: Could not find BookshelfController script on template");
    }
    
    public IUnit BlankIUnit()
    {
        return new Bookshelf("", "", template.ShelfCount, template.ShelfWidth);
    }
}