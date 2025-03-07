using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] Transform cameraPosition;

    void Update()
    {
        Vector3 targetPosition = cameraPosition.position;
        RaycastHit hit;
        if (Physics.Raycast(cameraPosition.position, -cameraPosition.forward, out hit, 0.5f))
        {
            targetPosition = hit.point + (cameraPosition.forward * 1.5f);
        }

        transform.position = targetPosition;
    }
}