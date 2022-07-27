using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Util;

namespace ProjectUniverse.Environment.Radiation
{
    public class IRadiationZone : MonoBehaviour
    {
        public SphereCollider radiationArea;
        public BoxCollider radiationAreaPool;
        [Tooltip("Radiological activity of material per gram. Unit is 10^12 Becquerels. Ex: 1g CS-137 is 3.215TBq.")]
        [SerializeField] private float teraBecquerels = 3.215f;//x10^12, activity of one gram CS-137
        [Tooltip("Mass of radioactive material in grams. Linearly increases radioactivity.")]
        [SerializeField] private float gramsMaterial = 1;
        //private float exposureRateConstant = 3.4f;//R*cm^2/hr*mCi
        //private float mCurie;
        [SerializeField] private bool connectedToGenerator;
        private float tBqs_rated;
        private float _generatorLeakMultiplier = 0f; //0f to 1f
        //private float energyFluenceRate = 0.0000005263f;//5.263x10^-7
        private float iron_attenuation = 0.3825f;//ish.
        //private float mass_absCoeff = 0.02836f; //iron Uen/P
        //private float tabulatedEnergy_CS137 = 0.1576555265f;//SUM of MeV for CS-137 decomp
        private float lastGrams;
        private float lastLeakMult;

        private void Start()
        {
            lastGrams = gramsMaterial;
            //lastLeakMult = _generatorLeakMultiplier;
            tBqs_rated = teraBecquerels * gramsMaterial * _generatorLeakMultiplier;
            //set the radius of the radiation area as the distance to .074 (1g CS-137 at 20m)
            float r_m = Utils.MaxRadiationExposureRange(tBqs_rated, 0.074f);
            radiationArea.radius = r_m;
        }

        public bool ConnectedToGenerator
        {
            get { return connectedToGenerator; }
            set { connectedToGenerator = value; }
        }

        public float GeneratorLeakMultiplier { 
            get { return _generatorLeakMultiplier; } 
            set { _generatorLeakMultiplier = value; } 
        }

        private void Update()
        {
            if(lastLeakMult != _generatorLeakMultiplier)
            {
                //float delt = lastLeakMult - _generatorLeakMultiplier;
                //if((delt/lastLeakMult) > 0.025f)
                //{
                    if (connectedToGenerator)
                    {
                        tBqs_rated = _generatorLeakMultiplier * teraBecquerels * gramsMaterial;
                    }
                    else
                    {
                        tBqs_rated = teraBecquerels * gramsMaterial;
                    }
                    float r_m = Utils.MaxRadiationExposureRange(tBqs_rated, 0.74f);
                    radiationArea.radius = r_m;
                    lastGrams = gramsMaterial;
                    //lastLeakMult = _generatorLeakMultiplier;
                //}
            }
            if (lastGrams != gramsMaterial)
            {
                if (connectedToGenerator)
                {
                    tBqs_rated = _generatorLeakMultiplier * teraBecquerels * gramsMaterial;
                }
                else
                {
                    tBqs_rated = teraBecquerels * gramsMaterial;
                }
                float r_m = Utils.MaxRadiationExposureRange(tBqs_rated, 0.74f);
                radiationArea.radius = r_m;
                lastGrams = gramsMaterial;
                //lastLeakMult = _generatorLeakMultiplier;
            }
            lastLeakMult = _generatorLeakMultiplier;
        }

        public float GetThickness(Vector3 target)
        {
            float thickness = 0f;
            target = new Vector3(target.x, target.y + 1f, target.z);
            Vector3 direction = (target - transform.position);
            //raycast from source to the PVC and get everything between the two (except generator and player)
            RaycastHit[] rHits = Physics.RaycastAll(transform.position, direction.normalized, direction.magnitude);
            //Debug.Log("--");
            for(int i = 0; i < rHits.Length; i++)
            {
                RaycastHit firstHit = rHits[i];
                RaycastHit secondHit;
                //Debug.Log(rHits[i].collider.gameObject);
                //get thickness of collider along this axis
                Vector3 origin = (firstHit.point + (direction.normalized));
                Vector3 closest = firstHit.collider.ClosestPoint(origin);
                Ray opDirRay = new Ray(closest, (direction.normalized*-1f));
                //Debug.DrawRay(origin, (direction.normalized * -1f), Color.magenta);
                //Debug.DrawRay(closest, (direction.normalized * -1f), Color.red);
                if(firstHit.collider.Raycast(opDirRay, out secondHit,5f))
                {
                    //thickness assumes the object is solid, which it isn't. X0.1f adjusts to a more 'real' thickness.
                    thickness += (firstHit.point - secondHit.point).magnitude * 0.1f;
                    //Debug.Log(thickness);
                    //Debug.Log(firstHit.collider.gameObject+" "+ ((firstHit.point - secondHit.point).magnitude * 0.1f));
                }
                else//thicker than 1m
                {
                    //test against player and volumes
                    if(!firstHit.collider.gameObject.TryGetComponent<VolumeAtmosphereController>(out VolumeAtmosphereController vac))
                    {
                        if (!firstHit.collider.gameObject.CompareTag("Player"))
                        {
                            //Debug.Log(firstHit.collider.gameObject + " 0.1f");
                            thickness += 0.1f;//need guard against 2D planes.
                        }
                    }
                    
                }
            }
            return thickness*100f;//convert to cm
        }

