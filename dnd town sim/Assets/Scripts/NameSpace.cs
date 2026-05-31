using UnityEngine;

public enum TileType
{
    e_Road,
    e_Building,
    e_BeatenPath,
    e_None,
}

[System.Serializable]
public class BuildingData
{
    public Vector2 m_originChunk;
    public Vector2 m_originTile;
    public int m_width = 3;
    public int m_height = 3;
}
