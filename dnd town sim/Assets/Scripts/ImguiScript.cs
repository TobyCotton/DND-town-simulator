using UnityEngine;

public class ImguiScript : MonoBehaviour
{
    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 100, 90), "debug window");

        if (GUI.Button(new Rect(20, 40, 80, 20), "debug print"))
        {
            Debug.LogWarning("It works");
        }
    }
}
