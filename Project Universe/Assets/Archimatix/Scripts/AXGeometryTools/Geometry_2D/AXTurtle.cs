using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using AXClipperLib;
using Path 		= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

//using AX;

namespace AXGeometry
{
    [System.Serializable]
    public class AXTurtle {


        float rotation = 90.0f;

        float cos;
        float sin;

        float ratio = Mathf.Deg2Rad;// (2*Mathf.PI) / 360;



        public AXSpline s;

        public Path path;

        public Paths paths;
        public Paths clips;

        //Path colliderPath;

        public PolyTree polytree;

        public Vector2 lastMoveTo;

        public const float minRadius = .0001f;


        //bool drawingHole = false;

        public AXTurtle() {

            updateTrigs();
            dir(0);

            s = new AXSpline();

            paths = new Paths();
            clips = new Paths();

            polytree = new PolyTree();

            //Path collider;

        }


        public void startNewPath(bool isSubj) {
            path = new Path();

            if (isSubj)
                paths.Add(path);
            else
                clips.Add(path);


        }

        public void currentPathAsCollider()
        {
            //colliderPath = path;

            path = new Path();
        }



        public void createBasePolyTreeFromDescription()
        {

            // combine all paths
            polytree = new PolyTree();
            Clipper c = new Clipper(Clipper.ioPreserveCollinear);

            // sort paths for subj and holes


            c.AddPaths(paths, PolyType.ptSubject, true);
            if (clips.Count > 0)
            {
                //Debug.Log ("Have clip");
                c.AddPaths(paths, PolyType.ptClip, true);
            }

            c.Execute(ClipType.ctDifference, polytree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);


        }





        public Path getPath() {
            return path;
        }
        public int sup(float num)
        {
            return (int)(num * AXGeometry.Utilities.IntPointPrecision);
        }

        public void updateTrigs() {
            cos = Mathf.Cos(ratio * rotation);
            sin = Mathf.Sin(ratio * rotation);
        }


        public void dir(float degs) {
            rotation = degs;
            updateTrigs();
        }
        public void adir(float dx, float dy) {
            dir(Mathf.Atan2(dy, dx) / ratio);
        }

        // TURN_L
        public void turnl(float degs) {
            rotation += degs;
            updateTrigs();
        }

        // TURN_R
        public void turnr(float degs) {
            rotation -= degs;
            updateTrigs();
        }


        public void mov(float xx, float yy) {

            Vector2 newMov = new Vector2(lastMoveTo.x + xx, lastMoveTo.y + yy);

            // - clipper
            if (path == null || path.Count > 1)
                startNewPath(true);
            else
                path.Clear();

            path.Add(new IntPoint(sup(xx), sup(yy)));

            lastMoveTo = newMov;

        }

        public void colllider() {
            currentPathAsCollider();

        }


        /*
		public void movfwd(float l) {


			IntPoint curPt = new IntPoint(); 

			// - clipper
			if (path == null || path.Count > 1)
			{
				curPt = path[path.Count-1];
				startNewPath(true);
			}
				
			path.Add( new IntPoint( curPt.X + sup(l*cos), curPt.Y + sup(l*sin) ) );
		}
		*/

        public void assertPath()
        {
            if (path == null)
            {
                mov(0, 0);
                dir(90);
            }
        }

        public void assertPath(Vector2 a)
        {
            if (path == null)
            {
                mov(a.x, a.y);
                dir(90);
            }
        }

        public void fwd(float l, float d = 0) {
            if (l == 0 && d == 0)
                return;

            d = -d;

            assertPath();

            // - clipper 
            if (d == 0)
            {
                IntPoint ip = new IntPoint(path[path.Count - 1].X + sup(l * cos), path[path.Count - 1].Y + sup(l * sin));

                if (path.Count > 0 && ip != path[path.Count - 1])
                    path.Add(ip);
            }
            else
            {
                float ang = -Mathf.Atan2(d, l) * Mathf.Rad2Deg;

                turnr(ang);

                fwd(Mathf.Sqrt(l * l + d * d));

                turnl(ang);

            }

        }

