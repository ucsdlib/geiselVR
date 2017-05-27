using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public RowController Row { get; set; }
    public int Index { get; set; }

    public delegate void UpdateContents(Unit unit, bool right);

    public UpdateContents UpdateContentsDelegate;
}