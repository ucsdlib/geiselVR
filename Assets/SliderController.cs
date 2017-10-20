using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderController : MonoBehaviour, IEndDragHandler {
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Ending Drag");
    }
}
