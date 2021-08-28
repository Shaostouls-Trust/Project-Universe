#define ARCHIMATIX
#define AX_1_00_OR_NEWER
#define AX_1_01_OR_NEWER
#define AX_1_02_OR_NEWER
#define AX_1_03_OR_NEWER
#define AX_1_04_OR_NEWER
#define AX_1_05_OR_NEWER
#define AX_1_06_OR_NEWER

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Linq;


using AX.SimpleJSON;




using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXGeometry;
using AX.Generators;
using AX;

using Parameters = System.Collections.Generic.List<AX.AXParameter>;
using IndexedParameters = System.Collections.Generic.Dictionary<string, AX.AXParameter>;



namespace AX
{

	[ExecuteInEditMode()]
	[System.Serializable]
	public class AXModel : MonoBehaviour
	{

		public static int modelCount;






		// DELEGATE: Build
		public delegate void ModelGenerateHandler(AXModel m, EventArgs e = null);
		public event ModelGenerateHandler ModelDidGenerate;

		public delegate void BuildHandler(AXModel m, EventArgs e = null);
		public event BuildHandler ModelDidBuild;



		// DELEGATE: RuntimeHandleDownHandler
		public delegate void RuntimeHandleDownHandler(AXModel m, EventArgs e);
		public event RuntimeHandleDownHandler RuntimeHandleDown;

		// DELEGATE: RuntimeHandleUpHandler
		public delegate void RuntimeHandleUpHandler(AXModel m, EventArgs e);
		public event RuntimeHandleUpHandler RuntimeHandleUp;

		// DELEGATE: RuntimeHandleEnterHandler
		public delegate void RuntimeHandleEnterHandler(AXModel m, EventArgs e);
		public event RuntimeHandleEnterHandler RuntimeHandleEnter;

		// DELEGATE: RuntimeHandleExitHandler
		public delegate void RuntimeHandleExitHandler(AXModel m, EventArgs e);
		public event RuntimeHandleExitHandler RuntimeHandleExit;



		public PrecisionLevel precisionLevel = PrecisionLevel.Meter;

		public void setRealtimePrecision()
		{
			AXGeometry.Utilities.setRealtimePrecisionFactor(precisionLevel);

		}
		public void setBuildPrecision()
		{
			AXGeometry.Utilities.setBuildPrecisionFactor(precisionLevel);
		}

		//public StopWatch sw;

		public AnimationClip amclip;
		public AnimationCurve amCurve;


		public bool staticFlagsEnabled;
		public bool staticFlagsJustEnabled;

		public bool createSecondaryUVs;
		public bool createSecondaryUVsJustEnabled;

		[SerializeField]
		private string m_guid = System.Guid.NewGuid().ToString();
		public string Guid
		{
			get { return m_guid; }
			set { /* can't be set by others */ }
		}

		public float totalVolume;
		public float volume;
		public float mass;
		public float height;

		private int m_stats_VertCount = 0;
		public int stats_VertCount
		{
			get { return m_stats_VertCount; }
			set { m_stats_VertCount = value; }
		}

		private int m_stats_TriangleCount = 0;
		public int stats_TriangleCount
		{
			get { return m_stats_TriangleCount; }
			set { m_stats_TriangleCount = value; }
		}



		public float segmentReductionFactor = 1;

		public bool canRegenerate = true;

		public bool automaticModelRegeneration = true;


		// INSPECTOR DISPLAY
		public bool displayModelDefaults = false;
		public bool displayModelRuntimeParameters = false;
		public bool displayModelSelectedNodes = true;

		public bool displayRuntimeUI = false;

		// DO NOT DERIALIZE THESE!!!
		// Otherwise the Undo system will compare all these meshes against previous versions.
		// This is just a temporary holding bin for geometry while dragging or on the fly editing.

		[System.NonSerialized]
		public AXParameter latestEditedParameter;

		[System.NonSerialized]
		public AXHandle latestHandleClicked;

		[System.NonSerialized]
		public int axRenderMeshCount = 0;

		[System.NonSerialized]
		private AXMesh[] axRenderMeshes = new AXMesh[10000];

		[System.NonSerialized]
		public AXMeshCache axmeshCache;


		// SPRITE GENERATION
		// Each model can generate only one Sprite at a time. 
		// These are all rendered to a Layer called AX2D_Sprite
		public bool isSpriteGenerator = false;
		public Material spritePreviewMaterial;
		public AXSpriteMesh spritePreviewMesh;
		public AXSpriter spriter;



		[System.NonSerialized]
		public List<AXParametricObject> AlteredPOs;

		[System.NonSerialized]
		public List<AXParametricObject> AlteredHeadPOs;

		[System.NonSerialized]
		public List<GameObject> gameObjectsToDelete;

		[System.NonSerialized]
		public List<AXParametricObject> visitedPOs;


		public bool isDirty;


		public bool isGenerating = false;

		public bool _autoBuild = true;

		public StopWatch autoBuildStopwatch;
		//public 

		public bool rigidbodiesUndergroundAreKinematic = false;

		// Graph Editor State
		public Vector2 focusPointInGraphEditor;
		public float zoomScale = 1;


		[System.NonSerialized]
		public bool isPanningToPoint = false;

		[System.NonSerialized]
		public Vector2 panToPoint;


		public enum BuildStatus { Generated, Built, Locked, Dormant, Lightmapped, lightmapUVs };
		public BuildStatus buildStatus;

		public enum RenderMode { DrawMesh, GameObjects };
		public RenderMode renderMode;


		// this means there was just a mouse down or some other signal for an intention to edit.
		public bool readyToRegisterUndo = false;

		// PARAMETRIC_OBJECTS
		public List<AXParametricObject> parametricObjects = new List<AXParametricObject>();

		// Canonical list of parameter relations
		// Relations are two way rather than a linkage defined by an AXParameter::DependsOn
		// These are mostly for Floats and Ints
		public List<AXRelation> relations = new List<AXRelation>();

		/* Parameters Registry 
		 * 
		 * Maintain a registry of all parameters so that a 
		 * live link may be set up quickly 
		 * 
		 */
		[System.NonSerialized]
		public Dictionary<string, AXParameter> m_indexedParameters;
		public Dictionary<string, AXParameter> indexedParameters
		{
			get
			{
				if (m_indexedParameters == null)
					m_indexedParameters = new IndexedParameters();
				return m_indexedParameters;
			}
			set { m_indexedParameters = value; }
		}





		//public List<AXParameter> exposedParameters;
		public List<AXParameterAlias> m_exposedParameterAliases;
		public List<AXParameterAlias> exposedParameterAliases
		{
			get
			{
				if (m_exposedParameterAliases == null)
					m_exposedParameterAliases = new List<AXParameterAlias>();
				return m_exposedParameterAliases;
			}
			set { m_exposedParameterAliases = value; }
		}

		public AXParameter getExposedParameter(string name)
		{
			return exposedParameterAliases.Where(m => m.alias == name).First().parameter;

		}

		public void addExposedParameter(AXParameter p)
		{

			if (!exposedParameterAliases.Exists(x => x.Guid == p.Guid))
			{
				// persistent list
				AXParameterAlias pa = new AXParameterAlias();
				pa.Guid = p.Guid;
				pa.alias = p.parametricObject.Name + "_" + p.Name;
				pa.parameter = p;

				exposedParameterAliases.Add(pa);
			}
		}
		public void removeExposedParameter(AXParameter p)
		{
			p.exposeAsInterface = false;
			if (exposedParameterAliases.Exists(x => x.Guid == p.Guid))
			{
				exposedParameterAliases.Remove(exposedParameterAliases.Where(m => m.Guid == p.Guid).First());
			}
		}

		// GET PARAMETER
		public AXParameter getParameter(string alias)
		{


			List<AXParameterAlias> res = exposedParameterAliases.Where(m => m.alias == alias).ToList();


			if (res != null && res.Count > 0)
			{
				return ((AXParameterAlias)res[0]).parameter;
			}
			else
			{
				Debug.Log("GetParameter: " + alias + " not found. Are you sure it is an exposed parameter in the model?");
			}
			return null;

		}





		/* Handles Registry 
		 * 
		 * Maintain a registry of all handles so that a 
		 * live link may be set up quickly 
		 * 
		 */
		[System.NonSerialized]
		public Dictionary<string, AXHandle> m_indexedHandles;
		public Dictionary<string, AXHandle> indexedHandles
		{
			get
			{
				if (m_indexedHandles == null)
					m_indexedHandles = new Dictionary<string, AXHandle>();
				return m_indexedHandles;
			}
			set { m_indexedHandles = value; }
		}



		// EXPOSED RUNTIME HANDLES

		private GameObject m_RuntimeHandlePrefab_PlanarKnob;
		public GameObject RuntimeHandlePrefab_PlanarKnob
		{
			get
			{
				//				if (m_RuntimeHandlePrefab_PlanarKnob == false)
				//				{
				//					Resources.Load ("RuntimeHandlePrefab_PlanarKnob");
				//				}
				return m_RuntimeHandlePrefab_PlanarKnob;
			}
			set
			{
				m_RuntimeHandlePrefab_PlanarKnob = value;
			}

		}

		public List<AXHandleRuntimeAlias> m_runtimeHandleAliases;
		public List<AXHandleRuntimeAlias> runtimeHandleAliases
		{
			get
			{
				if (m_runtimeHandleAliases == null)
					m_runtimeHandleAliases = new List<AXHandleRuntimeAlias>();
				return m_runtimeHandleAliases;
			}
			set { m_runtimeHandleAliases = value; }
		}

		public AXHandleRuntimeAlias addHandleRuntimeAlias(AXHandle han)
		{
			if (han == null)
				return null;

			AXHandleRuntimeAlias rth = null;

			if (!m_runtimeHandleAliases.Exists(x => x.handleGuid == han.Guid))
			{
				// persistent list
				//Debug.Log("ADDING AXHandleRuntimeAlais to list"); 
				rth = new AXHandleRuntimeAlias();
				rth.handleGuid = han.Guid;
				rth.alias = han.parametricObject.Name + "." + han.Name;
				rth.handle = han;

				m_runtimeHandleAliases.Add(rth);
			}
			else
				rth = m_runtimeHandleAliases.Where(m => m.handleGuid == han.Guid).First();

			return rth;
		}
		public void removeHandleRuntimeAlias(AXHandle han)
		{
			if (m_runtimeHandleAliases.Exists(x => x.handleGuid == han.Guid))
			{
				AXHandleRuntimeAlias rha = m_runtimeHandleAliases.Where(m => m.handleGuid == han.Guid).First();

				if (rha.handle != null && rha.handle.runtimeHandleBehavior != null)
					DestroyImmediate(rha.handle.runtimeHandleBehavior.gameObject);

				m_runtimeHandleAliases.Remove(rha);
			}
		}



		// DELEGATE CALLS FOR RUNTIME HANDLES
		public void OnRuntimeHandleDown()
		{
			if (RuntimeHandleDown != null)
				RuntimeHandleDown(this, new EventArgs());
		}
		public void OnRuntimeHandleUp()
		{
			if (RuntimeHandleUp != null)
				RuntimeHandleUp(this, new EventArgs());
		}

		public void OnRuntimeHandleEnter()
		{
			if (RuntimeHandleEnter != null)
				RuntimeHandleEnter(this, new EventArgs());
		}

		public void OnRuntimeHandleExit()
		{
			if (RuntimeHandleExit != null)
				RuntimeHandleExit(this, new EventArgs());
		}






		// SELECTED AX_GAME_OBJECT
		// Use this to cycle-select a iven AXGameObject and its parents in SceneView.
		// The AXGameObject clicked remains the clickSelectedAXGO,
		// but its ancestry is activated by additional consecutive clicks 
		//that set the cycleSelectedAXGO
		[System.NonSerialized]
		public CycleList<Transform> selectedTransfromAncestry;

		// SELECTED PO IN NODE GRAPH
		// When you create this, you are copying the AXGameObject info of 
		// AXParametricObjectObject and consumer address into a GenerNode which 
		// survives between model.builds() but requires a lookup at each po in the ancestry to establish the world matrix
		// for the selected PO's handles to be drawn into.
		[System.NonSerialized]
		public CycleList<GenerNode> selectedGenerNodeAncestry;


		[System.NonSerialized]
		public AXGameObject clickSelectedAXGO = null;

		[System.NonSerialized]
		public AXGameObject cycleSelectedAXGO = null;

		[System.NonSerialized]
		public AXParametricObject cycleSelectedPO = null;


		// Only show nodes in the current dispalyGroup and that are currently "isOpen"
		[System.NonSerialized]
		public AXParametricObject currentWorkingGroupPO = null;

		public string currentWorkingGroupPOGUID;




		public Transform nextAncestor(int gen = 0)
		{
			if (gen > 25)
				return null;

			Transform t = selectedTransfromAncestry.next;

			if (t.gameObject.GetComponent<AXGameObject>() == null)
			{       //return nextAncestor(gen++);
				deselectAll();
				selectedTransfromAncestry.reset();
				return null;
			}

			return t;
		}




		// SELECTED PARAMETER 
		[System.NonSerialized]
		public AXParameter selectedParameterInputRelation;




		// SELECTED PARAMETRIC_OBJECTS
		[System.NonSerialized]
		private List<AXParametricObject> m_selectedPOs = new List<AXParametricObject>();
		public List<AXParametricObject> selectedPOs
		{
			get
			{
				if (m_selectedPOs == null)
					m_selectedPOs = new List<AXParametricObject>();
				return m_selectedPOs;
			}
			set { m_selectedPOs = value; }
		}

		[System.NonSerialized]
		private List<AXParametricObject> m_activeFreeCurves = new List<AXParametricObject>();
		public List<AXParametricObject> activeFreeCurves
		{
			get
			{
				if (m_activeFreeCurves == null)
					m_activeFreeCurves = new List<AXParametricObject>();
				return m_activeFreeCurves;
			}
			set { m_activeFreeCurves = value; }
		}
		public void clearActiveFreeCurves()
		{
			foreach (AXParametricObject po in activeFreeCurves)
			{
				if (po.generator is FreeCurve3D)
				{
					FreeCurve3D gener = (FreeCurve3D)po.generator;
				}
				else
				{
					FreeCurve gener = (FreeCurve)po.generator;
					gener.deslectAllItems();
				}


			}
			activeFreeCurves.Clear();
		}




		[System.NonSerialized]
		private AXParametricObject m_mostRecentlySelectedPO;
		public AXParametricObject mostRecentlySelectedPO
		{
			get { return m_mostRecentlySelectedPO; }
			set { m_mostRecentlySelectedPO = value; }
		}

		[System.NonSerialized]
		public AXParametricObject mostRecentlyInstantiatedPO;



