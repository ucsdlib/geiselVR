using System.Collections;
using UnityEngine;

/// <summary>
/// Component required for a <see cref="RowController"/> object to manage this gameobject
/// </summary>
public class Unit : MonoBehaviour
{
    /// <summary>
    /// <see cref="RowController"/> object managing this Unit
    /// </summary>
    public RowController Row { get; set; }

    /// <summary>
    /// True when units has completed any loading operations
    /// </summary>
    public bool DoneLoading { get; set; }

    /// <summary>
    /// Delegate to tell this Unit to update its contents in the given direction
    /// </summary>
    /// <param name="unit">Unit used as reference for update</param>
    /// <param name="direction">Direction to load to from reference unit</param>
    public delegate IEnumerator UpdateContentsDelegate(Unit unit, Direction direction);

    /// <summary>
    /// Delegate to tell this Unit to load data from an <see cref="IUnit"/> object
    /// </summary>
    /// <param name="unit"></param>
    public delegate IEnumerator LoadContentsDelegate(IUnit unit);

    /// <summary>
    /// <see cref="UpdateContentsDelegate"/> target.
    /// </summary>
    public UpdateContentsDelegate UpdateContents;
    
    /// <summary>
    /// <see cref="LoadContentsDelegate"/> target.
    /// </summary>
    public LoadContentsDelegate LoadContents;
}