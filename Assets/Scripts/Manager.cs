using System;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }

    [Header("Database")]
    public string DataBasePath;
    public string TableName;
    public int BufferSize;
    
    [HideInInspector]
    public static DbWrapper DbWrapper { get { return Instance.dbWrapper; }}

    private DbWrapper dbWrapper;
    
    private void Awake()
    {
        Instance = this;
        dbWrapper = new DbWrapper(DataBasePath, TableName);
    }
}