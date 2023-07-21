
using Unity.Netcode;
using ProjectUniverse.Player.PlayerController;
using UnityEngine;

public class WeaponSwitching : NetworkBehaviour
{
    private ProjectUniverse.PlayerControls controls;
    public int selectedWeapon = 0;
    private NetworkVariable<int> netSelectedWeapon = new NetworkVariable<int>(0);//new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone } ,

    private void OnEnable()
    {
        try
        {
            controls.Player.ScrollWheel.Enable();
        }
        catch (System.NullReferenceException)
        {
            Debug.Log("Controls not initialized.");
        }
    }
    private void OnDisable()
    {
        controls.Player.ScrollWheel.Disable();
    }

    void Start()
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
        {
            controls = networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>().PlayerController;
        }
        else
        {
            controls = new ProjectUniverse.PlayerControls();
        }
        controls.Player.ScrollWheel.Enable();
        controls.Player.ScrollWheel.performed += ctx =>
        {
            if (IsLocalPlayer)
            {
                Vector2 axisdelt = ctx.ReadValue<Vector2>();
                if (axisdelt.y < 0f)//
                {
                    if (selectedWeapon >= transform.childCount - 1)
                    {
                        //selectedWeapon = 0;
                        netSelectedWeapon.Value = 0;
                    }
                    netSelectedWeapon.Value++;
                    selectedWeapon = netSelectedWeapon.Value;
                }
                else if (axisdelt.y > 0f)
                {
                    if (selectedWeapon <= 0)
                    {
                        //selectedWeapon = transform.childCount - 1;
                        netSelectedWeapon.Value = transform.childCount - 1;
                    }
                    netSelectedWeapon.Value--;
                    selectedWeapon = netSelectedWeapon.Value;
                }
                SelectWeaponServerRpc();
            }
        };
        SelectWeaponServerRpc();
    }

    /* void Update()
     {
         //does not show other players what you have equiped
         if (IsLocalPlayer)
         {
             int previousSelectedWeapons = selectedWeapon;

             if (Input.GetAxis("Mouse ScrollWheel") < 0f)
             {
                 if (selectedWeapon >= transform.childCount - 1)
                 {
                     //selectedWeapon = 0;
                     netSelectedWeapon.Value = 0;
                 }
                 netSelectedWeapon.Value++;
                 selectedWeapon = netSelectedWeapon.Value;
             }
             if (Input.GetAxis("Mouse ScrollWheel") > 0f)
             {
                 if (selectedWeapon <= 0)
                 {
                     //selectedWeapon = transform.childCount - 1;
                     netSelectedWeapon.Value = transform.childCount - 1;
                 }
                 netSelectedWeapon.Value--;
                 selectedWeapon = netSelectedWeapon.Value;
             }

             if (previousSelectedWeapons != selectedWeapon)
             {
                 SelectWeaponServerRpc();
             }
         }
     }*/


    [ServerRpc(RequireOwnership = false)]
    public void SelectWeaponServerRpc()
    {
        SelectWeaponClientRpc();
    }

    [ClientRpc]
    void SelectWeaponClientRpc()
    {
        int i = 0;
        foreach(Transform weapon in transform)
        {
            if (i == netSelectedWeapon.Value)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);
            
            i++;
        }
    }
}
