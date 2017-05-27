using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Book : MonoBehaviour
{
    
    private int _callNumber;
    
    public void LoadData(int callNumber)
    {
        _callNumber = callNumber;
        // TODO render text
    }
}
