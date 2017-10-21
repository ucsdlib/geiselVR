using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionUI : MonoBehaviour
{
	public Text Title;
	public Slider Slider;

	public void OnEndDrag(PointerEventData eventData)
	{
		// TODO
	}
	
	public void OnSliderValueChanged(float value)
	{
		Debug.Log(value);
	}
}
