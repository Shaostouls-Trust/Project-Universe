using ProjectUniverse.Environment.Fluid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Environment.Gas
{
    public class PipeSection : MonoBehaviour
    {
        [SerializeField] private bool gasPipe;
        [SerializeField] private bool fluidPipe;
        [Tooltip("All pipes in this section of pipes. One of these pipes must also be present in another section list for a valid connection.")]
        [SerializeField] private List<IGasPipe> gasPipesInSection;
        [SerializeField] private List<IFluidPipe> fluidPipesInSection;

        public List<IGasPipe> GasPipesInSection
        {
            get { return gasPipesInSection; }
            set { gasPipesInSection = value; }
        }
    }
}