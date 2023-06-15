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
        //5% U235 95% U238 is .126 TBq
        [SerializeField] private float teraBecquerels = 3.215f;//x10^12, activity of one gram CS-137
        [Tooltip("Mass of radioactive material in grams. Linearly increases radioactivity.")]
        [SerializeField] private float gramsMaterial = 1;
        [SerializeField] private bool connectedToGenerator;
        private float tBqs_rated;
        [SerializeField] private float _generatorLeakMultiplier = 0f; //0f to 1f
        //at 500 KeV linear atten coeff of Al is .227, iron is .655
        //70% al, 30% iron is randomly chosen distribution of metals
        private float average_attenuation = 0.3554f;//at 500KeV, per cm
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
            RaycastHit[] rHits = Physics.RaycastAll((transform.position+radiationArea.center), direction.normalized, direction.magnitude);
            for(int i = 0; i < rHits.Length; i++)
            {
                RaycastHit firstHit = rHits[i];
                RaycastHit secondHit;
                //Debug.Log(rHits[i].collider.gameObject);
                //get thickness of collider along this axis
                Vector3 origin = (firstHit.point + (direction.normalized));
                Vector3 closest = firstHit.collider.ClosestPoint(origin);
                //Ray opDirRay = new Ray(closest, (direction.normalized*-1f));
                //Debug.DrawRay(origin, (direction.normalized * -1f), Color.magenta);
                //Debug.DrawRay(closest, (direction.normalized * -1f), Color.red);
                //for now, assume all objects are effectively .25cm thick
                thickness += 0.0025f;
                /*
                if(firstHit.collider.Raycast(opDirRay, out secondHit,1f))//5f
                {
                    //thickness assumes the object is solid, which it isn't. For all "thin" object, increase thickness by 0.25cm
                    thickness += 0.0025f; //(firstHit.point - secondHit.point).magnitude * 0.1f;
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
                            thickness += 0.02f;//need guard against 2D planes.//assumes 2cm effective thickness
                        }
                    }
                }*/
            }
            //Debug.Log(thickness);
            return thickness*100f;//convert to cm
        }

        void OnTriggerEnter(Collider other)
        {
            PlayerVolumeController playerVC;
            DroneVolumeController droneVC = null;
            DoscimeterTerminalController DTC = null;
            if (other.gameObject.TryGetComponent<PlayerVolumeController>(out playerVC) ||
                other.gameObject.TryGetComponent<DroneVolumeController>(out droneVC) || 
                other.gameObject.TryGetComponent<DoscimeterTerminalController>(out DTC))
            {
                Transform playerTans = other.gameObject.transform;
                float shield_cm = GetThickness(playerTans.position);
                if (radiationArea != null)
                {
                    float deltaX = (playerTans.position.x - transform.position.x + radiationArea.center.x) * 100;
                    float deltaY = (playerTans.position.y - transform.position.y + radiationArea.center.y) * 100;
                    float deltaZ = (playerTans.position.z - transform.position.z + radiationArea.center.z) * 100;
                    float distance_cm = (float)Mathf.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ));
                    // Calculate the exposure rate of the player using distance
                    float exposure = Utils.RadiationExposureRate(tBqs_rated, distance_cm);
                    //Debug.Log(shield_cm + " " + distance_cm + " " + (tBqs_rated));
                    float absRate = Utils.RadiationDoseRate(exposure, shield_cm, average_attenuation);

                    if (playerVC != null)
                    {
                        playerVC.SetRadiationExposureRate(absRate);//exposure
                    }
                    else if(droneVC != null)
                    {
                        droneVC.SetRadiationExposureRate(absRate);//exposure
                    }
                    else if(DTC != null)
                    {
                        DTC.DetectedRoentgen = absRate;
                        DTC.RadiationAt10Meters = Utils.RadiationExposureRate(tBqs_rated, 1000f);
                    }
                    
                    //float absRate = Utils.RadiationAbsorbedDose(energyFluenceRate, 
                    //    (teraBecquerels * gramsMaterial * _generatorLeakMultiplier),
                    //    photonEnergy, mass_absCoeff, iron_attenuation, shield_cm, distance_cm);
                    if (playerVC != null) 
                    {
                        playerVC.AbsorbtionRate = absRate;
                        playerVC.CalculateAbsorbedDose();
                    }
                    else if(droneVC != null)
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
            DoscimeterTerminalController DTC = null;
            if (other.gameObject.TryGetComponent<PlayerVolumeController>(out playerVC) ||
                other.gameObject.TryGetComponent<DroneVolumeController>(out droneVC) ||
                other.gameObject.TryGetComponent<DoscimeterTerminalController>(out DTC))
            {
                Transform playerTans = other.gameObject.transform;
                float shield_cm = GetThickness(playerTans.position);
                if (radiationArea != null)
                {
                    //distances in cm
                    float deltaX = (playerTans.position.x - transform.position.x + radiationArea.center.x) *100;
                    float deltaY = (playerTans.position.y - transform.position.y + radiationArea.center.y) *100;
                    float deltaZ = (playerTans.position.z - transform.position.z + radiationArea.center.z) *100;
                    float distance_cm = (float)Mathf.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ));
                    // Calculate the exposure rate of the player using distance, ExposureRateConstant, and mCurie
                    //float level = exposureRateConstant * mCurie / (distance_cm * distance_cm);
                    //Debug.Log(shield_cm + " " + distance_cm + " " + (tBqs_rated));
                    float exposure = Utils.RadiationExposureRate(tBqs_rated, distance_cm);
                    float absRate = Utils.RadiationDoseRate(exposure, shield_cm, average_attenuation);
                    if (playerVC != null)
                    {
                        playerVC.SetRadiationExposureRate(absRate);//exposure
                        playerVC.AbsorbtionRate = absRate;
                        playerVC.CalculateAbsorbedDose();
                    }
                    else if(droneVC != null)
                    {
                        droneVC.SetRadiationExposureRate(absRate);
                        droneVC.AbsorbtionRate = absRate;
                        droneVC.CalculateAbsorbedDose();
                    }
                    else if (DTC != null)
                    {
                        DTC.DetectedRoentgen = absRate;
                        DTC.RadiationAt10Meters = Utils.RadiationExposureRate(tBqs_rated, 1000f);
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
                else if (droneVC != null)
                {
                    droneVC.AddRadiationExposureTime(Time.deltaTime);
                }
                
            }
        }

        void OnTriggerExit(Collider other)
        {
            PlayerVolumeController playerVC;
            DroneVolumeController droneVC = null;
            DoscimeterTerminalController DTC = null;
            if (other.gameObject.TryGetComponent<PlayerVolumeController>(out playerVC) ||
                other.gameObject.TryGetComponent<DroneVolumeController>(out droneVC) ||
                other.gameObject.TryGetComponent<DoscimeterTerminalController>(out DTC))
            {
                if(playerVC != null)
                {
                    playerVC.SetRadiationExposureRate(0);//not going to work with multiple rad sources
                }
                else if (droneVC != null)
                {
                    droneVC.SetRadiationExposureRate(0);//not going to work with multiple rad sources
                }
                else if (DTC != null)
                {
                    DTC.DetectedRoentgen = 0;
                    DTC.RadiationAt10Meters = 0;
                }
            }
        }

        public float RadiationAtOneMeter()
        {
            return Utils.RadiationExposureRate(tBqs_rated, 100f);
        }
    }
}