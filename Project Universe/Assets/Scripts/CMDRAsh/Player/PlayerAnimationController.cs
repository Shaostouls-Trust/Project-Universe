using CMF;
using ProjectUniverse.Player.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Animation.Controllers
{
    [RequireComponent(typeof(SupplementalController))]
    //[RequireComponent(typeof(Mover))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator bodyAnimator;
        [SerializeField] private GameObject camHeadTracker;
        private SupplementalController sc;
        //private Mover moverComp;

        void Start()
        {
            sc = GetComponent<SupplementalController>();
            //moverComp = GetComponent<Mover>();
        }

        /// <summary>
        /// Play the crouch animation, adjust player height in Advanced Walker, set animstate variables
        /// </summary>
        public void OnPlayerCrouch()
        {
            if (sc.prone)
            {
                //from prone to crouch
                //bodyAnimator.SetBool("ExitToStand", false);
                //bodyAnimator.SetBool("ExitToProne", false);
                //bodyAnimator.SetBool("ExitToCrouch", false);
                //bodyAnimator.SetBool("Crouched", true);
                //bodyAnimator.SetBool("Proned", false);
                //bodyAnimator.Play("CrouchFromProne");
                sc.crouching = true;
                sc.prone = false;
                //GetComponent<AdvancedWalkerController>().movementSpeed = sc.CrouchSpeed;
                Debug.Log("setHeight Crouch");
                camHeadTracker.GetComponent<PoVCamHeadTracker>().UpdateHeight(sc.CrouchHeight);
            }
            else
            {
                //from stand to crouch
                //bodyAnimator.SetBool("ExitToStand", false);
                //bodyAnimator.SetBool("ExitToProne", false);
                //bodyAnimator.SetBool("ExitToCrouch", false);
                //bodyAnimator.SetBool("Crouched", true);
                //bodyAnimator.SetBool("Proned", false);
                //bodyAnimator.Play("Crouch");
                Debug.Log("setHeight Crouch");
                //GetComponent<AdvancedWalkerController>().movementSpeed = sc.CrouchSpeed;
                sc.crouching = true;
                sc.prone = false;
                camHeadTracker.GetComponent<PoVCamHeadTracker>().UpdateHeight(sc.CrouchHeight);
            }
            
            //moverComp.SetStepHeightRatio();
        }

        public void OnPlayerProne()
        {
            if (sc.crouching)
            {
                //from crouch to prone
                //bodyAnimator.SetBool("ExitToStand", false);
                //bodyAnimator.SetBool("ExitToProne", false);
                //bodyAnimator.SetBool("ExitToCrouch", false);
                //bodyAnimator.SetBool("Crouched", false);
                //bodyAnimator.SetBool("Proned", true);
                //bodyAnimator.Play("ProneFromCrouch");
                sc.crouching = false;
                sc.prone = true;
                Debug.Log("setHeight Prone");
                //GetComponent<AdvancedWalkerController>().movementSpeed = sc.ProneSpeed;
                camHeadTracker.GetComponent<PoVCamHeadTracker>().UpdateHeight(0.45f);//sc.ProneHeight
            }
            else
            {
                //from stand to prone
                //bodyAnimator.SetBool("ExitToStand", false);
                //bodyAnimator.SetBool("ExitToProne", false);
                //bodyAnimator.SetBool("ExitToCrouch", false);
                //bodyAnimator.SetBool("Crouched", false);
                //bodyAnimator.SetBool("Proned", true);
                //bodyAnimator.Play("Prone");
                //GetComponent<AdvancedWalkerController>().movementSpeed = sc.ProneSpeed;
                sc.crouching = false;
                sc.prone = true;
                Debug.Log("setHeight Prone");
                camHeadTracker.GetComponent<PoVCamHeadTracker>().UpdateHeight(0.45f);
            }
            
        }

        public void OnExitProne()
        {
            /*
            if (sc.crouching)
            {
                //Prone to crouch
                bodyAnimator.SetBool("ExitToProne", true);
                bodyAnimator.SetBool("ExitToStand", false);
                bodyAnimator.Play("ProneToCrouch");
                //should be turned off by an animation event
                bodyAnimator.SetBool("Crouched", true);
                bodyAnimator.SetBool("Proned", false);
                Debug.Log("setHeight Crouch");
                moverComp.SetColliderHeight(sc.CrouchHeight);
                camHeadTracker.GetComponent<PoVCamHeadTracker>().UpdateHeight(sc.CrouchHeight);
            }
            else*/
            //{

                //prone to stand
                //bodyAnimator.SetBool("ExitToStand", true);
                //bodyAnimator.SetBool("ExitToProne", false);
                //bodyAnimator.SetBool("ExitToCrouch", false);
                //bodyAnimator.Play("StandFromProne");
            //should be turned off by an animation event
            //GetComponent<AdvancedWalkerController>().movementSpeed = sc.WalkSpeed;
            //bodyAnimator.SetBool("Crouched", false);
                //bodyAnimator.SetBool("Proned", false);
            sc.crouching = false;
            sc.prone = false;
            Debug.Log("setHeight Stand");
                camHeadTracker.GetComponent<PoVCamHeadTracker>().UpdateHeight(sc.StandHeight);
            //}
        }

        public void OnExitCrouch()
        {
            /*
            if (sc.prone)
            {
                //crouch to prone
                bodyAnimator.SetBool("ExitToProne", true);
                bodyAnimator.SetBool("ExitToStand", false);
                bodyAnimator.Play("CrouchToProne");
                //should be turned off by an animation event
                bodyAnimator.SetBool("Crouched", false);
                bodyAnimator.SetBool("Proned", true);
                Debug.Log("setHeight Prone");
                moverComp.SetColliderHeight(sc.ProneHeight);
                camHeadTracker.GetComponent<PoVCamHeadTracker>().UpdateHeight(sc.ProneHeight);
            }
            else
            {*/
                //crouch to stand
                //bodyAnimator.SetBool("ExitToStand", true);
                //bodyAnimator.SetBool("ExitToProne", false);
            //bodyAnimator.SetBool("ExitToCrouch", false);
            //bodyAnimator.Play("StandFromCrouch");
            //should be turned off by an animation event
            //GetComponent<AdvancedWalkerController>().movementSpeed = sc.WalkSpeed;
            //bodyAnimator.SetBool("Crouched", false);
                //bodyAnimator.SetBool("Proned", false);
            sc.crouching = false;
            sc.prone = false;
            Debug.Log("setHeight Stand");
                camHeadTracker.GetComponent<PoVCamHeadTracker>().UpdateHeight(sc.StandHeight);
            //}

        }

        /// <summary>
        /// Called at the end of crouch/stand animations. NYI.
        /// </summary>
        public void ResetCrouchStates()
        {
            bodyAnimator.SetBool("Crouched", false);
            bodyAnimator.SetBool("ExitToStand", false);
        }
    }
}
