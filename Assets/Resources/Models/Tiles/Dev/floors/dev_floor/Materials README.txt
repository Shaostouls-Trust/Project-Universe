When setting up materials they should be as follows
===================================================================================
HOW ARE THE FILES SAVED!?
===================================================================================
All maps with the exception of the AS and CMSM should be saved at 1024x1024 or a similar size.
AS, and CMSM maps should be saved at 4096. ( This is important so as to prevent tearing or inaccurate coloring.

FileType .DDS
Photoshop Intel Texture Works v1.0.4

Preset: Project_Universe

Texture Type: Color + Alpha

Compression: BC3 8bpp (Linear)

Auto Generate Mip Maps

For color mask, export with no mip maps.
===================================================================================
WHAT ARE THESE MAPS?!
===================================================================================
Tile_Name_CMSM: This is the Color and scratches  Map,
RGB channel is setup as follows.
-Primary Color 		:#FF0000 (RED)
-Secondary Color 	:#00FF00 (GREEN)
-Tertiary Color 	:#0000FF (BLUE)
-Quaternary Color 	:#FFFF00 (YELLOW)
-Detail Color 		:#00FFFF (CYAN)
-Secondary Detail Color :#FF00FF (MAGENTA)
Insure there are no deviations in color for each specific color, it should be a solid color of each noted above.
Alpha channel is reserved for the scratch map.


Tile_Name_AS: This is the Albedo along with a scratch map that acts as a mask that sits ontop of the colors so as to not recieve any color.
 The RGB color channels are for the Albedo.
 The "Alpha" channel is unused atm. a change may come regarding this and guidance will be pushed out at that time.

Tile_Name_MMSM: This is the Metalness and Smoothness texture. 
 The RGB color channels are for Metalness.
 The "Alpha" channel is for Smoothness.

Tile_Name_EMAO: This is the Emissve/Ambient Occlusion texture.
 The RGB color channels are for Emissive.  (Has Color modifier, see material)
 The "Alpha" channel is for Ambient Occlusion.

Tile_Name_NMDM: This is the normal map channel. 
 RGB Channels are for the normal map.
 Alpha channel is reserved for detail mask.

Tile_Name_DWM: This is the dirt/weathering mask. it will be applied ontop of albedo/color map so as to enable blood, dirt, oil, etc to appear ontop of all others and uncolored.
The RGB channels are for any details you wish to have applied.
The "Alpha" channel is for the opacity of all details contained in "RGB".
===================================================================================