        public void movfwd(float l) {

            // - clipper 
            IntPoint prev = path[path.Count - 1];
            startNewPath(true);
            path.Add(new IntPoint(prev.X + sup(l * cos), prev.Y + sup(l * sin)));
        }
        public void movfwd(float l, float d) {

            // - clipper 
            IntPoint prev = path[path.Count - 1];
            startNewPath(true);
            path.Add(new IntPoint(prev.X + sup(l * cos) - sup(d * sin), prev.Y + sup(l * sin) - sup(d * cos)));
        }


        public void back(float l, float d = 0) {
            if (l == 0)
                return;

            turnl(180f);
            fwd(l, -d);
            turnr(180f);

        }


        public void drw(float xx, float yy) {
            // - axspline


            assertPath();


            IntPoint ip = new IntPoint(sup(xx), sup(yy));
            if (path.Count > 0 && path[path.Count - 1] == ip)
                return;

            path.Add(ip);
        }

        public void rdrw(float xx, float yy) {
            // - clipper 
            if (xx == 0 && yy == 0)
                return;

            path.Add(new IntPoint(path[path.Count - 1].X + sup(xx), path[path.Count - 1].Y + sup(yy)));

            dir(getDir(path[path.Count - 1], path[path.Count - 2]));
        }


        public void newp() 
        {

            rmov(0, 0);
        }


		public void rmov(float xx , float yy) {
			// - clipper 
			IntPoint prev = new IntPoint();
			if (path == null)
				mov(0, 0);

			if (path.Count > 0)
				prev = path[path.Count-1];

			startNewPath(true);

			IntPoint newPt = new IntPoint( prev.X + sup(xx), prev.Y + sup(yy) );

			path.Add (newPt);
			//Debug.Log(newPt);
			//dir(getDir(path[path.Count-1], prev));
		}






	

		public float getDir(IntPoint fromPt, IntPoint toPt)
		{
			//Debug.Log("dy="+(toPt.Y-fromPt.Y));

			float ang = Mathf.Rad2Deg*(Mathf.Atan2( (fromPt.Y-toPt.Y), (fromPt.X-toPt.X)) );
			//Debug.Log(ang);
			return ang;
		}
		
		public void left(float l) {
			if (l==0)
				return;

			turnl(90f);
			fwd(l);
			turnr(90f);
		}
		public void right(float l) {
			if (l==0)
				return;

			turnr(90f);
			fwd(l);
			turnl(90f);
		} 

		public void arcr(float degs , float radius , int segs) {
			
			arcr(degs, radius, segs, degs);
		}
		public void arcr(float degs , float radius , int segs, float perAngle, int min=1) {

			assertPath();

			// VALIDATION: Assert a min radius due to the inprecision of IntPoints where verts would get lost.
			radius = Mathf.Max(minRadius, radius);


			float sug_dtheta  = perAngle/(float)(segs);
			float seg_actual = Mathf.Floor(degs/sug_dtheta);


			seg_actual = (degs == 360f) ? seg_actual-1 : seg_actual;

			// VALIDATION: Assert that actual_segs can never be less than 1.
			//raise min? Always have at least one seg per 120 degs.
			if (degs > 120 && min<2) min=2;
			if (degs > 240 && min<3) min=3;

			seg_actual = Mathf.Max(seg_actual, min);

			float dtheta  = degs/seg_actual;

			
			//trace("dtheta=" + dtheta);
			float opp = radius * Mathf.Sin(ratio * dtheta);
			float adj = radius - (radius * Mathf.Cos(ratio * dtheta));
			
			float span = Mathf.Sqrt(opp*opp + adj*adj);
			
			turnr(dtheta/2f);
			for (int n=0; n<seg_actual; n++) {
				fwd(span);
				if (n != (seg_actual-1f)) turnr(dtheta);
				
			}
			turnr(dtheta/2f);


			
		}

		public void arcl(float degs , float radius , int segs) {
			arcl(degs, radius, segs, degs);
		}

