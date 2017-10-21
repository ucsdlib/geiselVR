using System;
using Oculus.Platform;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionUI : MonoBehaviour
{
	private string[] alphabet = new[]
	{
		"A", "B", "C", "D", "E", "F", "G",
		"H", "I", "J", "K", "L", "M", "N",
		"O", "P", "Q", "R", "S", "T", "U",
		"V", "W", "X", "Y", "Z"
	};
	
	public Text Title;
	public Slider Slider;
	public RowController Row;

	public void OnEndDrag(PointerEventData eventData)
	{
		
	}
	
	public void OnSliderValueChanged(float value)
	{
		Title.text = GetSliderTranslation(value);
	}
	
	public void OnLoadClick()
	{
		Row.SetPosition(Title.text);
	}
	
	private string GetSliderTranslation(float value)
	{
		var index = (int) Math.Floor(value * 27);
		return alphabet[index];
	}
}
