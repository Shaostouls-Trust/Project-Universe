using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VolumeGlobalAtmosphereController : MonoBehaviour
{
    private float roomPressure;
    public float roomTemp;
    public float roomOxygenation;
    public float humidity;
    public float toxicity;
    public string[] globalGases;
    private List<GameObject> exposedNodes = new List<GameObject>();

    public void SetExposedNodes(List<GameObject> newNodes)
    {
        exposedNodes = newNodes;
    }

    public List<GameObject> GetExposedNodes()
    {
        return exposedNodes;
    }

    public void NullifyNodeLink(List<GameObject> nodesToNullify)
    {
        foreach(GameObject node in nodesToNullify)
        {
            node.GetComponent<VolumeNode>().SetGlobalVolume(null);
        }
    }
    public void NullifyNodeLink(GameObject nodeToNullify)
    {
        if (exposedNodes.Contains(nodeToNullify))
        {
            nodeToNullify.GetComponent<VolumeNode>().SetGlobalVolume(null);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //other.GetComponent<PlayerVolumeController>().OnVolumeEnter(roomPressure, roomTemp, roomOxygenation);
            PlayerVolumeController player = other.GetComponent<PlayerVolumeController>();
            player.OnVolumeEnter(roomPressure, roomTemp, roomOxygenation);
            player.SetPlayerVolume(this.GetComponents<Volume>());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("_VolumeNode"))
        {
            //Add to list to compare. Whatever exists in VAC is removed from VGAC
            if (!exposedNodes.Contains(other.gameObject))
            {
                //Debug.Log(this.name + " detecting VolumeNode: " + other.gameObject.name);
                exposedNodes.Add(other.gameObject);
                other.GetComponent<VolumeNode>().SetGlobalVolume(this.gameObject);
            }
        }
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerVolumeController player = other.GetComponent<PlayerVolumeController>();
            player.OnVolumeEnter(roomPressure, roomTemp, roomOxygenation);
            player.SetPlayerVolume(this.GetComponents<Volume>());
        }
    }
    public float GetPressure()
    {
        return roomPressure;
    }
    public void SetPressure(float value)
    {
        roomPressure = value;
    }
}
