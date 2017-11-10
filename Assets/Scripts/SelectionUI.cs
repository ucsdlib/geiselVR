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
    public UnitFactory Factory;

    public void OnSliderValueChanged(float value)
    {
        Title.text = GetSliderTranslation(value);
    }

    public void OnLoadClick()
    {
        var unit = Factory.UnitStartCallNum(Title.text);
        Row.SetPosition(unit);
    }

    private string GetSliderTranslation(float value)
    {
        var index = (int) Math.Floor(value * 27);
        return alphabet[index];
    }

    // DEBUG
    private void Update()
    {
        if (OVRInput.Get(OVRInput.Button.Two))
        {
            var unit = Factory.UnitStartCallNum("R");
            Row.SetPosition(unit);
        }
    }
}