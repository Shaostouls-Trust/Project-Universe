using ProjectUniverse.PowerSystem;
using ProjectUniverse.PowerSystem.Nuclear;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class LeverSwitchController : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsWithStatesToToggle;
    [SerializeField] private PlayableDirector director;
    public AudioClip stateOn;//main power
    public AudioClip stateOff;//Aux power
    public AudioSource src;
    public AudioSource powerStateSrc;
    public AudioClip stateOnBGM;//main power ambient
    public AudioClip stateOffBGM;//aux power ambient
    [SerializeField] private bool state;
    [SerializeField] private Light auxLight;
    [SerializeField] private Light mainLight;
    [SerializeField] private Image auximg;
    [SerializeField] private Image mainimg;
    [SerializeField] private Color auxOn;
    [SerializeField] private Color auxOff;
    [SerializeField] private Color mainOn;
    [SerializeField] private Color mainOff;
    [SerializeField] private IBreakerBox[] lightBreakers;
    [SerializeField] private SteamTurbine[] turbines;
    [SerializeField] private IRouter[] routers;

    private void Start()
    {
        if (!state)
        {
            auxLight.enabled = true;
            mainLight.enabled = false;
            auximg.color = auxOn;
            mainimg.color = mainOff;
            powerStateSrc.clip = stateOffBGM;
            powerStateSrc.Play();
            //disable all lights
            for(int i = 0; i < lightBreakers.Length; i++)
            {
                lightBreakers[i].RunMachine = false;
            }
        }
        else
        {
            auxLight.enabled = false;
            mainLight.enabled = true;
            auximg.color = auxOff;
            mainimg.color = mainOn;
            powerStateSrc.clip = stateOnBGM;
            powerStateSrc.Play();
        }
    }

    public void ExternalInteractFunc(int param)
    {
        if (!director.state.Equals(PlayState.Playing))
        {
            director.Play();
        }
    }

    private void Update()
    {
        if (turbines[0].PowerOutput != 0f || turbines[1].PowerOutput != 0f || turbines[2].PowerOutput != 0f || turbines[3].PowerOutput != 0f)
        {
            if(powerStateSrc.clip != stateOnBGM)
            {
                powerStateSrc.clip = stateOnBGM;
                powerStateSrc.Play();
            }
        }
        else
        {
            if (powerStateSrc.clip != stateOffBGM)
            {
                powerStateSrc.clip = stateOffBGM;
                powerStateSrc.Play();
            }
        }
    }

    //called in the animation clip
    public void animEventTrigger()
    {
        //toggle the states of the referenced objects
        state = !state;
        if (state)
        {
            src.Stop();
            src.PlayOneShot(stateOn);
            if (turbines[0].PowerOutput != 0f || turbines[1].PowerOutput != 0f
                || turbines[2].PowerOutput != 0f|| turbines[3].PowerOutput != 0f)
            {
                powerStateSrc.clip = stateOnBGM;
                powerStateSrc.Play();
            }
            
            auxLight.enabled = false;
            mainLight.enabled = true;
            auximg.color = auxOff;
            mainimg.color = mainOn;
            powerStateSrc.clip = stateOnBGM;
            //enable all lights
            for (int i = 0; i < lightBreakers.Length; i++)
            {
                lightBreakers[i].RunMachine = true;
            }
            turbines[0].outputToRouter = true;
            turbines[1].outputToRouter = true;
            turbines[2].outputToRouter = true;
            turbines[3].outputToRouter = true;
            routers[0].UseGeneratorPower = false;
            routers[1].UseGeneratorPower = false;
        }
        else
        {
            src.Stop();
            src.PlayOneShot(stateOff);
            powerStateSrc.clip = stateOffBGM;
            powerStateSrc.Play();
            auxLight.enabled = true;
            mainLight.enabled = false;
            auximg.color = auxOn;
            mainimg.color = mainOff;
            powerStateSrc.clip = stateOffBGM;
            //disable all lights
            for (int i = 0; i < lightBreakers.Length; i++)
            {
                lightBreakers[i].RunMachine = false;
            }
            turbines[0].outputToRouter = false;
            turbines[1].outputToRouter = false;
            turbines[2].outputToRouter = false;
            turbines[3].outputToRouter = false;
            routers[0].UseGeneratorPower = true;
            routers[1].UseGeneratorPower = true;
        }
    }
}
