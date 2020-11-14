using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AXGeometry
{
	public class AXImageProcessor
	{




		// Thanks to @Fogman and @Steffan-Poulsen - https://forum.unity.com/threads/contribution-texture2d-blur-in-c.185694/
		public static Texture2D Blur (Texture2D image, int blurSize)
		{
			Texture2D blurred = new Texture2D (image.width, image.height);
     
			// look at every pixel in the blur rectangle
			for (int xx = 0; xx < image.width; xx++) {
				for (int yy = 0; yy < image.height; yy++) {
					float avgR = 0, avgG = 0, avgB = 0, avgA = 0;
					int blurPixelCount = 0;
     
					// average the color of the red, green and blue for each pixel in the
					// blur size while making sure you don't go outside the image bounds
					for (int x = xx; (x < xx + blurSize && x < image.width); x++) {
						for (int y = yy; (y < yy + blurSize && y < image.height); y++) {
							Color pixel = image.GetPixel (x, y);
     
							avgR += pixel.r;
							avgG += pixel.g;
							avgB += pixel.b;
							avgA += pixel.a;
     
							blurPixelCount++;
						}
					}
     
					avgR = avgR / blurPixelCount;
					avgG = avgG / blurPixelCount;
					avgB = avgB / blurPixelCount;
					avgA = avgA / blurPixelCount;
     
					// now that we know the average for the blur size, set each pixel to that color
					for (int x = xx; x < xx + blurSize && x < image.width; x++)
						for (int y = yy; y < yy + blurSize && y < image.height; y++)
							blurred.SetPixel (x, y, new Color (avgR, avgG, avgB, avgA));
				}
			}
			blurred.Apply ();
			return blurred;
		}


	}

}