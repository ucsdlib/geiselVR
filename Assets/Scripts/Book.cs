using UnityEngine;

public class Book : MonoBehaviour
{
    public TextRenderer TitleRenderer;

    public int CallNumber
    {
        get { return _callNumber; }
    }
    public float Width
    {
        get { return _width; }
    }

    private int _callNumber;
    private float _width;

    private void Awake()
    {
        TitleRenderer = transform.Find("Title").GetComponent<TextRenderer>();
        if (!TitleRenderer)
        {
            Debug.Log("Could not find title renderer"); // DEBUG
        }
    }

    public void LoadData(int callNumber)
    {
        Debug.Log("Loading data for: " + callNumber); // DEBUG
        _callNumber = callNumber;
        _width = 0.1f;
        TitleRenderer.GenerateText(_callNumber.ToString());
    }
}