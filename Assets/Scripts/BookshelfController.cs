using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BookshelfController : MonoBehaviour
{
    public int Id;
 
    private void Awake()
    {
        Unit unit = GetComponent<Unit>(); 
        if (unit != null)
        {
            unit.UpdateContentsDelegate += LoadBooks;
        }
    }

    private void LoadBooks(Unit unit, bool right)
    {
        BookshelfController last = unit.GetComponent<BookshelfController>();
        if (!last) return;

        Id = right ? last.Id + 1 : last.Id - 1;
        Debug.Log("Set Id to: " + Id);
    }
}
