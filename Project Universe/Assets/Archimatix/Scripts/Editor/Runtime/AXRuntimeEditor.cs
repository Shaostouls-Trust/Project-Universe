using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;

using AX;

namespace AXEditor
{
	public class AXRuntimeEditor
	{
		// Supports the generation of runtime controllers for AXModels


		public static void createControllerButtonAction(AXModel model)
		{

			GameObject runtimeControllerGO = null;

			foreach (Transform child in model.transform)
			{
			    if (child.name == "runtimeController"){
					runtimeControllerGO = child.gameObject;
			        break;
			    } 
			}

			if (runtimeControllerGO == null){
				// Create one
				runtimeControllerGO = new GameObject("runtimeController");
				runtimeControllerGO.transform.parent = model.gameObject.transform;
			}


			string fullPath = "";

			if (runtimeControllerGO != null)
			{

				AXRuntimeControllerBase runtimeController = runtimeControllerGO.GetComponent<AXRuntimeControllerBase>();


				// 1. GET runctimeController filePath...

				if (runtimeController != null)
				{
					// 1A. GET FilePath from Component
					MonoScript ms = MonoScript.FromMonoBehaviour(runtimeController);
					string relativeAssetPath = AssetDatabase.GetAssetPath(ms.GetInstanceID());
					fullPath =  ArchimatixUtils.getAbsoluteLibraryPath(relativeAssetPath);
				}
				else
				{
					// 1B. Since a controller does not exist, we must create one from a tempalte, update it and the 
					// 	   let AX know that a new class will be available to add to the GameObject after scripts reload.

					// 1B.1. CREATE file based on a template...
					fullPath = EditorUtility.SaveFilePanel(
										"Save New Controller File",
										ArchimatixUtils.getAbsoluteLibraryPath("Assets"),
										"MyRuntimeController",
										"cs");



					// 1B.2. LOCATE TEMPLATE file AXRuntimeControllerTemplate.cs

					DirectoryInfo info = new DirectoryInfo(Application.dataPath);
					FileInfo[] files = info.GetFiles("AXRuntimeControllerTemplate.cs", SearchOption.AllDirectories);
					string templateFilePath = "";
					if (files != null && files.Length > 0)
						templateFilePath = files[0].ToString();


					// 1B.3. COPY TEMPLATE to fullPath

					if (! string.IsNullOrEmpty(templateFilePath) && File.Exists(templateFilePath))
						File.Copy(templateFilePath, fullPath, true);


					// 1B.4. REPLACE the classname
					string newClassName = System.IO.Path.GetFileNameWithoutExtension(fullPath);
					File.WriteAllText(fullPath, File.ReadAllText(fullPath).Replace("AXRuntimeControllerTemplate", newClassName));

				
					// 1B.5. SET UP TO CONNECT CLASS TO GAMEOBJECT AFTER ALL SCRIPTS RELOAD
					// ArchimatixEngine will notice this on DidReloadAllScripts and add this class as a component to the runtimeControllerGO
					EditorPrefs.SetString("AddComponentByGameObjectIDAndClassname", (runtimeControllerGO.GetInstanceID() + "_" + newClassName));
				}


				// 2. REWRITE the auto-generated region of the file
				// 	  based on the model's exposed runtime parameters.
				if (File.Exists(fullPath))
					AXRuntimeEditor.updateControllerFile(fullPath, model);
				

				// 3. Let the user know that this script writing 
				//	  starts a Reload of all scripts
				EditorUtility.DisplayDialog("Reloading Scripts",
                    "This may take a few seconds.",
                    "Ok");

				// 4. REFRESH DB - This is asynchronous and will reload all scripts.
				AssetDatabase.Refresh();

			}
		}