		public void arcl(float degs , float radius , int segs, float perAngle, int min=1) 
		{
			assertPath();

			// VALIDATION: Assert a min radius due to the inprecision of IntPoints where verts would get lost.
			radius = Mathf.Max(minRadius, radius);

			float sug_dtheta  = perAngle/(float)(segs);
			float seg_actual = Mathf.Floor(degs/sug_dtheta);

				seg_actual = (degs == 360f) ? seg_actual-1 : seg_actual;

			// VALIDATION: Assert that actual_segs can never be less than 1.
			if (degs > 120 && min<2) min=2;
			if (degs > 240 && min<3) min=3;

			seg_actual = Mathf.Max(seg_actual, min);

			float dtheta  = degs/seg_actual;


			//Debug.Log("min="+min+", seg_actual="+seg_actual);
			float opp = radius * Mathf.Sin(ratio * dtheta);
			float adj = radius - (radius * Mathf.Cos(ratio * dtheta));
			
			float span = Mathf.Sqrt(opp*opp + adj*adj);
			
			turnl(dtheta/2f);
			for (int n=0; n<seg_actual; n++) {
				
				fwd(span);
				if (n != (seg_actual-1)) turnl(dtheta);
				
			}
			turnl(dtheta/2f);


		}

		public void bezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, int segs)
		{
			assertPath(a);

			float dt = 1f/(float)segs;

			for (float i=1; i<=segs; i++)
			{
				Vector2 pt = AXTurtle.bezierValue(a, b, c, d,  i*dt);
				path.Add (new IntPoint(sup(pt.x), sup(pt.y)));
			}


			if (c.x == d.x && c.y != d.y)
			{
				if (c.y > d.y)
					dir(270);
				else
					dir(90);
			}
			else  
				dir( Mathf.Atan2(d.y-c.y, d.x-c.x) * Mathf.Rad2Deg );


		}

		public void molding(string type, Vector2 a, Vector2 b, float segs = 3, float tension = .3f)
		{
			assertPath(a);

			switch(type)
			{
			case "cove":
				cove(a, b, segs, tension);
				break;

			case "ovolo":
				ovolo(a, b, segs, tension);
				break;

			case "cymarecta":
				cymarecta(a, b, segs);
				break;

			case "cymareversa":
				cymareversa(a, b, segs);
				break;

			case "onion":
				onion(a, b, segs, tension);
				break;

			case "dome":
				dome(a, b, segs, tension);
				break;


			}

		}

		public void cove(Vector2 a, Vector2 b, float segs = 3, float tension = .3f)
		{
			float dt = 1f/(float)segs;

			Vector2 d = b-a;


			float hanx = tension*d.x;
			float hany = tension*d.y;

			// bezier 1

			Vector2 ha = new Vector2(a.x,  a.y+hany);
			Vector2 hb = new Vector2(b.x-hanx, 	b.y);



			Vector2 pt;
			for (float i=1; i<=segs; i++)
			{
				pt = AXTurtle.bezierValue(a, ha, hb, b,  i*dt);
				path.Add (new IntPoint(sup(pt.x), sup(pt.y)));
			}

		}
		public void ovolo(Vector2 a, Vector2 b, float segs = 3, float tension = .3f)
		{
			float dt = 1f/(float)segs;

			Vector2 d = b-a;


			float hanx = tension*d.x;
			float hany = tension*d.y;

			// bezier 1

			Vector2 ha = new Vector2(a.x+hanx,  a.y);
			Vector2 hb = new Vector2(b.x, 	b.y-hany);



			Vector2 pt;
			for (float i=1; i<=segs; i++)
			{
				pt = AXTurtle.bezierValue(a, ha, hb, b,  i*dt);
				path.Add (new IntPoint(sup(pt.x), sup(pt.y)));
			}

		}


		public void cymarecta(Vector2 a, Vector2 b, float segs = 3, float tension = .3f)
		{
			float dt = 1f/(float)segs;

			Vector2 d = b-a; 

			Vector2 midpt = a + d/2;

			float hanx = tension*d.x;
			float hany = tension*d.y;

			// bezier 1

			Vector2 ha1 = new Vector2(a.x+hanx,  a.y);
			Vector2 ha2 = new Vector2(midpt.x-hanx/2, 	midpt.y-hany);

			Vector2 pt;
			for (float i=1; i<=segs; i++)
			{
				pt = AXTurtle.bezierValue(a, ha1, ha2, midpt,  i*dt);
				path.Add (new IntPoint(sup(pt.x), sup(pt.y)));
			}

			// bezier 2

			Vector2 hb1 = new Vector2(midpt.x+hanx/2, 		midpt.y+hany);
			Vector2 hb2 = new Vector2(b.x-hanx, 	b.y);

			for (float i=1; i<=segs; i++)
			{
				pt = AXTurtle.bezierValue(midpt, hb1, hb2, b,  i*dt);
				path.Add (new IntPoint(sup(pt.x), sup(pt.y)));
			}

		}