        void OnTriggerEnter(Collider other)
        {
            PlayerVolumeController playerVC;
            DroneVolumeController droneVC = null;
            if (other.gameObject.TryGetComponent<PlayerVolumeController>(out playerVC) ||
                other.gameObject.TryGetComponent<DroneVolumeController>(out droneVC))
            {
                Transform playerTans = other.gameObject.transform;
                float shield_cm = GetThickness(playerTans.position);
                if (radiationArea != null)
                {
                    float deltaX = (playerTans.position.x - transform.position.x) * 100;
                    float deltaY = (playerTans.position.y - transform.position.y) * 100;
                    float deltaZ = (playerTans.position.z - transform.position.z) * 100;
                    float distance_cm = (float)Mathf.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ));
                    // Calculate the exposure rate of the player using distance, ExposureRateConstant, and mCurie
                    //float level = exposureRateConstant * mCurie / (distance_cm * distance_cm);
                    float exposure = Utils.RadiationExposureRate(tBqs_rated, distance_cm);
                    if (playerVC != null)
                    {
                        playerVC.SetRadiationExposureRate(exposure);
                    }
                    else
                    {
                        droneVC.SetRadiationExposureRate(exposure);
                    }
                    //Debug.Log(shield_cm + " " + distance_cm + " " + (tBqs_rated));
                    float absRate = Utils.RadiationDoseRate(exposure, shield_cm, iron_attenuation);
                    //float absRate = Utils.RadiationAbsorbedDose(energyFluenceRate, 
                    //    (teraBecquerels * gramsMaterial * _generatorLeakMultiplier),
                    //    photonEnergy, mass_absCoeff, iron_attenuation, shield_cm, distance_cm);
                    if (playerVC != null) 
                    {
                        playerVC.AbsorbtionRate = absRate;
                        playerVC.CalculateAbsorbedDose();
                    }
                    else
                    {
                        droneVC.AbsorbtionRate = absRate;
                        droneVC.CalculateAbsorbedDose();
                    }
                    
                }
                //if (radiationAreaPool != null)
                //{
                //    playerVC.SetRadiationExposureRate(level);
                //}
            }
        }

        void OnTriggerStay(Collider other)
        {
            PlayerVolumeController playerVC;
            DroneVolumeController droneVC = null;
            if (other.gameObject.TryGetComponent<PlayerVolumeController>(out playerVC) ||
                other.gameObject.TryGetComponent<DroneVolumeController>(out droneVC))
            {
                Transform playerTans = other.gameObject.transform;
                float shield_cm = GetThickness(playerTans.position);
                if (radiationArea != null)
                {
                    //distances in cm
                    float deltaX = (playerTans.position.x - transform.position.x)*100;
                    float deltaY = (playerTans.position.y - transform.position.y)*100;
                    float deltaZ = (playerTans.position.z - transform.position.z)*100;
                    float distance_cm = (float)Mathf.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ));
                    // Calculate the exposure rate of the player using distance, ExposureRateConstant, and mCurie
                    //float level = exposureRateConstant * mCurie / (distance_cm * distance_cm);
                    //Debug.Log(shield_cm + " " + distance_cm + " " + (tBqs_rated));
                    float exposure = Utils.RadiationExposureRate(tBqs_rated, distance_cm);
                    float absRate = Utils.RadiationDoseRate(exposure, shield_cm, iron_attenuation);
                    if (playerVC != null)
                    {
                        playerVC.SetRadiationExposureRate(exposure);
                        playerVC.AbsorbtionRate = absRate;
                        playerVC.CalculateAbsorbedDose();
                    }
                    else
                    {
                        droneVC.SetRadiationExposureRate(exposure);
                        droneVC.AbsorbtionRate = absRate;
                        droneVC.CalculateAbsorbedDose();
                    }
                    
                }
                //if (radiationAreaPool != null)
                //{
                //    playerVC.SetRadiationExposureRate(roentgen);
                //}
                if(playerVC != null)
                {
                    playerVC.AddRadiationExposureTime(Time.deltaTime);
                }
                else
                {
                    droneVC.AddRadiationExposureTime(Time.deltaTime);
                }
                
            }
        }

        void OnTriggerExit(Collider other)
        {
            PlayerVolumeController playerVC;
            DroneVolumeController droneVC = null;
            if (other.gameObject.TryGetComponent<PlayerVolumeController>(out playerVC) ||
                other.gameObject.TryGetComponent<DroneVolumeController>(out droneVC))
            {
                if(playerVC != null)
                {
                    playerVC.SetRadiationExposureRate(0);//not going to work with multiple rad sources
                }
                else
                {
                    droneVC.SetRadiationExposureRate(0);//not going to work with multiple rad sources
                }
                
            }
        }
    }
}