using ProjectUniverse.Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Environment.Volumes
{
    public class VolumeConstructionController : MonoBehaviour
    {
        [SerializeField] private VolumeConstructionSection[] sectionCeiling;
        [SerializeField] private VolumeConstructionSection[] sectionFloor;
        [SerializeField] private VolumeConstructionSection[] sectionWalls;
        [SerializeField] private VolumeConstructionSection[] sectionDoors;
        [SerializeField] private VolumeConstructionSection[] sectionBreakers;
        [SerializeField] private VolumeConstructionSection[] sectionLights;
        [SerializeField] private VolumeConstructionSection[] sectionDucts;
        [SerializeField] private VolumeConstructionSection[] sectionPipes;
        [SerializeField] private VolumeConstructionSection[] sectionOverhead;
        [SerializeField] private GameObject[] enableOnComplete;
        private bool posts = false; // welding from 0/0 to stage0, WFC
        private bool postsIdle = false;
        private bool alums = false; // welding from stage0 to stage1, WFC
        private bool alumIdle = false;
        private bool mach00 = false; // welding from 0/0 to stage0, DBL
        private bool mach00Idle = false;
        private bool WFC02 = false; // welding from stage1 to stage 2, WFC
        private bool WFC02Idle = false;
        private bool DP00 = false; // welding from 0/0 to stage0, DP00
        private bool DP00Idle = false;
        private bool DP02 = false; // welding from DP stage1 to DP stage2
        private bool DP02Idle = false;
        private bool O01 = false;
        private bool O01Idle = false;
        private bool O02 = false; // welding from O stage1 to O stage2
        private bool O02Idle = false;
        private bool mach02 = false; // machines are built
        private bool mach02Idle = false;
        private bool OMIdle = false;
        private bool constructed = false;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!constructed)
            {
                // when stage0 posts,
                if (!posts)
                {
                    bool allSectionsPosts = true;
                    for (int a = 0; a < sectionCeiling.Length; a++)
                    {
                        if (!sectionCeiling[a].Stage0)
                        {
                            allSectionsPosts = false;
                            break;
                        }
                    }
                    if (allSectionsPosts)
                    {
                        for (int b = 0; b < sectionFloor.Length; b++)
                        {
                            if (!sectionFloor[b].Stage0)
                            {
                                allSectionsPosts = false;
                                break;
                            }
                        }
                        if (allSectionsPosts)
                        {
                            for (int c = 0; c < sectionWalls.Length; c++)
                            {
                                if (!sectionWalls[c].Stage0)
                                {
                                    allSectionsPosts = false;
                                    break;
                                }
                            }
                        }
                    }
                    posts = allSectionsPosts;
                }
                else
                {
                    if (!postsIdle)
                    {
                        // weld stage1 (alum)
                        Debug.Log("Alum");
                        for (int a = 0; a < sectionCeiling.Length; a++)
                        {
                            sectionCeiling[a].UpdateSectionCost(1);
                        }
                        for (int a = 0; a < sectionFloor.Length; a++)
                        {
                            sectionFloor[a].UpdateSectionCost(1);
                        }
                        for (int a = 0; a < sectionWalls.Length; a++)
                        {
                            sectionWalls[a].UpdateSectionCost(1);
                        }
                        postsIdle = true;
                    }
                }

                // When WFC stage1, 
                if (!alums)
                {
                    bool allSectionsAlum = true;
                    for (int a = 0; a < sectionCeiling.Length; a++)
                    {
                        if (!sectionCeiling[a].Stage1)
                        {
                            allSectionsAlum = false;
                            break;
                        }
                    }
                    if (allSectionsAlum)
                    {
                        for (int b = 0; b < sectionFloor.Length; b++)
                        {
                            if (!sectionFloor[b].Stage1)
                            {
                                allSectionsAlum = false;
                                break;
                            }
                        }
                        if (allSectionsAlum)
                        {
                            for (int c = 0; c < sectionWalls.Length; c++)
                            {
                                if (!sectionWalls[c].Stage1)
                                {
                                    allSectionsAlum = false;
                                    break;
                                }
                            }
                        }
                    }
                    alums = allSectionsAlum;
                }
                else
                {
                    if (!alumIdle)
                    {
                        // show alum construction
                        for (int a = 0; a < sectionCeiling.Length; a++)
                        {
                            sectionCeiling[a].CON00GameObject.SetActive(false);
                            sectionCeiling[a].CON01GameObject.SetActive(true);
                        }
                        for (int a = 0; a < sectionFloor.Length; a++)
                        {
                            sectionFloor[a].CON00GameObject.SetActive(false);
                            sectionFloor[a].CON01GameObject.SetActive(true);
                        }
                        for (int a = 0; a < sectionWalls.Length; a++)
                        {
                            sectionWalls[a].CON00GameObject.SetActive(false);
                            sectionWalls[a].CON01GameObject.SetActive(true);
                        }

                        // weld stage0 on DBL
                        Debug.Log("DBL0");
                        for (int b = 0; b < sectionDoors.Length; b++)
                        {
                            sectionDoors[b].gameObject.SetActive(true);
                            sectionDoors[b].CON00GameObject.SetActive(true);
                        }
                        for (int b = 0; b < sectionBreakers.Length; b++)
                        {
                            sectionBreakers[b].gameObject.SetActive(true);
                            sectionBreakers[b].CON00GameObject.SetActive(true);
                        }
                        for (int b = 0; b < sectionLights.Length; b++)
                        {
                            sectionLights[b].gameObject.SetActive(true);
                            sectionLights[b].CON00GameObject.SetActive(true);
                        }
                        alumIdle = true;
                    }
                }

                // When DBL stage0, 
                if (alumIdle)
                {
                    if (!mach00)
                    {
                        bool allMachs0 = true;
                        for (int b = 0; b < sectionDoors.Length; b++)
                        {
                            if (!sectionDoors[b].Stage0)
                            {
                                allMachs0 = false;
                                break;
                            }
                        }
                        if (allMachs0)
                        {
                            for (int b = 0; b < sectionBreakers.Length; b++)
                            {
                                if (!sectionBreakers[b].Stage0)
                                {
                                    allMachs0 = false;
                                    break;
                                }
                            }
                            if (allMachs0)
                            {
                                for (int b = 0; b < sectionLights.Length; b++)
                                {
                                    if (!sectionLights[b].Stage0)
                                    {
                                        allMachs0 = false;
                                        break;
                                    }
                                }
                            }
                        }
                        mach00 = allMachs0;
                    }
                    else
                    {
                        if (!mach00Idle)
                        {
                            // weld WFC stage2
                            Debug.Log("WFC2");
                            for (int a = 0; a < sectionCeiling.Length; a++)
                            {
                                sectionCeiling[a].UpdateSectionCost(2);
                            }
                            for (int a = 0; a < sectionFloor.Length; a++)
                            {
                                sectionFloor[a].UpdateSectionCost(2);
                            }
                            for (int a = 0; a < sectionWalls.Length; a++)
                            {
                                sectionWalls[a].UpdateSectionCost(2);
                            }
                            mach00Idle = true;
                        }
                    }

                }

                // When WFC stage2,
                if (!WFC02)
                {
                    bool allWFCdone = true;
                    for (int a = 0; a < sectionCeiling.Length; a++)
                    {
                        if (!sectionCeiling[a].Complete)
                        {
                            allWFCdone = false;
                            break;
                        }
                    }
                    if (allWFCdone)
                    {
                        for (int b = 0; b < sectionFloor.Length; b++)
                        {
                            if (!sectionFloor[b].Complete)
                            {
                                allWFCdone = false;
                                break;
                            }
                        }
                        if (allWFCdone)
                        {
                            for (int c = 0; c < sectionWalls.Length; c++)
                            {
                                if (!sectionWalls[c].Complete)
                                {
                                    allWFCdone = false;
                                    break;
                                }
                            }
                        }
                    }
                    WFC02 = allWFCdone;
                }
                else
                {
                    if (!WFC02Idle)
                    {
                        // show WFC complete
                        for (int a = 0; a < sectionCeiling.Length; a++)
                        {
                            sectionCeiling[a].CON01GameObject.SetActive(false);
                            sectionCeiling[a].CompletedGameObject.SetActive(true);
                        }
                        for (int a = 0; a < sectionFloor.Length; a++)
                        {
                            sectionFloor[a].CON01GameObject.SetActive(false);
                            sectionFloor[a].CompletedGameObject.SetActive(true);
                        }
                        for (int a = 0; a < sectionWalls.Length; a++)
                        {
                            sectionWalls[a].CON01GameObject.SetActive(false);
                            sectionWalls[a].CompletedGameObject.SetActive(true);
                        }
                        // weld DP stage 0
                        Debug.Log("DP0");
                        for (int b = 0; b < sectionDucts.Length; b++)
                        {
                            sectionDucts[b].CON00GameObject.SetActive(true);
                        }
                        for (int b = 0; b < sectionPipes.Length; b++)
                        {
                            sectionPipes[b].CON00GameObject.SetActive(true);
                        }
                        WFC02Idle = true;
                    }
                }

                // When DP stage0,
                if (!DP00)
                {
                    bool dpStage00 = true;
                    for (int a = 0; a < sectionDucts.Length; a++)
                    {
                        if (!sectionDucts[a].Stage0)
                        {
                            dpStage00 = false;
                            break;
                        }
                    }
                    if (dpStage00)
                    {
                        for (int b = 0; b < sectionPipes.Length; b++)
                        {
                            if (!sectionPipes[b].Stage0)
                            {
                                dpStage00 = false;
                                break;
                            }
                        }
                    }
                    DP00 = dpStage00;
                }
                else
                {
                    if (!DP00Idle)
                    {
                        // weld DP stage2
                        Debug.Log("DP2");
                        for (int a = 0; a < sectionDucts.Length; a++)
                        {
                            sectionDucts[a].UpdateSectionCost(2);
                        }
                        for (int a = 0; a < sectionPipes.Length; a++)
                        {
                            sectionPipes[a].UpdateSectionCost(2);
                        }
                        DP00Idle = true;
                    }
                }

                // When DP stage2,
                if (!DP02)
                {
                    bool dpStage02 = true;
                    for (int a = 0; a < sectionDucts.Length; a++)
                    {
                        if (!sectionDucts[a].Complete)
                        {
                            dpStage02 = false;
                            break;
                        }
                    }
                    if (dpStage02)
                    {
                        for (int b = 0; b < sectionPipes.Length; b++)
                        {
                            if (!sectionPipes[b].Complete)
                            {
                                dpStage02 = false;
                                break;
                            }
                        }
                    }
                    DP02 = dpStage02;
                }
                else
                {
                    if (!DP02Idle)
                    {
                        // show DP stage2
                        for (int b = 0; b < sectionDucts.Length; b++)
                        {
                            sectionDucts[b].CON00GameObject.SetActive(false);
                            sectionDucts[b].CompletedGameObject.SetActive(true);
                        }
                        for (int b = 0; b < sectionPipes.Length; b++)
                        {
                            sectionPipes[b].CON00GameObject.SetActive(false);
                            sectionPipes[b].CompletedGameObject.SetActive(true);
                        }
                        // weld O stage0
                        Debug.Log("O0");
                        for (int b = 0; b < sectionOverhead.Length; b++)
                        {
                            sectionOverhead[b].CON00GameObject.SetActive(true);
                        }
                        // show DBL02 for welding
                        Debug.Log("Show DBL02");
                        for (int b = 0; b < sectionDoors.Length; b++)
                        {
                            sectionDoors[b].CON00GameObject.SetActive(false);
                            sectionDoors[b].CompletedGameObject.SetActive(true);
                        }
                        for (int b = 0; b < sectionBreakers.Length; b++)
                        {
                            sectionBreakers[b].CON00GameObject.SetActive(false);
                            sectionBreakers[b].CompletedGameObject.SetActive(true);
                        }
                        for (int b = 0; b < sectionLights.Length; b++)
                        {
                            sectionLights[b].CON00GameObject.SetActive(false);
                            sectionLights[b].CompletedGameObject.SetActive(true);
                        }
                        DP02Idle = true;
                    }
                }

                // When O stage0,
                if (!O01)
                {
                    O01 = true;
                    for(int a = 0; a < sectionOverhead.Length; a++)
                    {
                        if (!sectionOverhead[a].Stage0)
                        {
                            O01 = false;
                        }
                    }
                }
                else
                {
                    if (!O01Idle)
                    {
                        // weld O stage2
                        Debug.Log("O2");
                        for (int b = 0; b < sectionOverhead.Length; b++)
                        {
                            sectionOverhead[b].UpdateSectionCost(2);
                        }
                        O01Idle = true;
                    }
                }

                // When O stage2, 
                if (!O02)
                {
                    bool allOverheadsBuilt = true;
                    for (int a = 0; a < sectionOverhead.Length; a++)
                    {
                        if (!sectionOverhead[a].MachineFullyBuilt)
                        {
                            allOverheadsBuilt = false;
                            break;
                        }
                    }
                    O02 = allOverheadsBuilt;
                }
                else
                {
                    if (!O02Idle)
                    {
                        //show O02 complete,
                        Debug.Log("Show O2");
                        for (int b = 0; b < sectionOverhead.Length; b++)
                        {
                            sectionOverhead[b].CompletedGameObject.SetActive(true);
                        }
                        O02Idle = true;
                    }
                }
                // when DBLs complete,
                if (mach00 && !mach02)
                {
                    bool allMachsBuilt = true;
                    for (int m = 0; m < sectionDoors.Length; m++)
                    {
                        //.CompletedGameObject.GetComponent<IConstructible>().MachineFullyBuilt
                        if (!sectionDoors[m].Complete)
                        {
                            allMachsBuilt = false;
                            break;
                        }
                    }
                    if (allMachsBuilt)
                    {
                        for (int mb = 0; mb < sectionBreakers.Length; mb++)
                        {
                            //CompletedGameObject.GetComponent<IConstructible>().MachineFullyBuilt
                            if (!sectionBreakers[mb].Complete)
                            {
                                allMachsBuilt = false;
                                break;
                            }
                        }
                        if (allMachsBuilt)
                        {
                            for (int mc = 0; mc < sectionLights.Length; mc++)
                            {
                                //CompletedGameObject.GetComponent<IConstructible>().MachineFullyBuilt
                                if (!sectionLights[mc].Complete)
                                {
                                    allMachsBuilt = false;
                                    break;
                                }
                            }
                        }
                    }
                    mach02 = allMachsBuilt;
                }
                else
                {
                    if (!mach02Idle)
                    {
                        mach02Idle = true;
                    }
                }
                // activate crap,
                if (O02 && mach02)
                {
                    if (!OMIdle)
                    {
                        Debug.Log("Finish");
                        foreach (GameObject obj in enableOnComplete)
                        {
                            obj.SetActive(true);
                        }
                        OMIdle = true;
                        constructed = true;
                    }
                }

            }
        }
    }
}