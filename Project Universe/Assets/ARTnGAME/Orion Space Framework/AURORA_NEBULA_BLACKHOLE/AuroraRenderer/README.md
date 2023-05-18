# AuroraRendererUnity

An interactive renderer drawing Earth and the Aurora Borealis in 3D, built in the Unity game engine.

This requires a fairly good graphics processor, but works on most platforms, including
in VR on the VIVE (if you add the SteamVR package for Unity), or
WebGL (if you switch to Gamma colorspace).

A WebGL demo is available here:
    http://lawlor.cs.uaf.edu/~olawlor/2019/AuroraRendererWebGL/

Keyboard shortcuts:
	Mouse: look around
	WASD: move horizontally
	QZ: move up and down
	Spacebar: toggle Earth rotation
	Escape: exit


This began as the 2013-era GLSL codebase we built here:
   https://github.com/olawlor/directcompression

for our 2010 paper:
   https://www.cs.uaf.edu/~olawlor/papers/2010/aurora/lawlor_aurora_2010.pdf

This version was converted to HLSL shaders in 2019, and everything was slightly updated and retuned.

The shader code and datasets not otherwise copyrighted are released to the public domain.

The original volume datasets and rendering architecture are by:
Dr. Jon Genetti, jdgenetti@alaska.edu

The Unity port, shader code, and GPU volume renderer are by:
Dr. Orion Lawlor, lawlor@alaska.edu

