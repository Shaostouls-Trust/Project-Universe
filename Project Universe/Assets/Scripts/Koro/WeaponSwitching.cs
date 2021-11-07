
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class WeaponSwitching : NetworkBehaviour
{
    public int selectedWeapon = 0;
    private NetworkVariableInt netSelectedWeapon = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone } ,0);

    void Start()
    {
        SelectWeaponServerRpc();
    }

    void Update()
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
            /*
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                selectedWeapon = 0;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2)
            {
                selectedWeapon = 1;
            }
             */
            if (previousSelectedWeapons != selectedWeapon)
            {
                SelectWeaponServerRpc();
            }
        }
       
    }

    
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
