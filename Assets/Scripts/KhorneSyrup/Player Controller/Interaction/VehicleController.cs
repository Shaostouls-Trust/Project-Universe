using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour, IInteractable
{
    [SerializeField] private Camera vehicleCamera;
    [SerializeField] private MonoBehaviour CarController;

    private void Awake()
    {
       // CarController = gameObject.GetComponent<UnityStandardAssets.Vehicles.Car.CarUserControl>();
    }

    public void Interact()
    {
        CarController.enabled = true;
        vehicleCamera.enabled = true;
    }

    public void RecieveDamage(float damage)
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
