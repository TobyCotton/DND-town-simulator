using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color m_baseColour, m_offsetColour;
    [SerializeField] private SpriteRenderer m_renderer;
    [SerializeField] private GameObject m_highlight;
    [SerializeField] private GameObject m_road;
    [SerializeField] private float m_moveCost;
    private TileType m_tileType = TileType.e_None;

    public void Init(bool isOffset)
    {
        m_renderer.color = isOffset ? m_baseColour : m_offsetColour;
    }

    void OnMouseEnter()
    {
        if (Input.GetMouseButton(0))
        {
            SetTileToRoad();
        }
        m_highlight.SetActive(true);
    }
    void OnMouseExit()
    {
        m_highlight.SetActive(false);
    }

    private void OnMouseDown()
    {
        SetTileToRoad();
    }

    private void SetTileToRoad()
    {
        m_road.SetActive(true);
        m_tileType = TileType.e_Road;
    }

    public bool isHighlighted()
    {
        return m_highlight.activeSelf;
    }
    public TileType ThisTylesType()
    {
        return m_tileType;
    }
}
