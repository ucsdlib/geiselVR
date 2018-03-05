using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ConvertIfStatementToSwitchStatement

/// <summary>
/// Controls a physical bookshelf. It is the view for a <see cref="Bookshelf"/> model. Designed to
/// be used with a pool.
/// </summary>
public class BookshelfController : MonoBehaviour
{
    public Bookshelf Data { get; private set; }

    [Tooltip("Starting call number")]
    public string CallNumber;

    [Tooltip("Number of shelves per bookshelf")]
    public int ShelfCount = 3;
    
    [Tooltip("Height of shelf")]
    public float ShelfHeight = 0.37f;
    
    [Tooltip("Width of shelf")]
    public float ShelfWidth = 1.0f;
    
    [Tooltip("Y coordinate of the top shelf")]
    public float TopShelfY = 1.6f;
    
    [Tooltip("Moves location of all shelves by given amount")]
    public Vector3 Offset = Vector3.zero;
    
    [Tooltip("Enables guides showing where shelves would be with current parameters")]
    public bool ShowGuides;
    
    [Tooltip("Display where to show starting call number")]
    public Text Display;

    private Unit unit; // communication to unit
    private ObjectPool<BookController> bookPool; // pool of BookControllers to draw from
    private readonly List<BookController> books = new List<BookController>(); // all books contained

    /// <summary>
    /// Stop managing the given <see cref="BookController"/> object. This operation prevents the
    /// object from being deleted when this <see cref="BookshelfController"/> is disabled /
    /// destroyed, and sets its parents to null if found.
    /// </summary>
    /// <param name="bookController">Object to stop managing</param>
    /// <returns>true if <paramref name="bookController"/> was found and removed.
    /// false otherwise.</returns>
    public bool ReleaseBook(BookController bookController)
    {
        if (!books.Remove(bookController)) return false;
        bookController.transform.parent = null;
        return true;
    }

    private void Awake()
    {
        unit = GetComponent<Unit>();
        if (unit != null)
        {
            unit.UpdateContents += HandleUpdateEvent;
            unit.LoadContents += HandleLoadEvent;
            unit.DoneLoading = false;
        }

        Data = new Bookshelf(CallNumber, CallNumber, ShelfCount, ShelfWidth);
        bookPool = Manager.BookPool;
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

    private void OnDisable()
    {
        Clear();
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

    // Helper function for InstantiateTable()
    private void InstantiateShelf(LinkedList<Book> shelf, Vector3 start)
    {
        var shelfGameObj = new GameObject("Shelf"); // FIXME excessive shelves created
        shelfGameObj.transform.parent = transform;
        shelfGameObj.transform.localPosition = start;
        shelfGameObj.transform.rotation = transform.rotation;
        shelfGameObj.transform.Translate(Offset);

        // Instantiate in order
        var current = Vector3.zero;
        foreach (var meta in shelf)
        {
            var book = bookPool.Borrow();
            book.SetMeta(meta);
            book.ParentBookshelf = this;

            // move to location and retain offset rotation and position
            book.transform.parent = shelfGameObj.transform;
            book.transform.localPosition = current + book.transform.position;
            book.transform.rotation = shelfGameObj.transform.rotation * book.transform.rotation;
            current += book.Width * Vector3.right;

            book.LoadData();
            books.Add(book);
        }
    }

    // Remove all books from bookshelf
    private void Clear()
    {
        foreach (var book in books)
        {
            bookPool.GiveBack(book);
        }

        books.Clear();
        Display.text = "";
    }
    
    private void Clear2()
    {
    }
}