using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }

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
        Instance = this;
        DbWrapper = new DbWrapper(DataBasePath, TableName);
        UnitFactory = new UnitFactory(TemplateUnit);
    }
}