		[System.NonSerialized]
		private AXParametricObject m_recentlySelectedPO;
		public AXParametricObject recentlySelectedPO
		{
			get { return m_recentlySelectedPO; }
			set { m_recentlySelectedPO = value; }
		}


		[System.NonSerialized]
		private AXParametricObject m_mostRecentlyClickedPO;
		public AXParametricObject mostRecentlyClickedPO
		{
			get { return m_mostRecentlyClickedPO; }
			set { m_mostRecentlyClickedPO = value; }
		}

		[System.NonSerialized]
		private List<AXParametricObject> _poSelectionHistory;
		public List<AXParametricObject> poSelectionHistory
		{
			get
			{
				if (_poSelectionHistory == null)
					_poSelectionHistory = new List<AXParametricObject>();
				return _poSelectionHistory;
			}
			set { _poSelectionHistory = value; }
		}
		public AXParametricObject getPOSelectedBefore(AXParametricObject po)
		{
			if (poSelectionHistory.Count == 1)
			{
				if (poSelectionHistory[0].Guid == po.Guid)
					return null;
				else
					return poSelectionHistory[0];
			}
			else if (poSelectionHistory.Count > 1)
			{
				if (poSelectionHistory[0].Guid == po.Guid)
					return poSelectionHistory[1];
				else
					return poSelectionHistory[0];
			}
			return null;
		}

		public void moveSelectedPOs(Vector2 displ)
		{

			foreach (AXParametricObject po in selectedPOs)
				po.rect.position += displ;
		}

		[System.NonSerialized]
		public AXParametricObject mouseOverPO;



		[System.NonSerialized]
		private AXParameter m_selectedParameter;
		public AXParameter selectedParameter
		{
			get { return m_selectedParameter; }
			set { m_selectedParameter = value; }
		}

		[System.NonSerialized]
		private AXRelation m_selectedRelation;
		public AXRelation selectedRelation
		{
			get { return m_selectedRelation; }
			set { m_selectedRelation = value; }
		}

		[System.NonSerialized]
		public AXRelation selectedRelationInGraph;



		public Rect nodeGraphRect;

		public Vector2 editorWindowSpaceOffset = Vector2.zero;


		public GUISkin guiSkin;


		// These are the default material and texture objects that will be used with cleangraph 
		// to set any po references where a MaterialTool is not downstream

		// Legacy for older saved scenes.
		[SerializeField]
		private Material _mat = null;
		public Material Mat
		{
			get
			{
				if (_mat == null)
				{
					Shader shdr = Shader.Find("Standard");
					if (shdr == null)
						shdr = (Shader.Find("Specular"));

					_mat = new Material(shdr);
				}
				return _mat;
			}
			set { _mat = value; }
		}


		public AXMaterial axMat;
		public AXTexCoords axTex;

		bool doRemapMaterials = false;

		public bool showDefaultMaterial;
		public bool showPhysicMaterial;

		public static Material defaultMaterial;
		public ColliderType defaultColliderType = ColliderType.Mesh;
		public bool overrideColliderOnLibraryItem = false;
		public static PhysicMaterial defaultPhysicMaterial;
		public static float defaultDensity = 1;

		[System.NonSerialized]
		private bool traversalIsLocked = false;



		public GameObject thumbnailLightGO = null;
		public GameObject thumbnailCameraGO = null;
		public Camera thumbnailCamera = null;
		public GameObject thumbnailCameraTarget = null;
		public Mesh thumbnailMaterialMesh = null;
		public Matrix4x4 remoteThumbnailLocation;


		public GameObject spriteCameraGO = null;
		public Camera spriteCamera = null;



		[System.NonSerialized]
		public List<Light> allLightsInTheScene = null;





		[System.NonSerialized]
		public bool isDragging;

		public float snapSizeGrid = .25f;



		void OnEnable()
		{
			//Debug.Log("Enable()");
			//sw = new StopWatch();


			//RUNTIME

#if UNITY_EDITOR

#endif


			//Debug.Log ("Model [] enabled");
			axmeshCache = new AXMeshCache();

			AlteredPOs = new List<AXParametricObject>();
			AlteredHeadPOs = new List<AXParametricObject>();


			isGenerating = false;

			// Prior to Unity 5 - If there are no lights in the scene, make a default directional light

			/* Timw to say good bywe to Unity4 and the lack of directional lights
			Light[] lights = FindObjectsOfType(typeof(Light)) as Light[];;
			if (lights == null || lights.Length == 0)
			{
				GameObject defaultDirectionalLight = new GameObject("defaultDirectionalLight");
				addSceneDirectionalLight(defaultDirectionalLight);
			}
			*/

			cleanGraph();


			// MAKE SURE ALL PO"S ARE ALTERED TO REDRAW ALL THUMBNAILS
			setAllAltered();


			//generateAllMeshes(false);


			Transform g = gameObject.transform.Find("generatedGameObjects");
			if (g != null)
				generatedGameObjects = g.gameObject;



			Transform tl = gameObject.transform.Find("thumbnailLight");
			if (tl != null)
				thumbnailLightGO = tl.gameObject;

			Transform tc = gameObject.transform.Find("thumbnailCamera");
			if (tc != null)
				thumbnailCameraGO = tc.gameObject;



			if (axMat == null)
			{
				axMat = new AXMaterial();

				if (Mat != null)
					axMat.mat = Mat;
			}


			if (axTex == null)
				axTex = new AXTexCoords();



			// Check for legacy materials
			// A old scene (v 1.0.0 might has po.Mat. Need to upgrade this to po.axMat.mat
			for (int i = 0; i < parametricObjects.Count; i++)
			{
				AXParametricObject po = parametricObjects[i];

				if (po.generator != null && parametricObjects[i].generator is MaterialTool)
				{
					//Debug.Log(po.Name+" Legacy Material? " + ((po.Mat == null) ? "NULL": po.Mat.name) + " ++++ " +  po.axMat+ " ++++ " +  po.axMat.mat);

					if ((po.axMat == null || po.axMat.mat == null) && po.Mat != null)
					{
						if (po.axMat == null)
							po.axMat = new AXMaterial();
						po.axMat.mat = po.Mat;
						doRemapMaterials = true;
					}
				}
			}
			if (doRemapMaterials)
				remapMaterialTools();


			if (isSpriteGenerator)
				assertSpriteSupport();

			if (currentWorkingGroupPO != null)
				selectAllVisibleInGroup(currentWorkingGroupPO);

		}




		void Start()
		{
			//Debug.Log("Start()");


			// Can't gernerate the thumbnails in OnEnable because of a bug in the mac version of Unity 5.4.
			// The Camera.Render does not seem to want to be in OnEnable when returning from Play mode or loading a scene
			regenerateAXMeshes(true);


		}


		// Update is called once per frame
		void Update()
		{
			//sw.restart();
			//Debug.Log("Update");
			// drawMeshes here were called relatively infrequently in Scene View
			// The call to drawMeshes here is now redundent with the one in ArchimatixEngine.OnSceneGUI()
			// but this call is necessary for runtime AX, where the editor loop will not be run.
			if (Application.isPlaying && renderMode == RenderMode.DrawMesh)
			{
				// DRAW MESHES

				drawMeshesInScene();


				// DELETE GAME_OBJECTS IN RUNTIME NOW THAT DRAW_MESH IS GOING
				if (Application.isPlaying)
				{
					if (gameObjectsToDelete != null && gameObjectsToDelete.Count > 0)
					{
						for (int i = 0; i < gameObjectsToDelete.Count; i++)
						{
							//							if (gameObjectsToDelete[i] != null)
							//								Debug.Log("destroy " + gameObjectsToDelete[i].name);
							if (Application.isPlaying)
								Destroy(gameObjectsToDelete[i]);
							else
								DestroyImmediate(gameObjectsToDelete[i]);

						}
						// This brilliant hack of using UnloadUnusedAssets comes from https://forum.unity3d.com/threads/why-is-destroy-and-then-null-not-giving-the-memory-back.28441/
						Resources.UnloadUnusedAssets();
						gameObjectsToDelete = null;
					}
				}
				if (!Application.isPlaying)
					canRegenerate = true;


			}


			//generateIfDirty();




			if (autoBuildStopwatch != null)
			{
				//Debug.Log("autoBuildTimer " );
				if (autoBuildStopwatch.time() > 500)
				{
					autobuild();
				}
			}
		}

		public void drawMeshesInScene()
		{



			if (renderMode == RenderMode.DrawMesh)
			{
				Material renderMat = null;

				//Debug.Log("drawMeshesInScene " + axRenderMeshCount);

				for (int i = 0; i < axRenderMeshCount; i++)
				{
					// REALTIME RENDER EVERY FRAME TO MAIN SCENE VIEW (Not PO cameras)
					// - later shift to editorwindow as a call only when mouse down.
					renderMat = axRenderMeshes[i].mat;


					if (renderMat == null)
						renderMat = axMat.mat;


					if (renderMat == null)
					{
						Debug.Log("No Material. Is default material missing?");
						break;
					}





					Graphics.DrawMesh(axRenderMeshes[i].drawMesh, transform.localToWorldMatrix * axRenderMeshes[i].transMatrix, renderMat, 0);





					//Graphics.DrawMesh(axRenderMeshes[i].drawMesh, transform.localToWorldMatrix * axRenderMeshes[i].transMatrix, renderMat);
				}

				// SPRITE PREVIEW MESH

				//if (! Application.isPlaying && this.spritePreviewMesh != null && spriter != null && spriter.spriteRenderTexture != null)
				//{

				//	spritePreviewMaterial.SetTexture("_MainTex", spriter.spriteRenderTexture);

				//	spritePreviewMesh.resize(10,10);
				//	Graphics.DrawMesh(spritePreviewMesh.mesh, Matrix4x4.identity, spritePreviewMaterial, 0);
				//}



			}

		}





		public void buildModel()
		{
			renderMode = RenderMode.GameObjects;
			createGameObjects();
		}





		public static AXModel getModelWithGUID(string guid)
		{
			// gets a model from the scene
			AXModel[] mos = FindObjectsOfType(typeof(AXModel)) as AXModel[];

			for (int i = 0; i < mos.Length; i++)
			{

				if (mos[i].Guid == guid)
					return mos[i];
			}
			return null;
		}

		public AXParametricObject getParametricObjectByGUID(string g)
		{
			AXParametricObject po = parametricObjects.Find(x => x.Guid.Equals(g));
			return po;
		}
		public AXParametricObject getParametricObjectByName(string n)
		{
			AXParametricObject po = parametricObjects.Find(x => x.Name.Equals(n));
			return po;
		}

		public AXParameter getParameterByGUID(string guidKey)
		{
			if (String.IsNullOrEmpty(guidKey))
				return null;

			// Moving from legacy with '%' as delimeter in guid
			// New guids should not have PO guids in them, but for old ones, just take the parameter guid at the end.
			string gkey = guidKey.Substring(guidKey.IndexOf('%') + 1);

			if (indexedParameters.ContainsKey(gkey))
				return indexedParameters[gkey];

			return null;
		}

		public AXHandle getHandleByGUID(string guidKey)
		{
			if (String.IsNullOrEmpty(guidKey))
				return null;

			if (indexedHandles.ContainsKey(guidKey))
				return indexedHandles[guidKey];



			return null;
		}



		public void doAnyDataMigration(AXParametricObject po)
		{
			//po.isStowed = false;

			// change "Turtle" to "Shape"
			if (po.GeneratorTypeString == "Turtle")
				po.GeneratorTypeString = "Shape";
			else if (po.GeneratorTypeString == "PairIterator")
				po.GeneratorTypeString = "PairRepeater";
			else if (po.GeneratorTypeString == "StepIterator")
				po.GeneratorTypeString = "StepRepeater";
			else if (po.GeneratorTypeString == "GridIterator")
				po.GeneratorTypeString = "GridRepeater";
			else if (po.GeneratorTypeString == "PlanSec")
				po.GeneratorTypeString = "PlanSweep";
			else if (po.GeneratorTypeString == "ShapeRepeater2D")
				po.GeneratorTypeString = "GridRepeater2D";

			// rename "shift_" to "trans_"
			AXParameter tmpp;
			tmpp = po.getParameter("Input Spline");
			if (tmpp != null)
				tmpp.Name = "Input Shape";
			tmpp = po.getParameter("shift_x");
			if (tmpp != null)
				tmpp.Name = "Trans_X";
			tmpp = po.getParameter("shift_y");
			if (tmpp != null)
				tmpp.Name = "Trans_Y";
			tmpp = po.getParameter("shift_z");
			if (tmpp != null)
				tmpp.Name = "Trans_Z";



			//Debug.Log ("cleanGraph :: " + po.Name);

			for (int i = 0; i < po.parameters.Count; i++)
			{
				AXParameter p = po.parameters[i];
				if (p.PType == AXParameter.ParameterType.BoundsControl)
					p.PType = AXParameter.ParameterType.GeometryControl;
			}

			/*
			for (int i = 0; i < po.shapes.Count; i++) {
				AXShape shape = po.shapes [i];
				for (int j = 0; j < shape.inputs.Count; j++) {
					AXParameter p = shape.inputs [j];
				}
			} 
			*/


		}



		public void assertDefaultMaterial()
		{

			if (axMat == null)
			{
				axMat = new AXMaterial();
			}
			if (axMat.mat == null)
			{
				if (Mat != null)
					axMat.mat = Mat;// legacy mat for opening old scenes
				else
					axMat.mat = defaultMaterial;
				axMat.physMat = defaultPhysicMaterial;
				axMat.density = defaultDensity;
			}

		}





		public void setWorkingGroup(AXParametricObject po)
		{

			if (po != null)
			{
				currentWorkingGroupPO = po;
				currentWorkingGroupPOGUID = po.Guid;
			}

		}
		public void clearCurrentWorkingGroup()
		{
			currentWorkingGroupPO = null;
			currentWorkingGroupPOGUID = null;
		}



