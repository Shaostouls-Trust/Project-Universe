using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



using AX;

public class GrayShipDesigner : AXRuntimeControllerBase
{



	// RUNTIME PARAMETER REFERENCES
	// These are references to parameters
	// in the node graph that have been designated runtime parameters

	private AXParameter P_EngineSize;
	private AXParameter P_Channel;
	private AXParameter P_HullShape;
	private AXParameter P_WeaponSize;


	// INTERNAL VALUES

	// Player - Sadly, the player can't increase her fortunes in this demo.
	public float 		account 	= 352000;

	// General
	private string 		shipsName 		= "AXX Grayship";

	// Engine
	private int 		engineType 	= 1;
	private float 		engineSize 	= 1;

	// Hull
	private float 		hullArea 	= 245000;
	public float 		hullAreaFactor = 2223;
	// WEAPONS
	private float 		weaponSize 	= 1;


	// FINAL SHIP SPECS 
	private float 		mass;
	private float 		crew;
	private float 		speed;
	private float 		cost;



	// HEADER UI
	public UnityEngine.UI.Text 			shopTitleText;
	public UnityEngine.UI.Text 			playerAccountText;

	// GENERAL
	public UnityEngine.UI.InputField 	shipNameText;

	// HULL UI
	public UnityEngine.UI.Text 			areaValueLabel;

	// ENGINE UI
	public UnityEngine.UI.Slider 		radiusSlider;
	public UnityEngine.UI.Dropdown 		engineTypeDropdown;

	// WEAPON UI
	public UnityEngine.UI.Slider 		bigGunsSlider;


	// FINAL SPECS UI
	public UnityEngine.UI.Text 			massValueText;
	public UnityEngine.UI.Text 			crewValueText;
	public UnityEngine.UI.Text 			speedValueText;
	public UnityEngine.UI.Text 			costValueText;



	// SHIP'S NAMEPLATES
	public GameObject 					nameplatePort;
	public GameObject 					nameplateStarboard;
	public GameObject 					nameplateBow;

	private UnityEngine.UI.Text m_TextStarboard;
	private UnityEngine.UI.Text m_TextPort;
	private UnityEngine.UI.Text m_TextBow;


	// Use this for initialization
	void Start ()
	{

		// Establish refernces to AXModel parameters.


		// EXPOSED PARAMETERS

		if (model != null) {
			P_Channel = model.getParameter ("EngineSwitch.Channel");
			if (P_Channel != null)
				P_Channel.WasAltered += OnEngineChannelChanged;


			P_EngineSize = model.getParameter ("Engine.radius");
			if (P_EngineSize != null)
				P_EngineSize.WasAltered += OnEngineSizeChanged;


			P_HullShape = model.getParameter ("HullShape");
			if (P_HullShape != null)
				P_HullShape.WasAltered += OnHullChanged;


			P_WeaponSize = model.getParameter ("BigGuns.radius");
			if (P_WeaponSize != null)
				P_WeaponSize.WasAltered += OnWeaponsChanged;


		}


		// UI CALLBACKS FOR RUNTIME PARAMETERS 
		// Bind UI elements to local methods (this could also be done in the Inspector for each UI item. 

		if (shipNameText != null)
		{
			shipNameText.onEndEdit.AddListener (
				delegate {
					shipNameUpdate ();
				}
			);
		}

		if (engineTypeDropdown != null)
		{
			engineTypeDropdown.onValueChanged.AddListener (
				delegate {
					engineTypeDropdownUpdate ();
				}
			);
		}
		 
		if (radiusSlider != null)
		{
			radiusSlider.onValueChanged.AddListener (
				delegate {
					radiusSliderUpdate ();
				}
			);
		}

		if (bigGunsSlider != null)
		{
			bigGunsSlider.onValueChanged.AddListener (
				delegate {
					bigGunsSliderUpdate ();
				}
			);
		}
		

		//Initialize AXModel parameters based on UI elements.

		if (P_EngineSize != null)
			P_EngineSize.	initiatePARAMETER_Ripple_setFloatValueFromGUIChange 	(radiusSlider.value);

		if (P_Channel != null)
			P_Channel.		initiateRipple_setIntValueFromGUIChange 		(engineTypeDropdown.value);

		if (P_WeaponSize != null)
			P_WeaponSize.	initiatePARAMETER_Ripple_setFloatValueFromGUIChange 	(bigGunsSlider.value);



	

		// GENERATE THE AX_MODEL.
		model.autobuild ();


		// CALCULATE VALUES FOR THE UI DISPLAY
		recalculate ();
	}


	void OnEnable ()
	{
		model.autobuild ();
		recalculate ();
	}

