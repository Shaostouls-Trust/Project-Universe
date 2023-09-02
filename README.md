# Project Universe 1.0.2.7
* What's new:
	- Updated to 2022.3.8f1
	- Updated Entities Packages
	- Added Dedicated Server Support
	- Netcode.IO.Net Implementation
	- Tested Network Connection and Transport
	
* What's Next:
	- The XML fields likely will need updated, and thus the loaders too.
	- Test sending collider data to server
	- Test sending position and state data
	- Test server events to client

# Project Universe 1.0.2.6
* What's new:
	- Split Volume Controls

* What's Next:
	- The XML fields likely will need updated, and thus the loaders too.
	- We will attempt to split certain resources, such as textures, into a package file that can be downloaded separately to reduce the initial project download size. 
	- Barring that, additional cleaning to the file structure will be performed.
	- Basic low-level Renetcode integration
	- Test sending collider data to server

# Project Universe 1.0.2.5
* What's new:
	- Interaction Controller Keybinding Fix.
	- Dynamic Keybinds.
	- Bullet and Casing Lifetimes.
	- Resource XMLs have been updated.

* What's Next:
	- The XML fields likely will need updated, and thus the loaders too
	- Split volume controls
	- We will attempt to split certain resources, such as textures, into a package file that can be downloaded separately to reduce the initial project download size. Barring that, additional cleaning to the file structure will be performed.

# Project Universe 1.0.2.4
* What's New:
	- Volume Atmosphere Controllers have been moved to a Jobs System
	- The Submachine logic (IE doors and lights) have been moved to a Jobs system
	
* Changes:
	- Raytracing was disabled.
	- Fission reactors will not be moved to GPGPU/HLSL methods at this time
	
* What's **Next**:
	- Split Volume Controls
	- Dynamic Keybinds
	- Updated resource manifests
	- Object spawning and interaction improvements

# Project Universe 1.0.2.3
* What's New:
	- A Readme... sort of
	
* Changes:
	- Added Exception checks to resource loaders to prevent series load failure.
	- Added Globalization considerations for pesky comma control.
	
* Fixes:
	- Fixed most of the negative-scaled colliders.
	- Fixed network variable write-event order warning.
	
* What's **Next**:
	- Split Volume Controls
	- Render Load Optimizations
	- Post-Processing-related bugs
	- Assetbundles?
	- Rivit changeover?
