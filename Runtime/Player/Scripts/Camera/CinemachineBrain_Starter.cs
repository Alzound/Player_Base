using Unity.Cinemachine;
using UnityEngine;

namespace LvlPlayer
{
    public class CinemachineBrain_Starter : MonoBehaviour
    {
        private void Awake()
        {
            Camera cam = Camera.main;

            if(cam == null)
            {
                return; 
            }

            CinemachineBrain brain = cam.GetComponent<CinemachineBrain>();

            if(brain == null)
            {
                cam.gameObject.AddComponent<CinemachineBrain>();
            }
        }
    }
}