	void OnDisable ()
	{
		
	}

	void Update()
	{

		if (Input.GetMouseButtonUp(0))
		{
			//model.autobuild ();
		}
	}

	public void processFloat (AXParameter p, float value)
	{
		p.initiatePARAMETER_Ripple_setFloatValueFromGUIChange (value);
		model.autobuild ();
	}





	// Handle UI Activity
	// Set the value of the parameter and let this change ripple through the network of Relations
	// Then let the model know to rebuild GameObjects using isAltered() or autobuild().
	public void shipNameUpdate ()
	{
		shipsName = shipNameText.text;
	}

	public void radiusSliderUpdate ()
	{
		P_EngineSize.initiatePARAMETER_Ripple_setFloatValueFromGUIChange (radiusSlider.value);
		model.isAltered();
		//model.autobuild ();
	}
	 
	public void buildmodel ()
	{
		model.autobuild ();
	}

	public void engineTypeDropdownUpdate ()
	{
		P_Channel.initiateRipple_setIntValueFromGUIChange (engineTypeDropdown.value);
		model.autobuild ();
	}

	public void bigGunsSliderUpdate ()
	{
		P_WeaponSize.initiatePARAMETER_Ripple_setFloatValueFromGUIChange (bigGunsSlider.value);
		model.isAltered();
		//model.autobuild ();
	}


	public void launchStoreSite ()
	{
		Application.OpenURL ("http://u3d.as/qYW");
	}






	// CALLBACKS from changes initiated in UI, Runtime Handles, or other runtime logic.

	public void OnEngineChannelChanged ()
	{
		// When the engine type is changed, reset the size to the minimum.

		engineSize 			= 1;
		radiusSlider.value 	= 1;

		recalculate ();
	}

	public void OnEngineSizeChanged ()
	{
		recalculate ();
	}

	public void OnHullChanged ()
	{
		hullArea = P_HullShape.area;
		recalculate ();
	}

	public void OnWeaponsChanged ()
	{		
		recalculate ();
	}






	/// <summary>
	///  Game Specific Calculations
	///
	///  The mass of the ship is a function of 
	///    1. the area of the hull
	///    2. the size of the engines
	///    3. the size of the weapons
	///	 The count of crewmen is simply a function of the hullArea
	///  Top speed is a logarithmic funtion of the engine type, engine size and mass of the ship.
	/// </summary>
	
	void recalculate ()
	{

		engineType = engineTypeDropdown.value + 1;
		engineSize = radiusSlider.value;
		weaponSize = bigGunsSlider.value;

		if (P_HullShape != null)
			hullArea = P_HullShape.area;

		// Mass
		mass = hullArea * 2234 + engineSize * 1555 + weaponSize * 5000;

		// Crew
		crew = hullArea / 5;

		// Speed
		float factor = (Mathf.Pow (10, engineType) * engineSize * 1500000) / mass;
		speed = Mathf.Log (factor, 3);

		// Cost
		cost = ((Mathf.Pow (2, (engineType * 2)) * engineSize * 10000) + mass * 3 + weaponSize * 10000) / 2;


		// UPDATE UI


		if (areaValueLabel 		!= null)
			areaValueLabel.text  = hullArea.ToString 		("0,0.0") 	+ " sq.m";

		// TEXT LABELS ON THE SHIP
		if (nameplateStarboard != null) {
			m_TextStarboard = nameplateStarboard.GetComponent<UnityEngine.UI.Text> ();
			m_TextStarboard.text = shipsName;
			Debug.Log("m_TextStarboard.text="+m_TextStarboard.text);
		}
		if (nameplatePort != null) {
			m_TextPort = nameplatePort.GetComponent<UnityEngine.UI.Text> ();
			m_TextPort.text = shipsName;
		}
		if (nameplateBow != null) {
			m_TextBow = nameplateBow.GetComponent<UnityEngine.UI.Text> ();
			m_TextBow.text = shipsName;
		}

		// UPDATE THE SPECS UI WITH THESE VALUES

		if (playerAccountText 	!= null)
			playerAccountText.text 	= account.ToString 		("0,0") 	+ " imp.cr";

		if (massValueText 		!= null)
			massValueText.text 	 = mass.ToString 			("0,0") 	+ " m.tons";

		if (crewValueText 		!= null)
			crewValueText.text 	 = crew.ToString 			("0,0");

		if (speedValueText 		!= null)
			speedValueText.text  = "Warp " + speed.ToString ("0.0");

		if (costValueText 		!= null)
			costValueText.text 	 = cost.ToString 			("0,0") 	+ " imp.cr";


		
	}


}
