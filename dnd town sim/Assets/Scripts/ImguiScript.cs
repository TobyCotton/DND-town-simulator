using UnityEngine;

public class ImguiScript : MonoBehaviour
{
    [SerializeField] GridManager m_gridManager;
    GameObject m_grid = null;
    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 200, 90), "debug window");

        if (GUI.Button(new Rect(30, 40, 160, 20), "Spawn Building"))
        {
            m_gridManager.m_nextClickSpawnBuilding = true;
        }

        if (GUI.Button(new Rect(30, 60, 160, 20), "Toggle Floor"))
        {
            if(m_grid == null)
            {
                m_grid = GameObject.Find("Grid");
            }

            m_grid.SetActive(!m_grid.activeSelf);
        }
    }
}
