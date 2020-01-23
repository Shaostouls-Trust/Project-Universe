using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemClass : MonoBehaviour
{
    // ###############################################--- DEFINE ENUMS ---###############################################
    public enum ItemType
    {
        Helmet, Chestplate, RightShoulder, LeftShoulder, RightBracer, LeftBracer, RightGlove, LeftGlove, Belt, BackPack, Tasset, RightThigh, LeftThigh, RightShin, LeftShin, RightBoot, LeftBoot, ArmorAttachment,
        Weapon, WeaponAttachment, Tool, Consumeable, Resource, Componenet
    };
    public enum AttachmentSlot { NONE, Helmet, ChestPlate, Shoulder, Bracer, Glove, Belt, BackPack, Tasset, Thigh, Shin, Boot, Stock, Bolt, Barrel, Muzzle, Scope, Rail, Grip, ChargeHandle, Bayonnet };
    public enum DamageType { NONE, Kinetic, Heat, Cold, Acid, Blast, Electricity, Void, AntiMatter, Other }
    public enum WeaponType { NONE, HandGun,HandCannon, SubMachineGun,Rifle,Shotgun,SniperRifle,HeavyMachineGun,RocketLauncher,GrenadeLauncher,Sword,Staff,Knuckler, Bow, CrossBow};
    public enum AmmoType
    {
        NONE, SmallBattery, LargeBattery, LightAmmo, Ammo, MediumAmmo, HeavyAmmo, Other
    };
    // ###############################################--- END ENUMS ---###############################################

    // ###############################################--- DEFINE CLASSES ---###############################################
    [System.Serializable]
    public class Resistances
    {
        public float Kinetic;
        public float Heat;
        public float Cold;
        public float Acid;
        public float Blast;
        public float Electricity;
        public float Void;
        public float AntiMatter;
        public float Vacuum;
        public float Other;
    }
    [System.Serializable]
    public class Stats
    {
        public WeaponType weaponType;
        public float minDamage, maxDamage;
        public float criticalHitMultiplier;
        public float fireRate;
        public float range;
        public float stability;
        public float reloadSpeed;
        public float magazineSize;
        public float maxAmmo;
        public float ammoCost;
        public AmmoType ammoType;
        public DamageType damageType;

    }
    // ###############################################--- END CLASSES ---###############################################
    public UIItem Icon;
    public InventoryManager inventory;
    public GameObject DisplayMesh;
    public Material DefaultMaterial;
    public GameObject GameMesh;
    public string ItemName;
    public string ItemAuthor;
    public Vector2 Durability = new Vector2(100, 100);
    public string Description;
    public string FlavorText;
    public ItemType itemType;
    public AttachmentSlot attachmentSlot;
    public Resistances resistances;
    public Stats stats;


    // Start is called before the first frame update
    void Start()
    {
        if (GameMesh != null)
        {
           foreach (Transform model in transform)
            {
                if (model.name == "GUIMODEL")
                {
                    DisplayMesh = model.gameObject;
                }
            }
        }
    }

    void AddItemToInventory()
    {
        gameObject.transform.parent = inventory.inventoryContainer.transform;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            inventory = collision.gameObject.GetComponent<SimplePlayerController>().inventory;
            inventory.processed = false;
            AddItemToInventory();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
