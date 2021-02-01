using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXGeometry;
using AXCurvePoint = AXGeometry.CurveControlPoint2D;
using AXCurve = System.Collections.Generic.List<AXGeometry.CurveControlPoint2D>;

using CsPotrace;


namespace AXGeometry
{

    [System.Serializable]
    public class Pather
    {

        public Path path;
        public int[] segment_lengths;

        public Pather(Path p)
        {
            path = p;
            segment_lengths = getSegmentLengths();
        }



        public static int getSegLenBasedOnSubdivision(Path path, int subdivision)
        {
            if (subdivision <= 0)
                return 10000;

            Paths paths = new Paths();
            paths.Add(path);
            return getSegLenBasedOnSubdivision(paths, subdivision);

        }

        public static int getSegLenBasedOnSubdivision(Paths paths, int subdivision)
        {
            if (subdivision <= 0)
                return 10000;

            IntRect bounds = AXClipperLib.Clipper.GetBounds(paths);



            long bot = bounds.bottom;
            long top = bounds.top;
            long left = bounds.left;
            long right = bounds.right;



            if (left > right)
            {
                left = bounds.right;
                right = bounds.left;
            }
            if (bot > top)
            {
                bot = bounds.top;
                top = bounds.bottom;
            }
            int width = (int)Math.Abs(right - left);
            int height = (int)Math.Abs(top - bot);

            int max = width > height ? width : height;

            return max / subdivision;



        }


        public static Paths cleanPaths(Paths paths, int t = 10)
        {
            Paths retps = new Paths();

            for (int i = 0; i < paths.Count; i++)
            {
                retps.Add(cleanPath(paths[i], t));
            }

            return retps;

        }
        public static Path cleanPath(Path path, int t = 10)
        {

            Path retp = new Path();

            int t2 = t * t;

            for (int i = 0; i < path.Count - 1; i++)
            {
                long d2 = DistanceSquared(path[i], path[i + 1]);


                if (d2 < t2)
                {

                    retp.Add(Lerp(path[i], path[i + 1], .5f));
                    i++;
                }
                else
                {
                    retp.Add(path[i]);

                    if (i == path.Count - 2)
                        retp.Add(path[i + 1]);
                }
            }

            return retp;

        }


        public static float getAreaInSquareMeters(Path path)
        {
            double pathArea = Clipper.Area(path);
            return IntArea2SquareMeters(pathArea);
        }
        public static float IntArea2SquareMeters(double pathArea)
        {
            return (float)pathArea / ((float)AXGeometry.Utilities.IntPointPrecision * AXGeometry.Utilities.IntPointPrecision);

        }

        public static float getHeightFloat(Path path)
        {
            IntRect b = Pather.getBounds(path);
            return Mathf.Abs(Pather.IntPoint2Float(b.top - b.bottom));
        }


        public static Paths segmentPaths(Paths paths, long bay = 5000, bool isClosed = true)
        {
            Paths retps = new Paths();

            for (int i = 0; i < paths.Count; i++)
            {
                retps.Add(segmentPath(paths[i], bay, isClosed));
                //AXGeometry.Utilities.printPath(segmentPath(paths[i], bay, isClosed));
            }
            //AXGeometry.Utilities.printPaths(retps);
            return retps;

        }

        public static Path segmentPath(Path path, long bay = 5000, bool isClosed = true)
        {
            Path retp = new Path();

            int endIndex = path.Count - 1;

            for (int i = 0; i <= endIndex; i++)
            {
                retp.Add(path[i]);

                int next = (i == path.Count - 1) ? 0 : i + 1;

                if (next == 0 && !isClosed)
                {
                    break;
                }
                //Debug.Log("i="+i+", next="+next);
                long d = Distance(path[i], path[next]);

                if (d > bay)
                {
                    //a dd interstitial points
                    int steps = (int)(d / bay);



                    for (int j = 1; j < steps; j++)
                    {
                        //Debug.Log("- " + j);

                        float t = ((float)j) / ((float)steps);
                        retp.Add(Lerp(path[i], path[next], t));
                    }
                }
                //				


            }

            //retp.Add(path[path.Count-1]);

            return retp;


        }



        // MANPAGE: http://www.archimatix.com/uncategorized/axspline-getinsetcornerspines
        public Paths getInsetCornerSplinesOLD(float inset)
        {
            //There can't be more subsplines then there are vertices...
            // Each of these subslines will have at least 3 points


            Paths returnPaths;


            // PLANSWEEPS AT CORNERS
            // First, go around and group verts that are closer
            // to  each other than min_sengmentLength
            float min_sengmentLength = 2 * inset;



            // FIND FIRST CURSOR VERTEX

            int cursor = 0; // essentially 0

            if (segment_lengths[cursor] < min_sengmentLength)
            {
                cursor = path.Count - 1;

                // back up to first long segment you find
                while (segment_lengths[cursor] < min_sengmentLength)
                {
                    if (cursor == 0)
                    {
                        // if we mad it all the way back to 0, then all the segments were too small.
                        // just return this AXSpline
                        returnPaths = new Paths();
                        returnPaths.Add(path);
                        return returnPaths;
                    }
                    cursor--;
                }

            }


            // OK: Now we have our starting point: cursor. 
            // Proceed forward from here with the grouping.

            // Use a single array of ints with -88888 as seperators and -99999 as terminator.
            // Cant have more the 2*verCount entries (a seperator between each vert)

            int[] groupedIndicies = new int[2 * path.Count * 100];

            int index = 0;
            groupedIndicies[index++] = cursor++;


            int countOfSplines = 1;

            // GROUP VERTS THAT DEFINE THE SUBSPLINES
            while ((cursor % path.Count) != groupedIndicies[0])
            {
                if (segment_lengths[cursor % path.Count] > min_sengmentLength)
                {
                    countOfSplines++;
                    groupedIndicies[index++] = -88888; // add break code
                }

                groupedIndicies[index++] = cursor % path.Count;

                // starting from cursor, add vertices to subspline

                cursor++;
            }

            // done... add terminator
            groupedIndicies[index++] = -99999;



            // Take each group and add a beginning and ending vertex inset by margin.
            returnPaths = new Paths();


            Path subpath = null;

            for (int j = 0; j < groupedIndicies.Length; j++)
            {
                if (j == 0 || groupedIndicies[j] == -88888 || groupedIndicies[j] == -99999)
                {
                    // End a spline
                    if (groupedIndicies[j] == -88888 || groupedIndicies[j] == -99999)
                    {
                        // Add end vert
                        int nexti = (groupedIndicies[j - 1] + 1) % path.Count;
                        float percentInset = inset / segment_lengths[nexti];
                        IntPoint endVert = Lerp(path[groupedIndicies[j - 1]], nextPoint(groupedIndicies[j - 1]), percentInset);

                        subpath.Add(endVert);
                        returnPaths.Add(subpath);

                        if (groupedIndicies[j] == -99999)
                            break;
                    }

                    // Begin a spline
                    if (j == 0 || groupedIndicies[j] == -88888)
                    {
                        // skip over -88888
                        if (groupedIndicies[j] == -88888)
                            j++;
                        // start new AXSpline...
                        subpath = new Path();

                        //int nexti = (groupedIndicies[j-1]+1) % path.Count;
                        float percentInset = inset / segment_lengths[groupedIndicies[j]];

                        IntPoint begVert = Lerp(previousPoint(groupedIndicies[j]), path[groupedIndicies[j]], 1 - percentInset);
                        subpath.Add(begVert);
                        subpath.Add(path[groupedIndicies[j]]);
                    }
                }
                else
                {
                    subpath.Add(path[groupedIndicies[j]]);
                }
            }

            /*
			Debug.Log("===========================");
			for(int j=0; j<groupedIndicies.Length; j++)
			{
				Debug.Log(groupedIndicies[j]);
				if (groupedIndicies[j] == -99999)
					break;
			}

			foreach(Path s in returnPaths)
			{
				Debug.Log("----");
				AXGeometry.Utilities.printPath(s);
			}
			*/


            return returnPaths;

        }



