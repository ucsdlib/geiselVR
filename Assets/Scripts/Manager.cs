using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global state manager
/// </summary>
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
    [Tooltip("Path to database file relative to Assets folder")]
    public string DataBasePath;

    [Tooltip("Name of desired table in database")]
    public string TableName;

    [Tooltip("Default size of database buffers")]
    public int BufferSize;

    [Tooltip("Default size of pools unless otherwise specified")]
    public int DefaultPoolSize;

    [Header("Instantiators")]
    public Instantiator BookInstantiator;

    public Instantiator UnitInstantiator;

    [Header("Materials")]
    [Tooltip("List from which to randomly color physical books")]
    public List<Material> BookMaterials;

    public static DbWrapper DbWrapper { get; private set; }
    public static UnitFactory UnitFactory { get; private set; }
    public static ObjectPool<BookController> BookPool { get; private set; }
    public static ObjectPool<Unit> UnitPool { get; private set; }

    private void Awake()
    {
        if (BookInstantiator == null)
        {
            Debug.LogError("Manager: missing Book Instantiator reference");
        }

        instance = this;
        DbWrapper = new DbWrapper(DataBasePath, TableName);
        UnitFactory = new UnitFactory(UnitInstantiator.Template);
        BookPool = new ObjectPool<BookController>(BookInstantiator, DefaultPoolSize);
        UnitPool = new ObjectPool<Unit>(UnitInstantiator, 5);
    }
}