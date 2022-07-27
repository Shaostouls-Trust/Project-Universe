using UnityEngine;
using System.Collections;

public class EMUI_Animations : MonoBehaviour {

	public void ToggleFadeIn() {
		Animator animator = GetComponent<Animator>();
		bool condition = (animator.GetBool ("fadeIn") == true) ? false : true;
		animator.SetBool("fadeIn", condition);
	}
}