		public void setAllAltered()
		{
			for (int i = 0; i < parametricObjects.Count; i++)
				parametricObjects[i].isAltered = true;

		}
		public void cleanGraph()
		{
			//Debug.Log ("CLEAN GRAPH ----------------- ");

			// RENAME THIS onDeserialize
			// - this will have to be expanded to allow for 
			// - model interconnections. (OR NOT!)
			// - perhaps there will need to be a global AXController GameObject in the scene.
			// - Either that or the EditorWindow takes on the role....
			// 
			// - Anyway, for now, only recreate isolate model graph on deserialize.


			// two sweeps - one to clear all references, then one to link up

			// for backward compatibility for older means of storing default material directly in mat vs. axMat.mat
			assertDefaultMaterial();

			// SWEEP 1: 
			// A. Clear parameters index and rebuild...
			// B. Clear dependencies in each parameter
			indexedParameters = new IndexedParameters();
			indexedHandles = new Dictionary<string, AXHandle>();


			for (int i = 0; i < parametricObjects.Count; i++)
			{


				AXParametricObject po = parametricObjects[i];


				po.model = this;
				//po.isAltered = true;
				//doAnyDataMigration(po);
				po.clearParameterSets();
				po.Groupees = null;



				// Top Level Parameters in PO
				for (int j = 0; j < po.parameters.Count; j++)
				{

					AXParameter p = po.parameters[j];


					p.parametricObject = po;
					p.Parent = po;

					//Debug.Log(" - " + p.Name + " " + p.parametricObject.Name);

					//Debug.Log(po.Name+":"+p.Name + "  ......... " + p.Type + " - " + p.PType);

					switch (p.PType)
					{
						case AXParameter.ParameterType.Base:
							po.assertBaseControls();
							po.baseControls.addChild(p);
							break;

						case AXParameter.ParameterType.Input:
							po.assertInputControls();
							po.inputControls.addChild(p);
							break;

						case AXParameter.ParameterType.Output:
							po.assertOutputsNode();
							po.outputsNode.addChild(p);
							break;

						case AXParameter.ParameterType.PositionControl:
							po.assertPositionControls();
							po.positionControls.addChild(p); ;
							break;

						case AXParameter.ParameterType.GeometryControl:
							po.assertGeometryControls();
							po.geometryControls.addChild(p);
							break;

						case AXParameter.ParameterType.TextureControl:
							break;

						default:
							p.ParentNode = po;

							p.parameters.Clear();
							// use the serializedParameters to create runtime AX.Parameters
							break;
					}


					indexedParameters.Add(p.Guid, p);
					if (p.exposeAsInterface)
					{
						addExposedParameter(p);
					}






					// set the live link (not serialized)
					// set these in the next step
					p.DependsOn = null;
					p.Dependents.Clear();
				}
				// Shapes - rig up all shapes in this po.
				if (po.shapes != null && po.shapes.Count > 0)
				{
					po.assertInputControls();
					for (int j = 0; j < po.shapes.Count; j++)
					{
						AXShape shape = po.shapes[j];
						shape.parametricObject = po;
						shape.ParentNode = po.inputControls;
						shape.Parent = po;
						// multiple inputs List
						for (int k = 0; k < shape.inputs.Count; k++)
						{
							AXParameter input = shape.inputs[k];
							rigUpShapeParameter(po, shape, input);
						}
						// specific outputs
						rigUpShapeParameter(po, shape, shape.difference);
						rigUpShapeParameter(po, shape, shape.differenceRail);
						rigUpShapeParameter(po, shape, shape.intersection);
						rigUpShapeParameter(po, shape, shape.intersectionRail);
						rigUpShapeParameter(po, shape, shape.union);
						rigUpShapeParameter(po, shape, shape.grouped);
					}
				}







				// HANDLES INDEX AND SETUP


				for (int j = 0; j < po.handles.Count; j++)
				{

					//Debug.Log("handle " + i + " " + po.handles [j].Guid);

					AXHandle h = po.handles[j];
					po.handles[j].parametricObject = po;


					//Debug.Log("+++++++++++++>>>>>>>>>>>> " + po.handles [j].parametricObject);

					// HANDLES INDEX
					// Add to the index of handles.
					if (!indexedHandles.ContainsKey(h.Guid))
						indexedHandles.Add(h.Guid, h);



					//					if (h.exposeForRuntime) {
					//						AXHandleRuntimeAlias rth = addHandleRuntimeAlias(h);
					//
					//						Debug.Log("YOA: "+rth.handleGuid);
					//						Debug.Log("YOB: "+rth.handle);
					//					}
					//					else
					//					{
					//						if (h.runtimeHandle != null)
					//						{
					//							// destroy it.
					//
					//						}
					//					}

				}

				po.getAllInputParameters(true);


			}  // each po







			// 2. Traverse index and set up references

			foreach (KeyValuePair<string, AXParameter> entry in indexedParameters)
			{
				// do something with entry.Value or entry.Key
				AXParameter p = entry.Value;

				if (!String.IsNullOrEmpty(p.dependsOnKey))
				{
					string dkey = p.dependsOnKey.Substring(p.dependsOnKey.IndexOf('%') + 1);

					if (indexedParameters.ContainsKey(dkey))
					{
						AXParameter dp = indexedParameters[dkey];
						p.DependsOn = dp;
						dp.addDependent(p);
					}
				}
			}

			for (int i = 0; i < parametricObjects.Count; i++)
			{
				AXParametricObject po = parametricObjects[i];
				po.onDeserialize();
			}



			for (int i = 0; i < relations.Count; i++)
			{
				AXRelation r = relations[i];
				r.setupFullReferencesInModel(this);
			}


			//Debug.Log("*********************************** cleanGraph");


			for (int i = 0; i < parametricObjects.Count; i++)
			{
				AXParametricObject po = parametricObjects[i];

				if (po == null || po.generator == null)
					continue;

				po.generator.pollInputParmetersAndSetUpLocalReferences();


				// GROUPERS


				if (!String.IsNullOrEmpty(po.grouperKey) && po.grouperKey != po.Guid)
				{
					AXParametricObject grouper = getParametricObjectByGUID(po.grouperKey);

					if (grouper != null)
					{

						grouper.addGroupee(po);
					}

				}



			}

			//			if (currentWorkingGroupPO != null)
			//				currentWorkingGroupPO = getParametricObjectByGUID(currentWorkingGroupPO.Guid);

			if (!string.IsNullOrEmpty(currentWorkingGroupPOGUID))
			{
				currentWorkingGroupPO = getParametricObjectByGUID(currentWorkingGroupPOGUID);
			}


			// MATERIALS
			//Debug.Log("remap");
			remapMaterialTools();




			for (int i = 0; i < parametricObjects.Count; i++)
			{
				if (parametricObjects[i].is3D() && parametricObjects[i].prototypeGameObject != null)
				{
					AXPrototype proto = (AXPrototype)parametricObjects[i].prototypeGameObject.GetComponent("AXPrototype");

					if (proto != null && !proto.parametricObjects.Contains(parametricObjects[i]))
						proto.parametricObjects.Add(parametricObjects[i]);

				}

			}
			/*
			List<AXParametricObject> heads = getAllHeadPOs();
			for (int i = 0; i < heads.Count; i++) 
			{

				heads [i].generator.adjustWorldMatrices();
			}
			 */

			// Set up references for exposedParameterAliases
			for (int i = 0; i < exposedParameterAliases.Count; i++)
			{
				AXParameterAlias pa = exposedParameterAliases[i];
				pa.parameter = getParameterByGUID(pa.Guid);
			}



			// LINK UP RUNTIME HANDLES

			AXRuntimeHandleBehavior[] rhbs = runtimeHandlesGameObjects.GetComponentsInChildren<AXRuntimeHandleBehavior>(true);

			for (int j = 0; j < rhbs.Length; j++)
			{
				if (rhbs[j] != null && rhbs[j].gameObject != null)
				{
					rhbs[j].handle = getHandleByGUID(rhbs[j].handleGuid);

				}
			}

			for (int i = 0; i < runtimeHandleAliases.Count; i++)
			{
				AXHandleRuntimeAlias rha = runtimeHandleAliases[i];

				rha.handle = getHandleByGUID(rha.handleGuid);

			}






		}








		public void rigUpShapeParameter(AXParametricObject po, AXShape shape, AXParameter pm)
		{
			if (pm == null)
				return;

			//Debug.Log ("rigUpShapeParameter: " + pm.Name);
			pm.Parent = po; //depricated

			pm.parametricObject = po;
			pm.ParentNode = shape;

			if (!indexedParameters.ContainsKey(pm.Guid))
				indexedParameters.Add(pm.Guid, pm);


			pm.parameters.Clear();


		}

		public void pollAllInputParmeters()
		{
			for (int i = 0; i < parametricObjects.Count; i++)
			{
				if (parametricObjects[i] == null || parametricObjects[i].generator == null)
					continue;

				parametricObjects[i].generator.pollInputParmetersAndSetUpLocalReferences();
			}

		}




		public bool hasRelation(AXRelation rel)
		{
			foreach (AXRelation r in relations)
			{
				if ((r.pA_guid == rel.pA_guid && r.pB_guid == rel.pB_guid) || (r.pA_guid == rel.pB_guid && r.pB_guid == rel.pA_guid))
					return true; // relation already exists.
			}
			return false;
		}

		public void addRelationJSON(string jn_r)
		{
			AXRelation r = AXRelation.fromJSON(jn_r);

			if (relations == null)
				relations = new List<AXRelation>();

			if (hasRelation(r))
				return;

			r.setupReferencesInModel(this);
		}

		public AXRelation relate(AXParameter pA, AXParameter pB)
		{
			//Debug.Log("******** Model::relate ************************* " + pA.Parent.Name+"."+pA.Name + " ["+pA.Guid+"] <--> " + pB.Parent.Name+"."+pB.Name + " ["+pB.Guid+"] ");
			if (pA == null || pB == null)
				return null;

			if (relations == null)
				relations = new List<AXRelation>();


			// 1. Check if relation already exists		 
			foreach (AXRelation r in relations)
			{
				if ((r.pA_guid == pA.Guid && r.pB_guid == pB.Guid) || (r.pB_guid == pA.Guid && r.pA_guid == pB.Guid))
				{

					Debug.Log("relation already exists");
					return null; // relation already exists.

				}
			}

			//Debug.Log ("Model::relate : Create relation ");
			//Debug.Log (" Before ... pA.relations.Count = " + pA.relations.Count + ", " + "pB.relations.Count = " + pB.relations.Count);

			// create relation
			AXRelation relation = new AXRelation(pA, pB);

			relations.Add(relation);

			//Don't need to use relation.setupReferencesInModel(this) here since we already have the parameters...

			//Debug.Log ("Model::relate - relations.add pA="+pA+" : " + pA.Guid);		
			pA.relations.Add(relation);


			//Debug.Log ("Model::relate - relations.add pB="+pB+" : " + pB.Guid);
			pB.relations.Add(relation);


			//Debug.Log ("Model::relate : relation created: " + pA.Parent.Name+"."+pA.Name +":"+pA.Guid + "  to  " + pB.Parent.Name+"."+pB.Name+":"+pB.Guid);

			//Debug.Log (" After ... pA.relations.Count = " + pA.relations.Count + ", " + "pB.relations.Count = " + pB.relations.Count);


			pA.parametricObject.setInputsStatus();
			pB.parametricObject.setInputsStatus();




			return relation;
		}

		public void unrelate(AXRelation relation)
		{

			//Debug.Log ("UNRELATE: "+relation);
			if (relation != null && relations.Contains(relation))
			{
				// Debug.Log ("Delete");
				if (relation.pA != null && relation.pA.relations != null)
					relation.pA.relations.Remove(relation);

				if (relation.pB != null && relation.pB.relations != null)
					relation.pB.relations.Remove(relation);

				relations.Remove(relation);
			}

		}





		public void addParametricObject(AXParametricObject po)
		{
			//Debug.Log ("addParametricObject(AXParametricObject po)");
			parametricObjects.Add(po);
			po.model = this;
			po.instantiateGenerator();
			indexPO(po);


		}



		public AXParametricObject addParametricObject(string type, bool panTo = false, AXParametricObject basedOnPO = null)
		{

			//Debug.Log ("addParametricObject("+type+")");
			AXParametricObject po = new AXParametricObject(type, type + "_" + parametricObjects.Count);


			if (basedOnPO != null)
			{
				//Debug.Log ("****** * * * * * ********* * * * * *********");

				po.intValue("Axis", basedOnPO.intValue("Axis"));

				po.intValue("Align_X", basedOnPO.intValue("Align_X"));
				po.intValue("Align_Y", basedOnPO.intValue("Align_Y"));
				po.intValue("Align_Z", basedOnPO.intValue("Align_Z"));


				po.floatValue("Trans_X", basedOnPO.floatValue("Trans_X"));
				po.floatValue("Trans_Y", basedOnPO.floatValue("Trans_Y"));
				po.floatValue("Trans_Z", basedOnPO.floatValue("Trans_Z"));


			}




			// select this parametricObject
			//selectedParametricObjectIndex = parametricObjects.Count-1;

			po.model = this;

			// add a Generator to this - 
			// Generators are not seialized but always only runtime.
			po.instantiateGenerator();

			po.generator.init_parametricObject();



			//Debug.Log ("addParametricObject("+type+") HERE");
			po.generator.pollInputParmetersAndSetUpLocalReferences();
			po.generator.pollControlValuesFromParmeters();
			po.generator.adjustWorldMatrices();

			po.rect.width = po.generator.minNodePaletteWidth - 4;

			po.showControls = true;

			po.showLogic = true;


			parametricObjects.Add(po);

			renderThumbnails();

			//if (panTo)
			//selectAndPanToPO(po);

			selectPO(po);
			//remapMaterialTools();
			//generate (); don't do this here because there may be a number of interdependent po's being created
			return po;
		}



		public AXParametricObject createNode(string nodeName, bool panTo = true, AXParametricObject basedOnPO = null, float pos_x = 0, float pos_y = 0)
		{

			//Debug.Log("createNode " + nodeName);
			// Does the requested node type exist in the assembly?
			Type typeBasedOnString = ArchimatixUtils.AXGetType(nodeName);

			if (typeBasedOnString == null)
				return null;

			// CREATE PARAMETRIC_OBJECT
			AXParametricObject mostRecentPO = recentlySelectedPO;
			AXParametricObject npo = addParametricObject(nodeName, panTo, basedOnPO);



			if (mostRecentPO != null)
			{
				pos_x = (int)(mostRecentPO.rect.x + mostRecentPO.rect.width + 50);
				pos_y = (int)mostRecentPO.rect.y;
			}
			npo.rect = new Rect(pos_x, pos_y, (npo.generator.minNodePaletteWidth - 2), 100);

			selectedPOs.Clear();
			selectPO(npo);
			cycleSelectedPO = npo;

			//npo.showInputs = true;
			npo.showHandles = false;
			npo.showControls = false;
			npo.showLogic = false;

			if (npo.inputControls != null)
				npo.inputControls.isOpen = true;



			// GENERATOR
			// introduce the generator and the parametricObject!
			// (this double link is not serialized, but recreated at PO::deserialization
			//Generator generator = (AX.Generators.Generator) Activator.CreateInstance(typeBasedOnString);

			//npo.generator = generator;
			//generator.parametricObject = npo;

			//npo.generator.init_parametricObject();

			//Debug.Log(npo.Name + " call pollInputParmetersAndSetUpLocalReferences ****");


			indexPO(npo);


			cleanGraph();

			autobuild();

			recentlySelectedPO = npo;

			return npo;


		}

