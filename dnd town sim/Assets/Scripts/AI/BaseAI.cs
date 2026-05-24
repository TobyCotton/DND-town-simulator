using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BaseAI : MonoBehaviour
{
    [SerializeField] float m_speed;
    [SerializeField] GameObject m_highlight;
    private GridManager m_gridManager;
    private Vector3 m_destination;
    private bool m_currentlyMoving;

    private void Start()
    {
        m_destination = transform.position;
        m_currentlyMoving = false;
    }
    private void Update()
    {
        if (m_currentlyMoving)
        {
            float onRoad = m_gridManager.GetTileType(new Vector2(Mathf.FloorToInt(Mathf.FloorToInt(transform.position.x)/m_gridManager.m_chunksSide), Mathf.FloorToInt(Mathf.FloorToInt(transform.position.y) / m_gridManager.m_chunksSide)), new Vector2(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y))) == TileType.e_Road ? 2.0f: 1.0f ;
            transform.position = Vector3.MoveTowards(transform.position, m_destination, m_speed * onRoad * Time.deltaTime);
        }
    }
    public void SetGridManager(GridManager gridManager)
    {
        m_gridManager = gridManager;
    }
    public void SetDestination(Vector3 destination)
    {
        m_currentlyMoving = true;
        m_destination = new Vector3(destination.x,destination.y,-1);
        m_highlight.SetActive(false);
    }
    public void MoveToSquare(Vector3 newPos)
    {
        this.transform.position = newPos;
    }
    public bool isSelected()
    {
        return m_highlight.activeSelf;
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) // right click
        {
            m_highlight.SetActive(true);
        }
    }
}
