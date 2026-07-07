using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class BaseBuilding : MonoBehaviour
{
    [SerializeField] private Image m_image;
    bool m_isPlaced = false;
    void SetPlaced()
    {
        m_isPlaced = true;
    }
    private void OnMouseDown()
    {
        m_image.gameObject.SetActive(!m_image.gameObject.activeSelf);
    }
}
