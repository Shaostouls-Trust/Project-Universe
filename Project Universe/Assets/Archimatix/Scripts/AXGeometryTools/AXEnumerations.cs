using System;

namespace AXGeometry
{
	public enum ShapeState {Closed, Open};
	public enum GeneratorType {Shape, MesherGenerators, Repeaters};
	public enum ColliderType {None, Box, Sphere, Capsule, Mesh, ConvexMesh};
	public enum RepeaterItem {Node, Cell, SpanU, SpanV, Corner};
	public enum Axis {NONE, X, Y, Z, NX, NY, NZ};
	public enum ThumbnailState {Open, Closed, Custom};

	public enum CurvePointType {Point, BezierMirrored, BezierUnified, BezierBroken, Smooth };

	public enum LineType{Line, Rail, Opening};

	public enum PrecisionLevel {Millimeter, Meter, Kilometer};

    //// Bitmask
    //enum AXBlockSides
    //{
    //    Left    = (1 << 0),
    //    Right   = (1 << 1),
    //    Up      = (1 << 2),
    //    Down    = (1 << 3),
    //    Front   = (1 << 4),
    //    Back    = (1 << 5)
    //}

    public struct AXBlockSides
    {
        //Variable declaration
        //Note: I'm explicitly declaring them as public, but they are public by default. You can use private if you choose.
        public bool Left;
        public bool Right;
        public bool Top;
        public bool Bottom;
        public bool Front;
        public bool Back;


        //Constructor (not necessary, but helpful)
        public AXBlockSides(bool front, bool back, bool left, bool right, bool top, bool bottom)
        {
            this.Front = front;
            this.Back = back;
            this.Left = left;
            this.Right = right;
            this.Top = top;
            this.Bottom = bottom;

        }
    }



}

