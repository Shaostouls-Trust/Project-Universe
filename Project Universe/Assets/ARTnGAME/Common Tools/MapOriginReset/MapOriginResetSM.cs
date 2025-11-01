using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.CommonTools
{
    //[ExecuteInEditMode]
    public class MapOriginResetSM : MonoBehaviour
    {
        public Transform MapToMove;
        public float moveAfterDist = 1000;
        public Transform player;    
        public Material shadowsMat;
        public bool operateOnlyXZ = true;// do the move only in X,Z            

        Vector3 initPlayerPos;

        // Start is called before the first frame update
        void Start()
        {
            if (shadowsMat != null && shadowsMat.HasProperty("cameraWSOffset"))
            {               
                shadowsMat.SetVector("cameraWSOffset", Vector3.zero);
            }
            if (initPlayerPos != null)
            {
                initPlayerPos = player.transform.position;
            }
        }

        //v0.1
        Vector3 planetShift = Vector3.zero;

        // Update is called once per frame
        void Update()
        {            
            float dist = Vector3.Distance(player.transform.position, initPlayerPos);
            if (operateOnlyXZ)
            {
                dist = Vector2.Distance(
                    new Vector2(player.transform.position.x, player.transform.position.z),
                    new Vector2(initPlayerPos.x, initPlayerPos.z));
            }

            if (shadowsMat != null && shadowsMat.HasProperty("cameraWSOffset"))
            {
                if (Application.isPlaying)
                {                   
                    shadowsMat.SetVector("cameraWSOffset", planetShift);
                }
                else
                {
                    shadowsMat.SetVector("cameraWSOffset", Vector3.zero);
                }
            }

            if (MapToMove != null && dist > moveAfterDist && Application.isPlaying)
            {                
                //player.transform.position = Vector3.zero;
                if (operateOnlyXZ)
                {
                    MapToMove.transform.position -= new Vector3(player.transform.position.x, 0, player.transform.position.z);
                }
                else
                {
                    MapToMove.transform.position -= player.transform.position;
                }

                //Now reset player to zero
                if (operateOnlyXZ)
                {
                    player.transform.position = new Vector3(0, player.transform.position.y, 0);
                }
                else
                {
                    player.transform.position = Vector3.zero;
                }

                //v0.1
                initPlayerPos = player.transform.position;
            }
        }
    }
}