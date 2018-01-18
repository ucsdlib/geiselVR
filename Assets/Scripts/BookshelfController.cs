using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ConvertIfStatementToSwitchStatement

public class BookshelfController : MonoBehaviour
{
    public string CallNumber;
    public BookController BookTemplate;
    public float MaxBookSize = 500f;
    public int ShelfCount = 3;
    public float ShelfHeight = 0.37f;
    public float ShelfWidth = 1.0f;
    public float TopShelfY = 1.6f;
    public Vector3 Offset = Vector3.zero;
    public bool ShowGuides;
    public Text Display;

    private Unit unit;
    private readonly List<BookController> books = new List<BookController>();
    public Bookshelf Data { get; private set; }

    private void Awake()
    {
        // Establish connection to Unit
        unit = GetComponent<Unit>();
        if (unit != null)
        {
            unit.UpdateContents += HandleUpdateEvent;
            unit.LoadContents += HandleLoadEvent;
            unit.DoneLoading = false;
        }
        Data = new Bookshelf(CallNumber, CallNumber, ShelfCount, ShelfWidth);
    }

    private void OnDrawGizmos()
    {
        if (!ShowGuides) return;
        Gizmos.color = Color.green;
        for (var i = 0; i < ShelfCount; i++)
        {
            var center = Vector3.zero;
            center.x = ShelfWidth / 2;
            center.y = TopShelfY - i * ShelfHeight;
            center.z = -0.14f;
            center += Offset;

            var size = new Vector3(ShelfWidth, 0, 0.28f);

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(center, size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }

    private IEnumerator HandleUpdateEvent(Unit lastUnit, Direction direction)
    {
        unit.DoneLoading = false; // TODO consider letting this be accessed by row controller
        if (direction == Direction.Null)
        {
            Clear();
            unit.DoneLoading = true;
            yield break;
        }

        // get references to data
        var last = lastUnit.GetComponent<BookshelfController>();
        if (last == null)
        {
            Debug.LogError("Could not get last shelf");
            yield break;
        }
        var lastData = last.Data;
        if (lastData == null)
        {
            Debug.LogError("Last unit did not have data");
            yield break;
        }
       
        // load data
        var done = false;
        Data = new Bookshelf(lastData.Start, lastData.End, ShelfCount, ShelfWidth);
        new Thread(o =>
        {
            Data.Load(direction);
            done = true;
        }).Start();

        // instantiate gameObjects
        while (!done) yield return null;
        unit.DoneLoading = true;
        yield return InstantiateTable();
        Display.text = Data.Start;
    }

    private IEnumerator HandleLoadEvent(IUnit unit)
    {
        var shelf = unit as Bookshelf;
        if (shelf == null)
        {
            Debug.LogError("BookshelfController: IUnit not Bookshelf on LoadEvent");
            yield break;
        }

        Data = shelf;
        yield return InstantiateTable();
        Display.text = Data.Start;
    }

    private IEnumerator InstantiateTable()
    {
        var i = 0;
        foreach (var shelf in Data.Table)
        {
            var start = Vector3.up * (TopShelfY - i++ * ShelfHeight);
            InstantiateShelf(shelf, start);
            yield return null;
        }
        unit.DoneLoading = true;
    }

    private void InstantiateShelf(LinkedList<Book> shelf, Vector3 start)
    {
        var shelfGameObj = new GameObject("Shelf");
        shelfGameObj.transform.parent = transform;
        shelfGameObj.transform.localPosition = start;
        shelfGameObj.transform.rotation = transform.rotation;
        shelfGameObj.transform.Translate(Offset);

        // Instantiate in order
        var current = Vector3.zero;
        foreach (var meta in shelf)
        {
            var book = Instantiate(BookTemplate);
            book.SetMeta(meta);

            // move to location and retain offset rotation and position
            book.transform.parent = shelfGameObj.transform;
            book.transform.localPosition = current + book.transform.position; 
            book.transform.rotation = shelfGameObj.transform.rotation * book.transform.rotation;
            current += book.Width * Vector3.right;

            book.LoadData();
            books.Add(book);
        }
    }

    private void Clear()
    {
        Data = null;
        foreach (var book in books)
        {
            Destroy(book);
        }
        books.Clear();
        Display.text = "";
    }
}