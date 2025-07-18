using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularPath : MonoBehaviour
{
    public Transform target; //Target Object
    public float speed = 5f; // Speed of the movement
    public float radius = 5f; //Radius of the circular path
    private float angle = 0f; // current angle of the object
    [SerializeField] public float height = 0f; // Whether the movement is active

    void Update()
    {
        float x = target.position.x + Mathf.Cos(angle) * radius;
        float z = target.position.z + Mathf.Sin(angle) * radius;

        transform.position = new Vector3(x, height, z);
        transform.LookAt(target);

        angle += speed * Time.deltaTime; // Increment the angle based on speed and time
    }

}