		public void cymareversa(Vector2 a, Vector2 b, float segs = 3, float tension = .3f)
		{
			float dt = 1f/(float)segs;

			Vector2 d = b-a;

			Vector2 midpt = a + d/2;

			float hanx = tension*d.x;
			float hany = tension*d.y;

			Vector2 pt;

			// bezier 1

			Vector2 ha1 = new Vector2(a.x,  a.y+hanx);
			Vector2 ha2 = new Vector2(midpt.x-hanx, 	midpt.y-hany/2);
			for (float i=1; i<=segs; i++)
			{
				pt = AXTurtle.bezierValue(a, ha1, ha2, midpt,  i*dt);
				path.Add (new IntPoint(sup(pt.x), sup(pt.y)));
			}

			// bezier 2

			Vector2 hb1 = new Vector2(midpt.x+hanx, 		midpt.y+hany/2);
			Vector2 hb2 = new Vector2(b.x, 	b.y-hany);
			for (float i=1; i<=segs; i++)
			{
				pt = AXTurtle.bezierValue(midpt, hb1, hb2, b,  i*dt);
				path.Add (new IntPoint(sup(pt.x), sup(pt.y)));
			}

		}


		public void onion (Vector2 a, Vector2 b, float segs=3,  float tension = .9f)
		{


			// VALIDATION: Assert at least 3 sides
			segs = Mathf.Max(segs, 3);

			float dt = 1f/(float)segs;

			Vector2 d = b-a;

			Vector2 midpt = a + d/2;
			midpt.x = a.x - .2f*d.x;
			midpt.y = a.y + .5f*d.y;

			float hanx = 1.2f*Mathf.Abs(tension*d.x);
			float hany = .20f * Mathf.Abs(tension*d.y);



			Vector2 pt;


			// bezier 1

			Vector2 ha1 = new Vector2(a.x+hanx,  a.y);
			Vector2 ha2 = new Vector2(midpt.x+hanx, 	midpt.y-hany);
			for (float i=1; i<=segs; i++)
			{
				pt = AXTurtle.bezierValue(a, ha1, ha2, midpt,  i*dt);
				path.Add (new IntPoint(sup(pt.x), sup(pt.y)));
			}


			// bezier 2

			Vector2 hb1 = new Vector2(midpt.x-hanx, 		midpt.y+hany);
			Vector2 hb2 = new Vector2(b.x, 	b.y-.5f*hany);
			for (float i=1; i<=segs; i++)
			{
				pt = AXTurtle.bezierValue(midpt, hb1, hb2, b,  i*dt);
				path.Add (new IntPoint(sup(pt.x), sup(pt.y)));
			}




		}

		public void dome(Vector2 a, Vector2 b, float segs = 3, float tension = .3f)
		{
			float dt = 1f/(float)segs;

			Vector2 d = b-a;


			float hanx = tension*d.x;
			float hany = tension*d.y;

			// bezier 1

			//Vector2 ha = new Vector2(a.x+hanx,  a.y);
			//Vector2 hb = new Vector2(b.x, 	b.y-hany);

			Vector2 ha = new Vector2(a.x,  a.y+hany);
			Vector2 hb = new Vector2(b.x-hanx, 	b.y);



			Vector2 pt;
			for (float i=1; i<=segs; i++)
			{
				pt = AXTurtle.bezierValue(a, ha, hb, b,  i*dt);
				path.Add (new IntPoint(sup(pt.x), sup(pt.y)));
			}

		}

        public static Path VerticalLine(Vector2 pt1, float height)
        {
            Path retp = new Path();

            IntPoint ip = Pather.Vector2IP(pt1);
            retp.Add(ip);
            retp.Add(new IntPoint(ip.X, ip.Y + Pather.Float2IntPoint(height)));

            return retp;

        }