		public AXParametricObject addNode(string nodeName, bool panTo = true, AXParametricObject basedOnPO = null)
		{
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(this, "Add Generator named " + nodeName);
#endif

			AXParametricObject npo = createNode(nodeName, panTo, basedOnPO);

			if (basedOnPO != null)
				basedOnPO.copyTransformTo(npo);


			return npo;


		}


		// Create a new ShapeDistributor, transfer all src_p's depndents to the new po's output
		// and then connect the src_p output to the ShapeDistributor input
		public void insertShapeDistributor(AXParameter src_p, AXParameter new_input_p = null)
		{
			// Create new PO of type ShapeDistributor

			AXParametricObject distributorPO = addNode("ShapeDistributor");


			// Connect the OutputParameterBeingDragged to the new distributorPO
			AXParameter output = distributorPO.generator.P_Output;

			distributorPO.generator.P_Input.makeDependentOn(src_p);

			distributorPO.rect = src_p.parametricObject.rect;
			distributorPO.rect.x += 250;

			if (new_input_p != null)
			{
				new_input_p.shapeState = src_p.shapeState;
				new_input_p.makeDependentOn(output);
			}

			for (int i = 0; i < src_p.Dependents.Count; i++)
			{
				AXParameter d = src_p.Dependents[i];
				if (d != distributorPO.generator.P_Input)
					d.makeDependentOn(output);
			}

			setAltered(distributorPO);

			if (new_input_p != null)
			{


				setAltered(new_input_p.parametricObject);

			}

		}









		public void indexPO(AXParametricObject po)
		{
			//Debug.Log ("ADDING PO: Add all p's to the index ");
			foreach (AXParameter p in po.parameters)
			{
				if (!indexedParameters.ContainsKey(p.Guid))
					indexedParameters.Add(p.Guid, p);
			}
			if (po.shapes != null)
			{
				foreach (AXShape shape in po.shapes)
				{
					foreach (AXParameter input in shape.inputs)
					{
						if (!indexedParameters.ContainsKey(input.Guid)) indexedParameters.Add(input.Guid, input);

						foreach (AXParameter subp in input.parameters)
							if (!indexedParameters.ContainsKey(subp.Guid)) indexedParameters.Add(subp.Guid, subp);
					}
					indexedParameters.Add(shape.difference.Guid, shape.difference);
					indexedParameters.Add(shape.differenceRail.Guid, shape.differenceRail);
					indexedParameters.Add(shape.intersection.Guid, shape.intersection);
					indexedParameters.Add(shape.intersectionRail.Guid, shape.intersectionRail);
					indexedParameters.Add(shape.union.Guid, shape.union);
					indexedParameters.Add(shape.grouped.Guid, shape.grouped);


				}
			}

		}

		public void delistParameter(AXParameter p)
		{
			removeExposedParameter(p);

			if (p.DependsOn != null)
				p.DependsOn.Dependents.Remove(p);

			foreach (AXParameter dp in p.Dependents)
				dp.DependsOn = null;

			// remove from parameter registry
			indexedParameters.Remove(p.Guid);

			// remove any relations with this parameter...
			for (int i = 0; i < relations.Count; i++)
			{
				AXRelation r = relations[i];
				if (r.pA_guid == p.Guid || r.pB_guid == p.Guid)
					unrelate(r);
			}

		}







		public void removeParametricObject(AXParametricObject po)
		{
			// remove any dependencies in the graph

			//Debug.Log("removeParametricObject " + po.Name);
			foreach (AXParameter p in po.parameters)
				delistParameter(p);

			if (po.shapes != null)
			{
				foreach (AXShape shape in po.shapes)
				{
					foreach (AXParameter input in shape.inputs)
					{
						foreach (AXParameter subp in input.parameters)
						{
							delistParameter(subp);
						}
						delistParameter(input);
					}
					delistParameter(shape.difference);
					delistParameter(shape.differenceRail);
					delistParameter(shape.intersection);
					delistParameter(shape.intersectionRail);
					delistParameter(shape.union);
					delistParameter(shape.grouped);
				}
			}

			//Debug.Log("REMOVE " + po.Name);
			parametricObjects.Remove(po);
			po = null;

		}






		#region PO SELECTION

		// SELECTION OPERATIONS

		public void addActiveFreeCurve(AXParametricObject po)
		{
			if (!activeFreeCurves.Contains(po))
				activeFreeCurves.Add(po);
		}

		public void deselectAll()
		{
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(this, "Deselect All");
#endif
			selectedPOs.Clear();


			clearActiveFreeCurves();
		}

		public void selectAll()
		{
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(this, "Select All");
#endif

			selectedPOs.Clear();

			for (int i = 0; i < parametricObjects.Count; i++)
			{
				//Debug.Log(parametricObjects[i].Name + " :: " + parametricObjects[i].isOpen);
				if (parametricObjects[i].isOpen)
				{
					selectPO(parametricObjects[i]);//selectedPOs.Add(parametricObjects[i]);
					if ((parametricObjects[i].generator is FreeCurve || parametricObjects[i].generator is FreeCurve3D) && !activeFreeCurves.Contains(parametricObjects[i]))
						activeFreeCurves.Add(parametricObjects[i]);
				}
			}
		}




		public void selectAllVisibleInGroup(AXParametricObject group_po)
		{
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(this, "Select All");
#endif

			selectedPOs.Clear();

			for (int i = 0; i < parametricObjects.Count; i++)
			{
				//Debug.Log(parametricObjects[i].Name + " :: " + parametricObjects[i].isOpen);
				if (parametricObjects[i].isOpen && parametricObjects[i].grouper == group_po)
				{
					selectPO(parametricObjects[i]);//selectedPOs.Add(parametricObjects[i]);
					if ((parametricObjects[i].generator is FreeCurve || parametricObjects[i].generator is FreeCurve3D) && !activeFreeCurves.Contains(parametricObjects[i]))
						activeFreeCurves.Add(parametricObjects[i]);
				}
			}
		}






		public void selectOnlyPO(AXParametricObject po)
		{
			if (selectedPOs.Contains(po))
				return;

			selectedPOs.Clear();
			selectPO(po);

			clearActiveFreeCurves();

			if (po.generator is FreeCurve || po.generator is FreeCurve3D)
				activeFreeCurves.Add(po);



			//	Debug.Log ("+++++++++++++++++++++++++++++++++++++++++ selecting: " + po.Name + " === " + selectedPOs.Count);

		}

		public void selectPOandSubMesh(AXParametricObject po, string subItemAddress)
		{
			po.subItemSelect(subItemAddress);
			selectedPOs.Clear();
			clearActiveFreeCurves();
			selectPO(po);


		}

		public void selectAndPanToPO(AXParametricObject po)
		{
			if (selectedPOs.Contains(po))
				return;


			selectedPOs.Clear();
			clearActiveFreeCurves();
			selectPO(po);

			beginPanningToRect(po.rect);

		}


		public void beginPanningToPoint(Vector2 pt)
		{
			panToPoint = pt;
			isPanningToPoint = true;

		}
		public void beginPanningToRect(Rect r)
		{

			panToPoint = r.center;
			isPanningToPoint = true;


		}
		public void endPanningToPoint()
		{

			isPanningToPoint = false;

		}


		public void selectPO(AXParametricObject po)
		{

			if (po == null)
				return;

			clearActiveFreeCurves();

			recentlySelectedPO = mostRecentlySelectedPO;
			mostRecentlySelectedPO = po;
			cycleSelectedPO = po;
			poSelectionHistory.Add(po);

			mostRecentlyInstantiatedPO = null;


			if (!selectedPOs.Contains(po))
			{
				selectedPOs.Add(po);
				//Debug.Log ("+++++++++++++++++++++++++++++++++++++++++ "+ selectedPOs.Count+" selecting: " + po.Name);

			}

			if ((po.generator is FreeCurve || po.generator is FreeCurve3D) && !activeFreeCurves.Contains(po))
				activeFreeCurves.Add(po);

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif


		}
		public void deselectPO(AXParametricObject po)
		{
			if (po == null)
				return;
			//Debug.Log ("+++++++++++++++++++++++++++++++++++++++++ deselecting: " + po.Name);

			if (selectedPOs.Contains(po))
				selectedPOs.Remove(po);

			if (activeFreeCurves.Contains(po))
				activeFreeCurves.Remove(po);

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif
		}

		public AXParametricObject selectPOByGuid(string guid)
		{
			selectedPOs.Clear();
			foreach (AXParametricObject po in parametricObjects)
			{
				if (po.Guid == guid)
				{
					selectPO(po);
					return po;
				}
			}

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif

			return null;
		}


		public AXParametricObject selectFirstHeadPO()
		{
			foreach (AXParametricObject po in parametricObjects)
			{
				if (po.isHead() && po.grouper == null)
					return po;
			}

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif

			return null;

		}





		#endregion





		/* CYCLE-SELECT FROM A PO
		 * Go through consumers and see if any are selected. 
		 * If so, select their consumer or select this.
		 */
		public void cycleSelectPOWithGUID(AXGameObject axgo)
		{
			// there has been a click event on a gameobject in the space
			AXParametricObject po = getParametricObjectByGUID(axgo.makerPO_GUID);

			if (po == null)
				return;

			Debug.Log("CYCLE_SELECT::maker: " + po.Name);

			// Go up the transform hierarchy
			if (cycleSelectedAXGO != null)
			{

			}
			else if (clickSelectedAXGO != null)
			{

			}




			AXParametricObject selectedConsumer = getSelectedConsumer(po);

			if (selectedConsumer != null)
			{
				AXParametricObject consumerToSelect = selectedConsumer.getConsumer();

				if (consumerToSelect != null)
					selectPOandSubMesh(consumerToSelect, axgo.consumerAddress);
				else
					selectPOandSubMesh(po, axgo.consumerAddress);
			}
			else
			{
				// nothing downstream is selected
				if (isSelected(po))
				{
					AXParametricObject consumer = po.getConsumer();
					if (consumer != null)
						selectPOandSubMesh(consumer, axgo.consumerAddress);
					else
						selectPOandSubMesh(po, axgo.consumerAddress);
				}
				else
					selectPOandSubMesh(po, axgo.consumerAddress);
			}


		}

		public AXParametricObject getSelectedConsumer(AXParametricObject po, int genlev = 0)
		{
			if (genlev > 7)
				return null;

			AXParametricObject consumer = po.getConsumer();

			if (consumer == null)
				return null;

			if (isSelected(consumer))
				return consumer;

			// no selected at this level? Then continue downstream
			return getSelectedConsumer(consumer, genlev++);
		}



		public AXParametricObject getFirstSelected2DPO()
		{
			AXParametricObject shape = null;
			foreach (AXParametricObject po in selectedPOs)
			{
				if (po.is2D())
				{
					shape = po;
					break;
				}
			}
			return shape;


		}




		public bool isSelected(AXParametricObject po)
		{
			if (selectedPOs.Contains(po))
				return true;

			return false;
		}




		// use guid traversal!
		public void selectConnected(AXParametricObject po)
		{

			if (isSelected(po))
				return;

			//Debug.Log ("Select connected::: "+ po.Name);
			selectPO(po);

			//List<AXParameter> pmList = po.getAllInputMeshes();
			List<AXParameter> pList = po.getAllInputParameters();

			//Debug.Log ("pList.Count="+pList.Count);

			//foreach(AXParameter p in po.parameters)
			foreach (AXParameter p in pList)
			{
				if (p.Name.Contains("External") || (p.PType != AXParameter.ParameterType.Input))
					continue;

				if (p.Dependents != null)
				{
					foreach (AXParameter d in p.Dependents)
					{
						if (d.PType == AXParameter.ParameterType.Output || d.Type == AXParameter.DataType.Mesh || d.Type == AXParameter.DataType.Spline || d.Type == AXParameter.DataType.Curve3D)
							selectConnected(d.Parent);
					}
					if (p.DependsOn != null)
						selectConnected(p.DependsOn.Parent);




				}
			}

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif
			// INPUT SHAPES


		}


		public void deletePO(AXParametricObject po)
		{

			if (activeFreeCurves.Contains(po))
				activeFreeCurves.Remove(po);

			po.generator.deleteRequested();

			po.removeInputDependencies();
			po.removeOutputDependencies();

			po.deleteRelations();

			if (po.grouper != null)
				po.grouper.Groupees.Remove(po);

			//Transform t = transform.FindChild(po.Guid);
			//if (t != null)
			//	DestroyImmediate(t.gameObject);

			removeParametricObject(po);

			selectedPOs.Remove(po);

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif

		}

		public void deleteSelectedPOsAndInputs()
		{
			List<AXParametricObject> poList = new List<AXParametricObject>(selectedPOs);
			foreach (AXParametricObject po in poList)
			{

				deselectPO(po);
				selectConnected(po);
			}
			deleteSelectedPOs();
		}


		public void deleteSelectedPOs()
		{
			List<AXParametricObject> poList = new List<AXParametricObject>(selectedPOs);

			// add any hidden subparts to the list

			List<AXParametricObject> subpartsToAdd = new List<AXParametricObject>();
			foreach (AXParametricObject po in poList)
			{
				if (po.allInputsAreStowed())
				{
					po.gatherSubnodes();

					// add its parts to poList, check to make sure no dups
					foreach (AXParametricObject subparts in po.subnodes)
					{
						if (!subpartsToAdd.Contains(subparts))
							subpartsToAdd.Add(subparts);
					}
				}

			}
			foreach (AXParametricObject subpart in subpartsToAdd)
			{
				if (!poList.Contains(subpart))
					poList.Add(subpart);
			}



			foreach (AXParametricObject po in poList)
			{
				deletePO(po);
				//po.removeDependencies();
				//po.deleteRelations();

				//removeParametricObject(po);



			}

			selectedPOs.Clear();

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif


			renderThumbnails();

		}




		public List<AXParametricObject> getHeadPOsFromList(List<AXParametricObject> poList)
		{
			List<AXParametricObject> retList = new List<AXParametricObject>();

			foreach (AXParametricObject po in poList)
			{
				Debug.Log("po " + po.Name + " isHead=" + po.isHead());
				if (po.isHead())
					retList.Add(po);
			}

			return retList;
		}


		public void instanceSelectedPOs()
		{
			instancePOsFromList(selectedPOs);
		}

		public void instanceHeadsOfPOsFromList(List<AXParametricObject> poList)
		{

			instancePOsFromList(getHeadPOsFromList(poList));
		}





