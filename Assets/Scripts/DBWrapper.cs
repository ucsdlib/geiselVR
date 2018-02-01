using System;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using Oculus.Platform.Models;
using UnityEngine;

/// <summary>
/// Wraps SQL queries and handles database connection
/// </summary>
public class DbWrapper
{
    private static DbWrapper _instance;

    private IDbConnection connection;
    private readonly string dbPath;
    private string tableName;

    private bool Connected
    {
        get { return connection.State == ConnectionState.Open; }
    }

    public DbWrapper(string assetsDbPath, string tableName)
    {
        dbPath = "URI=file:" + Application.dataPath + "/" + assetsDbPath;
        this.tableName = tableName;
        Connect();
    }

    private void Connect()
    {
        connection = new SqliteConnection(dbPath);
        connection.Open();
        if (!Connected)
        {
            Debug.LogError("Could not connect to db: " + dbPath);
        }
    }

    public void QueryCallNum(string callNum)
    {
        if (!Connected) return;
        
        // Execute query
        var query = string.Format(
            "SELECT * FROM {0} WHERE call == '{1}'", tableName, callNum);
        var command = connection.CreateCommand();
        command.CommandText = query;
        var reader = command.ExecuteReader();

        // Read results
        reader.Read();
        var entry = new DataEntry();
        entry.Read(reader);

        // output data in some way

        reader.Close();
        command.Dispose();
    }

    /// <summary>
    /// Gets a certain number of data points starting at a call number
    /// </summary>
    /// <param name="results">where to append data points to</param>
    /// <param name="startCallNum">starting call number</param>
    /// <param name="count">how many data points to load</param>
    /// <param name="forward">if true then loading is in increasing call num direction</param>
    public void QueryCount(ref List<DataEntry> results, string startCallNum, int count, bool forward)
    {
        if (!Connected) return;

        // construct query
        string op, order;
        if (forward)
        {
            op = ">";
            order = "ASC";
        }
        else
        {
            op = "<";
            order = "DESC";
        }
        var query = string.Format(
            "SELECT * FROM {0} " +
            "WHERE call {1} '{2}' " +
            "ORDER BY call {3} " +
            "LIMIT {4}",
            tableName, op, startCallNum, order, count);

        // execute query
        var command = connection.CreateCommand();
        command.CommandText = query;
        IDataReader reader;
        try
        {
            reader = command.ExecuteReader(CommandBehavior.SingleResult);
        }
        catch (Exception e)
        {
            Debug.LogError("Database excpetion: " + e.ToString());
            return;
        }

        var table = new DataTable();
        table.Load(reader);

        var id = table.Columns["id"];
        var call = table.Columns["call"];
        var title = table.Columns["title"];
        var author = table.Columns["author"];
        var width = table.Columns["width"];
        var genre = table.Columns["genre"];
        var subject = table.Columns["subject"];
        var summary = table.Columns["summary"];

        foreach (DataRow row in table.Rows)
        {
            var entry = new DataEntry
            {
                Id = row[id] as string,
                Call = row[call] as string,
                Title = row[title] as string,
                Author = row[author] as string,
                Width = Convert.ToDouble(row[width]),
                Genre = row[genre] as string,
                Subject = row[subject] as string,
                Summary = row[summary] as string
            };
            results.Add(entry);
        }

        reader.Close();
        command.Dispose();
    }

    public List<DataEntry> QueryRange(ref List<DataEntry> results, string startCallNum, string endCallNum,
        bool startInclusive, bool endInclusive)
    {
        if (!Connected) return null;

        // construct query
        var lowOp = (startInclusive) ? ">=" : ">";
        var highOp = (endInclusive) ? "<=" : "<";
        var query = string.Format(
            "SELECT * FROM {0} " +
            "WHERE call {1} '{2}' AND call {3} '{4}' " +
            "ORDER BY call",
            tableName, lowOp, startCallNum, highOp, endCallNum);

        // execute query
        var command = connection.CreateCommand();
        command.CommandText = query;
        var reader = command.ExecuteReader();

        // read results
        while (reader.Read())
        {
            var entry = new DataEntry();
            entry.Read(reader);
            results.Add(entry);
        }

        reader.Close();
        command.Dispose();
        return results;
    }
}

public class DataEntry
{
    public string Id;
    public string Call;
    public string Title;
    public string Author;
    public double Width;
    public string Genre;
    public string Subject;
    public string Summary;

    public void Read(IDataReader reader)
    {
        // TODO deprecate
        Title = reader.GetString(1);
        Width = reader.GetDouble(2);
    }
    
    public override string ToString()
    {
        return string.Format("Data: {0}|{1}|{2}", Call, Title, Width);
    }
}