using UnityEngine;

public class BookSpawn : MonoBehaviour
{
    [SerializeField] private Transform _book;

    private void Start()
    {
        Debug.Log("Started BookSpawn Script");
    }

    private void Update()
    {
        // Check if the user pressed the button
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            Debug.Log("Pressed 'A' Button");
            Instantiate(_book, gameObject.transform.position,
                gameObject.transform.rotation);
        }
    }
}