		public void instancePOsFromList(List<AXParametricObject> poList)
		{
			Debug.Log("...................................instancePOsFromList with " + poList.Count + " items");

			selectedPOs = new List<AXParametricObject>();

			foreach (AXParametricObject src_po in poList)
			{
				//if (! src_po.isHead ())
				//	continue;

				AXParametricObject inst_po = addParametricObject("Instance");

				//inst_po.instantiateGenerator();


				Type typeBasedOnString = System.Type.GetType("AX.Generators.Instance");
				//Type typeBasedOnString = ArchimatixUtils.GetType("Instance");

				AX.Generators.Generator generator = (AX.Generators.Generator)Activator.CreateInstance(typeBasedOnString);

				// double-link the genreator and the instance 
				inst_po.generator = generator;
				generator.setParametricObject(inst_po);


				// let the generator initialize its inst_po
				generator.init_parametricObject();

				inst_po.intValue("Axis", src_po.intValue("Axis"));

				src_po.copyTransformTo(inst_po);


				AXParameter src_output_p = src_po.getParameter("Output Mesh");

				// if this is an instance of an instance, for one more down the line to the src of that one.
				if (src_po.GeneratorTypeString == "Instance")
					src_output_p = src_po.getParameter("Input Mesh").DependsOn.Parent.getParameter("Output Mesh");

				AXParameter inst_input_p = inst_po.getParameter("Input Mesh");

				// link the original to the new instance.
				inst_input_p.makeDependentOn(src_output_p);

				autobuild();

				selectPO(inst_po);
			}

		}


		public void replicateSelectedPOs()
		{
			List<AXParametricObject> poList = new List<AXParametricObject>(selectedPOs);

			selectedPOs = new List<AXParametricObject>();

			foreach (AXParametricObject po in poList)
			{
				AXParameter output_p = po.generator.P_Output;//po.getParameter("Output Mesh");

				// can't replicate something that does not have output
				if (output_p == null)
					continue;

				AXParametricObject npo = addParametricObject("Replicant");

				Type typeBasedOnString = System.Type.GetType("AX.Generators.Replicant");
				//Type typeBasedOnString = ArchimatixUtils.GetType("Replicant");

				AX.Generators.Generator generator = (AX.Generators.Generator)Activator.CreateInstance(typeBasedOnString);
				npo.generator = generator;
				generator.setParametricObject(npo);

				// let the generator initialize the po
				generator.init_parametricObject();



				// clone control parameters and handles
				npo.syncControlsOfPTypeWith(AXParameter.ParameterType.PositionControl, po);
				npo.syncControlsOfPTypeWith(AXParameter.ParameterType.TextureControl, po);
				npo.syncControlsOfPTypeWith(AXParameter.ParameterType.GeometryControl, po);
				npo.syncHandlesWith(po);


				npo.getParameter("Input Mesh").makeDependentOn(output_p);

				npo.instantiateGenerator();

				npo.hasCustomLogic = false;
				npo.code = po.code;
				npo.showControls = po.showControls;
				npo.showHandles = po.showHandles;
				npo.showLogic = po.showLogic;
				npo.axMat = po.axMat;
				npo.axTex = po.axTex;
				npo.isOpen = po.isOpen;
				npo.inputsStowed = false;

				npo.rect = new Rect(po.rect.x + 100, po.rect.y + 100, po.rect.width, po.rect.height);

				autobuild();

				selectPO(npo);
			}





		}


		// DUPLICATE_SELECTED_POs
		public void duplicateSelectedPOs()
		{
			Debug.Log("Duplicate : " + traversalIsLocked);
			/*
			if (traversalIsLocked)
			{
				Debug.Log ("Model: traversalIsLocked... try again later");
				return;
			}
			*/
			//duplicatePOsInList(selectedPOs);


		}

		// use guid traversal!
		/*
		public void duplicatePOsInList(List<AXParametricObject> poList) 
		{
			AXParametricObject po = poList[poList.Count-1];
			
			//Debug.Log (Library.poWithSubNodes_2_JSON(po, true));
			
			.pasteFromJSONstring(Library.poWithSubNodes_2_JSON(po, true));
			
		

			// 1. Gather po's

			// see if any items on the list are folded inputs,
			// if so, add those to the poList
			List<AXParametricObject> subpartsToAdd = new List<AXParametricObject>();
			foreach (AXParametricObject po in poList)
			{
				if (po.inputsStowed)
				{
					po.gatherPartsFromInputs();
					// add its parts to poList, check to make sure no dups
					foreach (AXParametricObject subparts in po.subparts)
						if (! subpartsToAdd.Contains(subparts))
							subpartsToAdd.Add (subparts);
				}
			}
			foreach (AXParametricObject subpart in subpartsToAdd)
				if (! poList.Contains(subpart))
					poList.Add (subpart);

			
			
			
			// Now we have all the po's possible in the poList, including hidden po's

			Dictionary<string, AXParametricObject> newPOs = new Dictionary<string, AXParametricObject>();
			
			
			// this temporary Dictionary maps the source po to the new po's guids
			Dictionary<string, string> guidMap = new Dictionary<string, string>();
			
			Debug.Log ("DUPLICATE IN poList:"+poList.Count);
			
			for (int i = 0; i < poList.Count; i++) {
				AXParametricObject po = poList [i];
				
				
				// do something with entry.Value or entry.Key
				Debug.Log ("duplicate: " + po.Name);
				
				// clone
				AXParametricObject npo = addParametricObject(po.Type);
				
				foreach (AXParameter p in po.parameters)
				{
					//Debug.Log ("copy parameter: "+ p.Name);
					AXParameter np = npo.getParameter(p.Name);
					if (np == null)
						npo.addParameter(p.clone());
					//Debug.Log ("tmp");
					else 
						np.copyValues(p);
				}
				
				foreach(AXHandle h in po.handles)
				{
					npo.addHandle(h.clone());
				}
				
				npo.instantiateGenerator();
				
				npo.hasCustomLogic 	= po.hasCustomLogic;
				npo.code 			= po.code;
				npo.showControls 	= po.showControls;
				npo.showHandles 	= po.showHandles;
				npo.showLogic 		= po.showLogic;
				npo.Mat 			= po.Mat;
				npo.isStowed 		= po.isStowed;
				npo.inputsStowed	= po.inputsStowed; 
				
				npo.rect = new Rect(po.rect.x+100, po.rect.y+100, po.rect.width, po.rect.height);
				Debug.Log (npo.rect);
				newPOs.Add(npo.Guid, npo);
				Debug.Log ("DUP 1");
				guidMap.Add(po.Guid, npo.Guid);
				Debug.Log ("DUP 2");
			}
			Debug.Log ("DUP 2.1");
			// connect Guids
			// if po Guid in dictionary is to an object in the poList, link to mapped, else copy dependsOn guid.
		
			for (int i = 0; i < poList.Count; i++) {
				AXParametricObject po = poList [i];
				Debug.Log ("DUP 3");
				// get the dup npo:
				AXParametricObject npo = newPOs [guidMap [po.Guid]];
				Debug.Log ("DUP 4");
				foreach (AXParameter p in po.parameters) {
					Debug.Log ("DUP 5");
					if (p.DependsOn != null) {
						// get dup parameter from newPO
						AXParameter np = npo.getParameter (p.Name);
						// copy?Dictionary
						//if ( isSelected(p.DependsOn.Parent))
						if (poList.Contains (p.DependsOn.Parent)) {
							//AXParameter ndo = p.DependsOn.Parent().getParameter(p.DependsOn.Name);
							AXParametricObject ndoParent = newPOs [guidMap [p.DependsOn.Parent.Guid]];
							AXParameter nDependsOn = ndoParent.getParameter (p.DependsOn.Name);
							np.makeDependentOn (nDependsOn);
						}
						else {
							np.makeDependentOn (p.DependsOn);
						}
					}
				}
			}
			Debug.Log ("DUP 6");
			
			// select the duplicates...
			
			selectedPOs.Clear();
			
			// select duplicated items... but not stowed items
			foreach(KeyValuePair<string, AXParametricObject> entry in newPOs)
			{
				AXParametricObject npo = entry.Value;
				if (! npo.isStowed)
					selectPO(npo);
			}
			
			
		}
		*/


		/// <summary>
		/// Adds the and rig up POs from a poList
		/// The assumption is that the pos need new GUIDs as
		/// they are coming from an axobj file.
		/// </summary>
		/// <param name="poList">Po list.</param>
		/// <param name="relList">Rel list.</param>
		public void addAndRigUp(List<AXParametricObject> poList, List<AXRelation> relList)
		{
			// 1. re-map guids
			// 2. traverse graph and add live links

			reassignGuids(ref poList, ref relList);

			foreach (AXParametricObject po in poList)
			{


				addParametricObject(po);
				po.isAltered = true;
				//Debug.Log ("++++++++++++++++++++ added po :" + po.Guid + " " + po.Name);

				// Add CurrentWorkingPO to all objects that don't have an GUID
				if (!(po.generator is IReplica) && currentWorkingGroupPO != null && currentWorkingGroupPO.Name != po.Name && String.IsNullOrEmpty(po.grouperKey))
					po.grouperKey = currentWorkingGroupPO.Guid;

			}


			for (int i = 0; i < relList.Count; i++)
			{
				//Debug.Log ("["+i+"] relating...");
				AXRelation r = relList[i];

				r.setupFullReferencesInModel(this);


				relations.Add(r);
				/*
				AXRelation newRel = relate (r.pA, r.pB);
				newRel.expression_AB = r.expression_AB;
				newRel.expression_BA = r.expression_BA;
				*/
			}



			cleanGraph();



			selectPO(poList[poList.Count - 1]);
		}


		public void reassignGuids(ref List<AXParametricObject> poList, ref List<AXRelation> relList)
		{
			/* Give each po and its parameters a new guid and 
			 * make sure and dependsOn_guids have been remapped accordingly.
			 * 
			 * * Assume that only dependsOn links within this list of po's is supported.
			 * 
			 */

			// This temporary Dictionary maps the original guid to a new guid
			Dictionary<string, string> guidMap = new Dictionary<string, string>();

			// Populate the dictionary with new guids for each guid.
			foreach (AXParametricObject po in poList)
			{
				// get the duplicate npo:

				guidMap[po.Guid] = System.Guid.NewGuid().ToString();
				//Debug.Log(po.Name +": "+po.Guid + "   {"+guidMap[po.Guid]+"}");
				po.Guid = guidMap[po.Guid];

				if (!string.IsNullOrEmpty(po.grouperKey) && guidMap.ContainsKey(po.grouperKey))
					po.grouperKey = guidMap[po.grouperKey];

				foreach (AXParameter p in po.parameters)
				{
					guidMap[p.Guid] = System.Guid.NewGuid().ToString();
					p.Guid = guidMap[p.Guid];



					foreach (AXParameter sp in p.parameters)
						sp.Guid = guidMap[sp.Guid];
					//Debug.Log (po.Name +":  ----- % -----" + p.Name + " " + p.Guid + " {"+guidMap[p.Guid]+"}");
				}

				// expressions
				/*
				foreach (AXParameter p in po.parameters)
				{	
					foreach (string exp in p.expressions)
					{
						//Debug.Log (p.Name + " exp: " + exp);
					}
				}
				*/

				if (po.shapes != null)
				{

					foreach (AXShape shape in po.shapes)
					{
						// inputs
						foreach (AXParameter p in shape.inputs)
						{
							guidMap[p.Guid] = System.Guid.NewGuid().ToString();
							p.Guid = guidMap[p.Guid];
						}

						// outputs
						guidMap[shape.difference.Guid] = System.Guid.NewGuid().ToString();
						shape.difference.Guid = guidMap[shape.difference.Guid];

						guidMap[shape.differenceRail.Guid] = System.Guid.NewGuid().ToString();
						shape.differenceRail.Guid = guidMap[shape.differenceRail.Guid];

						guidMap[shape.intersection.Guid] = System.Guid.NewGuid().ToString();
						shape.intersection.Guid = guidMap[shape.intersection.Guid];

						guidMap[shape.intersectionRail.Guid] = System.Guid.NewGuid().ToString();
						shape.intersectionRail.Guid = guidMap[shape.intersectionRail.Guid];

						guidMap[shape.union.Guid] = System.Guid.NewGuid().ToString();
						shape.union.Guid = guidMap[shape.union.Guid];

						if (shape.grouped != null)
						{
							guidMap[shape.grouped.Guid] = System.Guid.NewGuid().ToString();
							shape.grouped.Guid = guidMap[shape.grouped.Guid];
						}


					}



				}


			}


			/*
			foreach(KeyValuePair<string, string> entry in guidMap)
			{
				Debug.Log (":DICT : "+entry.Key +" : " + entry.Value);
				////Debug.Log (":NEW: "+ entry.Value);
			}
			*/



			// Now that all the guids have been changed,
			// use the map to reassign any dependsOn guids...

			foreach (AXParametricObject po in poList)
				po.remapGuids(ref guidMap);


			// Also use the map to 
			// reassignGuids relations
			//Debug.Log ("REMAPPING relList: " + relList.Count);
			foreach (AXRelation r in relList)
			{
				//Debug.Log ("Remap r");

				r.remapGuids(ref guidMap);


			}




		}






		public void setRenderMode(RenderMode rmode)
		{
			bool isChangeOfState = (renderMode != rmode);

			renderMode = rmode;

			if (isChangeOfState)
			{
				//Debug.Log("WHOOOOA");
				//generate();

				//if (renderMode == RenderMode.DrawMesh)
				//clearGeneratedGameObjects();

				//else if (renderMode == RenderMode.GameObjects)
				//	createGameObjects();
			}

		}





		public void setAltered(AXParametricObject po)
		{
			if (!AlteredPOs.Contains(po))
			{
				AlteredPOs.Add(po);
			}
			po.isAltered = true;


		}

		public void setAlteredHead(AXParametricObject po)
		{
			//Debug.Log ("Model::setAlteredHead(): " +po.Name);
			if (!AlteredHeadPOs.Contains(po))
				AlteredHeadPOs.Add(po);
		}

		public void generateIfDirty()
		{
			if (isDirty)
				generate("generateIfDirty");

		}

		public void isAltered(int caller_id)
		{
			//Debug.Log("AXModel.isAltered ** ** ** ** caller_id="+caller_id);
			isAltered();
		}

		public void isAltered()
		{
			//Debug.Log("isAltered");
			buildStatus = BuildStatus.Generated;
			if (automaticModelRegeneration)
			{

				isDirty = true;

				renderMode = RenderMode.DrawMesh;


				if (canRegenerate)
				{
					//Debug.Log("generating on canRegenerate");
					generate();
				}
			}


			// #if UNITY_EDITOR
			// EditorUtility.SetDirty(gameObject);
			// #endif

			//SceneView.RepaintAll();

		}


		#region generate
		public void generate(string caller)
		{
			//Debug.Log ("AXModel::generate("+caller+")");
			generate();


		}


