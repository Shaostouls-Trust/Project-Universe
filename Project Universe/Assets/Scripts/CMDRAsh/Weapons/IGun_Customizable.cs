using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Items.Weapons
{
    public class IGun_Customizable : MonoBehaviour
    {
        [SerializeField] private float Damage = 10f;
        [SerializeField] private float RoundsPerMinute = 180f;
        //[SerializeField] private float Range = 1000f; //Estimate based off of gun and bullet properties, bullets will travel until they hit something
        [SerializeField] private float BulletSpeed = 850f;//m/s
        [SerializeField] private float AccuracyBase = 0.95f;//IE (1-angle/deviation) on first fire
        [SerializeField] private float AccuracryFiring = 0.9f;//IE accuracy when firing on automatic.
        [SerializeField] private float Weight = 5f;
        [SerializeField] private float RecoilForce = 10f;
        [SerializeField] private float DrawTime = 1f;
        [SerializeField] private float ReloadTime = 2.5f;
        [SerializeField] private float NoiseLevel = 10f;
        [SerializeField] private float AcceptedMagCaliber = 7.62f;//IE 7.62x51 NATO
        [SerializeField] private float MagSize = 25f; //mag stuff might come from a Magazine class
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
        private float bulletsRemaining = 25f;

        // Start is called before the first frame update
        void Start()
        {
            CalcFireRate();
        }

        // Update is called once per frame
        void Update()
        {
            if (nexttimetofire > 0)
            {
                nexttimetofire -= Time.deltaTime;
            }
            if (Input.GetKeyDown(KeyCode.Mouse0) && bulletsRemaining > 0)//&& nexttimetofire <= 0f
            {
                //Debug.Log("");
                FireGun(FireMode);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ReloadGun();
            }
        }

        public void FireGun(int mode)
        {

            if (mode == 0)
            {
                //fire as fast as mouse can be clicked
                bulletsRemaining--;
                //place the muzzleEffectOnShot stack
                for (int me = 0; me < MuzzleEffectOnShot.Length; me++)
                {
                    //play or place the effects
                }

                Speaker.clip = ShotSound;
                Speaker.Play(0);
                playerRB.AddForce(Vector3.back * RecoilForce);

                //create a motion vector for the bullet based off the gun accuracy, the speed
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

                //place muzzle effect after shot
                for (int mep = 0; mep < MuzzleEffectAfterShot.Length; mep++)
                {

                }
            }
            else if (mode == 1)
            {
                //fire bursts at normal weap speed
                //multiple bursts cannot be fired at once
                if (nexttimetofire <= 0f)
                {

                }
            }
            else if (mode == 2)
            {
                //fire continuously until ammo or mouse up at norm weap speed
                if (nexttimetofire <= 0f)
                {

                }
            }
            if (nexttimetofire <= 0)
            {
                CalcFireRate();
            }
        }

        public void ReloadGun()
        {
            bulletsRemaining = MagSize;
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
        }

    }
}
