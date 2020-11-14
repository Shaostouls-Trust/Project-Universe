using UnityEngine;
using System.Collections;

//using AX;

namespace AXGeometry
{
	

	public class AXLine  {
				
		Vector2 v1;
		Vector2 v2;
		
		float slope; 
		float b_constant;
		
		
		// use 2 points to initialize the line constants
		public AXLine(Vector2 vv1, Vector2 vv2) {
			v1 = vv1;
			v2 = vv2;
			
			// set the slope of the line
			if (Mathf.Abs(v2.x - v1.x) < 0.00001 ) {
				// line is vertical
				slope = Mathf.Infinity;
			} else if (Mathf.Abs(v2.y - v1.y) < 0.00001) {
				// line is horizontal
				slope = 0;
			} else {
				slope = (v2.y - v1.y) / (v2.x - v1.x);
			}
			
			// set b, the displacement of the line
			b_constant = v1.y - slope * v1.x;		
			
		}
		
		
		public float getLength() {
			return Vector2.Distance(v1, v2);
		}
		
		public bool isHorizontal() {
			if (slope == 0) 
				return true;
			return false;
		}
		
		public bool isVertical() {
			if (slope == Mathf.Infinity) 
					return true;
			return false;
		}
		
		public float getAngle() {
			if (slope == Mathf.Infinity) 
				return Mathf.PI/2;

			return Mathf.Atan(slope);
		}
		
		public Vector2 intersection (AXLine l) {
			if (slope == l.slope) {
				// parallel
				return new Vector2(-999999, -999999);	
			}
			
			float xx;
			float yy;
			if (isVertical()) {
				xx = v1.x;
				yy = l.getY(xx);
				return new Vector2(xx, yy);
			} else if (isHorizontal()) {
				yy = v1.y;
				xx = l.getX(yy);
				return new Vector2(xx, yy);
			}

			// ... replace line below with the actual intersection 
			return new Vector2(0, 0);
		}
		
		
		public float getX(float yy) {
			if (v1.x == v2.x) 
				return v1.x;
			
			float tmp_y = yy;
			
			return (tmp_y-b_constant)/slope;
		}
		
		
		public float getY(float x) {
			if (v1.y == v2.y) 
				return v1.y;
			return slope*x + b_constant;
		}
		
		public Vector2 midPoint() {
			return new Vector2((v2.x - v1.x)/2 + v1.x, (v2.y - v1.y)/2 + v1.y);
		}
		
		
		public bool isInLine(Vector2 v) {
			
			float accu = AXGeometry.Utilities.IntPointPrecision;
			float perc = .001f;
			

			float ymin 	= Mathf.Floor( accu * Mathf.Min(v1.y, v2.y) )   / accu;
			float ymax 	= Mathf.Floor( accu * Mathf.Max(v1.y, v2.y) )   / accu;
			float y 		= Mathf.Floor( accu * v.y ) 					/ accu;
			
			float xmin 	= Mathf.Floor( accu * Mathf.Min(v1.x, v2.x) )   / accu;
			float xmax 	= Mathf.Floor( accu * Mathf.Max(v1.x, v2.x) )   / accu;
			float x 		= Mathf.Floor( accu * v.x ) 					/ accu;
			
			if (xmin-perc <= x && x <= xmax+perc  && ymin-perc <= y && y <= ymax+perc) {
				return true;
			} else {
				return false;
			}
			
		}
		
		public bool isOnLine(Vector2 v) {
			float accu = AXGeometry.Utilities.IntPointPrecision;
			float perc = .001f;
			
			if (v1.x == v2.x) {
				float ymin 	= Mathf.Floor( accu * Mathf.Min(v1.y, v2.y) )	/  accu;
				float ymax 	= Mathf.Floor( accu * Mathf.Max(v1.y, v2.y) )	/  accu;
				float y 	= Mathf.Floor( accu * v.y ) 					/ accu;
				if (v.x == v1.x) {
					if (ymin-perc <= y && y <= ymax+perc) 
						return true;
					return false;
				}
			} else if (v.y == v2.y) {
				float xmin  	= Mathf.Floor( accu * Mathf.Min(v1.x, v2.x) )  /  accu;
				float xmax  	= Mathf.Floor( accu * Mathf.Max(v1.x, v2.x) )  /  accu;
				float x  		= Mathf.Floor( accu * v.x ) 					 / accu;
				
				if (v.y == v1.y) {
					if(xmin-perc <= x && x <= xmax+perc) 
						return true;
					return false;
				}
			}
			
			float xx = getX(v.y);
			
			if (xx == v.x) 
				return true;
			
			return false;
			
			
			
		}
		
		
		
		
	}
}