        public static Path Rectangle(float width, float height, bool isCentered = true)
        {

            AXTurtle t = new AXTurtle();
            if (isCentered)
                t.mov(-width / 2, -height / 2);
            else
                t.mov(0, 0);

            t.dir(90);

            t.right(width);
            t.fwd(height);
            t.left(width);

            return t.getPath();
        }


        public static Path Rectangle(Rect rect)
        {

            AXTurtle t = new AXTurtle();

            t.mov(rect.x, rect.y);

            t.dir(90);

            t.right(rect.width);
            t.fwd(rect.height);
            t.left(rect.width);

            return t.getPath();
        }



        public static Path RoundedRectangle(float width, float height, float r, int segs = 1, bool grounded = false)
        {
            AXTurtle t = new AXTurtle();

            t.mov(-width / 2 + r, -height / 2);
            t.dir(0);

            t.fwd(width - 2 * r);
            t.arcl(90, r, segs);
            t.fwd(height - 2 * r);
            t.arcl(90, r, segs);

            t.fwd(width - 2 * r);
            t.arcl(90, r, segs);
            t.fwd(height - 2 * r);
            t.arcl(90, r, segs);
            return t.getPath();
        }





        public static Path RoundedRectangle(Rect rect, float r, int segs = 1)
        {
            AXTurtle t = new AXTurtle();

            float r2 = r * 2;
            t.mov(rect.x + r, rect.y);
            t.dir(0);

            t.fwd(rect.width    - r2);
            t.arcl(90, r, segs);
            t.fwd(rect.height   - r2);
            t.arcl(90, r, segs);

            t.fwd(rect.width    - r2);
            t.arcl(90, r, segs);
            t.fwd(rect.height   - r2);
            t.arcl(90, r, segs);
            return t.getPath();
        }

        public static Path Circle(float radius, int segs, bool rectify = false)
        {

            // VALIDATION: Assert at least 3 sides
            segs = Mathf.Max(segs, 3);

            Path circlePath = new Path(); ;

            float segsf = (float)segs;

            float dsegsf = 360f / segsf;
            // circle spline creation...

            for (int i = 0; i < segs; i++)
            {
                float rads = Mathf.Deg2Rad * (((float)i) * dsegsf);

                //Debug.Log("rads = "+rads);

                IntPoint pt = new IntPoint((radius * Mathf.Cos(rads) * AXGeometry.Utilities.IntPointPrecision), (radius) * Mathf.Sin(rads) * AXGeometry.Utilities.IntPointPrecision);
                circlePath.Add(pt);
            }
            return circlePath;

        }
        public static Path Circle(float radius, int segs, Vector2 center, bool rectify = false)
        {
            Path circle = Circle(radius, segs, rectify);
            Pather.shiftPath(circle, center);

            return circle;

          


        }

        public static Path Arc(float radius, float begAngle = 0, float endAngle = 270, int segs = 8)
        {
            Path arcPath = new Path();

            float arcAngle = endAngle - begAngle;

            float deltaAngle = arcAngle / (float)segs;

            // circle spline creation...
            for (int i = 0; i <= segs; i++)
            {
                float rads = Mathf.Deg2Rad * (begAngle + i * deltaAngle);
                IntPoint pt = new IntPoint((radius * AXGeometry.Utilities.IntPointPrecision) * Mathf.Cos(rads), (radius * AXGeometry.Utilities.IntPointPrecision) * Mathf.Sin(rads));
                arcPath.Add(pt);
            }
            return arcPath;
        }



        // AN ARC THAT BEGINS AND ENDS WITH SEGMENTS PERPENDICULAR TO THEIR ANGLES
        public static Path SemiCircS(float radius, float flat, int segs = 8)
        {
            Path arcPath = new Path();

            float arcAngle = 180;

            float deltaAngle = arcAngle / (float)segs;

            long flatLong = (long) (flat * AXGeometry.Utilities.IntPointPrecision);

            IntPoint pt0 = new IntPoint((radius * AXGeometry.Utilities.IntPointPrecision), 0);
            arcPath.Add(pt0);

            // circle spline creation...
            for (int i = 0; i <= segs; i++)
            {
                float rads = Mathf.Deg2Rad * (i * deltaAngle);
                IntPoint pt = new IntPoint((radius * AXGeometry.Utilities.IntPointPrecision) * Mathf.Cos(rads), (radius * AXGeometry.Utilities.IntPointPrecision) * Mathf.Sin(rads) + flatLong);
                arcPath.Add(pt);
            }

            IntPoint ptn = new IntPoint((-radius * AXGeometry.Utilities.IntPointPrecision), 0);
            arcPath.Add(ptn);

            

            return arcPath;
        }





