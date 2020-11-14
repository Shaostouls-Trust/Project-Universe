ARCHIMATIX
==========

Welcome to Archimatix, a node-based Parametric modeler for Unity 3D. 

Designed to help the game developer build "smart" models in the Unity editor, 
Archimatix provides the ability to generate meshes non-destructively, 
allowing for iterative and interactive work flows for the creation of props and environments.



OVERVIEW:

	There are two important editor windows used in Archimatix: 

	1. 	Library EditorWindow: 
		From the library, you can call up parametric models that can be modified using SceneView Handles and numerical sliders in the Inspector.

	2. 	Node Graph EditorWindow:  
		In order to modify the logic of Library models, or create new models from scratch, you can use the Node Graph Editor, 
		which allows you to create new nodes and parameters and link these nodes together using expressions. 
		Any edits to a ParametricObject may be saved as a new, unique Library item for later use.



QUICKSTART:

	1. From the Unity menu, select GameObject > 3D Object > Archimatix > Stairs > Basic Stair
	2. Press the "f" to frame the Basic Stair in the scene. 
	3. Click and drag any point handle to edit the parameters of the Basic Stair.

	4. From the Unity menu, open the Library Editor Window by selecting Window > Archimatix > Library
	5. Click on any item in the Library and play with any handles visible in the scene to modify the object parametrically.

	6. From the Unity menu choose Window > Archimatix > Graph Editor
	7. Click on the Circle in the lefthand sidebar. Press "f" to frame the node in the editor window and in the SceneView.
	8. In the Node Graph Editor Window, click on the button to the right of the "Output Shape" parameter to start a new connection.
	9. Clcik on the "Extrude" node icon on the righthand sidebar menu. This will create an extruded cylinger. 
   10. Manuiplute the radius and height handels in the SceneView.

	For more help getting started with Archimatix, please read the UserGuide.pdf included in the Documentation folder.



DEMOS:

	Several demo scenes are included in the DemoSecenes folder.
	These scenes have parametric models already instantiated in them.



DETAILED DOCUMENTATION:

	The UserGuide.pdf included in the Documentation folder.
	Detailed documentation, including tutorials and a manual, can be found at archimatix.com

	For documentation on runtime Archimatix, see:
	http://www.archimatix.com/manual/runtime-archimatix


SUPPORT EMAIL
	support@archimatix.com


