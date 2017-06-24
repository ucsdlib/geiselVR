using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class DBWrapper : MonoBehaviour
{
    private IDbConnection _connection;

    private const string TableName = "testing";

    private bool Connected
    {
        get { return _connection.State == ConnectionState.Open; }
    }

    private void Awake()
    {
        Connect();
        QueryCallNum("VK18 .G5");
    }

    private void Connect()
    {
        var dbUri = "URI=file:" + Application.dataPath + "/call-only.db";
        _connection = new SqliteConnection(dbUri);
        _connection.Open();
    }

    private void QueryCallNum(string callNum)
    {
        if (!Connected) Connect();

        // Execute query
        var query = string.Format(
            "SELECT * FROM {0} WHERE call == '{1}'", TableName, callNum);
        var command = _connection.CreateCommand();
        command.CommandText = query;
        var reader = command.ExecuteReader();

        // Read results
        reader.Read();
        var call = reader.GetString(0);
        var title = reader.GetString(1);
        var width = reader.GetDouble(2);

        Debug.Log("NEW VALUES: " + call + " " + title + " " + width); // DEBUG

        reader.Close();
        command.Dispose();
    }
}

public class DataEntry
{
    public string CallNum;
    public string Title;
    public double Width;
}
