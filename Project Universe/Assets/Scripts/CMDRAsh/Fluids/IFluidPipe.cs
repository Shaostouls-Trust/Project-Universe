using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ProjectUniverse.Environment.Gas;

namespace ProjectUniverse.Environment.Fluid
{
    /// <summary>
    /// Transfer Fluids of X types at whatever rate/pressure determined by the pump
    /// Vars:
    /// IFluid[] - Array of fluids found in pipe
    /// | next - next pipe in linkedlist
    /// | temp - temperature of the pipe. Influenced by internal fluid temp and ambient temp.
    /// | tempTol[H,C] - pipe tollerance of hot or cold before bursting
    /// | GlobalPressure - total system/static pressure. Does not factor in temp or volume.
    /// | LocalPressure - interal applied pressure based on temp and volume
    /// | maxP - maxiumum intenal pressure the pipe can handle
    /// | health - pipe's health
    /// | leakrate - % leak from bullet holes into local atmo
    /// | bulletholes - List of all bullet holes in the pipe. Mainly for VFX placemement purposes.
    /// | insulationRating - percent of heat kept in or out. Intended to be higher for superhot pipes, and lower for ducts and water pipes.
    /// 
    /// Transfer fluids, pressure, and temp to the 'next' duct(s)
    /// </summary> 

    public class IFluidPipe : MonoBehaviour
    {
        [SerializeField] private List<IFluid> fluids = new List<IFluid>();
        [SerializeField] private IGasPipe[] neighbors;
        [SerializeField] private float temp;
        [SerializeField] private float[] tempTol = new float[2];
        [SerializeField] private float globalPressure;
        [SerializeField] private float appliedPressure;
        [SerializeField] private float maxP;
        [SerializeField] private float volume_m3;//standard pipe is ?m3
        [SerializeField] private float health;
        [SerializeField] private float leakRate;
        //[SerializeField] private GameObject[] bulletHoles;
        [SerializeField] private float throughput_m3;
        [SerializeField] private Volume ductVolume;
        [SerializeField] private float insulationRating = 0.5f;
        [SerializeField] private bool burst;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}