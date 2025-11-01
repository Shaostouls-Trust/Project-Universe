using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Artngame.SKYMASTER;

namespace Artngame.Orion
{
    public class shooterNebulaControlHDRP : MonoBehaviour
    {

        public connectSuntoNebulaCloudsHDRP nebulaSRP;

        public bool fireBalstClouds = false;
        public Light fireBlastLight;

        public bool affectVortexPos = false;
        public Transform vortexPos;
        public float vortexPosMultiplier = 1;
        public float invertX = 1;
        public float invertZ = 1;

        public bool openOnLeftMouse = false;
        public bool closeOnRightMouse = false;

        public bool openVortex = false;
        public float openVortexSpeed = 1;
        public bool affectVortexDepth = false;

        public float minVortexRadius = 2000;
        public float maxVortexRadius = 15000;

        public float maxVortexDepth = 165500;
        public float minVortexDepth = 265500;

        public bool closeVortex = false;
        public float closeVortexSpeed = 2;

        // Start is called before the first frame update
        void Start()
        {
            if(nebulaSRP.vortexPosRadius.w > 0)
            {
                nebulaSRP.vortexPosRadius.w = 0;
                if (affectVortexDepth)
                {
                    nebulaSRP.thickness = minVortexDepth;
                }                
            }
        }
        
        // Update is called once per frame
        void Update()
        {

            if (Input.GetMouseButton(0) && fireBalstClouds && fireBlastLight != null)
            {
                nebulaSRP.localLight = fireBlastLight;                
            }

            if (openOnLeftMouse)
            {
                if (Input.GetMouseButton(0) && !openVortex)
                {
                    openVortex = true;
                    //closeVortex = false;

                    if (fireBalstClouds && fireBlastLight != null)
                    {
                        nebulaSRP.localLight = fireBlastLight;
                    }
                }
                //else if (Input.GetMouseButton(0) && openVortex)
                //{
                //    openVortex = false;
                //}

                

            }
            if (closeOnRightMouse)
            {
                if (Input.GetMouseButton(1) && (!closeVortex || openVortex))
                {
                    closeVortex = true;
                    openVortex = false;
                }
                //else if (Input.GetMouseButton(1) && closeVortex)
                //{
                //    closeVortex = false;
                //}
            }

            if (openVortex)
            {
                if(nebulaSRP.vortexPosRadius.w < maxVortexRadius)
                {
                    nebulaSRP.vortexPosRadius.w = Mathf.Lerp(nebulaSRP.vortexPosRadius.w, maxVortexRadius, openVortexSpeed * Time.deltaTime);
                }
                if (affectVortexDepth)
                {
                    if (nebulaSRP.thickness > maxVortexDepth)
                    {
                        nebulaSRP.thickness = Mathf.Lerp(nebulaSRP.thickness, maxVortexDepth, openVortexSpeed * Time.deltaTime);
                    }
                }
            }

            if (affectVortexPos && vortexPos != null)
            {
                nebulaSRP.vortexPosRadius = new Vector4(vortexPos.position.x * vortexPosMultiplier*invertX, nebulaSRP.vortexPosRadius.y,
                    vortexPos.position.z * vortexPosMultiplier* invertZ, nebulaSRP.vortexPosRadius.w);
            }

            if (closeVortex)
            {
                if (nebulaSRP.vortexPosRadius.w > minVortexRadius)
                {
                    nebulaSRP.vortexPosRadius.w = Mathf.Lerp(nebulaSRP.vortexPosRadius.w, minVortexRadius, closeVortexSpeed * Time.deltaTime);
                }
                if (affectVortexDepth)
                {
                    if (nebulaSRP.thickness < minVortexDepth)
                    {
                        nebulaSRP.thickness = Mathf.Lerp(nebulaSRP.thickness, minVortexDepth, closeVortexSpeed * Time.deltaTime);
                    }
                }
            }

        }
    }
}