        // BI_CHAMFER_SIDE

        public static Path BiChamferSide(	float 	H			= 1, 
											float 	R2			= 0, 	
											float 	R1			= 0, 
											int 	SegsPer90	= 3,
											bool 	BevelOut	= true, 
											float 	Taper		= 0, 
											float 	TopLip		= 0, 
											float 	BotLip		= 0,
											float 	LipEdge		= 0,
											float 	LipEdgeBot	= 0,
											int		Segs		= 1
											)
		{

			// DERIVED VARIABLES

			/*
			float o 		= H-(R1+R2);
			float a 		= Mathf.Atan2(o, Taper) * Mathf.Rad2Deg;

			float d 		= Mathf.Sqrt(Taper*Taper + o*o);
			float dr 		= Mathf.Abs(R1-R2);
			float b 		= Mathf.Asin(dr/d) * Mathf.Rad2Deg;

			// his is the slope of the line or straight edge of the extrude side.
			float dir 		= (R2 > R1) ? 180 - (a+b)  : 270 - (a + (90-b));

			float s 		= Mathf.Sqrt(d*d - dr*dr);
			*/

             

			float o 		= H-(R1+R2);
			float l 		= Taper+R2-R1;

			float a 		= Mathf.Atan2(o, l) * Mathf.Rad2Deg;

			float d 		= Mathf.Sqrt(l*l + o*o);
			float dr 		= Mathf.Abs(R1-R2);
			float s 		= Mathf.Sqrt(d*d - dr*dr);


			float b 		= Mathf.Asin(dr/d) * Mathf.Rad2Deg;

			// his is the slope of the line or straight edge of the extrude side.
			float dir 		= (R2 > R1) ? 180 - (a+b)  : 270 - (a + (90-b));

             



			// START_X
			//float startX = 0;
			float startX = (R2 > R1) ? R2-R1 : 0;

			startX -= BotLip;

			if (! BevelOut)
				startX -= R1;


			float startY = LipEdgeBot;


			// DRAW SHAPE

			AXTurtle t = new AXTurtle();

			t.mov(startX, startY);
			t.dir(90);

			if (LipEdgeBot != 0)
				t.back(LipEdgeBot);

			t.dir(0);

			if (BotLip > 0)
				t.fwd(BotLip);

			// Chamfer Bottom

			if (R1 > 0)
				t.arcl(dir, R1,  Mathf.FloorToInt((dir/90f)*SegsPer90));
			else 
				t.dir( dir );

			for (int i=0; i<Segs; i++)
				t.fwd(s/Segs);



			// Chamfer Top

			if (R2 > 0)
				t.arcl((180-dir), R2, Mathf.FloorToInt(((180-dir)/90f)*SegsPer90));
			else 
				t.dir(180);

			// TopLip
			if (TopLip > 0)
				t.fwd(TopLip);


			if (LipEdge != 0)
				t.left(LipEdge);


			return t.path;
		}








		// TAPERED_SIDE