		public static void updateControllerFile (string filepath, AXModel model)
		{
		
			if (!File.Exists (filepath)) {
				return;
			}


			string line;
			string trimLine;

			// Read the file and display it line by line.
			System.IO.StreamReader file = new System.IO.StreamReader (filepath);

			bool isInAutoSection = false;

			// Lines for the new file content
			List<string> nlines = new List<string> ();



			// 	Step through each line of the file...

			while ((line = file.ReadLine ()) != null) {

				trimLine = line.Trim ();

				// This parsing does not support nested regions (of which there should be none in the auto-generated code)

				if (!isInAutoSection && trimLine == "#region AUTO_GENERATED AX BINDINGS") {
					isInAutoSection = true;
				} else if (isInAutoSection) {
					if (trimLine == "#endregion") {
						// write auto-generated section which creates dynamic variables bound to AXParameters
						// ...
						nlines.AddRange(generateLinesOfAutoRuntimeParametersBindingCode(model));

						isInAutoSection = false;
					} else {
						// do not write this line, since we are removing the old contents
						continue;
					}
				}

				nlines.Add (line);
			}

			file.Close ();


//		foreach (string nline in nlines)
//		{
//			Debug.Log(nline);
//		}



			StreamWriter sw = new StreamWriter (filepath);
			for (int i = 0; i < nlines.Count; i++) {
				sw.WriteLine (nlines [i]);
			}
			sw.Close ();


		}



		public static List<string> generateLinesOfAutoRuntimeParametersBindingCode (AXModel model)
		{
			List<string> nlines = new List<string> ();

		
			nlines.Add ("");
			nlines.Add ("\t// *** PARAMETER_REFERENCES_DEFINITION *** //");
			nlines.Add ("");

			foreach (AXParameterAlias pa in model.exposedParameterAliases) {
				nlines.Add ("\tprivate AXParameter P_" + pa.alias.Replace (" ", "_") + ";");
			}
			nlines.Add ("");
			nlines.Add ("\t// *** PARAMETER_REFERENCES_DEFINITION *** //");
			nlines.Add ("");
			nlines.Add ("");


			nlines.Add ("");
			nlines.Add ("\t// *** DYNAMIC_VARIABLES *** //");
			nlines.Add ("");

			foreach (AXParameterAlias pa in model.exposedParameterAliases) {
				nlines.Add ("\tpublic " + pa.parameter.dataTypeAsString () + " " + pa.alias.Replace (" ", "_") + " {");
				nlines.Add ("\t\tget {");
				nlines.Add ("\t\t\tif (model != null)");
				nlines.Add ("\t\t\t\treturn P_" + pa.alias.Replace (" ", "_") + "." + pa.parameter.valueTypeString () + ";");
				nlines.Add ("\t\t\treturn " + pa.parameter.returnString () + ";");
				nlines.Add ("\t\t}");

				nlines.Add ("\t\tset {");
				nlines.Add ("\t\t\tif (P_" + pa.alias.Replace (" ", "_") + " != null)");
				nlines.Add ("\t\t\t{");
				nlines.Add ("\t\t\t\tP_" + pa.alias.Replace (" ", "_") + ".setValue(value);");
				nlines.Add ("\t\t\t\tparameterWasAltered = true;");
				nlines.Add ("\t\t\t}");
				nlines.Add ("\t\t}");

				nlines.Add ("\t}");

			}
			nlines.Add ("");
			nlines.Add ("\t// *** DYNAMIC_VARIABLES *** //");
			nlines.Add ("");
			nlines.Add ("");


			nlines.Add ("");
			nlines.Add ("\t// *** PARAMETER_REFERENCE_INIT *** //");
			nlines.Add ("");

			nlines.Add ("\tprotected override void InitializeParameterReferences()");
			nlines.Add ("\t{");

			foreach (AXParameterAlias pa in model.exposedParameterAliases) {
				nlines.Add ("\t\tP_" + pa.alias.Replace (" ", "_") + "\t= model.getParameter (\"" + pa.alias + "\");");
			}

			nlines.Add ("\t}");

			nlines.Add ("");
			nlines.Add ("\t// *** PARAMETER_REFERENCE_INIT *** //");
			nlines.Add ("");
			nlines.Add ("");


			return nlines;
		}


	}

}
