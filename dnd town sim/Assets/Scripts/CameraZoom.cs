using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] float m_zoomSpeed, m_minFOV, m_maxFOV, m_camspeed;
    Camera m_cam;

    private void Awake()
    {
        m_cam = GetComponent<Camera>();
    }
    // Update is called once per frame
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0.0f)
        {
            m_cam.orthographicSize = Mathf.Clamp(m_cam.orthographicSize - scroll * m_zoomSpeed,m_minFOV, m_maxFOV);
        }
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        transform.position += new Vector3(horizontal, vertical, 0) * m_camspeed * Time.deltaTime;
    }
}