		public void generate()
		{
			//sw.milestone(  "BEGIN generate");

			//Debug.Log("generate");

			canRegenerate = false;


			//Debug.Log("Model::generate");
			//StopWatch sw = new StopWatch();

			//Debug.Log("GENERATE ================== AlterdPOs.Count = " + AlteredPOs.Count);
			//StopWatch sw2 = new StopWatch();

			//sw.restart();

			isGenerating = true;

			buildStatus = BuildStatus.Generated;
			renderMode = RenderMode.DrawMesh;



			//generateMeshes();
			generateAlteredPOs();


			isGenerating = false;

			if (AlteredPOs != null)
				AlteredPOs.Clear();

			if (AlteredHeadPOs != null)
				AlteredHeadPOs.Clear();

			//sw2.milestone("DONE");

			volume = 0;
			//calculateTotalVolume();


			//sw.dump();


			//sw2.dump();

			//Debug.Log(sw.stop());

			isDirty = false;

			if (Application.isPlaying)
				canRegenerate = true;

			if (ModelDidGenerate != null)
				ModelDidGenerate(this, new EventArgs());

		}



		public void deleteGameObjectsMadeByAlteredPOs()
		{
			// DELETE FORMER GAME OBJECTS AND THEIR MESHES
			if (isSpriteGenerator && generatedGameObjects != null)
			{
				SpriteRenderer sr = generatedGameObjects.GetComponentInChildren<SpriteRenderer>();
				if (sr != null && !Application.isPlaying)
					DestroyImmediate(sr.gameObject);
			}

			if (generatedGameObjects == null)
				return;

			if (gameObjectsToDelete == null)
				gameObjectsToDelete = new List<GameObject>();

			if (AlteredHeadPOs != null)
			{

				for (int i = 0; i < AlteredHeadPOs.Count; i++)
				{

					AXGameObject[] axgos = generatedGameObjects.GetComponentsInChildren<AXGameObject>();

					// TRAVERSE TO FIND GAME_OBJECTS OF ALTERED PO'

					bool someObjectsDestroyed = false;
					for (int j = 0; j < axgos.Length; j++)
					{
						if (axgos[j] != null && axgos[j].gameObject != null && axgos[j].makerPO_GUID == AlteredHeadPOs[i].Guid)
						{
							if (Application.isPlaying)
							{
								// store game_object to be destroyed in Update to avoid runtime flicker
								// between when the object is destroyed and the first DrawMesh is called.
								if (!gameObjectsToDelete.Contains(axgos[j].gameObject))
									gameObjectsToDelete.Add(axgos[j].gameObject);
							}
							else
							{
								// When in the editor, just destroy here.
								DestroyImmediate(axgos[j].gameObject);
								someObjectsDestroyed = true;
							}
						}
					}
					if (!Application.isPlaying && someObjectsDestroyed)
					{
						// This brilliant hack of using UnloadUnusedAssets comes from https://forum.unity3d.com/threads/why-is-destroy-and-then-null-not-giving-the-memory-back.28441/
						Resources.UnloadUnusedAssets();
					}

				}

			}
		}


		// GENERATE ALTERED POs

		public void generateAlteredPOs()
		{
			//Debug.Log("GENNEY_ALT: AlterdPOs.Count = " + AlteredPOs.Count );

			//sw.restart();
			//sw.milestone("start gen");


			if (AlteredPOs == null || AlteredPOs.Count == 0)
			{

				generateAllMeshes();
				//sw.milestone("done gen  ALL MESHES");
				//sw.dump();
				return;
			}
			buildStatus = BuildStatus.Generated;
			renderMode = RenderMode.DrawMesh;


			axmeshCache.reset();

			axRenderMeshCount = 0;

			// The current id of this generation
			//Debug.Log ("Model::generateAlteredPOs()[ ** START **]:                    "+genid);


			for (int i = 0; i < parametricObjects.Count; i++)
				parametricObjects[i].generator.parametersHaveBeenPolled = false;

			for (int i = 0; i < parametricObjects.Count; i++)
				parametricObjects[i].generator.pollControlValuesFromParmeters();



			// GATHER HEAD_POS THAT NEED TO BE REGENERATED
			// BASED ON THE ALTERED PO's - FINDOUT HOW MANY HEAD_POS HAVE BEEN AFFECTED


			foreach (AXParametricObject po in AlteredPOs)
			{
				//Debug.Log (po.Name + " ... [[[altered]]]");
				po.generator.setHeadsAltered();

			}



			//if (visitedPOs == null)
			visitedPOs = new List<AXParametricObject>();

			visitedPOs.Clear();



			for (int i = 0; i < AlteredHeadPOs.Count; i++)
			{

				//Debug.Log ("AH ["+i+"] "+AlteredHeadPOs[i].Name);


				// DELETE FORMER GAME OBJECTS AND THEIR MESHES


				AlteredHeadPOs[i].generateOutputNow(false, AlteredHeadPOs[i], true);

				//AlteredHeadPOs[i].generator.adjustWorldMatrices();
			}


			// RENDER_THUMBNAILS
			//Debug.Log("caching here visitedPOs = "+visitedPOs.Count  + ", alteredPOs="+AlteredPOs.Count);
			if (visitedPOs != null && visitedPOs.Count < 10)
			{
				//Debug.Log("+++++++++++++++++");		
				renderThumbnails();
			}


			// CLEAR THE ALTERED DESIGNATIONS
			for (int i = 0; i < parametricObjects.Count; i++)
				parametricObjects[i].isAltered = false;



			deleteGameObjectsMadeByAlteredPOs();


			AlteredPOs.Clear();
			AlteredHeadPOs.Clear();


			// There may be a better way to get this real time tally than going through all the po's in the model to sum the heads.
			stats_TriangleCount = 0;
			stats_VertCount = 0;

			for (int i = 0; i < parametricObjects.Count; i++)
			{
				if (parametricObjects[i].is3D() && parametricObjects[i].generator.isHead() && parametricObjects[i].grouper == null)
				{
					//Debug.Log(parametricObjects[i].Name +": " + parametricObjects[i].stats_VertCount);
					stats_VertCount += parametricObjects[i].stats_VertCount;
					stats_TriangleCount += parametricObjects[i].stats_TriangleCount;
				}
			}

			//sw.milestone("done gen altered");
			//sw.dump();


		}



		public void regenerateAXMeshes(bool doCacheTheThumbnails = true)
		{

			//Debug.Log(" % % % % % % % % % % % % % % % % % % % % % % %");
			axmeshCache.reset();
			axRenderMeshCount = 0;

			//if (visitedPOs == null)
			visitedPOs = new List<AXParametricObject>();

			visitedPOs.Clear();

			// TRAVERSE GRAPH 

			/// First init all values from parameters

			for (int i = 0; i < parametricObjects.Count; i++)
				if (parametricObjects[i] != null && parametricObjects[i].generator != null)
					parametricObjects[i].generator.parametersHaveBeenPolled = false;


			for (int i = 0; i < parametricObjects.Count; i++)
			{
				if (parametricObjects[i] != null && parametricObjects[i].generator != null)
					parametricObjects[i].generator.pollControlValuesFromParmeters();
			}

			List<AXParametricObject> allHeads = getAllHeadPOs();

			for (int i = 0; i < parametricObjects.Count; i++)
			{
				//Debug.Log("setAltered... "+ parametricObjects[i].Name);
				setAltered(parametricObjects[i]);

			}

			for (int i = 0; i < allHeads.Count; i++)
			{

				//Debug.Log ("AH ["+i+"] "+allHeads[i].Name);

				allHeads[i].generateOutputNow(false, allHeads[i], true);
			}


			if (doCacheTheThumbnails)
				renderThumbnails();

			// CLEAR THE ALTERED DESIGNATIONS
			for (int i = 0; i < parametricObjects.Count; i++)
				parametricObjects[i].isAltered = false;

			AlteredPOs.Clear();
			AlteredHeadPOs.Clear();

		}



		public void generateAllMeshes(bool doCacheTheThumbnails = true)
		{
			//if (Archimatix.doDebug)
			//Debug.Log("GENERATE ALL MESHES... " );


			// reset the arrray of all meshes to be rendered each editor update

			//for (int i=0; i<axRenderMeshCount;  i++)
			//DestroyImmediate(axRenderMeshes[i].mesh);

			clearGeneratedGameObjects();


			if (axmeshCache == null)
				return;

			axmeshCache.reset();


			axRenderMeshCount = 0;

			//Debug.Log ("Model::generate()[START]:                    ");


			//	if (visitedPOs == null)
			visitedPOs = new List<AXParametricObject>();

			visitedPOs.Clear();


			// TRAVERSE GRAPH 

			/// First init all values from parameters

			for (int i = 0; i < parametricObjects.Count; i++)
				if (parametricObjects[i] != null && parametricObjects[i].generator != null)
					parametricObjects[i].generator.parametersHaveBeenPolled = false;


			for (int i = 0; i < parametricObjects.Count; i++)
			{
				if (parametricObjects[i] != null && parametricObjects[i].generator != null)
					parametricObjects[i].generator.pollControlValuesFromParmeters();
			}



			stats_VertCount = 0;
			stats_TriangleCount = 0;




			List<AXParametricObject> allHeads = getAllHeadPOs();

			for (int i = 0; i < parametricObjects.Count; i++)
			{
				setAltered(parametricObjects[i]);
			}



			for (int i = 0; i < allHeads.Count; i++)
			{

				//Debug.Log ("AH ["+i+"] "+allHeads[i].Name);


				if (generatedGameObjects != null)
				{
					AXGameObject[] axgos = generatedGameObjects.GetComponentsInChildren<AXGameObject>();
					for (int j = 0; j < axgos.Length; j++)
					{
						if (axgos[j] != null && axgos[j].gameObject != null && axgos[j].makerPO_GUID == allHeads[i].Guid)
							DestroyImmediate(axgos[j].gameObject);
					}
				}


				allHeads[i].generateOutputNow(false, allHeads[i], true);

				//allHeads[i].generator.adjustWorldMatrices();
			}

			/*

			for (int i=0; i<parametricObjects.Count; i++) 
			{

				//if (Archimatix.doDebug)
					//Debug.Log ("*** - MODEL::parametricObjects["+i+"] generate:::" + parametricObjects[i].Name + " using genid="+genid);
				
				if (visitedPOs.Contains(parametricObjects[i]))
					continue;
				
				//Debug.Log ("-- really generate");
				parametricObjects[i].generateOutputNow(ref visitedPOs, false, parametricObjects[i], true);
				
			}
			*/


			for (int i = 0; i < allHeads.Count; i++)
			{

				if (allHeads[i].is3D() && allHeads[i].grouper == null)
				{
					//Debug.Log(allHeads[i].Name +": " + parametricObjects[i].stats_VertCount);
					stats_VertCount += allHeads[i].stats_VertCount;
					stats_TriangleCount += allHeads[i].stats_TriangleCount;
				}
			}


			//Debug.Log ("model time: "+swm.stop ());

			//if (renderMode == RenderMode.GameObjects)
			//	modelRoot.resetGeneratedGameObjects();
			if (doCacheTheThumbnails)
				renderThumbnails();

			// CLEAR THE ALTERED DESIGNATIONS
			for (int i = 0; i < parametricObjects.Count; i++)
				parametricObjects[i].isAltered = false;

			AlteredPOs.Clear();
			AlteredHeadPOs.Clear();

			//Debug.Log ("Model::generate()[FINISH]:                     "+genid);


		}

		#endregion






		public List<AXParametricObject> getAllHeadPOs()
		{

			List<AXParametricObject> allHeads = new List<AXParametricObject>();

			for (int i = 0; i < parametricObjects.Count; i++)
			{
				//if (parametricObjects[i].is3D() && parametricObjects[i].generator.isHead())

				if (parametricObjects[i] != null && parametricObjects[i].generator != null && parametricObjects[i].generator != null && parametricObjects[i].generator.isHead() && parametricObjects[i].grouper == null)
				{
					allHeads.Add(parametricObjects[i]);
				}
			}
			return allHeads;

		}



		public void setAXTexRecursive(AXParametricObject po, int governor = 0, AXParametricObject downstream_po = null)
		{

			if (governor > 25)
				return;
			governor++;

			if (po == null)
				return;

			if (!po.is3D())
				return;


			//Debug.Log(po.Name +" ["+downstream_po+"]");

			// 1. Set the axMat and axTex for the po based on upstream_po or model

			AXParameter P_MaterialTool = po.getParameter("Material");
			AXParameter materialInput_src_p = (P_MaterialTool != null && P_MaterialTool.DependsOn != null) ? AX.Generators.Generator.getUpstreamSourceParameter(P_MaterialTool) : null;

			if (materialInput_src_p != null)
			{

				// There is a MaterialTool connected to this po. rig up references to its axMat and axTex where appropriate
				po.axTex = materialInput_src_p.parametricObject.axTex;

			}
			else
			{
				// no material node connected - take either from downstream_po or model
				if (downstream_po == null)  // From the model's AXMaterial and AXTexCoords
					po.axTex = po.model.axTex;
				else // Fromm the downstream_po
					po.axTex = downstream_po.axTex;
			}


			// Now that po's stuff is settled, carry on upstream

			// INPUTS -- GOING UPSTREAM
			List<AXParameter> inputs = po.getAllInputParameters();

			//			for (int i=0; i<inputs.Count; i++)
			//			{
			//				if (inputs[i].DependsOn != null &&  (po.grouper != null && inputs[i].DependsOn.parametricObject != po.grouper) && ( inputs[i].DependsOn.parametricObject.generator is AX.Generators.Generator3D) && (inputs[i].DependsOn.parametricObject.materialTool == null || inputs[i].DependsOn.parametricObject.axMat.mat == inputs[i].DependsOn.parametricObject.model.axMat.mat))
			//				{
			//					setAXTexRecursive( inputs[i].DependsOn.parametricObject, governor, po);
			//				}		 
			//			} 

			for (int i = 0; i < inputs.Count; i++)
			{

				if (inputs[i].DependsOn == null)
					continue;

				//Debug.Log(" ["+i+"]         ==== > " + inputs[i].DependsOn.parametricObject.Name);
				bool dependsIsNotOwnGrouper = (po.grouper == null || inputs[i].DependsOn.parametricObject != po.grouper);
				bool depnedsIs3D = (inputs[i].DependsOn.parametricObject.generator is AX.Generators.Generator3D);
				bool hasTex = (inputs[i].DependsOn.parametricObject.materialTool == null || inputs[i].DependsOn.parametricObject.axTex == null);


				//Debug.Log(dependsIsNotOwnGrouper + " -  " + depnedsIs3D + " _ " + hasMat);

				//Debug.Log("po.grouper="+po.grouper + " :: " + (po.grouper != null)  + " -- "+ (inputs[i].DependsOn.parametricObject != po.grouper));

				if (dependsIsNotOwnGrouper && depnedsIs3D && hasTex)
					setAXTexRecursive(inputs[i].DependsOn.parametricObject, governor, po);
			}








			// GROUPEES - all groupee heads...
			if (po.Groupees != null && po.Groupees.Count > 0)
			{
				for (int i = 0; i < po.Groupees.Count; i++)
					//if (! po.generator.hasOutputsConnected())
					setAXTexRecursive(po.Groupees[i], governor, po);
			}
		}







