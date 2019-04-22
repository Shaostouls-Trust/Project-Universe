When setting up materials they should be as follows

Tile_Name_CM: This is the Color Map,
 The "Red" color channel is the "Main" color, (Has Color modifier, see material)
 The "Green" Channel is the "Secondary" color, (Has Color modifier, see material)
 the "Blue" color channel is the "Detail" Color, (Has Color modifier, see material)
 The "Alpha" channel is the Trim Color. Ensure any detail on each channel are 255,0,0 0,255,0 0,0,255 and 255,255,255 respectively.

 Otherwise it will cause some funkyness in the end color. Additionally, when making the Albedo, all colors on things intended to be colorable should be  neutral gray (50%) in order to be colored best.

Tile_Name_AS: This is the Albedo along with a scratch map that acts as a mask that sits ontop of the colors so as to not recieve any color.
 The "Red, Green, and Blue" color channels are for the Albedo.
 The "Alpha" channel is for the scratches/things you absolutely do not want colored under any circumstances.

Tile_Name_MS: This is the Metalness and Smoothness texture. 
 The "Red, Green, and Blue" color channels are for Metalness.
 The "Alpha" channel is for Smoothness.

Tile_Name_EAO: This is the Emissve/Ambient Occlusion texture.
 The "Red, Green, and Blue" color channels are for Emissive.  (Has Color modifier, see material)
 The "Alpha" channel is for Ambient Occlusion.

Tile_Name_NM: This is the normal map channel. 
 All channels are for normal map.

Tile_Name_DM: This is the detail mask.
 All channels are for detail mask.

Tile_Name_DW: This is the dirt/weathering mask. it will be applied ontop of albedo/color map so as to enable blood, dirt, oil, etc to appear ontop of all others and uncolored.

The "Red, Green and Blue" channels are for any details you wish to have applied.
The "Alpha" channel is for the opacity of all details contained in "RGB".

