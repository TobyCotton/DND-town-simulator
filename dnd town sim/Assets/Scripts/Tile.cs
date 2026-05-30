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
    private GridManager m_gridManager;
    private TileType m_tileType = TileType.e_None;

    //Higher the number the slower the tile is
    public const float COST_ROAD = 0.5f;
    public const float COST_BEATEN = 0.75f;
    public const float COST_DEFAULT = 1.0f;
    public const float COST_IMPASSABLE = float.MaxValue;

    public void Init(bool isOffset,GridManager gridManager)
    {
        m_renderer.color = isOffset ? m_baseColour : m_offsetColour;
        m_gridManager = gridManager;
    }

    void OnMouseEnter()
    {
        m_gridManager.SetHighlightedTile(this.transform.position);
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
    public bool IsWalkable()
    {
        return m_tileType != TileType.e_Building;
    }
    public float GetMoveCost()
    {
        float cost = COST_DEFAULT;
        switch (m_tileType)
        {
            case TileType.e_Road:
                cost = COST_ROAD;
                break;
            case TileType.e_Building:
                cost = COST_IMPASSABLE;
                break;
            case TileType.e_BeatenPath:
                cost = COST_BEATEN;
                break;
        }
        return cost;
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
