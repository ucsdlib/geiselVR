using UnityEngine;

public class Unit : MonoBehaviour
{
    public RowController Row { get; set; }

    public delegate void UpdateContents(Unit unit, Direction direction);

    public UpdateContents UpdateContentsDelegate;
}