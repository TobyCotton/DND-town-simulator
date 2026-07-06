using UnityEngine;

public class ImguiScript : MonoBehaviour
{
    [SerializeField] GridManager m_gridManager;
    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 200, 90), "debug window");

        if (GUI.Button(new Rect(30, 40, 160, 20), "Spawn Building"))
        {
            m_gridManager.m_nextClickSpawnBuilding = true;
        }
    }
}
