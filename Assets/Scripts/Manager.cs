using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance
    {
        get
        {
            if (instance == null)
                Debug.LogError("Got null Manager instance");
            return instance;
        }
    }

    private static Manager instance;

    [Header("Database")] 
    public string DataBasePath;
    public string TableName;
    public int BufferSize;
    public int DefaultPoolSize;

    [Header("Templates and Instantiators")] 
    public GameObject TemplateUnit;
    public Instantiator BookInstantiator;
    public Instantiator BookshelfInstantiator;

    public static DbWrapper DbWrapper { get; private set; }
    public static UnitFactory UnitFactory { get; private set; }
    public static ObjectPool<BookController> BookPool { get; private set; }
    public static ObjectPool<BookshelfController> BookshelfPool { get; private set; }

    private void Awake()
    {
        if (BookInstantiator == null)
        {
            Debug.LogError("Manager: missing Book Instantiator reference");
        }
        
        instance = this;
        DbWrapper = new DbWrapper(DataBasePath, TableName);
        UnitFactory = new UnitFactory(TemplateUnit);
        BookPool = new ObjectPool<BookController>(BookInstantiator, DefaultPoolSize);
        BookshelfPool = new ObjectPool<BookshelfController>(BookshelfInstantiator, 5);
    }
}