using UnityEngine;
using System.Collections;


public class AXShapeGenerator {
	
	
	/*
	public static AXSpline Square(float s) {
		float s2 = s/2.0f;
		
		AXTurtle t = new AXTurtle();
		
		t.mov(-s2, -s2);
		t.drw( s2, -s2);
		t.drw( s2,  s2);
		t.drw(-s2,  s2);
		
		return t;
	}
	
	
	
	public static AXSpline Rectangle(float w, float h) {
		float w2 = w/2.0f;
		float h2 = h/2.0f;
		
		AXTurtle t = new AXTurtle();
		
		t.mov(-w2, -h2);
		t.drw( w2, -h2);
		t.drw( w2,  h2);
		t.drw(-w2,  h2);
		
		return t;
	}
	
	public static AXSpline Rectangle(float x1, float x2, float y1, float y2) {
		AXTurtle t = new AXTurtle();
		
		t.mov(x1, y1);
		t.drw(x2, y1);
		t.drw(x2, y2);
		t.drw(x1, y2);
		
		return t;
	}
	
	public static AXSpline Circle(float radius, int segs) {
		
		AXTurtle t = new AXTurtle();

		if (segs == 8 || segs == 16) {
			float ang1 = Mathf.Deg2Rad * 360.0f/(2*segs);
			t.mov(radius*Mathf.Cos(ang1), radius*Mathf.Sin(ang1));
			t.dir(90.0f + 360.0f/(2.0f*segs));
			
		} else {
			t.mov(0,-radius);
			t.dir(0);
		}
		
		t.arcl(360, radius, segs);
		return t;
	}	
	
	
	
	public static AXSpline Romanesque_NaveRespond(float width, float depth, float r2, int segs ) {
		
		// width and depth are of the main block centered in the wall. 
		
		float r3 = r2/4.0f;
		
		AXTurtle t = new AXTurtle();

		
		
		
		t.mov(-width/2.0f, (depth+r2));
		
		t.dir(-90);
		
		t.fwd(depth - r3);
		t.arcl(90, r3, 4);
		
		
		t.fwd((width/2.0f-r2 - r3));
		
		t.dir(-90);
		t.arcl(180, r2, segs);
		
		t.dir(0);
		t.fwd(width/2.0f-r2-r3);
		
		t.arcl(90, r3, 4);
		
		t.drw(width/2.0f, (depth+r2));
		
		
		
		return t;
		
	}	
	
	
	public static AXSpline PilierCantonne(float r1, float r2, float offset, int segs1, int segs2) {
		
		
		if (r2 < (offset-r1)) offset = (r1+r2)*.95f;
		
		AXSpline c  = AXShapeGenerator.Circle(r1, segs1);
		
		AXSpline c1 = AXShapeGenerator.Circle(r2, segs2);
		AXSpline c2 = AXShapeGenerator.Circle(r2, segs2);
		AXSpline c3 = AXShapeGenerator.Circle(r2, segs2);
		AXSpline c4 = AXShapeGenerator.Circle(r2, segs2);
		
		c1.shift(  offset,    	  0);
		c2.shift( -offset,		  0);
		c3.shift(       0,	 offset);
		c4.shift(	    0,	-offset);
		
		AXSpline u = c.union(c1);
		u = u.union(c2);
		u = u.union(c3);
		u = u.union(c4);
		
		return u;
		
	}
	
	
	public static AXSpline am_vousoir2(float w, float h, float r1, int segs) {		
		var a = h -  4.0f*r1;
		var b = w - (4.0f*r1 * 2.0f);
		
		if (b < 0) {
			r1 = (w - .01f) / 8.0f;
			a = h -  4*r1;
		}
		
		AXTurtle t = new AXTurtle();

		//float inset = 2;
		
		t.mov(-w/2.0f,h);
		t.dir(0);
		
		t.right(a);
		t.fwd(r1);
		t.arcr(180, r1, segs);
		t.arcl(270, r1, segs);
		t.arcr(180, r1, segs);
		t.fwd(r1);
		
		t.dir(90);
		
		//t.right(b);
		//t.fwd(r1);
		//t.arcr(180, r1, segs);
		//t.arcl(270, r1, segs);
		//t.arcr(180, r1, segs);
		//t.fwd(r1);
		
		t.drw(w/2.0f, h);
		
		return t;
		
	}
	*/
	
	
}
