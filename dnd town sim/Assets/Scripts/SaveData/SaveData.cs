using System;
using System.Collections.Generic;

[Serializable]
public class TileSaveData
{
    public float m_chunkX, m_chunkY;
    public float m_tileX, m_tileY;
    public TileType m_tileType;
    public bool m_isBuildingOrigin;
    public int m_buildingWidth = 3;
    public int m_buildingHeight = 3;
}

[Serializable]
public class MapSaveData
{
    public List<TileSaveData> m_tiles = new List<TileSaveData>();
}
