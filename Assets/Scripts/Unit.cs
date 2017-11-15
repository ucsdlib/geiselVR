using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public RowController Row { get; set; }

    public bool DoneLoading { get; set; }

    public delegate IEnumerator UpdateContentsDelegate(Unit unit, Direction direction);

    public delegate IEnumerator LoadContentsDelegate(IUnit unit);

    public UpdateContentsDelegate UpdateContents;
    public LoadContentsDelegate LoadContents;
}