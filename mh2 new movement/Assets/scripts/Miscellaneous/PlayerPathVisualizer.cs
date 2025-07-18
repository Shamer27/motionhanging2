using UnityEngine;
using System.Collections.Generic;

public class PlayerPathVisualizer : MonoBehaviour
{
    public Color pathColor = Color.cyan;
    public float pointSpacing = 0.5f;

    private List<Vector3> pathPoints = new List<Vector3>();
    private Vector3 lastRecordedPosition;

    void Start()
    {
        lastRecordedPosition = transform.position;
        pathPoints.Add(lastRecordedPosition);
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, lastRecordedPosition) > pointSpacing)
        {
            pathPoints.Add(transform.position);
            lastRecordedPosition = transform.position;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = pathColor;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
        }
    }
}
