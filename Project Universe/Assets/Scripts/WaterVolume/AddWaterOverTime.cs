using UnityEngine;
namespace B
{
    public class AddWaterOverTime : MonoBehaviour
    {
        [SerializeField] private VolumeWaterData vwd;
        public float rate;

        // Update is called once per frame
        void Update()
        {
            //vwd.SetWaterVolume(vwd.GetWaterVolume() + rate * Time.deltaTime);
        }
    }
}