		public void setAXMatRecursive(AXParametricObject po, int governor = 0, AXParametricObject downstream_caller_po = null)
		{

			//			
			//			if (downstream_caller_po != null)
			//				Debug.Log(governor + " : " + po.Name + " :: " + downstream_caller_po.Name + " downstream_po.axMat.mat=" + downstream_caller_po.axMat.mat);
			//			else
			//				Debug.Log(governor + " : " + po.Name + " :: NONE " );


			if (governor > 25)
				return;
			governor++;

			if (po == null)
				return;

			if (!po.is3D())
				return;

			//if (po.grouper != null && downstream_po != null)
			//	return;

			// 1. Set the axMat for the po based on upstream_po or model

			AXParameter P_MaterialTool = po.getParameter("Material");
			AXParameter materialInput_src_p = (P_MaterialTool != null && P_MaterialTool.DependsOn != null) ? AX.Generators.Generator.getUpstreamSourceParameter(P_MaterialTool) : null;



			//Debug.Log("------------------------------- materialInput_src_p="+materialInput_src_p);

			bool checkDownstream = false;

			if (materialInput_src_p != null)
			{
				// There is a MaterialTool connected to this po. rig up references to if its axMat has a mat.

				AXParametricObject materialTool_po = materialInput_src_p.parametricObject;
				if (materialTool_po.axMat != null && materialTool_po.axMat.mat != null)
				{
					po.axMat = materialTool_po.axMat;
					checkDownstream = false;
				}
				else
					checkDownstream = true;
			}
			else
				checkDownstream = true;


			//Debug.Log("**** checkUpstream="+checkDownstream);

			if (checkDownstream)
			{
				if (downstream_caller_po == null)  // From the model's AXMaterial and AXTexCoords
					po.axMat = po.model.axMat;

				else                        // Fromm the downstream_po
					po.axMat = downstream_caller_po.axMat;
			}





			// Now that po's stuff is settled, carry on upstream

			// INPUTS -- GOING UPSTREAM
			List<AXParameter> inputs = po.getAllInputParameters();

			//Debug.Log("inputs.Count="+inputs.Count);
			for (int i = 0; i < inputs.Count; i++)
			{

				if (inputs[i].DependsOn == null)
					continue;

				//Debug.Log(" ["+i+"]         ==== > " + inputs[i].DependsOn.parametricObject.Name);
				bool dependsIsNotOwnGrouper = (po.grouper == null || inputs[i].DependsOn.parametricObject != po.grouper);
				bool depnedsIs3D = (inputs[i].DependsOn.parametricObject.generator is AX.Generators.Generator3D);
				bool hasMat = (inputs[i].DependsOn.parametricObject.materialTool == null || inputs[i].DependsOn.parametricObject.axMat.mat == inputs[i].DependsOn.parametricObject.model.axMat.mat);


				//Debug.Log(dependsIsNotOwnGrouper + " -  " + depnedsIs3D + " _ " + hasMat);

				//Debug.Log("po.grouper="+po.grouper + " :: " + (po.grouper != null)  + " -- "+ (inputs[i].DependsOn.parametricObject != po.grouper));

				if (dependsIsNotOwnGrouper && depnedsIs3D && hasMat)
					setAXMatRecursive(inputs[i].DependsOn.parametricObject, governor, po);
			}

			// GROUPEES - all groupee heads...
			if (po.Groupees != null && po.Groupees.Count > 0)
			{
				//Debug.Log(po.Name + " groupees.............. " + po.Groupees.Count);
				for (int i = 0; i < po.Groupees.Count; i++)
				{
					//Debug.Log("          ==== > " + po.Groupees[i].Name);
					//if (! po.generator.hasOutputsConnected())
					//{
					setAXMatRecursive(po.Groupees[i], governor, po);
					//}
				}
			}
		}











		public void remapMaterialTools()
		{

			//Debug.Log("----------- Model:: remapMaterialTools()");

			// Start at top, everytime you hit a "MaterialaTool", distribute it upstream 
			// by setting the consumerMaterialTool 
			// until you hit a node that has a local materialTool


			// TRAVERSE THE GRAPH
			// Setting a reference to the axMat and axTex foreach 3D parametric object

			List<AXParametricObject> heads = getAllHeadPOs();
			for (int i = 0; i < heads.Count; i++)
			{
				//Debug.Log("*** HEAD: " + heads[i].Name);

				AXParametricObject head = heads[i];

				setAXMatRecursive(head);
				setAXTexRecursive(head);
			}

			doRemapMaterials = false;
		}



		public void setLightmapStaticForAllPOs()
		{
			// Helpful: http://www.alanzucconi.com/2015/07/26/enum-flags-and-bitwise-operators/
			for (int i = 0; i < parametricObjects.Count; i++)
			{
				if (parametricObjects[i].is3D())
					parametricObjects[i].axStaticEditorFlags = parametricObjects[i].axStaticEditorFlags | (AXStaticEditorFlags.LightmapStatic);


			}

		}


		public void autobuildDelayed(long t)
		{
			// build in t milleseconds

			if (autoBuildStopwatch == null)
				autoBuildStopwatch = new StopWatch();

			autoBuildStopwatch.restart();
		}

		public void buildAll()
		{
			setAllAltered();
			autobuild();
		}
		public void autobuild()
		{



			autoBuildStopwatch = null;

			if (_autoBuild)
			{
				//Debug.Log("build");
				build();
			}

			else
			{
				Debug.Log("autobuild");
				setRenderMode(RenderMode.DrawMesh);
				generate();
			}

			//			if (Application.isPlaying)
			//			{
			//				AXPhysicsSceneManager psm = FindObjectOfType<AXPhysicsSceneManager>();
			//				if (psm)
			//					psm.restartPhysics();
			//			}


		}

		public void build()
		{

			setBuildPrecision();

			//Debug.Log("BUILD staticFlagsJustEnabled=" + staticFlagsJustEnabled);
#if UNITY_EDITOR
            // If auto lightmapping is checked in the Lightmapping window,
            // don't automatically add static flags to the created gameObjects
            if (Lightmapping.giWorkflowMode == Lightmapping.GIWorkflowMode.Iterative && !staticFlagsJustEnabled)
            {
                staticFlagsEnabled = false;

            }
#endif



			if (createSecondaryUVs && !createSecondaryUVsJustEnabled)
				createSecondaryUVs = false;


			// Does this generate really need to be here? Without it, undo does not have mesh.
			// Fix by making sure the generation of GameObject creates all meshes...
			// 	ROON 12/4/15
			//Debug.Log ("model.build()");															
			generate();

			createSecondaryUVsJustEnabled = false;

			renderMode = RenderMode.GameObjects;


			//Debug.Log("SET 0");
			//	stats_VertCount = 0;
			//stats_TriangleCount = 0;



			resetGeneratedGameObjects();




			buildStatus = BuildStatus.Built;

			// CALCULATE VOLUME...
			calculateTotalVolume();
			//Debug.Log("total vol="+volume + ", mass="+mass);


			// Call delegate

			if (ModelDidBuild != null)
				ModelDidBuild(this, new EventArgs());
		}

		public void printRelations()
		{
			for (int i = 0; i < relations.Count; i++)
			{
				AXRelation r = relations[i];
				Debug.Log(r.toString());
			}


			Debug.Log("=======");

			foreach (AXParametricObject po in parametricObjects)
			{
				foreach (AXParameter p in po.parameters)
				{
					if (p.relations != null && p.relations.Count > 0)
					{
						Debug.Log(po.Name + "." + p.Name + " has " + p.relations.Count + "relations ");
						foreach (AXRelation rr in p.relations)
						{
							Debug.Log("--> " + rr.pA.parametricObject.Name + "." + rr.pA.Name + " -> " + rr.pB.parametricObject.Name + "." + rr.pB.Name);

						}

					}
				}

			}


		}

		public void createGameObjects()
		{
			//Debug.Log("create GameObjects()");
			resetGeneratedGameObjects();


		}









		// MESH MANAGMENT
		// The meshes managed by the model are temporary for rendering 
		// the geometry during live editing. These meshes are drawn to screen every frame
		// and are not used to generate game objects untill they are called to do so
		// for example, after the edits are done.


		public void resetRenderMeshes()
		{
			axRenderMeshCount = 0;
			//axRenderMeshes = new AXMesh[5000];

		}

		public void addAXMeshes(List<AXMesh> ax_meshes)
		{

			//Debug.Log("** ax_meshes.Count=" + ax_meshes.Count);
			for (int i = 0; i < ax_meshes.Count; i++)
			{
				AXMesh axmesh = ax_meshes[i];
				if (axRenderMeshCount > 5000)
				{
					//Debug.Log ("WHY SO MANY MESHES? - 5000");
					break;
				}
				//Debug.Log("ADD "+axmesh.mesh.vertices.Length);
				//Debug.Log("ADD "+ axmesh.mesh.vertices.Length);
				//stats_VertCount += axmesh.mesh.vertices.Length;
				//stats_TriangleCount += axmesh.mesh.triangles.Length / 3;





				// REMOVE 2018_09_13
				// *** Not sure what this was supposed to be doing!
				// *** Became and issue when AXMesh was no longer derived from UnityEngine.Object

				//				if (axRenderMeshes [axRenderMeshCount] != null)
				//				{
				//					if (Application.isPlaying)
				//					{
				//						Destroy(axRenderMeshes [axRenderMeshCount].mesh);
				//						Destroy(axRenderMeshes [axRenderMeshCount].drawMesh);
				//						Destroy(axRenderMeshes [axRenderMeshCount]);
				//					}
				//					else
				//					{
				//						DestroyImmediate(axRenderMeshes [axRenderMeshCount].mesh);
				//						DestroyImmediate(axRenderMeshes [axRenderMeshCount].drawMesh);
				//						DestroyImmediate(axRenderMeshes [axRenderMeshCount]);
				//					}								
				//				}





				axRenderMeshes[axRenderMeshCount++] = axmesh;
			}
		}



		// GENERATED GAME_OBJECT MANAGEMENT
		[System.NonSerialized]
		public GameObject generatedGameObjects;

		public void clearGeneratedGameObjects()
		{
			if (generatedGameObjects != null)
			{
				//Debug.Log("clearGeneratedGameObjects");

				// DESTROY MESHES
				//				Transform[] tt = generatedGameObjects.GetComponentsInChildren<Transform>();
				//				for (int k=0; k<tt.Length; k++)
				//				{
				//					MeshFilter mf = tt[k].gameObject.GetComponent<MeshFilter>();
				//
				//					if (mf != null)
				//					{
				//						if (Application.isPlaying)
				//							Destroy (mf.sharedMesh);
				//						else
				//							DestroyImmediate (mf.sharedMesh);	
				//					}
				//				}


				if (Application.isPlaying)
					Destroy(generatedGameObjects);
				else
					DestroyImmediate(generatedGameObjects);

				// This brilliant hack of using UnloadUnusedAssets comes from https://forum.unity3d.com/threads/why-is-destroy-and-then-null-not-giving-the-memory-back.28441/
				Resources.UnloadUnusedAssets();

			}

		}


		// RUNTIME HANDLE GAME OBJECTS
		[System.NonSerialized]
		private GameObject _runtimeHandlesGameObjects;
		public GameObject runtimeHandlesGameObjects
		{
			get
			{
				if (_runtimeHandlesGameObjects == null)
				{
					Transform pg = gameObject.transform.Find("runtimeHandles");
					if (pg != null)
						_runtimeHandlesGameObjects = pg.gameObject;
					else
					{
						_runtimeHandlesGameObjects = new GameObject("runtimeHandles");
						_runtimeHandlesGameObjects.transform.parent = gameObject.transform;

					}
				}

				return _runtimeHandlesGameObjects;
			}
			set { _runtimeHandlesGameObjects = value; }

		}

		public void clearRuntimeHandleGameObjects()
		{
			if (runtimeHandlesGameObjects != null)
			{
				//Debug.Log("clearGeneratedGameObjects");

				if (Application.isPlaying)
					Destroy(runtimeHandlesGameObjects);
				else
					DestroyImmediate(runtimeHandlesGameObjects);

				// This brilliant hack of using UnloadUnusedAssets comes from https://forum.unity3d.com/threads/why-is-destroy-and-then-null-not-giving-the-memory-back.28441/
				Resources.UnloadUnusedAssets();

			}

		}


		// PROTOTYPE GAME_OBJECTS

		[System.NonSerialized]
		private GameObject _prototypeGameObjects;
		public GameObject prototypeGameObjects
		{
			get
			{
				if (_prototypeGameObjects == null)
				{
					Transform pg = gameObject.transform.Find("prototypGameObjects");
					if (pg != null)
						_prototypeGameObjects = pg.gameObject;
					else
					{
						_prototypeGameObjects = new GameObject("prototypGameObjects");
						_prototypeGameObjects.transform.parent = gameObject.transform;

					}
				}

				return _prototypeGameObjects;
			}
			set { _prototypeGameObjects = value; }

		}
		public void resetPrototypeGameObjects()
		{

			if (_prototypeGameObjects == null)
			{
				Transform pg = gameObject.transform.Find("prototypGameObjects");
				if (pg != null)
					_prototypeGameObjects = pg.gameObject;
			}
			if (_prototypeGameObjects != null)
				DestroyImmediate(_prototypeGameObjects);

			_prototypeGameObjects = new GameObject("prototypGameObjects");
			_prototypeGameObjects.transform.parent = gameObject.transform;

		}

		public int lastProtypeID = 1;

		public GameObject createNewPrototypeGameObject()
		{
			GameObject newPrototype = new GameObject("prototype_" + lastProtypeID);
			lastProtypeID++;
			return newPrototype;

		}



		public bool isTrailhead(AXParametricObject po)
		{
			if (po.grouper != null)
				return false;

			if (!po.hasDependents())
				return true;

			AXParameter output = po.generator.P_Output;//  po.getParameter("Output Mesh", "Output");
			if (output != null)
			{
				foreach (AXParameter d in output.Dependents)
					if (d.parametricObject.generator is Grouper)
						return false;


				foreach (AXParameter d in output.Dependents)
				{
					if (!(d.parametricObject.generator is Instance) && !(d.parametricObject.generator is Replicant))
						return false;
				}
				return true;
			}
			return false;
		}





