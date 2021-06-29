
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public int selectedWeapon = 0;
    void Start()
    {
        SelectWeapon();
    }

    void Update()
    {
        int previousSelectedWeapons = selectedWeapon;

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            //Debug.Log("scroll");
            if (selectedWeapon >= transform.childCount - 1)
                selectedWeapon = 0;
            selectedWeapon++;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (selectedWeapon <= 0)
                selectedWeapon = transform.childCount - 1;
            selectedWeapon--;
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
            SelectWeapon();
        }
       
    }
    void SelectWeapon()
    {
        int i = 0;
        foreach(Transform weapon in transform)
        {
            if (i == selectedWeapon)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);
            
            i++;
        }
    }
}
