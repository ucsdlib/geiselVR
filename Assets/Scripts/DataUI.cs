using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataUI : MonoBehaviour
{
    public Text Text;

    public void SetData(Book book)
    {
        var res = "";
        res += "Title: " + book.Title + "\n";
        res += "Author: " + book.Author + "\n";
        res += "Genre: " + book.Genre + "\n";
        res += "Subject: " + book.Subject + "\n";
        res += "Summary: " + book.Summary + "\n";
        res += "Call Number: " + book.CallNumber + "\n";
        res += string.Format("Width: {0:N2}\n", book.Width);
        Text.text = res;
    }

    public void Clear()
    {
        Text.text = "";
    }
}