		public void resetGeneratedGameObjects()
		{
			//Debug.Log("resetGeneratedGameObjects ************************************************************************************************ ");
			if (axRenderMeshes == null)
				resetRenderMeshes();



			clearGeneratedGameObjects();

			generatedGameObjects = new GameObject("generatedGameObjects");

			//generatedGameObjects.isStatic = true;




			if (isSpriteGenerator && spriter != null)
			{


				// Re-init RenderTexture
				//spriter.renderTexture = new RenderTexture(spriter.tex_width, spriter.tex_height, 24, RenderTextureFormat.ARGB32);
				spriter.spriteRenderTexture = new RenderTexture(spriter.tex_width, spriter.tex_height, 24);
				spriter.spriteRenderTexture.Create();//			



				spriteCameraGO.SetActive(true);
				spriteCamera.orthographicSize = 10;
				spriteCamera.orthographic = true;



				int AX2D_Srite_Mask = LayerMask.GetMask("AX2D_Sprite");
				spriteCamera.cullingMask = AX2D_Srite_Mask;



				RenderTexture prevRenderTex = spriteCamera.targetTexture;
				spriteCamera.targetTexture = spriter.spriteRenderTexture;//tmpRenTex;


				// Paint the ax_meshes on to renderTexure
				foreach (AXParametricObject po in parametricObjects)// in ( (visitedPOs != null && visitedPOs.Count > 0) ? visitedPOs : parametricObjects))
				{
					if (po.isSpriteGenerator)
					{
						Debug.Log("rendering to sprite: " + po.Name);
						Generator3D gener = (Generator3D)po.generator;
						//Debug.Log(":: " + gener.P_Output.meshes.Count);
						spriter.renderPO2Sprite(po, false);
					}
				}


				GameObject spriteGO = new GameObject("_SpriteGO");
				SpriteRenderer spriteRenderer = spriteGO.AddComponent<SpriteRenderer>();


				// write to texture2D
				spriter.spriteTexture2D = new Texture2D(spriter.tex_width, spriter.tex_width);



				//					spriter.spriteTexture.ReadPixels(new Rect(0, 0, spriter.tex_width, spriter.tex_width), 0, 0);
				//					spriter.spriteTexture.Apply();


				RenderTexture prevActive = RenderTexture.active;
				RenderTexture.active = spriter.spriteRenderTexture;
				spriter.spriteTexture2D.ReadPixels(new Rect(0, 0, spriter.tex_width, spriter.tex_width), 0, 0);
				spriter.spriteTexture2D.Apply();
				RenderTexture.active = null; //prevActive;



				Color[] pixels = spriter.spriteTexture2D.GetPixels();
				//Generator3D gener = (Generator3D) generator;
				int ct = 0;
				foreach (Color c in pixels)
				{
					if (c.a > 0)
					{
						ct++;
					}
				}


				Debug.Log("Done pixels: " + ct);
				//////spriteRenderer.sprite = Sprite.Create ((Texture2D) spriter.spriteTexture, new Rect(0.0f, 0.0f, spriter.renderTexture.width, spriter.renderTexture.height), new Vector2(0.5f, 0.5f), 256/bounds.extents.x);
				spriteRenderer.sprite = Sprite.Create(spriter.spriteTexture2D, new Rect(0.0f, 0.0f, spriter.tex_width, spriter.tex_width), new Vector2(0.5f, 0.5f), 60);
				Debug.Log("yadda: " + spriteRenderer.sprite);
				//spriteGO.transform.Translate(new Vector3(gener.transX,0,0));

				spriteGO.transform.SetParent(generatedGameObjects.transform);//, retGO.transform);



			}












			/*
				1. Clear and create root gameObject
				2. foreach parametricObjects with no dependents, go upstream making gameObects until you hit a Combine.
				3. at that point, combine the axmeshes there according to material
				4. add collider
			
			*/


			int lodCount = 1;

			float[] lodPercent = { 1f, .5f, .25f };

			if (lodCount > 1)
				setAllAltered();

			GameObject[] lodGOs = new GameObject[lodCount];
			LOD[] lods = new LOD[3];

			float prevSegmentReductionFactor = segmentReductionFactor;

			for (int lod = 0; lod < lodCount; lod++)
			{
				if (lod > 0)
				{
					//Debug.Log("set all altered");
					setAllAltered();
				}


				if (lodCount > 1)
				{
					lodGOs[lod] = new GameObject(name + "_LOD" + lod);
					segmentReductionFactor = lodPercent[lod] * prevSegmentReductionFactor;



				}



				// top-level PO's
				Renderer[] renderers = new Renderer[parametricObjects.Count];

				for (int i = 0; i < parametricObjects.Count; i++)
				{
					AXParametricObject po = parametricObjects[i];

					if (po.is3D() && isTrailhead(po))
					{
						// this is a trailhead to generate GameObjects.
						// Now go upstream...
						//if (visitedPOs == null)
						visitedPOs = new List<AXParametricObject>();

						visitedPOs.Clear();
						//visited.Add (po.Guid);

						//Debug.Log ("FIRE TRAILHEAD po.generateOutputNow::::::::::::::::::::::::::::::::::::::::::::::::::::::::  " + po.Name);

						// ** GENERATE OUTPUT NOW **
						GameObject go = po.generateOutputNow(true, po, true);

						if (go != null)
						{
							//Debug.Log ("GENERATED GO: " + go.name);
							if (lodCount > 1)
							{
								renderers[i] = go.GetComponent<Renderer>();
								go.transform.SetParent(lodGOs[lod].transform);
							}
							else
								go.transform.parent = generatedGameObjects.transform;

						}
					}
				}


				// LOD 
				if (lodCount > 1)
				{
					lodGOs[lod].transform.SetParent(generatedGameObjects.transform);




					//Debug.Log(" * * * * * " + lod + " ::: " + 1.0f / (lod+1));
					lods[lod] = new LOD((1.0f / (lod + 1)), renderers);
				}

			}
			segmentReductionFactor = prevSegmentReductionFactor;


			generatedGameObjects.transform.parent = gameObject.transform;
			generatedGameObjects.transform.position = gameObject.transform.position;
			generatedGameObjects.transform.rotation = gameObject.transform.rotation;

			if (lodCount > 1)
			{

				//foreach (LOD lodd in lods)
				//     Debug.Log(lodd.screenRelativeTransitionHeight);
				LODGroup lodGroup = generatedGameObjects.AddComponent<LODGroup>();
				lodGroup.SetLODs(lods);
			}

			// Go through and add a AXGameObject to each gameObject

			//addAXGameObjects(generatedGameObjects);
			//Transform[] children = generatedGameObjects.GetComponentsInChildren<Transform>();
			//foreach (Transform t in children)
			//	t.gameObject.AddComponent<AXGameObject>();



			// add isKinematic to any rigidbodies underground
			if (rigidbodiesUndergroundAreKinematic)
			{
				Terrain terrain = Terrain.activeTerrain;
				if (terrain != null)
				{
					Rigidbody[] rbs = FindObjectsOfType(typeof(Rigidbody)) as Rigidbody[];
					foreach (Rigidbody rb in rbs)
					{

						if (!rb.isKinematic)
						{
							MeshFilter mf = rb.gameObject.GetComponent<MeshFilter>();

							if (mf != null && mf.sharedMesh != null)
							{
								Mesh mesh = mf.sharedMesh;

								float testHgt;

								// if any point is underground, make kinematic
								for (int i = 0; i < mesh.vertices.Length; i++)
								{
									Vector3 gVert = rb.gameObject.transform.TransformPoint(mesh.vertices[i]);
									testHgt = terrain.SampleHeight(gVert);
									if (gVert.y < testHgt)
									{
										rb.isKinematic = true;
										break;
									}

								}
							}
						}
					}
				}
			}










			return;


		}



		/// <summary>
		/// Calculate the total volume of all the meshes in the model.
		/// To do this, step through all the generatedGameObjects and sum up the volume of their AXGamObject.volume's
		/// </summary>
		public void calculateTotalVolume()
		{
			volume = 0;
			mass = 0;

			if (generatedGameObjects != null)
			{
				Transform[] transforms = generatedGameObjects.GetComponentsInChildren<Transform>();
				foreach (Transform transform in transforms)
				{
					AXGameObject axgo = transform.gameObject.GetComponent<AXGameObject>() as AXGameObject;
					if (axgo != null)
					{
						//Debug.Log("axgo.volume="+axgo.volume);
						volume += axgo.volume;
						mass += axgo.mass;
					}
				}
			}
		}


		public GameObject stamp()
		{
			if (generatedGameObjects != null)
			{
				GameObject go = (GameObject)Instantiate(generatedGameObjects, generatedGameObjects.transform.position, generatedGameObjects.transform.rotation);

				string _name = generatedGameObjects.transform.parent.gameObject.name;
				_name = _name.Replace(" (AX)", String.Empty);

				go.name = _name + " (Stamp)";




				Transform[] transforms = go.GetComponentsInChildren<Transform>();
				foreach (Transform transform in transforms)
				{
					AXGameObject axgo = transform.gameObject.GetComponent<AXGameObject>() as AXGameObject;
					DestroyImmediate(axgo);
				}

				deselectAll();

#if UNITY_EDITOR
                Selection.activeGameObject = go;
#endif
				return go;

			}
			return null;

		}










		#region THUMBNAILS
		// DRAW_THUMBNAILS -- re-create PO palette 3D thumbnails



		public Light addSceneDirectionalLight(GameObject lightGO)
		{
			lightGO.transform.Rotate(new Vector3(38, 50, 0));
			Light light = lightGO.AddComponent<Light>();
			light.intensity = .8f;
			light.type = LightType.Directional;
			light.shadows = LightShadows.Hard;
			light.shadowStrength = 1f;

			return light;

		}





		public void assertThumnailSupport()
		{
			// THUMBNAIL CAMERA - a GameObject attached to the model that is normally inactive
			if (thumbnailCameraGO == null)
			{
				thumbnailCameraGO = new GameObject("thumbnailCamera");
				thumbnailCameraGO.transform.parent = gameObject.transform;

				thumbnailCamera = thumbnailCameraGO.AddComponent<Camera>();
				thumbnailCamera.fieldOfView = 60;
				thumbnailCamera.farClipPlane = 5000;
				remoteThumbnailLocation = Matrix4x4.TRS(new Vector3(10000, 10000, 10000), Quaternion.identity, Vector3.one);
			}
			thumbnailCamera.backgroundColor = Thumbnail.backgroundColor;
			thumbnailCamera.clearFlags = CameraClearFlags.Color;

			thumbnailCameraGO.SetActive(true);

			// THUMBNAIL_LIGHT - a GameObject attached to the model GO that is normally inactive
			//Light light = null;
			if (thumbnailLightGO == null)
			{
				thumbnailLightGO = new GameObject("thumbnailLight");
				thumbnailLightGO.transform.parent = gameObject.transform;
				//thumbnailLight.hideFlags = HideFlags.HideInHierarchy;
				Light thumbnailLight = addSceneDirectionalLight(thumbnailLightGO);
				//thumbnailLight.color = new Color(.9f, .88f, .88f);
				thumbnailLight.color = Color.white;
				thumbnailLight.intensity = 1.66f;
				//thumbnailLight

			}
			thumbnailLightGO.SetActive(true);



			if (thumbnailMaterialMesh == null)
			{
				int segs = 32;
				float radius = 3;

				Path planPath = AXTurtle.Circle(radius, segs);
				Spline planSpline = new Spline(planPath, true, 60, 60);


				Path sectionPath = AXTurtle.Arc(radius, -90, 90, segs);
				Spline sectionSpline = new Spline(sectionPath, false, 60, 60);
				sectionSpline.shift(-radius, 0);

				PlanSweeper planSweeper = new PlanSweeper();

				thumbnailMaterialMesh = new Mesh();


				planSweeper.generate(ref thumbnailMaterialMesh, planSpline, sectionSpline, new AXTexCoords());
				thumbnailMaterialMesh.RecalculateNormals();
				thumbnailMaterialMesh.RecalculateBounds();
				thumbnailMaterialMesh.RecalculateTangents();

			}

		}



		// SPRITE GENERATION SUPPORT
		public void assertSpriteSupport()
		{
			isSpriteGenerator = true;

			if (spriter == null)
				initSprite();


			// SPRITE CAMERA - a GameObject attached to the model that is normally inactive
			if (spriteCameraGO == null)
			{
				spriteCameraGO = new GameObject("spriteCamera");
				spriteCameraGO.transform.parent = gameObject.transform;

				spriteCamera = spriteCameraGO.AddComponent<Camera>();
				spriteCamera.fieldOfView = 60;
				spriteCamera.farClipPlane = 5000;
			}

			spriteCamera.clearFlags = CameraClearFlags.Nothing;

			spriteCameraGO.SetActive(true);


			if (spritePreviewMaterial == null)
				spritePreviewMaterial = new Material(Shader.Find("Sprites/Default"));

			if (spritePreviewMesh == null || spritePreviewMesh.mesh == null || spritePreviewMesh.mesh.vertices == null)
			{
				spritePreviewMesh = new AXSpriteMesh();
				spritePreviewMesh.init();
			}

		}



		public void initSprite()
		{
			spriter = new AXSpriter();
			spriter.init();
		}








		public void renderThumbnails(string caller = "unknown", bool makeTexture = false)
		{

			// Here is where the thumbnail Texture2D's get created.
			// These thumbnails are more persistent than the RenderTextures used 
			// when modifying the PO.
			//
			// Since this is an expensive operation, it should only be
			// run after completing an interactive edit (e.g., when onMouseUp or after an Undo

			//return ;

			//Debug.Log("CACHE THUMNAILS.... " + visitedPOs.Count);

			// CACHE ENVIRONMENT SETTINGS
			Thumbnail.BeginRender();


			//Debug.Log ("caching thumbnails... "+caller);


			// DRAW EACH PARAMETRIC OBJECT IN THE SCENE TO ITS THUMBNAIL IMAGE
			//foreach (AXParametricObject po in parametricObjects)

			foreach (AXParametricObject po in ((visitedPOs != null && visitedPOs.Count > 0) ? visitedPOs : parametricObjects))
			{

				if (po.is2D()) //Library.cache2DThumbnail(po);
				{
					continue;
				}

				//Debug.Log("Cache: " + po.Name);

				Thumbnail.renderThumbnail(po, makeTexture);
			}
			Thumbnail.EndRender();



			// DRAW SPRITE
			if (isSpriteGenerator && spriter != null)
			{

				spriter.spriteRenderTexture = new RenderTexture(spriter.tex_width, spriter.tex_height, 24, RenderTextureFormat.ARGB32);
				//spriter.renderTexture = new RenderTexture(spriter.tex_width, spriter.tex_height, 24);
				spriter.spriteRenderTexture.Create();//			

				foreach (AXParametricObject po in parametricObjects)// in ( (visitedPOs != null && visitedPOs.Count > 0) ? visitedPOs : parametricObjects))
				{
					if (po.isSpriteGenerator)
						spriter.renderPO2Sprite(po, false);
				}
			}





		}

		#endregion









	}

}
