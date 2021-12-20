Character Movement Fundamentals

Changelog:
_____________________________________

Version 2.2

GENERAL:

MOVER:
- Added 'SetColliderThickness()' function to 'Mover', which can be used to change the thickness/width of the controller at runtime.
- Most variables related to collider size inside the 'Mover' script are now not public anymore (since they really shouldn't be changed directly at runtime).
- Instead, changing the controller's size/shape at runtime should be done only via the 'SetColliderHeight()'/'SetColliderThickness()'/'SetStepHeightRatio()' functions, which will automatically update the ground detection system based on the new size/shape.

CONTROLLERS:
- All air movement calculations inside 'AdvancedWalkerController' have been completely overhauled and now properly use the controller's momentum.
- The 'Air Control' setting has been replaced with 'Air Control Rate' (higher values result in a more direct movement while the controller is in the air).
- The slope movement calculations have also been overhauled and are now more effective at preventing the controller from going up slopes (which are too steep, based on slope limit).
- A few minor, general code improvements to the 'AdvancedWalkerController'.

CAMERA SYSTEM:
- Fixed a bug inside 'Camera Mouse Input' that resulted in a 'NaN' error when setting Time.timeScale to '0f' at runtime.
- Fixed a bug inside 'Camera Distance Raycaster' that could result in incorrect layermask settings.

MISC:
- A few minor code improvements to the 'AnimationControl' script.

_____________________________________

Version 2.1

GENERAL:

MOVER:
- 'Mover' now supports scaled colliders. 
(NOTE: It is still recommended to keep the controller's collider and root at scale (1, 1, 1), for physics stability reasons. Instead, please consider changing the collider size and only scale the model/mesh of your character.)

CONTROLLERS:
- 'AdvancedWalkerController' now also has a 'SetMomentum()' function for easier access to the controller's momentum.
- Slight change to jump logic inside 'AdvancedWalkerController': Jump key now needs to be released before controller can jump again. This prevents the controller from continually jumping if jump key is held down.
- Main controller update code was moved to its own 'ControllerUpdate()' function (which is called in FixedUpdate()).
- Added 'ClickToMoveController' script, a basic example of top-down, mouse-controlled movement.

CAMERA SYSTEM:
- Streamlined 'CameraController'. All mouse input handling was moved to 'CameraMouseInput' script.
- 'CameraMouseInput' now has a 'Mouse Input Multiplier' variable, that can be used to control mouse sensitivity.
- Fixed bug in 'CameraController' - script can now handle a project timescale of '0'.
- 'CameraDistanceRaycaster' now uses a separate "target" object for its obstacle raycasting. This target object can be freely moved around at run-time, allowing users to easily change the camera's position while the game is running.

PREFABS & SCENES:
- Scenes are now organized into 'Showcase' and 'Special' folders.
- Added example 'PlanetWalkerScene'.
- Added example 'ClickToMoveScene', which demonstrates the new 'ClickToMoveWalker' controller prefab.
- Moved 'ExternalCameraScene' to 'Special' folder.
- Added a 'blank' and 'animated' controller prefab for the new 'ClickToMoveController' component.

MISC:
- Removed 'MoveTowardVectorLength()' function in 'VectorMath' class. Unity's built-in 'Vector3.MoveTowards()' function is now used instead.
- Updated 'MouseCursorLock' - script should now work properly in newer Unity versions.
- Added 'AlignRigidbodyToTarget' script, which is used to align the controller to the planet's center in the 'PlanetWalkerScene'.
_____________________________________

Version 2.0

GENERAL:
- Implemented namespaces for all scripts. 
- Renamed 'Resources' folder to 'Source'.
- Fixed unnecessary memory allocation in some scripts.
- Added (inspector) tooltips to some variables.
- Some minor code improvements to improve readability.

MOVER:
- Overhauled and simplified mover inspector UI. Mover now supports proper editor undo/redo.
- Streamlined mover layermask system. Mover now automatically uses the same layermask for raycasting as its gameobject's layer (as set in the physics settings).
- Added (optional) ceiling detection script. This script can be attached to any object with an 'Advanced Walker Controller' script and will register collisions with the ceiling using the rigidbody's 'OnCollision' functions.
- Added functions to change mover collider dimensions at runtime via script (see manual).

CONTROLLERS:
- Separated input handling from controller scripts. Input is now handled by a separate script attached to the same gameobject. This makes implementing custom input a lot easier.
- Added two example input scripts for character movement (keyboard and gamepad controls).
- Added two example input scripts for camera movement (mouse and gamepad controls).
- Restructured controller script inheritance. All controller scripts now extend an abstract base controller script, 'Controller'. By extending from this script, users can easily use all the secondary scripts with their own custom controller scripts.
- Controller scripts now have a 'Camera Transform' field. If assigning a camera transform, the controller will now move relative to that camera's view.
- Merged 'Camera Walker Controller' and 'Basic Walker Controller' into a single controller script, 'Advanced Walker Controller'.
- Added 'Simple Walker Controller', a very simple example controller script. 

PREFABS & SCENES:
- Added two simple controller prefabs (located in Prefabs > Controller > Simplified).
- Added a new example scene ('ExternalCameraScene') to demonstrate a possible controller setup where the camera is a separate gameobject as the character.

MISC:
- Added a generic 'Turn Toward Transform Direction' script as an alternative to the existing 'Turn Toward Camera Direction' script.
- Added 'RotateObject' script, which is used in the new 'ExternalCameraScene'.
_____________________________________

Version 1.3

- Overhauled "slope-sliding" algorithm of 'Basic Walker Controller'. As a result 'Slide Gravity' now works more predictable and efficient.
- Added 'Use Local Momentum' option to 'Basic Walker Controller'. If enabled, controller momentum is calculated in relation to the controller's transform rotation.
- 'Sensor' and 'Camera Distance Raycaster' now use a different method to exclude certain colliders when raycasting/spherecasting. This fixes several glitches in the latest Unity version (2019).
- All visual scripts ('Turn Toward Controller Velocity', 'Smooth Position', 'Smooth Rotation', [...]) now can handle being enabled/disabled at runtime.
- Added 'Ignore Controller Momentum' option to 'Turn Toward Controller Velocity' script. If enabled, only the controller's movement velocity is used to calculate the new rotation.
- Some minor renaming of some variables to improve general code readability.
_____________________________________

Version 1.2

- Fixed a bug that caused visual glitching when viewing controller prefabs in the inspector in newer versions of Unity (2018 and up).
- Added a public 'Add Momentum' function to 'BasicWalkerController', which can be used to add a force to the controller (useful for jump pads, dashing, [...]).
- Added an option for more responsive jumping: By holding the 'jump' key longer, the character will jump higher.
- Added two public functions to 'CameraController'. By calling them from an external script, the camera can be rotated either toward a direction or a specific position (see manual for more details).
- 'SideScroller' controller prefab has been streamlined (code-wise) and is now compatible with the 'GravityFlipper' prefabs.
- Various minor changes to some of the code to improve readability.
_____________________________________

Version 1.1

- Replaced old showcase scene with new version (now with a proper controller selection menu).
- Polished controller prefab settings.
- Replaced 'ApplicationControl' and 'ChangeControllerType' scripts with new 'DemoMenu' script, which will now handle switching between different controller prefabs.
- Added 'DisableShadows', 'FPSCounter' and 'PlayerData' scripts, all of which are used in the new showcase scene.
- Decreased scene light intensity to '0.7'.
- Removed all (unnecessary) 'Showcase' controller prefabs, all example scenes now use the controller prefabs included in the projects.
- Added 'MouseCursorLock' script, which provides basic mouse cursor locking functionality. Old (unstable) 'FocusMouse' script was removed.
- Updated user manual to reflect changes.
_____________________________________

Version 1.0

- Initial release
_____________________________________

