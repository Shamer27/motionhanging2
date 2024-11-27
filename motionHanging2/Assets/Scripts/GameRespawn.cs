using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRespawn : MonoBehaviour
{
   public float threshold;

   void FixedUpdate()
   {
        if(transform.position.y < threshold)
        {
             transform.position = new Vector3(0f, 2.25f, 0f);
        }
   }
}
