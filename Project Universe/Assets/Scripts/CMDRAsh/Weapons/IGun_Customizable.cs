using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using ProjectUniverse.Player.PlayerController;
using ProjectUniverse.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Items.Weapons
{
    public class IGun_Customizable : NetworkBehaviour
    {
        //private ProjectUniverse.PlayerControls controls;//temp
        [SerializeField] private float Damage = 10f;
        [SerializeField] private float RoundsPerMinute = 180f;
        //[SerializeField] private float Range = 1000f; //Estimate based off of gun and bullet properties, bullets will travel until they hit something
        //unless in space, at which point we do need a bullet life.
        [SerializeField] private float BulletSpeed = 850f;//m/s
        [SerializeField] private float AccuracyBase = 0.95f;//IE (1-angle/deviation) on first fire
        [SerializeField] private float AccuracryFiring = 0.9f;//IE accuracy when firing on automatic.
        [SerializeField] private float RecoilForce = 10f;
        [SerializeField] private float DrawTime = 1f;
        [SerializeField] private float ReloadTime = 2.5f;
        [SerializeField] private float NoiseLevel = 10f;//eventually in decibles
        [SerializeField] private float AcceptedMagCaliber = 7.62f;//IE 7.62x51 NATO
        [SerializeField] private int MagSize = 25; //mag stuff might come from a Magazine class
        [SerializeField] private int FireMode = 0; //0 - semi, 1 - burst, 2 - auto

        //public ParticleSystem muzzleflash;
        [SerializeField] private GameObject[] MuzzleEffectOnShot;//VFX/particle prefabs like smoke to be placed in world-space
        [SerializeField] private GameObject[] MuzzleEffectAfterShot;//VFX prefab (barrel smoke,etc) placed at cease-fire
        [SerializeField] private GameObject MuzzleDummy;//where particles and bullets will be placed on fire
        [SerializeField] private GameObject SlugToShoot;
        [SerializeField] private GameObject CasingToShoot;
        [SerializeField] private GameObject CasingDummy;
        private GameObject[] impactEffect; //Determined by caliber and obj hit. Usually impact effect is either a hole or bit of lead on the wall
        [SerializeField] private AudioClip ReloadSound;
        [SerializeField] private AudioClip ShotSound;
        //[SerializeField] private AudioClip CasingSound;//sound played when casing hits the floor
        [SerializeField] private AudioSource Speaker;
        //private float forceonimpact = 30f; //Same force hits target as recoil force on player

        [SerializeField] private Rigidbody playerRB;
        private float nexttimetofire = 0f;
        private float fireRate = 0f;
        private int bulletsRemaining = 25;
        private bool firing = false;
        private GunAmmoUI ammoUI;

        public float Caliber
        {
            get { return AcceptedMagCaliber; }
        }

        public float MuzzleVelocity
        {
            get { return BulletSpeed; }
        }

        public Rigidbody PlayerRB
        {
            get { return playerRB; }
            set { playerRB = value; }
        }

        public GunAmmoUI AmmoUI
        {
            get { return ammoUI; }
            set { ammoUI = value; }
        }

        public int MagCap
        {
            get { return MagSize; }
        }

        // Start is called before the first frame update
        void Start()
        {
            CalcFireRate();
            bulletsRemaining = 25;
            //controls.Player.Reload.performed += ctx =>
            //{
            //    ReloadGun();
            //};
        }

        public void StopFiring()
        {
            firing = false;
        }

        public void FireMainCall()
        {
            //Debug.Log("Main Call");
            if (bulletsRemaining > 0)
            {
                //This connected client has tried to shoot (this goes to the server first, then back to this client. Can be optimized)
                if (FireMode == 0)
                {
                    if (!firing)
                    {
                        StartCoroutine(FireCoroutine(1));
                    }  
                }
                else if (FireMode == 1)
                {
                    //fire bursts at normal weap speed
                    //multiple bursts cannot be fired at once
                    if (!firing)
                    {
                        StartCoroutine(FireCoroutine(3));
                    }

                }
                else if (FireMode == 2)
                {
                    if (!firing)
                    {
                        StartCoroutine(FireCoroutine(-2));
                    }
                }
            }
        }

        private IEnumerator FireCoroutine(int threshold)
        {
            firing = true;
            while ((threshold > 0 || threshold == -2) && firing)
            {
                if (bulletsRemaining > 0)
                {
                    if (nexttimetofire <= 0f)
                    {
                        nexttimetofire = fireRate;
                        bulletsRemaining--;
                        if(threshold != -2)
                        {
                            threshold--;
                        }
                        //FireGunServerRpc();
                        SomethingIsBroken();
                        ammoUI.UpdateAmmoCurrent(bulletsRemaining);
                    }
                }
                else
                {
                    threshold = 0;
                    firing = false;
                }
                yield return null;
            }
            firing = false;
        }

        // Update is called once per frame
        void Update()
        {
            //if (IsLocalPlayer)
            //{
                if (nexttimetofire > 0)
                {
                    nexttimetofire -= Time.deltaTime;
                }
            //}
        }

        public void ModeSwitch()
        {
            string mode = "-";
            if(FireMode == 0)
            {
                FireMode = 1;
                mode = "x3";
            }
            else if(FireMode == 1)
            {
                FireMode = 2;
                mode = "Auto";
            }
            else if(FireMode == 2)
            {
                FireMode = 0;
                mode = "x1";
            }
            ammoUI.UpdateMode(mode);
        }

        //run on server, called by client
        [ServerRpc]//(RequireOwnership = false)
        public void FireGunServerRpc()
        {
            Debug.Log("Calling ServerRPC");
            //runs code on every client
            FireGunClientRpc();
        }

        //runs on clients, called by server
        [ClientRpc]
        public void FireGunClientRpc()//GameObject bullet, GameObject casing
        {
            Vector3 motionVector = CalculateBulletVector();
            Vector3 rotation = MuzzleDummy.transform.rotation.eulerAngles + new Vector3(0, -90, 0);

            //create a bullet to shoot
            GameObject bullet = Instantiate(SlugToShoot, MuzzleDummy.transform.position, Quaternion.Euler(rotation), MuzzleDummy.transform);
            bullet.SetActive(true);
            bullet.GetComponent<Rigidbody>().velocity = motionVector;
            bullet.transform.SetParent(null);
            bullet.GetComponent<IBullet>().SetDamageAmount(Damage);

            Vector3 casingRot = CasingDummy.transform.rotation.eulerAngles + new Vector3(0, 0, 0);
            //create a casing and spit it out as well
            GameObject casing = Instantiate(CasingToShoot, CasingDummy.transform.position, Quaternion.Euler(casingRot), CasingDummy.transform);
            casing.SetActive(true);
            casing.GetComponent<Rigidbody>().velocity = MuzzleDummy.transform.right * 3f + playerRB.velocity;
            casing.transform.SetParent(null);
            
            Speaker.clip = ShotSound;
            Speaker.Play(0);
            playerRB.AddForce(Vector3.back * RecoilForce);

            //place muzzle effect after shot
            //for (int mep = 0; mep < MuzzleEffectAfterShot.Length; mep++)
            //{
            //}
        }

        public void SomethingIsBroken()
        {
            Vector3 motionVector = CalculateBulletVector();
            Vector3 rotation = MuzzleDummy.transform.rotation.eulerAngles + new Vector3(0, -90, 0);

            //create a bullet to shoot
            GameObject bullet = Instantiate(SlugToShoot, MuzzleDummy.transform.position, Quaternion.Euler(rotation), MuzzleDummy.transform);
            bullet.SetActive(true);
            bullet.GetComponent<Rigidbody>().velocity = motionVector;
            bullet.transform.SetParent(null);
            bullet.GetComponent<IBullet>().SetDamageAmount(Damage);

            Vector3 casingRot = CasingDummy.transform.rotation.eulerAngles + new Vector3(0, 0, 0);
            //create a casing and spit it out as well
            GameObject casing = Instantiate(CasingToShoot, CasingDummy.transform.position, Quaternion.Euler(casingRot), CasingDummy.transform);
            casing.SetActive(true);
            casing.GetComponent<Rigidbody>().velocity = MuzzleDummy.transform.right * 3f + playerRB.velocity;
            casing.transform.SetParent(null);

            Speaker.clip = ShotSound;
            Speaker.Play(0);
            playerRB.AddForce(Vector3.back * RecoilForce);
        }
        
        public void ReloadGun()
        {
            bulletsRemaining = MagSize;
            ammoUI.UpdateAmmoCurrent(bulletsRemaining);
            ammoUI.UpdateAmmoStored(0);
        }

        private Vector3 CalculateBulletVector()
        {
            System.Random rand = new System.Random();

            double min = -1 * (1 - AccuracyBase);
            double max = (1 - AccuracyBase);
            double range = max - min;

            double sampleX = rand.NextDouble();
            double scaled = (sampleX * range) + min;
            float x = (float)scaled;

            double sampleY = rand.NextDouble();
            scaled = (sampleY * range) + min;
            float y = (float)scaled;

            //double sampleZ = rand.NextDouble();
            //scaled = (sampleZ * range) + min;
            //float z = (float)scaled;

            Vector3 motionVector = MuzzleDummy.transform.forward * BulletSpeed + playerRB.velocity;
            motionVector += new Vector3(x, y, 0);
            return motionVector;
        }

        public void CalcFireRate()
        {
            //rps = rpm/60s
            //Time.deltaTime = 1s/frame
            // 1/rps = frame (frame = nextTimeOfFire)
            nexttimetofire = 1 / (RoundsPerMinute / 60);
            fireRate = nexttimetofire;
        }

    }
}
