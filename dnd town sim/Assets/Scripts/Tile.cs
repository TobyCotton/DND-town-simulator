using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color m_baseColour, m_offsetColour;
    [SerializeField] private SpriteRenderer m_renderer;
    [SerializeField] private GameObject m_highlight;

    public void Init(bool isOffset)
    {
        m_renderer.color = isOffset ? m_baseColour : m_offsetColour;
    }

    void OnMouseEnter()
    {
        m_highlight.SetActive(true);
    }
    void OnMouseExit()
    {
        m_highlight.SetActive(false);
    }
}
