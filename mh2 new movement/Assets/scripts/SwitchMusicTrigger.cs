// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class SwitchMusicTrigger : MonoBehaviour
// {

//     public AudioClip newTrack;

//     private AudioManager AM;
//     // Start is called before the first frame update
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }

//     void OnTriggerEnter(Collider other) {
//         if (other.tag == "Player")
//         {
//             if (newTrack != null)
//             {
//                 AM = FindObjectOfType<AudioManager>();
//                 AM.ChangeBGM(newTrack);
//             }
//         }
//     }
// }

