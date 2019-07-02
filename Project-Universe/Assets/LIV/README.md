# LIV SDK v0.1.5 for Unity

## Quick Start

1. Import the LIV asset into your project.
2. Add the `LIV` script onto any GameObject.
3. Set both the `Tracked Space Origin` and `HMD Camera` properties.

That's it!


## Compatibility

Right now, this asset works only with OpenVR. If you'd like to allow your users to use LIV with Oculus or WMR headsets, make sure you distribute a copy of your project with OpenVR enabled.

There are also command-line switches you can use to force Unity to use a specific API, which you can use in Steamworks to create an OpenVR-specific launch configuration. Read more [here]( https://docs.unity3d.com/Manual/VROverview.html ).


### System Architecture

We only support x64 builds of Unity games, so please, when you're distributing your project, make sure you've selected `x86_64` from the build settings dialog!


## Testing

- Download the [LIV Client]( https://liv.tv/download#liv-client ).
- Run it, install the virtual camera driver.
- _Optional_: Get yourself a camera calibration and copy it into the root of your project's files (for testing in-editor), or next to the built executable of your project with the name `externalcamera.cfg`.
- In LIV, launch the compositor.
- Ensure that SDK support is enabled in the about screen of LIV.
- _Optional_: Create a camera profile and import your calibration from earlier.
- Run your project.
- Using "Manual" capture mode, pick your project from the list.

If all has gone well, you'll see your game in LIV's output, without seeing it in your project!


## Common Problems

### My output is completely black!

This is a really common problem, and it's usually caused by a postprocessing effect that clobbers the alpha channel of a rendering pass. 99% of the time, it's a bloom shader.

To double check that this _is_ the problem, whilst you're capturing your application with LIV, move to the "Output" tab and disable the foreground. If you can suddenly see your game, it's definitely a foreground problem!

To fix this, you can disable the effect specifically on the foreground pass of the MixedRealityRender component:

1. Identify which post-processing effect is breaking your foreground. Iteratively disable single effects at a time and see if the foreground works.
2. Open `MixedRealityRender.cs` and locate `RenderNear()`.
3. Add lines here to disable the post-processing effect on `_mrCamera.gameObject`, and make sure you re-enable them after `_mrCamera.Render()` has been called!

### My game's output goes into split screen / looks broken with LIV active!

Usually, this is a `SteamVR_ExternalCamera` conflict. We use the same config name for backwards compatibility, though this is being phased out.
To fix, simply find all `SteamVR_Render` components and set `External Camera Config Name` to `externalcamera-legacy.cfg`.

This will soft-disable SteamVR_ExternalCamera so that it cannot be active when LIV is, while keeping the old functionality.


### The namespace `Valve.VR` already contains a definition for ...

You have two copies of the file `openvr_api.cs` in your assets. Delete one of them, ideally the one in the LIV SDK. Generally speaking, it's safe to update this file. You'll see compile-time errors if something significant has changed!


### `Valve.VR.ETrackedControllerRole` does not contain a definition for `OptOut`

Your OpenVR API is out of date. There are a few files you'll need to update, that could be in any location within your assets folder. It is highly recommended that you do this anyway, especially as there have been major updates to OpenVR recently!

1. Search your project's assets for `openvr_api.cs`. You should only have one. Update it using a copy from [here]( https://github.com/ValveSoftware/openvr/blob/master/headers/openvr_api.cs ).

2. Search your project's assets for `openvr_api.dll`. In newer versions of Unity, you might not have this file, so skip this step. Replace it using the **64 bit** version from [here]( https://github.com/ValveSoftware/openvr/blob/master/bin/win64/openvr_api.dll ).


## Further Help & Feedback

We want to hear your ideas, plans, and what you're going to end up doing with the SDK - if you've got an exotic idea that you might need a hand with - reach out. We're just as enthusiastic as you <3

We're always available through our [Discord Community]( http://liv.chat ), so please pop in if you need help. If you're not familiar with Discord, it's a popular gaming-oriented instant messenger & VOIP client.

Alternately, you can email us through techteam@liv.tv!
