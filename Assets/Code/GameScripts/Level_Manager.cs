using UnityEngine;

public class Level_Manager : MonoBehaviour
{
    public static Level_Manager main;

    public Transform startPoint;
    public Transform[] path;
    private void Awake()
    {
        main = this;
    }
}
