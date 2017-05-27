using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookshelfController : MonoBehaviour {
    
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
        Debug.Log("Load Books called"); // DEBUG
    }
}
