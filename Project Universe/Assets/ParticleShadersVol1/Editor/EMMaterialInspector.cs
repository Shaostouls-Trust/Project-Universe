using System;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEditor
{
	internal class EMMaterialInspector : ShaderGUI
	{
		public enum LightingModes {MultiLightLit, MultiLightEmissive, Lit, LitEmissive, Unlit}
		public enum AlphaModes {Fade, Erosion}
		public enum BlendModes {AlphaBlend, Additive, AdditiveSoft} //only for Unlit shader

		private MaterialEditor m_MaterialEditor;

		//Popup properties
		private MaterialProperty m_LightingMode;
		private MaterialProperty m_BlendMode;
		private MaterialProperty m_AlphaMode;

		//Optional properties
		private static class Properties
		{
			public static MaterialProperty m_MainTexture;
			public static MaterialProperty m_HDRMultiplier;
			public static MaterialProperty m_TintColor;
			public static MaterialProperty m_Cutoff;

			public static MaterialProperty m_Thickness;
			public static MaterialProperty m_AlphaTransmitance;
			public static MaterialProperty m_AlphaContrast;

			public static MaterialProperty m_InvFade;
			public static MaterialProperty m_DistanceFadeStart;
			public static MaterialProperty m_DistanceFadeEnd;
		}

		private bool m_AlphaTransmitanceEnabled;
		private bool m_SoftParticlesEnabled;
		private bool m_DistanceFadeEnabled;

		private class Keywords
		{
			public const string AlphaTransmitance = "ALPHATRANSMITANCE_ON";
			public const string SoftParticles = "SOFTPARTICLE_ON";
			public const string DistanceFade = "DISTANCEFADE_ON";
			public const string AlphaErosion = "ALPHAEROSION_ON";
			public const string Emission = "EMISSION_ON";
			public const string Additive = "ADDITIVE_ON";
		}

		private static class Styles
		{
			public static readonly string[] m_LightingNames = Enum.GetNames (typeof (LightingModes));
			public static readonly string[] m_BlendNames = Enum.GetNames (typeof (BlendModes));
			public static readonly string[] m_AlphaNames = Enum.GetNames (typeof (AlphaModes));
			public static string m_LightingPopup = "Lighting Mode";
			public static string m_BlendPopup = "Blend Mode";
			public static string m_AlphaPopup = "Alpha Mode";

			public static string HeaderMain = "Main Settings";
			public static string HeaderOptional = "Additional Settings";

			public static GUIContent m_MainTextureText = new GUIContent ("Main Texture", "Diffuse color (RGB) and Transparency (A)");
			public static GUIContent m_HDRMultiplierText = new GUIContent ("HDR Multiplier", "Emissive color HDR multiplier");
			public static GUIContent m_CutoffText = new GUIContent ("Shadow Cutoff", "How much Transparency affects shadow visibility");
			public static GUIContent m_ThicknessText = new GUIContent ("Thickness", "Light transmittance of particle");
			public static GUIContent m_SoftParticlesToggle = new GUIContent ("Soft Particles");
			public static GUIContent m_AlphaTransmitanceToggle = new GUIContent ("Alpha Channel Light Transmission");
			public static GUIContent m_AlphaTransmitanceText = new GUIContent ("Alpha Influence", "Alpha channel influence on back light transmission");
			public static GUIContent m_AlphaContrastText = new GUIContent ("Alpha Contrast", "Alpha channel contrast");
			public static GUIContent m_InvFadeText = new GUIContent ("Soft Particles Factor", "Soft Particles strength factor");
			public static GUIContent m_DistanceFadeToggle = new GUIContent ("Distance Fade");
			public static GUIContent m_DistanceFadeStartText = new GUIContent ("Fade Start", "Startng fade distance from Camera to particle. Negative values are supported");
			public static GUIContent m_DistanceFadeEndText = new GUIContent ("Fade End", "Ending fade distance from Camera to particle");

		}

		public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
		{

			m_MaterialEditor = materialEditor;

			Material material = materialEditor.target as Material;

			FindProperties (material, props);
			ShaderPropertiesGUI (material);
		}
			


		public void FindProperties (Material material, MaterialProperty[] props)
		{
			m_LightingMode 	= FindProperty ("_LightingMode", props, false);
			m_BlendMode 	= FindProperty ("_BlendMode", props, false);
			m_AlphaMode 	= FindProperty ("_AlphaMode", props, false);

			Properties.m_MainTexture 	= FindProperty ("_MainTex", props);
			Properties.m_HDRMultiplier	= FindProperty ("_HDRMultiplier", props, false);
			Properties.m_TintColor 		= FindProperty ("_TintColor", props);
			Properties.m_Cutoff 		= FindProperty ("_Cutoff", props, false);
			Properties.m_Thickness	 	= FindProperty ("_Thickness", props, false);
			Properties.m_AlphaTransmitance = FindProperty ("_AlphaInfluence", props, false);
			Properties.m_AlphaContrast = FindProperty ("_AlphaContrast", props, false);
			Properties.m_InvFade 			= FindProperty ("_InvFade", props, false);
			Properties.m_DistanceFadeStart = FindProperty ("_FadeStart", props, false);
			Properties.m_DistanceFadeEnd   = FindProperty ("_FadeEnd", props, false);

			//returns false also when keyword does not exist
			m_AlphaTransmitanceEnabled = material.IsKeywordEnabled ("ALPHATRANSMITANCE_ON") ? true : false;
			m_SoftParticlesEnabled = material.IsKeywordEnabled ("SOFTPARTICLE_ON") ? true : false;
			m_DistanceFadeEnabled  = material.IsKeywordEnabled ("DISTANCEFADE_ON") ? true : false;
		}

		public void ShaderPropertiesGUI (Material material)
		{
			EditorGUIUtility.labelWidth = 0f;

			EditorGUI.BeginChangeCheck();
			{
				GUILayout.Label(Styles.HeaderMain, EditorStyles.boldLabel);
				LightingModePopup(material);
				AlphaModePopup(material);
				BlendModePopup(material);

				EditorGUILayout.Space();
				EditorGUILayout.Space();
				m_MaterialEditor.TexturePropertySingleLine(Styles.m_MainTextureText, Properties.m_MainTexture, Properties.m_TintColor);
				m_MaterialEditor.TextureScaleOffsetProperty (Properties.m_MainTexture);


				AdditionalSettings(material);
			}

		}

		private void LightingModePopup(Material material)
		{
			EditorGUI.showMixedValue = m_LightingMode.hasMixedValue;
			var lightingMode = (LightingModes)m_LightingMode.floatValue;
		
			EditorGUI.BeginChangeCheck ();
			lightingMode = (LightingModes)EditorGUILayout.Popup (Styles.m_LightingPopup, (int)lightingMode, Styles.m_LightingNames);
			if (EditorGUI.EndChangeCheck ()) {
				m_MaterialEditor.RegisterPropertyChangeUndo ("Lighting Mode");
				m_LightingMode.floatValue = (float)lightingMode;
			}
		
			EditorGUI.showMixedValue = false;

			switch ((int)m_LightingMode.floatValue) {
			case (int)LightingModes.MultiLightLit:
				material.shader = Shader.Find ("Ethical Motion/Particles/Lit MultiLight");
				material.DisableKeyword (Keywords.Emission);
				break;

			case (int)LightingModes.MultiLightEmissive:
				material.shader = Shader.Find ("Ethical Motion/Particles/Lit MultiLight");
				material.EnableKeyword (Keywords.Emission);
				break;

			case (int)LightingModes.Lit:
				material.shader = Shader.Find ("Ethical Motion/Particles/Lit");
				material.DisableKeyword (Keywords.Emission);
				break;

			case (int)LightingModes.LitEmissive:
				material.shader = Shader.Find ("Ethical Motion/Particles/Lit");
				material.EnableKeyword (Keywords.Emission);
				break;

			case (int)LightingModes.Unlit:
				material.DisableKeyword (Keywords.Emission);
				material.shader = Shader.Find ("Ethical Motion/Particles/Unlit");
				break;
			}
		}

		private void BlendModePopup(Material material)
		{
			if ((int)m_LightingMode.floatValue == (int)LightingModes.Unlit)
			{
				EditorGUI.showMixedValue = m_BlendMode.hasMixedValue;
				var blendMode = (BlendModes)m_BlendMode.floatValue;
			
				EditorGUI.BeginChangeCheck ();
				blendMode = (BlendModes)EditorGUILayout.Popup (Styles.m_BlendPopup, (int)blendMode, Styles.m_BlendNames);
				if (EditorGUI.EndChangeCheck ()) {
					m_MaterialEditor.RegisterPropertyChangeUndo ("Blend Mode");
					m_BlendMode.floatValue = (float)blendMode;
				}
			
				EditorGUI.showMixedValue = false;

				switch ((int)m_BlendMode.floatValue) {
				case (int)BlendModes.Additive:
					material.SetInt ("_BlendSrc", (int)UnityEngine.Rendering.BlendMode.One);
					material.SetInt ("_BlendDst", (int)UnityEngine.Rendering.BlendMode.One);
					material.EnableKeyword (Keywords.Additive);
					break;
				
				case (int)BlendModes.AdditiveSoft:
					material.SetInt ("_BlendSrc", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
					material.SetInt ("_BlendDst", (int)UnityEngine.Rendering.BlendMode.One);
					material.EnableKeyword (Keywords.Additive);
					break;
				
				case (int)BlendModes.AlphaBlend:
					material.SetInt ("_BlendSrc", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
					material.SetInt ("_BlendDst", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					material.DisableKeyword (Keywords.Additive);
					break;
				}
			}

		}

		private void AlphaModePopup(Material material)
		{
			EditorGUI.showMixedValue = m_AlphaMode.hasMixedValue;
			var alphaMode = (AlphaModes)m_AlphaMode.floatValue;
			
			EditorGUI.BeginChangeCheck();
			alphaMode = (AlphaModes)EditorGUILayout.Popup(Styles.m_AlphaPopup, (int)alphaMode, Styles.m_AlphaNames);
			if (EditorGUI.EndChangeCheck())
			{
				m_MaterialEditor.RegisterPropertyChangeUndo("Alpha Mode");
				m_AlphaMode.floatValue = (float)alphaMode;
			}
			
			EditorGUI.showMixedValue = false;

			switch ((int)m_AlphaMode.floatValue)
			{
			case (int)AlphaModes.Erosion:
				material.EnableKeyword(Keywords.AlphaErosion);
				break;
				
			case (int)AlphaModes.Fade:
				material.DisableKeyword(Keywords.AlphaErosion);
				break;
			}
		}

		private void AdditionalSettings(Material material)
		{
			//----------------------------------------HDR MULTIPLIER
			//----------------------------------------
			if (Properties.m_HDRMultiplier != null)
			{
				if ((LightingModes)m_LightingMode.floatValue == LightingModes.MultiLightEmissive ||
					(LightingModes)m_LightingMode.floatValue == LightingModes.LitEmissive)
				{
					m_MaterialEditor.RegisterPropertyChangeUndo (Styles.m_HDRMultiplierText.text);
					m_MaterialEditor.FloatProperty (Properties.m_HDRMultiplier, Styles.m_HDRMultiplierText.text);
				} 
			}
			EditorGUILayout.Space();

			//----------------------------------------SHADOW CUTOFF
			//----------------------------------------
			if (Properties.m_Cutoff != null)
			{
				m_MaterialEditor.RegisterPropertyChangeUndo (Styles.m_CutoffText.text);
				m_MaterialEditor.RangeProperty(Properties.m_Cutoff, Styles.m_CutoffText.text);
			}
			EditorGUILayout.Space();

			//----------------------------------------THICKNESS
			//----------------------------------------
			if (Properties.m_Thickness != null)
			{
				m_MaterialEditor.RegisterPropertyChangeUndo (Styles.m_ThicknessText.text);
				m_MaterialEditor.RangeProperty (Properties.m_Thickness, Styles.m_ThicknessText.text);
			}
			EditorGUILayout.Space();

			//----------------------------------------ALPHA TRANSMITANCE
			//----------------------------------------
			if (Properties.m_AlphaTransmitance !=  null)
			{
				var backLight = EditorGUILayout.Toggle (Styles.m_AlphaTransmitanceToggle, m_AlphaTransmitanceEnabled);

				if (backLight) {
					material.EnableKeyword (Keywords.AlphaTransmitance);

					m_MaterialEditor.RegisterPropertyChangeUndo (Styles.m_AlphaTransmitanceText.text);
					m_MaterialEditor.RangeProperty(Properties.m_AlphaTransmitance, Styles.m_AlphaTransmitanceText.text);
					m_MaterialEditor.RegisterPropertyChangeUndo (Styles.m_AlphaContrastText.text);
					m_MaterialEditor.RangeProperty(Properties.m_AlphaContrast, Styles.m_AlphaContrastText.text);
				} else
					material.DisableKeyword (Keywords.AlphaTransmitance);
			}
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			//----------------------------------------ADDITIONAL SETTINGS
			//----------------------------------------
			GUILayout.Label(Styles.HeaderOptional, EditorStyles.boldLabel);
			//----------------------------------------SOFT PARTICLES
			//----------------------------------------
			if (Properties.m_InvFade != null)
			{
				var softParticles = EditorGUILayout.Toggle(Styles.m_SoftParticlesToggle, m_SoftParticlesEnabled);

				if (softParticles)
				{
					material.EnableKeyword(Keywords.SoftParticles);

					m_MaterialEditor.RegisterPropertyChangeUndo (Styles.m_InvFadeText.text);
					m_MaterialEditor.RangeProperty(Properties.m_InvFade, Styles.m_InvFadeText.text);
				}
				else material.DisableKeyword(Keywords.SoftParticles);
			}
			EditorGUILayout.Space ();

			//----------------------------------------DISTANCE FADE
			//----------------------------------------
			if (Properties.m_DistanceFadeStart != null && Properties.m_DistanceFadeEnd != null)
			{
				
				var distanceFade = EditorGUILayout.Toggle(Styles.m_DistanceFadeToggle, m_DistanceFadeEnabled);

				if (distanceFade)
				{
					material.EnableKeyword(Keywords.DistanceFade);

					m_MaterialEditor.RegisterPropertyChangeUndo (Styles.m_DistanceFadeStartText.text);
					m_MaterialEditor.FloatProperty(Properties.m_DistanceFadeStart, Styles.m_DistanceFadeStartText.text);
					m_MaterialEditor.RegisterPropertyChangeUndo (Styles.m_DistanceFadeEndText.text);
					m_MaterialEditor.FloatProperty(Properties.m_DistanceFadeEnd, Styles.m_DistanceFadeEndText.text);
				}
				else material.DisableKeyword(Keywords.DistanceFade);
			}


		}
	}

}