		public static Path Tapered_Side(	float 	H			= 1, 
											float 	RBase		= .5f, 	
											float 	RTop		= .4f, 	
											float 	HoleRadius	= .05f, 	
											float 	ShiftY	= .05f, 	
											float 	RBevel		= 0, 
											int 	SegsPer90	= 3,
											int		SideSegs	= 1
											)
		{
			//Debug.Log("RBevel="+RBevel);

			// DERIVED VARIABLES

			float T = RBase - RTop;
			float a 		= Mathf.Atan2(H, T);// * Mathf.Rad2Deg;
			float a_degs  = a * Mathf.Rad2Deg;


			//Debug.Log ("H="+H+", T="+ T +", a_degs="+ a_degs );
			float cos_a = Mathf.Cos(a);
			float sin_a = Mathf.Sin(a);


			//float s1 = RBevel * (H/T);  
			float s1 = RBevel * cos_a;  
			float s2 = (RBevel + s1) * (T/H);
			float t1  = RBevel + s2;


			//float s3 = RBevel * sin_a;
			//float t2 = s3-s1;


			float M = H-2*RBevel;

			//Debug.Log ("t2="+t2+", H="+H+", M="+M);

			float S =  (sin_a == 0) ? M : (M / sin_a);



			//Debug.Log("H="+H+", O="+O+", d="+d + " a="+a_degs+", sin_a="+sin_a +", cos_a="+cos_a);



//			float bottomLip = RBase - t1 - HoleRadius;
//			float topLip 	= RTop  - t2 - HoleRadius;

			float taperAng = 180-a_degs;


			// START_X
			//float startX = 0;
			float startX = -(t1);// (R2 > R1) ? R2-R1 : 0;
			//Debug.Log("t1="+t1);
			//startX -= BotLip;



			float startY = 0;// ShiftY;


			// DRAW SHAPE

			AXTurtle t = new AXTurtle();

			t.mov(startX, startY);
			//t.dir(90);

			//if (HoleDepth != 0)
			//	t.back(HoleDepth);

			t.dir(0);

//			if (bottomLip > 0)
//				t.fwd(bottomLip);

			// Chamfer Bottom

			if (RBevel > 0)
				t.arcl(taperAng, RBevel,  Mathf.FloorToInt((taperAng/90f)*SegsPer90));
			else 
				t.dir( taperAng );

//			float vsegment = S/((float)SideSegs);
//			for (int i=0; i<SideSegs; i++)
//				t.fwd(vsegment);
			t.fwd(S);


			// Chamfer Top

			if (RBevel > 0)
				t.arcl((a_degs), RBevel, Mathf.FloorToInt(((a_degs)/90f)*SegsPer90));
			else 
				t.dir(180);

			// TopLip
//			if (topLip > 0)
//				t.fwd(topLip);


//			if (HoleDepth != 0)
//				t.left(HoleDepth);

			//Debug.Log ("dir = " + t.rotation +"rBase = " + RBase + ", rTop = " + RTop + "T = " + T + ", LAST "+ t.path [t.path.Count - 1].X);

			return t.path;
		}



		// ECHINUS

		public static Path Echinus(	
			float 	H			= 1, 
			float 	RBase		= .5f, 	
			float 	RTop		= .4f, 	
			float 	TaperBase	= .4f, 	
			float 	RBevBase	= .4f, 	
			float 	RBevTop		= .4f, 	
			float 	HanBase		= .4f, 	
			float 	HanTop		= .4f, 	
			float 	HoleRadius	= .05f, 	
			float 	HoleDepth	= .05f, 	
			int 	SegsPer90	= 5,
			int		SideSegs	= 1
			)
		{
			//Debug.Log("RBevel="+RBevel);

			// DERIVED VARIABLES

			// BASE
			float a_degs = 180-TaperBase;
			float a 		= a_degs*Mathf.Deg2Rad;
			float cos_a = Mathf.Cos(a);
			float sin_a = Mathf.Sin(a);


			float s1 = RBevBase 		 * cos_a;  
			float SY = RBevBase + s1;
			float SX = SY * cos_a;

			float t1  = RBevBase + SX;

			//float bottomLip = RBase - t1 - HoleRadius;

			float HanBaseX = -SX - HanBase * cos_a; 
			float HanBaseY =  SY + HanBase * sin_a; 

			// TOP

			float W 	= RTop-RBase;

			float b 	= Mathf.Atan2(H, W);// * Mathf.Rad2Deg;

			float cos_b = Mathf.Cos(b);
			float sin_b= Mathf.Sin(b);
			float b_degs = b * Mathf.Rad2Deg;

			float d1 = RBevTop 		 * cos_b;  
			float DY = H - (RBevTop + d1);
			float DX = DY * (W/H);


			//float TaperTop = Mathf.Atan2(H, W);

			float HanTopX = DX - HanTop * cos_b;
			float HanTopY = DY - HanTop * sin_b;




			//Debug.Log("H="+H+", O="+O+", d="+d + " a="+a_degs+", sin_a="+sin_a +", cos_a="+cos_a);



			// START_X
			float startX = -(t1);// (R2 > R1) ? R2-R1 : 0;
			float startY = 0;


			// DRAW SHAPE

			AXTurtle t = new AXTurtle();

			t.mov(startX, startY);
			t.dir(0);



			// Chamfer Bottom

			//Debug.Log("TaperBase="+TaperBase+", RBevBase=" + RBevBase);


			if (RBevBase > 0)
				t.arcl(TaperBase, RBevBase,  Mathf.FloorToInt((TaperBase/90f)*SegsPer90));
			else 
				t.dir( TaperBase );

			Vector2 A 	= new Vector2(-SX, SY);
			Vector2 AH 	= new Vector2(HanBaseX, HanBaseY);
			Vector2 BH 	= new Vector2(HanTopX, HanTopY);
			Vector2 B 	= new Vector2(DX, DY);


			t.bezier(A, AH, BH, B, SegsPer90);

			
			// Chamfer Top

			if (RBevTop > 0)
				t.arcl((a_degs+90), RBevTop, Mathf.FloorToInt(((b_degs)/90f)*SegsPer90));
			else 
				t.dir(180);



			return t.path;
		}







