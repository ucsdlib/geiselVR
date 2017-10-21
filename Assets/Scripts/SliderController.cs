using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderController : MonoBehaviour, IEndDragHandler
{
    private SelectionUI baseUi;
    
    public void Awake()
    {
        baseUi = gameObject.GetComponent<SelectionUI>();
        if (baseUi == null) Debug.Log("Slider Controller could not find base UI script");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        baseUi.OnEndDrag(eventData);
    }
}