        public static VertexProperties getVertexProperties(Path path, int i)
        {

            int prev_i = (i == 0) ? path.Count - 1 : i - 1;
            int next_i = (i == path.Count - 1) ? 0 : i + 1;

            Vector2 pp = new Vector2(path[prev_i].X, path[prev_i].Y);
            Vector2 p = new Vector2(path[i].X, path[i].Y);
            Vector2 np = new Vector2(path[next_i].X, path[next_i].Y);

            Vector2 v1 = p - pp;
            Vector2 v2 = np - p;

            // NODE ROTATION & TRANSFORM
            Vector2 v1PN = (new Vector2(v1.y, -v1.x)).normalized;
            Vector2 v2PN = (new Vector2(v2.y, -v2.x)).normalized;

            // -- BISECTOR: the addition of the normalized perpendicular vectors leads to a bisector
            Vector2 bisector = v1PN + v2PN;

            //float tmp_ang = -Mathf.Atan2(bisector.y, bisector.x) * Mathf.Rad2Deg;
            //if (tmp_ang < 0)
            //    tmp_ang += 360;


            // BEVEL ANGLE
            float bevelAng = Vector2.SignedAngle(bisector, v2) - 90;

            float vertAngle = Vector2.SignedAngle(v1, v2);

            return new VertexProperties(bisector, bevelAng, vertAngle);

        }




        /// <summary>
        /// Tese to see if the Path has a convex angle.
        /// </summary>
        /// <returns><c>true</c>, if a concave angle is found, it returns false.
        /// If it makes it to the end with no concave angles, it returns fales.</returns>
        /// <param name="path">Path.</param>
        public static bool isConvex(Path path)
        {
            for (int i = 0; i < path.Count; i++)
            {
                VertexProperties vps = getVertexProperties(path, i);

                if (vps.bevelAngle < 0)
                    return false;
            }

            return true;
        }







