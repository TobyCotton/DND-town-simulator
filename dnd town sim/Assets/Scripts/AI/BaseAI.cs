using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAI : MonoBehaviour
{
    public void MoveToSquare(Vector3 newPos)
    {
        this.transform.position = newPos;
    }
}
