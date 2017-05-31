using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public RowController Row { get; set; }

    public delegate void UpdateContents(Unit unit, Direction direction);

    public UpdateContents UpdateContentsDelegate;
}