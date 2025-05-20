using UnityEngine;

public class FreezePosition : MonoBehaviour
{
    public Transform movingObject; // Drag the moving object here in the inspector

    void Update()
    {
        if (movingObject != null)
        {
            // Set the frozen object's position to be the same as the moving object's position
            transform.position = movingObject.position;
        }
    }
}