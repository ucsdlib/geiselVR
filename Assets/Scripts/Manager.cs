using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}