using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
//using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
//fix ambiguity
using Vector3 = UnityEngine.Vector3;
using Color = UnityEngine.Color;//System.Drawing.Color

namespace DataHandling
{
    //Development object built to include all metadata used or otherwise present with unity GameObjects
    public class TileObjectBase : MonoBehaviour
    {
        //metadata fields
        //relational
        //public string name = "";
        //Physical
        //private fields
        private Vector3 position;
        private Vector3 rotation;
        private Vector3 scale;
        //Texture
        public Color mapPrimary;
        public Color mapSecondary;
        public Color mapTertiary;
        public Color mapQuartiary;
        public Color mapDetailPrimary;
        public Color mapDetailSecondary;
        //Transitive
        public Color emissiveColor;
        private float animFrame;
        public string decorationString = "";
        //private field for iGUID, the unique ID for each instance of an object
        private Guid iGUID;
        public readonly static Int32 GENERIC_ID = 0x00000001;//SHOULD BE OBTAINED FROM DATABASE (or dictionary)
        //mapping variables
        public readonly Int32 GENREF = GENERIC_ID;
        public readonly string iRefGuid;

        // This will be overwritten
        protected virtual void Start()
        {

        }

        // Update is called once per frame. Also Overwritten
        protected virtual void Update()
        {

        }

        //Construct new XYZ obj with default values
        public TileObjectBase()
        {
            //Fill in data with default values
            this.position = new Vector3(0, 0, 0);
            this.rotation = new Vector3(0, 0, 0);
            this.scale = new Vector3(1, 1, 1);
            System.Drawing.Color blankColor = System.Drawing.Color.FromArgb(0x00000000);
            Color unityBlankColor = new Color(blankColor.R, blankColor.G, blankColor.B, blankColor.A);
            this.mapPrimary = unityBlankColor;//set to black
            this.mapSecondary = unityBlankColor;
            this.mapTertiary = unityBlankColor;
            this.mapQuartiary = unityBlankColor;
            this.mapDetailPrimary = unityBlankColor;
            this.mapDetailSecondary = unityBlankColor;
            this.emissiveColor = unityBlankColor;
            this.animFrame = 0;
            this.decorationString = "";
            //GUID assignment
            this.iGUID = Guid.NewGuid();
            this.iRefGuid = this.iGUID.ToString();
        }

        //paramaterized XYZObject
        public TileObjectBase(Vector3 position, Vector3 rotation, Vector3 scale, Color[] mapColors, Color emissiveColor, float animFrame = 0, string decal = "")
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            //catch the most likely exception
            try
            {
                this.mapPrimary = mapColors[0];
                this.mapSecondary = mapColors[1];
                this.mapTertiary = mapColors[2];
                this.mapQuartiary = mapColors[3];
                this.mapDetailPrimary = mapColors[4];
                this.mapDetailSecondary = mapColors[5];
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine(ex);
                System.Drawing.Color errorColor = System.Drawing.Color.FromArgb(0x00000000);
                Color uErrorColor = new Color(errorColor.R, errorColor.G, errorColor.B, errorColor.A);
                this.mapPrimary = uErrorColor;//set to black
                this.mapSecondary = uErrorColor;
                this.mapTertiary = uErrorColor;
                this.mapQuartiary = uErrorColor;
                this.mapDetailPrimary = uErrorColor;
                this.mapDetailSecondary = uErrorColor;
            }
            this.emissiveColor = emissiveColor;
            this.animFrame = animFrame;
            this.decorationString = decal;
            //GUID assignment
            this.iGUID = Guid.NewGuid();
            this.iRefGuid = this.iGUID.ToString();
        }

        public Vector3 ObjectPosition
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 ObjectPos()
        {
            return this.position;
        }

        public Vector3 ObjectRotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Vector3 ObjectRot()
        {
            return this.rotation;
        }

        public Vector3 ObjectScale
        {
            get { return scale; }
            set { scale = value; }
        }
        public Vector3 ObjectScl()
        {
            return this.scale;
        }

        public float AnimationFrame
        {
            get { return animFrame; }
            set { animFrame = value; }
        }

        public float AnimFrame()
        {
            return this.animFrame;
        }

        public string GetiGUID_string()
        {
            return iGUID.ToString();
        }
        
        /*
         * Set iGUID for object. Only called on read-instantiation.
         */
        public void setIGUID(Guid value)
        {
            this.iGUID = value;
        }
        public Color[] GetCmaps()
        {
            Color[] colors = {this.mapPrimary,
                this.mapSecondary,
                this.mapTertiary,
                this.mapQuartiary,
                this.mapDetailPrimary,
                this.mapDetailSecondary
            };
            return colors;
        }
        public Color[] GetEmaps()
        {
            Color[] colors =
            {
                this.emissiveColor
            };
            return colors;
        }
        public string Decoration
        {
            get { return this.decorationString; }
            set { this.decorationString = value; }
        }

        public string Decor()
        {
            return this.decorationString;
        }
    }
}