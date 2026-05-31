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
    [SerializeField] private GameObject m_debugBuilding;
    [SerializeField] private float m_moveCost;
    private GridManager m_gridManager;
    private TileType m_tileType = TileType.e_None;
    private bool m_isBuildingOrigin = false;
    private BuildingData m_buildingData = null;

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
        if (Input.GetMouseButton(0) && m_tileType != TileType.e_Building)
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
        if (m_tileType != TileType.e_Building)
        {
            SetTileToRoad();
        }
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
    private void Update()
    {
        if (!isHighlighted())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            m_gridManager.TryPlaceBuilding(m_gridManager.WorldToGridChunk(transform.position),m_gridManager.WorldToGridTile(transform.position));
        }
    }

    public void SetOccupiedVisual()
    {
        m_renderer.color = Color.black;
    }
    public void OccupyAsBuilding(BuildingData data, bool isOrigin)
    {
        m_tileType = TileType.e_Building;
        m_isBuildingOrigin = isOrigin;
        m_buildingData = isOrigin ? data : null;
        m_debugBuilding.SetActive(isOrigin);
        SetOccupiedVisual();
    }

    public bool IsBuildingOrigin() => m_isBuildingOrigin;
    public BuildingData GetBuildingData() => m_buildingData;

    public TileSaveData GetSaveData(Vector2 chunkPos, Vector2 tilePos)
    {
        return new TileSaveData
        {
            m_chunkX = chunkPos.x,
            m_chunkY = chunkPos.y,
            m_tileX = tilePos.x,
            m_tileY = tilePos.y,
            m_tileType = m_tileType,
            m_isBuildingOrigin = m_isBuildingOrigin,
            m_buildingWidth = m_buildingData?.m_width ?? 3,
            m_buildingHeight = m_buildingData?.m_height ?? 3
        };
    }

    public void LoadTileType(TileType type)
    {
        m_tileType = type;
        switch (type)
        {
            case TileType.e_Road:
                {
                    m_road.SetActive(true); 
                    break;
                }
            case TileType.e_Building:
                { 
                    m_debugBuilding.SetActive(true);
                    SetOccupiedVisual();
                    break; 
                }
        }
    }
}
