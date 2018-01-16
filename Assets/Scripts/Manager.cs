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

    [Header("Units")] 
    public GameObject TemplateUnit;

    public static DbWrapper DbWrapper { get; private set; }
    public static UnitFactory UnitFactory { get; private set; }

    private void Awake()
    {
        instance = this;
        DbWrapper = new DbWrapper(DataBasePath, TableName);
        UnitFactory = new UnitFactory(TemplateUnit);
    }
}