        public static Paths difference(Paths paths, Paths cutters)
        {
             Clipper c = new Clipper(Clipper.ioPreserveCollinear);
           c.AddPaths(paths, PolyType.ptSubject, true);
            c.AddPaths(cutters, PolyType.ptClip, true);

            PolyTree polyTree = new AXClipperLib.PolyTree();
            c.Execute(ClipType.ctDifference, polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            return Clipper.PolyTreeToPaths(polyTree);
        }

        public static PolyTree differenceToPolyTree(Paths paths, Paths cutters)
        {
            Clipper c = new Clipper(Clipper.ioPreserveCollinear);
            c.AddPaths(paths, PolyType.ptSubject, true);
            c.AddPaths(cutters, PolyType.ptClip, true);

            PolyTree polyTree = new AXClipperLib.PolyTree();
            c.Execute(ClipType.ctDifference, polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            return polyTree;
        }

        public static Paths intersection(Path path, Path cutter)
        {
            if (path == null || path.Count == 0)
                return null;

            if (cutter == null || cutter.Count == 0)
            {
                Paths paths = new Paths();
                paths.Add(path);
                return paths;
            }

            Clipper c = new Clipper(Clipper.ioPreserveCollinear);
            c.AddPath(path, PolyType.ptSubject, true);
            c.AddPath(cutter, PolyType.ptClip, true);

            PolyTree polyTree = new AXClipperLib.PolyTree();
            c.Execute(ClipType.ctIntersection, polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            return Clipper.PolyTreeToPaths(polyTree);
        }
        public static Paths intersection(Paths paths, Paths cutters)
        {
            Clipper c = new Clipper(Clipper.ioPreserveCollinear);
            c.AddPaths(paths, PolyType.ptSubject, true);
            c.AddPaths(cutters, PolyType.ptClip, true);

            PolyTree polyTree = new AXClipperLib.PolyTree();
            c.Execute(ClipType.ctIntersection, polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            return Clipper.PolyTreeToPaths(polyTree);
        }

        public static Paths union(Path a, Path b)
        {
            Clipper c = new Clipper(Clipper.ioPreserveCollinear);
            c.AddPath(a, PolyType.ptSubject, true);
            c.AddPath(b, PolyType.ptSubject, true);


            PolyTree polyTree = new AXClipperLib.PolyTree();
            c.Execute(ClipType.ctUnion, polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            return Clipper.PolyTreeToPaths(polyTree);
        }
        public static Paths union(Paths paths)
        {
            Clipper c = new Clipper(Clipper.ioPreserveCollinear);
            c.AddPaths(paths, PolyType.ptSubject, true);


            PolyTree polyTree = new AXClipperLib.PolyTree();
            c.Execute(ClipType.ctUnion, polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            return Clipper.PolyTreeToPaths(polyTree);
        }


        /// <summary>
        /// Splits the into convex paths.
        /// If the Path has a concave angle, the bisector is extended to find an intersection
        /// with line sgements further along. 
        /// Once the intersection is found two paths are generated and returned.
        // This is run recursively.
        /// </summary>
        /// <returns>A set of convex paths.</returns>
        /// <param name="path">Path.</param>
        public static Paths splitIntoConvexPaths(Path path, int gener = 0, int part = 0)
        {
            Paths returnPaths = new Paths();

            if (gener > 6)
            {
                // TODO Debug.Log("HIT 6");
                returnPaths.Add(path);
                return returnPaths;
            }

            //			Debug.Log("["+gener+"]["+part+"] * * * * * * * * * * * * splitIntoConvexPaths * * * * * * * * * * * *");
            //			Pather.printPath(path);



            for (int i = 0; i < path.Count; i++)
            {

                VertexProperties vps = getVertexProperties(path, i);



                if (vps.bevelAngle < 0f && vps.bevelAngle > -80)
                {
                    //Debug.Log(vps.bevelAngle + " _ " + vps.vertAngle);
                    Vector2 nBisector = -vps.bisector;

                    //Debug.Log("CONCAVE FOUND at (" + i + "): " + path[i] +" this is concave! bisector=" + nBisector);

                    PointOnPathSegment pops = findSelfIntersection(path, i, nBisector);
                    //Debug.Log("Intersection Before: "+pops.index+", (" + pops.point.X+", "+pops.point.Y+")");

                    if (pops.index >= 0)
                    {
                        // split paths at this index

                        // A

                        Path a = new Path();

                        a.Add(path[i]);

                        if (pops.t < .2f)
                            a.Add(path[prevI(path, pops.index)]);
                        else if (pops.t < .9f)
                            a.Add(pops.point);

                        int j = pops.index;
                        while (j != i)
                        {
                            a.Add(path[j]);
                            j = nextI(path, j);
                        }


                        // B

                        Path b = new Path();

                        //Debug.Log (gener + ":"+part+ "; t="+pops.t + ",  index="+pops.index);

                        if (pops.t < .9f)
                        {
                            if (pops.t > .2f)
                                b.Add(pops.point);
                        }
                        else
                            b.Add(path[pops.index]);


                        b.Add(path[i]);

                        j = nextI(path, i);
                        while (j != pops.index)
                        {
                            b.Add(path[j]);
                            j = nextI(path, j);
                        }

                        // A sub-block must have more than 2 verts
                        if (a.Count > 2)
                        {
                            //Debug.Log(a.Count + " ... " + b.Count);

                            returnPaths.AddRange(splitIntoConvexPaths(a, gener + 1, 1));


                        }
                        if (b.Count > 2)
                        {
                            //Debug.Log(a.Count + " ... " + b.Count);


                            returnPaths.AddRange(splitIntoConvexPaths(b, gener + 1, 2));


                        }


                        //printPaths(returnPaths);
                    }
                    //					if (gener == 0)
                    //						printPaths(returnPaths);

                    return returnPaths;

                }
            }

            //Debug.Log("!!! ["+gener+"] No concave found..............");
            returnPaths.Add(path);

            return returnPaths;

        }
        public static Paths SplitPathAtX(Path p, float x = 0)
        {
            Paths paths = new Paths();



            Path left = new Path();
            Path right = new Path();


            int X = sup(x);

            bool isAboveX = (p[0].X > X) ? true : false;

            if (isAboveX)
                right.Add(p[0]);
            else
                left.Add(p[0]);




            for (int i = 1; i < p.Count; i++)
            {
                // check if this crossed axis. If so, then find intersetcion point
                if (isAboveX && p[i].X > X)
                {
                    right.Add(p[i]);
                }



            }
            //        if (p[i].X > X && !isAboveX)

            //        //    if (isAboveX)
            //        //{
            //        //    b.Add(p[i]);
            //        //    continue;
            //        //}

            //        //a.Add(p[i]);

            //        //Debug.Log(p[i].X + " -- " + X);
            //        //if (p[i].X > X)
            //        //{   // just crossed over
            //        //    // start b
            //        //    Debug.Log("crossed");
            //        //    isAboveX = true;
            //        //    b.Add(p[i]);
            //        //} else
            //    }

            //    paths.Add(a);
            //    paths.Add(b);

            //    return paths;
            //

            return paths;
        }


        public static Paths spltPathAtY(Path p, float y)
        {
            Paths paths = new Paths();

            Path a = new Path();
            Path b = new Path();


            int Y = sup(y);

            bool isAboveY = false;

            for (int i = 0; i < p.Count; i++)
            {

                if (isAboveY)
                {
                    b.Add(p[i]);
                    continue;
                }

                a.Add(p[i]);

                Debug.Log(p[i].Y + " -- " + Y);
                if (p[i].Y > Y)
                {   // just crossed over
                    // start b
                    Debug.Log("crossed");
                    isAboveY = true;
                    b.Add(p[i]);
                }
            }

            paths.Add(a);
            paths.Add(b);

            return paths;
        }


        public static PointOnPathSegment findSelfIntersection(Path path, int fromIndex, Vector2 normal)
        {
            // start at segment after next segment
            int prev_i = nextI(path, fromIndex);
            int i = nextI(path, prev_i);

            //Debug.Log(normal);

            while (i != fromIndex)
            {
                PointOnPathSegment pops = raySegmentIntersection(IP2Vector2(path[prev_i]), IP2Vector2(path[i]), IP2Vector2(path[fromIndex]), normal);
                //Debug.Log(" ---X " + inter);


                if (pops.point.X != -999999 && pops.point.Y != -999999)
                {
                    // We have found an intersection point!
                    pops.index = i;
                    return pops;

                }


                prev_i = i;
                i = nextI(path, i);
            }


            return new PointOnPathSegment(-1, new IntPoint());

        }




        //	This is looking for the intersection point between the line segments
        //	AB and CD. We are using the parametric equation A-bt = C - du, where
        //	b is the vector (B-A) and d is the vector (D-C). We want to solve for
        //	t and u - if they are both between 0 and 1, we have a valid
        //	intersection point.

        public static PointOnPathSegment raySegmentIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 n)
        {
            int rayLength = 10000000;

            Vector2 D = (C + (rayLength * n));
            Vector2 b = B - A;
            Vector2 d = D - C;
            Vector2 d_perp = new Vector2(-d.y, d.x);
            float denom = Vector2.Dot(d_perp, b);


            //if (denom == 0) { return new Vector2(-888888, -888888); } // parallel: no intersection possible


            Vector2 c = C - A;
            float numer = Vector2.Dot(d_perp, c);
            float t = numer / denom;


            if (0 <= t && t <= 1)
            {
                //Debug.Log("t = " + t);

                Vector2 b_perp = new Vector2(-b.y, b.x);

                numer = Vector2.Dot(b_perp, c);
                //numer = b_perp.x*c.x + b_perp.y*c.y;

                float u = numer / denom;
                if (0 <= u && u <= 1)
                {
                    // sements intersect!
                    //Debug.Log(" ===>  u = " + u);// + ", inbound = " + inbound);


                    return new PointOnPathSegment(t, Vector2IP(Vector2.Lerp(A, B, t)));
                }
            }
            // no intersection found
            return new PointOnPathSegment(-1, new IntPoint(-999999, -999999));
        }



        public static Path offset(Path path, float offset, AXClipperLib.JoinType _jt = AXClipperLib.JoinType.jtMiter)
        {

            // OFFSETTER
            ClipperOffset co = new ClipperOffset();

            
            co.AddPath(path, _jt, AXClipperLib.EndType.etClosedPolygon);


            AXClipperLib.PolyTree resPolytree = new AXClipperLib.PolyTree();
            co.Execute(ref resPolytree, (double)(offset * AXGeometry.Utilities.IntPointPrecision));



            return Clipper.ClosedPathsFromPolyTree(resPolytree)[0];

        }




        public static Paths offset(Paths paths, float offset, AXClipperLib.JoinType jt = AXClipperLib.JoinType.jtSquare)
        {
            // Set cleaning precision
            IntRect brect = Clipper.GetBounds(paths);
            int cleanPolygonPrecision = 2;
            if ((brect.right - brect.left) > 10000)
                cleanPolygonPrecision = 30;


            // Clean...
            //AXClipperLib.JoinType jt = AXClipperLib.JoinType.jtSquare;
            paths = AXGeometry.Utilities.cleanPaths(paths, cleanPolygonPrecision);



            Paths resPaths = new Paths();
            AXClipperLib.PolyTree resPolytree = null;


            // OFFSETTER
            ClipperOffset co = new ClipperOffset();
            co.MiterLimit = 2.0f;



            foreach (Path path in paths)
            {

                co.Clear();
                resPolytree = null;

                co.AddPath(path, jt, AXClipperLib.EndType.etClosedPolygon); //JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);

                // this resPolytree has transformed curves in it
                resPolytree = new AXClipperLib.PolyTree();
                co.Execute(ref resPolytree, (double)(offset * AXGeometry.Utilities.IntPointPrecision));
                resPaths.AddRange(Clipper.ClosedPathsFromPolyTree(resPolytree));
            }

            return resPaths;

        }



        public static PolyTree offsetToPolyTree(Paths paths, float offset, AXClipperLib.JoinType jt = AXClipperLib.JoinType.jtSquare)
        {
            // Set cleaning precision
            IntRect brect = Clipper.GetBounds(paths);
            int cleanPolygonPrecision = 2;
            if ((brect.right - brect.left) > 10000)
                cleanPolygonPrecision = 30;


            // Clean...
            //AXClipperLib.JoinType jt = AXClipperLib.JoinType.jtSquare;
            //paths = AXGeometry.Utilities.cleanPaths(paths, cleanPolygonPrecision);



            Paths resPaths = new Paths();
            AXClipperLib.PolyTree resPolytree = null;


            // OFFSETTER
            ClipperOffset co = new ClipperOffset();
            co.MiterLimit = 2.0f;


            co.Clear();
            resPolytree = null;

            foreach (Path path in paths)
            {
                co.AddPath(path, jt, AXClipperLib.EndType.etClosedPolygon); //JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);
            }

            resPolytree = new AXClipperLib.PolyTree();
            co.Execute(ref resPolytree, (double)(offset * AXGeometry.Utilities.IntPointPrecision));
            
            return resPolytree;

        }





        // UTILITIES
        public static long Float2IntPoint(float n)
        {
            return Mathf.FloorToInt(n * AXGeometry.Utilities.IntPointPrecision);
        }

        public static float IntPoint2Float(long n)
        {
            return (float)n / (float)AXGeometry.Utilities.IntPointPrecision;
        }

        public static Rect IntRect2Rect(IntRect ir)
        {
            return new Rect(
                ir.left / (float)AXGeometry.Utilities.IntPointPrecision,
                ir.bottom / (float)AXGeometry.Utilities.IntPointPrecision,
                (ir.right - ir.left) / (float)AXGeometry.Utilities.IntPointPrecision,
                (ir.top - ir.bottom) / (float)AXGeometry.Utilities.IntPointPrecision
                );
        }
        public static Vector2 IP2Vector2(IntPoint ip)
        {
            return new Vector2(ip.X, ip.Y);
        }

        public static IntPoint Vector2IP(Vector2 v)
        {
            return new IntPoint((int)v.x, (int)v.y);
        }



        public static Vector2 IP2Vector2WithPrecision(IntPoint ip)
        {
            return new Vector2((((float)ip.X) / AXGeometry.Utilities.IntPointPrecision), (((float)ip.Y) / AXGeometry.Utilities.IntPointPrecision));
        }
        public static IntPoint Vector2IPWithPrecision(Vector2 v)
        {
            return new IntPoint((int)(v.x * AXGeometry.Utilities.IntPointPrecision), (int)(v.y * AXGeometry.Utilities.IntPointPrecision));
        }


        public static Vector2[] path2Vector2Array(Path path)
        {
            if (path == null || path.Count == 0)
                return new Vector2[0];

            Vector2[] s = new Vector2[path.Count];

            for (int i = 0; i < path.Count; i++)
                s[i] = IP2Vector2WithPrecision(path[i]);

            return s;

        }



        public static Vector3[] path2Vector3Array(Path path, float newy = 0)
        {
            if (path == null || path.Count == 0)
                return new Vector3[0];

            Vector3[] s = new Vector3[path.Count];

            for (int i = 0; i < path.Count; i++)
            {
                Vector2 v2 = IP2Vector2WithPrecision(path[i]);
                s[i] = new Vector3(v2.x, newy, v2.y);
            }

            return s;
        }


        public static List<Vector3> path2Vector3List(Path path, float newy = 0)
        {
            if (path == null || path.Count == 0)
                return null;

            List<Vector3> s = new List<Vector3>();

            for (int i = 0; i < path.Count; i++)
            {
                Vector2 v2 = IP2Vector2WithPrecision(path[i]);
                s.Add(new Vector3(v2.x, newy, v2.y));
            }

            return s;
        }





        public static IntPoint Lerp(IntPoint a, IntPoint b, float p)
        {
            return new IntPoint(((b.X - a.X) * p + a.X), ((b.Y - a.Y) * p + a.Y));
        }


        public static bool Equals(IntPoint a, IntPoint b)
        {
            if (a.X == b.X && a.Y == b.Y)
                return true;

            return false;

        }
        public static long DistanceSquared(IntPoint a, IntPoint b)
        {
            if (a.X == b.X && a.Y == b.Y)
                return 0;

            long diffx = Math.Abs(b.X - a.X);
            long diffy = Math.Abs(b.Y - a.Y);

            return diffx * diffx + diffy * diffy;
        }



        public static long Distance(IntPoint a, IntPoint b)
        {
            if (a.X == b.X && a.Y == b.Y)
                return 0;

            return (long)Mathf.Sqrt((float)Mathf.Pow((b.X - a.X), 2) + (float)Mathf.Pow((b.Y - a.Y), 2));
        }

        public static int prevI(Path path, int i)
        {
            return (i == 0) ? path.Count - 1 : i - 1;
        }
        public static int nextI(Path path, int i)
        {
            return (i == path.Count - 1) ? 0 : i + 1;
        }


        // PATH_IS_RECTANGLE?
        public static bool pathIsRectangle(Path path)
        {
            if (path.Count < 4)
                return false;

            Paths paths = new Paths();
            paths.Add(path);
            IntRect b = Clipper.GetBounds(paths);

            // check if all the points are on the bounds rect
            // and fall within a tolerance of a bounds corner point.

            for (int i = 0; i < path.Count; i++)
            {

                IntPoint ip = path[i];

                if ((ip.X == b.left || ip.X == b.right) && (ip.Y == b.top || ip.Y == b.bottom))
                {
                    // this point passes test
                    continue;

                }
                else
                    return false;
            }

            return true;
        }


        public static void printPath(Path path)
        {
            //			for (int i = 0; i < path.Count; i++) {
            //				IntPoint ip = path [i];
            //				Debug.Log ("(" + i + ") " + ip.X + ", " + ip.Y);
            //			}
            Debug.Log(pathToString(path));
        }
        public static string pathToString(Path path)
        {
            string ret = "";
            for (int i = 0; i < path.Count; i++)
            {
                IntPoint ip = path[i];
                ret += "  [" + i + "] (" + ip.X + ", " + ip.Y + ")\r";
            }
            return ret;
        }
        public static void printPaths(Paths paths)
        {
            if (paths == null)
            {
                Debug.Log("print paths: EMPTY");
                return;
            }
            Debug.Log(paths.Count + " paths ------- ");
            int c = 0;
            foreach (Path p in paths)
                Debug.Log("[" + (c++) + "] " + pathToString(p));
            Debug.Log("end paths ------- ");

        }





        /***********************************
		 From a given vertex, get its previous vertex point.
		 If the given point is the first one, 
		 it will return  the last vertex;
		 ***********************************/
        public IntPoint previousPoint(int index)
        {

            if (path.Count > 0)
                return path[(((index - 1) < 0) ? (path.Count - 1) : index - 1)]; ;
            return new IntPoint();
        }
        public IntPoint nextPoint(int index)
        {
            if (path.Count > 0)
                return path[(index + 1) % path.Count];
            return new IntPoint();
        }

        public static IntRect getBounds(Path path)
        {
            Paths paths = new Paths();
            paths.Add(path);
            return AXClipperLib.Clipper.GetBounds(paths);

        }

        public static IntRect getBounds(Paths paths)
        {
           
            return AXClipperLib.Clipper.GetBounds(paths);

        }







        public static IntPoint getCenter(Path path)
		{
			IntRect b = getBounds(path);


			return new IntPoint(b.left + ((b.right - b.left) / 2), b.bottom + ((b.top - b.bottom) / 2));

		}











		public int[] getSegmentLengths()
        {
            segment_lengths = new int[path.Count];

            int segment_length = 0;

            for (int i = 0; i < path.Count; i++)
            {
                segment_length = (int)Distance(previousPoint(i), path[i]);
                segment_lengths[i] = segment_length;

                /*
                if (i > 0) 
                    curve_distance 	   += segment_length;

                curve_distances[i] 	= curve_distance;
                */
            }
            return segment_lengths;


        }






        // CLONE PATH (VECTOR_2 SHIFT)
        public static Path clone(Path path, Vector2 shift)
        {
            int shiftX = Mathf.FloorToInt(shift.x * AXGeometry.Utilities.IntPointPrecision);
            int shiftY = Mathf.FloorToInt(shift.y * AXGeometry.Utilities.IntPointPrecision);

            return clone(path, shiftX, shiftY);
        }

        // CLONE PATH (INT SHIFT)
        public static Path clone(Path path, int shiftX, int shiftY)
        {
            Path res = new Path();

            for (int i = 0; i < path.Count; i++)
                res.Add(new IntPoint(path[i].X + shiftX, path[i].Y + shiftY));

            return res;
        }

        // CLONE PATHS (VECTOR_2 SHIFT)
        public static Paths clone(Paths paths, Vector2 shift)
        {
            if (paths == null || paths.Count == 0)
                return null;

            Paths res = new Paths();

            int shiftX = Mathf.FloorToInt(shift.x * AXGeometry.Utilities.IntPointPrecision);
            int shiftY = Mathf.FloorToInt(shift.y * AXGeometry.Utilities.IntPointPrecision);

            for (int i = 0; i < paths.Count; i++)
                res.Add(clone(paths[i], shiftX, shiftY));

            return res;
        }





        // TRANSFORM_PATH

        public static void shiftPath(Path path, Vector2 s)
        {
            if (path == null)
                return;

            shiftPath(path, Mathf.FloorToInt(s.x * AXGeometry.Utilities.IntPointPrecision), Mathf.FloorToInt(s.y * AXGeometry.Utilities.IntPointPrecision));
        }

        public static void shiftPath(Path path, IntPoint ip)
        {
            if (path == null)
                return;

            shiftPath(path, ip.X, ip.Y);
        }

        // TRANSFORM_PATH
        public static void shiftPath(Path path, long shiftX, long shiftY)
        {
            if (path == null)
                return;

            for (int j = 0; j < path.Count; j++)
            {
                path[j] = new IntPoint(path[j].X + shiftX, path[j].Y + shiftY);
            }
        }

        // FLIP HORIZONTAL
        public static void flipHorizontal(Path path)
        {
            for (int j = 0; j < path.Count; j++)
            {
                path[j] = new IntPoint(-path[j].X, path[j].Y);
            }
            path.Reverse();
        }





        // JUSTIFICATION
        public static void justifyLeft(Path path)
        {
            IntRect b = getBounds(path);
            shiftPath(path, new IntPoint(0, -b.left));
        }
        public static void justifyRight(Path path)
        {
            IntRect b = getBounds(path);
            shiftPath(path, new IntPoint(0, -b.right));
        }
        public static void justifyBottom(Path path)
        {
            IntRect b = getBounds(path);
            shiftPath(path, new IntPoint(0, -b.bottom));
        }

        public static void justifyTop(Path path)
        {
            IntRect b = getBounds(path);
            shiftPath(path, new IntPoint(0, -b.top));
        }


        // ROTATIONS

        public static void rotate90CW(Path path)
        {
            for (int i = 0; i < path.Count; i++)
                path[i] = new IntPoint(-path[i].Y, path[i].X);
        }

        public static void rotate90CCW(Path path)
        {
            for (int i = 0; i < path.Count; i++)
                path[i] = new IntPoint(path[i].Y, -path[i].X);
        }

        public static void rotate180(Path path)
        {
            for (int i = 0; i < path.Count; i++)
                path[i] = new IntPoint(-path[i].X, -path[i].Y);
        }




        // TRANSFORM_PATHS (SHIFT VECTOR2)
        public static void shiftPaths(Paths paths, Vector2 shift)
        {
            if (paths == null)
                return;

            long shiftX = Mathf.FloorToInt(shift.x * AXGeometry.Utilities.IntPointPrecision);
            long shiftY = Mathf.FloorToInt(shift.y * AXGeometry.Utilities.IntPointPrecision);

            shiftPaths(paths, shiftX, shiftY);
        }

        // TRANSFORM_PATHS
        public static void shiftPaths(Paths paths, IntPoint ip)
        {
            if (paths == null)
                return;

            shiftPaths(paths, ip.X, ip.Y);
        }

        // TRANSFORM_PATHS
        public static void shiftPaths(Paths paths, long shiftX, long shiftY)
        {
            if (paths == null)
                return;

            for (int i = 0; i < paths.Count; i++)
                shiftPath(paths[i], shiftX, shiftY);
        }






        // TRANSFORM_POLY_TREE
        public static void shiftPolyTree(AXClipperLib.PolyTree polyTree, IntPoint ip)
        {
            if (polyTree == null)
                return;

            if (polyTree.Childs != null && polyTree.Childs.Count > 0)
                shiftPolyNode(polyTree.Childs, ip);
        }

        // TRANSFORM_POLY_NODE
        public static void shiftPolyNode(List<PolyNode> childs, IntPoint ip)
        {
            if (childs == null || childs.Count == 0)
                return;

            foreach (PolyNode child in childs)
            {
                //				Path tmpPath = shiftPath(child.Contour, ip);
                //				for (int i = 0; i < tmpPath.Count; i++) 
                //					child.Contour[i] = tmpPath [i];

                shiftPath(child.Contour, ip);

                if (child.Childs != null)
                    shiftPolyNode(child.Childs, ip);
            }

        }




        // TRANSFORM_POLY_TREE
        public static void subdividePolyTree(AXClipperLib.PolyTree polyTree, IntPoint ip)
        {
            if (polyTree == null)
                return;

            if (polyTree.Childs != null && polyTree.Childs.Count > 0)
                subdividePolyNode(polyTree.Childs, ip);
        }

        // TRANSFORM_POLY_NODE
        public static void subdividePolyNode(List<PolyNode> childs, IntPoint ip)
        {
            if (childs == null || childs.Count == 0)
                return;

            foreach (PolyNode child in childs)
            {
                //				Path tmpPath = shiftPath(child.Contour, ip);
                //				for (int i = 0; i < tmpPath.Count; i++) 
                //					child.Contour[i] = tmpPath [i];

                shiftPath(child.Contour, ip);

                if (child.Childs != null)
                    subdividePolyNode(child.Childs, ip);
            }

        }



        public static int sup(float num)
        {
            return (int)(num * AXGeometry.Utilities.IntPointPrecision);
        }




        public static Paths thicken(Paths paths, float t)
        {
            ClipperOffset co = new ClipperOffset();
            co.AddPaths(paths, JoinType.jtMiter, EndType.etOpenButt);

            AXClipperLib.PolyTree resPolytree = new AXClipperLib.PolyTree();
            co.Execute(ref resPolytree, (double)(t * AXGeometry.Utilities.IntPointPrecision));

            return Clipper.PolyTreeToPaths(resPolytree);

        }




        /// <summary>
        ///  Home-grown offsetter that can handle open paths (which clipper can't)
        ///  and returns paths for left and right. 
        /// </summary>
        /// <returns>The offsets.</returns>
        /// <param name="planSpline">Plan spline.</param>
        /// <param name="thickR">Thick r.</param>
        /// <param name="thickL">Thick l.</param>
        public static Paths wallOffsets(Spline planSpline, float thickR, float thickL)
        {
            /*
			 	When generating a PlanSweep, we need to know the breaking angles of each plan layer.
			 	This is because the original plan node might be convex at a certain section offset, it is concave, depending on how far out that section goes. 
			 	
			 	To do this:
			 	1. for each section node, do an offset using clipper - or use bevelAngles of the orginnal plan and make a new spline, whichever is more efficient
			 	2. store these offset plans as AX.Splines in a list.
			 	3. use these Splines for the isSharp and isBlend conditionals
			 
			 */

            if (planSpline == null || planSpline.controlVertices == null || planSpline.controlVertices.Count == 0)
                return null;


            //			if (tex == null)
            //				return null;




            Paths paths = new Paths();

            Path pathR = new Path();
            Path pathL = new Path();

            float samePointTolerence = .001f;


            int terminIndexSubtractor = 0;
            if (Vector2.Distance(planSpline.controlVertices[0], planSpline.controlVertices[planSpline.controlVertices.Count - 1]) < samePointTolerence)
                terminIndexSubtractor = 1;


            //Matrix4x4 prevBevelTransform;

            for (int i = 0; i < planSpline.controlVertices.Count - terminIndexSubtractor; i++)
            {

                Matrix4x4 bevelTransform = planSpline.nodeTransforms[i];

                if (planSpline.shapeState == ShapeState.Open)
                {
                    if (i == 0)
                        bevelTransform = planSpline.begTransform;
                    else if (i == planSpline.controlVertices.Count - 1)
                        bevelTransform = planSpline.endTransform;
                }


                // Transform plan vert
                Vector3 vertr = bevelTransform.MultiplyPoint(new Vector3(thickR, 0, 0));
                //Debug.Log(vertr);

                pathR.Add(Pather.Vector2IPWithPrecision(new Vector2(vertr.x, vertr.z)));

                Vector3 vertl = bevelTransform.MultiplyPoint(new Vector3(thickL, 0, 0));
                pathL.Add(Pather.Vector2IPWithPrecision(new Vector2(vertl.x, vertl.z)));


            }

            paths.Add(pathR);
            paths.Add(pathL);

            return paths;
        }





        public static Mesh getBoundingBox(Path path, float height = 1)
        {
            Mesh m = new Mesh();

            IntRect b = getBounds(path);

            float x1 = b.left;
            float x2 = b.right;
              
            float y1 = 0;
            float y2 = height;

            float z1 = b.bottom;
            float z2 = b.top;

            Vector3[] vertices = new Vector3[8];

            vertices[0] = new Vector3(x1, y1, z1);
            vertices[1] = new Vector3(x2, y1, z1);
            vertices[2] = new Vector3(x2, y1, z2);
            vertices[3] = new Vector3(x1, y1, z2);

            vertices[4] = new Vector3(x1, y2, z1);
            vertices[5] = new Vector3(x2, y2, z1);
            vertices[6] = new Vector3(x2, y2, z2);
            vertices[7] = new Vector3(x1, y2, z2);

            m.vertices = vertices;

            // we don't need uvs or triangles for the purposes of a bounds mesh.

            // But due to a bug in Unity v 2018, where CombineMeshes doesn't combine if there are no triangles in the mesh,
            // the workaround is to add a one dummy triangle
            int[] triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 4;
            triangles[4] = 5;
            triangles[5] = 6;
            m.triangles = triangles;


            return m;
        }




        public static Vector2[] GetMinMaxIntPointsAtY(Path path, float y)
        {

            // CHECK EACH SEGEMNET FOR ONE THAT CROSSES d
            // ADD POINT TO THAT SEGMENT at d
            Path intercepts = new Path();

            //Debug.Log("y=" + y);
            long Y = Float2IntPoint(y);

            List<long> xIntercepts = new List<long>();

            for (int i = 0; i < path.Count - 1; i++)
            {
                if (path[i].Y < Y && path[i + 1].Y > Y || path[i + 1].Y < Y && path[i].Y > Y)
                {
                    // record point

                    if (Mathf.Approximately(path[i].X, path[i + 1].X))
                    {
                        intercepts.Add(new IntPoint(path[i].X, Y));
                    }
                    else
                    {
                        float m = ((float)(path[i + 1].Y - path[i].Y)) / ((float)(path[i + 1].X - path[i].X));

                        long X = (long)((((float)(Y - path[i].Y)) / m) + path[i].X);

                        intercepts.Add(new IntPoint(X, Y));
                    }
                }

               
            }
            //Debug.Log("intercepts.Count="+intercepts.Count);

            return Pather.path2Vector2Array(intercepts);
            
           
        }


        public static Path YDivide(Path path, float y)
        {

            // CHECK EACH SEGEMNET FOR ONE THAT CROSSES d
            // ADD POINT TO THAT SEGMENT at d
            Path retPath = new Path();

            long Y = Float2IntPoint(y);

            retPath.Add(path[0]);

            for (int i=0; i<path.Count-1; i++)
            {
                if (path[i].Y < Y && path[i+1].Y > Y || path[i+1].Y < Y && path[i].Y > Y)
                {
                    // insert point

                    if (Mathf.Approximately(path[i].X, path[i + 1].X))
                    {
                        retPath.Add(new IntPoint(path[i].X, Y));
                    }
                    else
                    {
                        float m = ((float)(path[i + 1].Y - path[i].Y)) / ((float)(path[i + 1].X - path[i].X));

                        long  X = (long) ( (   ((float) (Y - path[i].Y)) / m   ) + path[i].X);

                        retPath.Add(new IntPoint(X, Y));
                    } 
                }

                retPath.Add(path[i+1]);
            }


            return retPath;
        }






        // DECIMATE
        public static Path decimatePath(Path path, float decimationAng)
        {
            Path decimatedPath = new Path();

            Vector2 B, C;

            decimatedPath.Add(path[0]);
            //deciatedPath.Add(path[1]);
            Vector2 pA = Pather.IP2Vector2(path[0]);
            Vector2 pB = Pather.IP2Vector2(path[1]);
            Vector2 pC = Pather.IP2Vector2(path[2]);

            int jA = 0;
            int jB = 1;
            int jC = 2;

            while (jC < path.Count - 1)
            {
                B = (pB - pA).normalized;
                C = (pC - pB).normalized;

                float ang = Vector2.SignedAngle(B, C);

                //Vector2 v = Pather.IP2Vector2WithPrecision(path[jA]) / 1000;

                if (Mathf.Abs(ang) < decimationAng)
                {
                    // colinear pt - simply skip....
                    //Debug.Log(jA+" NOT pt: " + v.x + ", " + v.y + " :: " + ang);
                }
                else
                {
                    // Debug.Log("Adding pt: " + v.x + ", " + v.y + " :: " + ang);
                    decimatedPath.Add(path[jB]);

                    jA = jB;
                    pA = pB;
                   
                }

                jB++;
                pB = pC;

                jC++;

                if (jC < path.Count)
                    pC = Pather.IP2Vector2(path[jC]);
            }

            decimatedPath.Add(path[jC - 1]);

            if (jC < path.Count)
                decimatedPath.Add(path[jC]);

            decimatedPath.Add(path[path.Count - 1]);

            return decimatedPath;
        }


        // DECIMATE
        public static Path decimatePathOld(Path path, float decimationAng)
        {
            Path decimatedPath = new Path();

            Vector2 B, C;

            decimatedPath.Add(path[0]);
            //deciatedPath.Add(path[1]);
            Vector2 pA = Pather.IP2Vector2(path[1]);
            Vector2 pB = Pather.IP2Vector2(path[2]);
            Vector2 pC = Pather.IP2Vector2(path[3]);

            int jA = 1;
            int jC = 3;

            while (jC < path.Count - 1)
            {
                B = (pB - pA).normalized;
                C = (pC - pB).normalized;

                float ang = Vector2.SignedAngle(B, C);

                //Vector2 v = Pather.IP2Vector2WithPrecision(path[jA]) / 1000;

                if (Mathf.Abs(ang) < decimationAng)
                {
                    // colinear pt - simply skip....
                    //Debug.Log(jA+" NOT pt: " + v.x + ", " + v.y + " :: " + ang);
                }
                else
                {
                   // Debug.Log("Adding pt: " + v.x + ", " + v.y + " :: " + ang);
                    decimatedPath.Add(path[jA]);
                    jA = jC - 1;
                    pA = pB;
                }
                pB = pC;

                jC++;
                jC++;

                if (jC < path.Count)
                    pC = Pather.IP2Vector2(path[jC]);
            }

            decimatedPath.Add(path[jC - 1]);

            if (jC < path.Count)
                decimatedPath.Add(path[jC]);

            decimatedPath.Add(path[path.Count-1]);

            return decimatedPath;
        }




        public static Vector2 DP2V2(CsPotrace.Potrace.DoublePoint P)
        {
            return new Vector2((float)P.X, (float)P.Y);
        }




        public static Paths TraceImageToPaths(Texture2D image, float alphaCutoff)
        {


            AXGeometry.Utilities.setRealtimePrecisionFactor(PrecisionLevel.Millimeter);


            AXGeometry.Utilities.IntPointPrecision = 1000;

            // PREPARE BITMAP
            bool[,] Result = new bool[image.width + 2, image.height + 2];




            // CREATE BITMAP bool[,] WHERE
            // TRUE IS EMPTY AND FALSE IS SOLID

            // FILL IN THE IMAGE SOLIDS BASED ON (ALPHA > CUTOFF) AS BLACK
            // AND (ALPHA < CUTOFF) IS WHITE
            for (int i = 0; i < image.width; i++)
            {
                for (int j = 0; j < image.height; j++)
                {
                    float a = image.GetPixel(i, j).a;
                    Result[i + 1, j + 1] = (a < alphaCutoff);
                }
            }

            // 
            // WHITE BORDER: Add a 1 pixel white border
            // to help with boundary conditions--make border "true".
            // Based on the algorithm in poTrace,
            // it is important that y starts at 1 instead of 0
            for (int x = 0; x < Result.GetLength(0); x++)
            {
                Result[x, 0] = true;
                Result[x, Result.GetLength(1) - 1] = true;
            }
            for (int y = 1; y < Result.GetLength(1) - 1; y++)
            {
                Result[0, y] = true;
                Result[Result.GetLength(0) - 1, y] = true;
            }





            // TRACE BITMAP INTP AN ArrayList
            ArrayList ListOfCurveArrays = new ArrayList();

            Potrace.potrace_trace(Result, ListOfCurveArrays);


            if (ListOfCurveArrays.Count == 0)
            {
                //Debug.Log("NO CURVE GENERATED FROM " + image.name);
                return null;
            }

            ArrayList FirstCurveArray = ListOfCurveArrays[0] as ArrayList;

           // Debug.Log("count: " + ListOfCurveArrays.Count);


            // Paths paths = ProcessCurveArray(FirstCurveArray);


            Paths paths = new Paths();

            foreach (ArrayList arrayList in ListOfCurveArrays)
            {
                Paths tmpPaths = ProcessCurveArray(arrayList);

                if (tmpPaths != null && tmpPaths.Count > 0)
                    paths.AddRange(tmpPaths);
            }


            return paths;
        }

        public static Paths ProcessCurveArray(ArrayList curveArray)
        { 
            // RETURN POLY_TREE
            Clipper c = new Clipper(Clipper.ioPreserveCollinear);


            // NOW "ListOfCurveArrays" HAS A SUBLIST FOR EACH CONTOUR + HOLES.
            // Normally, if only one object is in the image,
            // only one subarray will be generated
            //Debug.Log(" ================= A " + ListOfCurveArrays.Count);


            
           
           


            // PROCESS CURVE

            List<AXCurve> axCurves = new List<AXCurve>();

            AXCurvePoint curvePoint = null;


            if (curveArray.Count == 0)
                return null;

           
            // THE FIRST subCurve is the OUTER CONTOUR - the rest are HOLES


            // *** ====== EACH SUB_CURVE IN FIRST_CURVE_ARAY
            for (int j = 0; j < curveArray.Count; j++)
            {
                // MAP Potrace.Curves TO AX Curve

                Potrace.Curve[] poSubCurve = (Potrace.Curve[])curveArray[j];
                AXCurve axSubcurve = new AXCurve();

                Vector2 P;
                Vector2 A = Vector2.zero;
                Vector2 B = Vector2.zero;

                bool prevWasBezier = false;

                Vector2 nA = Vector2.zero;



                // *** ====== EACH POINT IN SUB_CURVE 
                for (int k = 0; k < poSubCurve.Length; k++)
                {
                    // CRAWL ALONG THE Potrace.IntPoints of the Potrace.Curve
                    //Debug.Log(k + ": " + Curves[k].Kind);

                    P = DP2V2(poSubCurve[k].A);



                    if (poSubCurve[k].Kind == Potrace.CurveKind.Bezier || prevWasBezier)
                    {
                        //new Vector2(sx * (float)poSubCurve[k].A.X, sy * (float)poSubCurve[k].A.Y);

                        // A
                        if (prevWasBezier)
                            A = nA;
                        else
                            A = P;


                        // B: THE CURVE TYPE DETERMINES WHAT THE NEXT POINT'S B WILL BE
                        if (poSubCurve[k].Kind == Potrace.CurveKind.Bezier)
                            B = DP2V2(poSubCurve[k].ControlPointA);
                        else
                            B = P;

                        curvePoint = new CurveControlPoint2D(P, A, B);
                        curvePoint.curvePointType = CurvePointType.BezierBroken;

                        // prepare for next
                        prevWasBezier = true;
                        nA = DP2V2(poSubCurve[k].ControlPointB);
                    }
                    else
                    {
                        curvePoint = new CurveControlPoint2D(P);
                        curvePoint.curvePointType = CurvePointType.Point;

                        prevWasBezier = false;
                    }

                    axSubcurve.Add(curvePoint);

                } // DONE CONVERTING SUBCURVE TO AX_CURVE


                // CONVERT CURVE TO PATH
                Path path = Curve2D.GetPathFromCurve(axSubcurve, ShapeState.Closed, 3);

                if (j==0) // CONTOUR
                    c.AddPath(path, PolyType.ptSubject, true);  
                
                else  // HOLE
                    c.AddPath(path, PolyType.ptClip, true);
                
            } // END CURVE GROUP


            //path = Pather.offset(path, 1f);
            PolyTree polyTree = new AXClipperLib.PolyTree();
            c.Execute(ClipType.ctDifference, polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);


            return Clipper.ClosedPathsFromPolyTree(polyTree);



        }







        








    } // /PATHER















    public struct VertexProperties
    {
        public Vector2 bisector;
        public float bevelAngle;
        public float vertAngle;

        public VertexProperties(Vector2 _b, float _a, float va = 0)
        {
            bisector = _b;
            bevelAngle = _a;
            vertAngle = va;
        }

    }
    public struct PointOnPathSegment
    {
        public int index;
        public float t;
        public IntPoint point;

        public PointOnPathSegment(float _t, IntPoint _p)
        {
            t = _t;
            point = _p;
            index = 0;

        }
        public PointOnPathSegment(int _i, float _t)
        {
            index = _i;
            t = _t;
            point = new IntPoint(-999999, -999999);
        }

        public PointOnPathSegment(int _i, float _t, IntPoint _p)
        {
            index = _i;
            t = _t;
            point = _p;
        }

















    }













} //\AX
