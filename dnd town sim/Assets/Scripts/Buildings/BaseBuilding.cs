using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class BaseBuilding : MonoBehaviour
{
    [SerializeField] private Image m_image;
    private Camera m_camera;
    [SerializeField] private float m_zDepth = -2f;
    bool m_isPlaced = true;
    private Collider2D m_collider;

    private void Awake()
    {
        m_camera = Camera.main;
        m_collider = GetComponent<Collider2D>();
    }

    public void SetPlaced(bool placed)
    {
        m_isPlaced = placed;
        if (m_collider != null)
        {
            m_collider.enabled = placed;
        }
    }

    private void OnMouseDown()
    {
        m_image.gameObject.SetActive(!m_image.gameObject.activeSelf);
    }

    private void Update()
    {
        if (m_isPlaced == false)
        {
            Vector3 mouseWorld = m_camera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(Mathf.Round(mouseWorld.x), Mathf.Round(mouseWorld.y), m_zDepth);
        }
    }
}