		public static Vector2  bezierValue(Vector2 pt0, Vector2 pt1, Vector2 pt2, Vector2 pt3, float t)
		{
			
			return 	  Mathf.Pow((1-t), 3)*pt0    +    3*Mathf.Pow((1-t), 2)*t*pt1    +    3*(1-t)*t*t*pt2   +   Mathf.Pow(t, 3)*pt3;

		}




		float CosH(float t) 
		{
       		return (Mathf.Exp(t) + Mathf.Exp(-t))/2;
    	}









	


		//Catenary returns a DelegateFunction
//  		public Path Caterary(double P1x, double P1y, double P2x, double P2y, double L){
//    
//	    double d = P2x - P1x; //d is the distance on the x axis
//	    double h = P2y - P1y; //h is the distance on the y axis
//	    
//	    // g is: the function is a
//	    DelegateFunction g = delegate(double a0){ return 2.0 * a0 * Math.Sinh(d / (2.0 * a0)) - Math.Sqrt((L * L) - (h * h)); };
//	    
//	    // dg is: the first derivative in a
//	    DelegateFunction dg = delegate(double a0){ return 2.0 * Math.Sinh(d / (2.0 * a0)) - d * Math.Cosh(d / (2.0 * a0)) / a0; };
//	    
//	    int iterations; double errorMargin;
//	    double a = SolveNumerically(g, dg, 1.0, tolerance, 100, out iterations, out errorMargin);
//	    
//	    double x1 = 0.5 * (a * Math.Log((L + h) / (L - h)) - d);
//	    double x2 = 0.5 * (a * Math.Log((L + h) / (L - h)) + d);
//	    
//	    double k = x1 - P1x;
//	    double m = P1y - a * Math.Cosh(x1 / a);
//	    
//	    Print(string.Format("z = {0:0.00}* cosh((x + {1:0.00}) / {2:0.00}) + {3:0.00}", a, k, a, m));
//	    Print(string.Format("took {0} iterations solve", iterations));
//	    
//	    return delegate(double v){ return a * Math.Cosh((v + k) / a) + m; };
//	  }

		/*
		public static Vector2  bezierValue(Vector2 pt0, Vector2 pt1, Vector2 pt2, Vector2 pt3, float t)
		{
			return    Mathf.Pow((1-t), 3)*pt0    +    3*Mathf.Pow((1-t), 2)*t*pt1    +    3*(1-t)*t*t*pt2   +   Mathf.Pow(t, 3)*pt3;

		}
		public static Vector2  bezierValue(CurvePoint a, CurvePoint b, float t)
		{
			return    Mathf.Pow((1-t), 3)*a.position    +    3*Mathf.Pow((1-t), 2)*t*(a.position+a.localHandleB)    +    3*(1-t)*t*t*(b.position+b.localHandleA)   +   Mathf.Pow(t, 3)*b.position;

		}
		*/

		
	}

}