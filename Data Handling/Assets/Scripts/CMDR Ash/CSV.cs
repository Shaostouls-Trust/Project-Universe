using System;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
//using System.Drawing;
using System.Reflection;
using System.IO;
//fix ambiguity
using Vector3 = UnityEngine.Vector3;
using Color = UnityEngine.Color;

namespace DataHandling
{
    class CSV
    {
        //CSV vars
        public string delimiter = "~";
        public string newCellToken = "Tab";
        private string root = "..\\..\\Data";
        public int query;
        //general
        public string filename = "Test.csv";
        public List<Object> MyDataStack;
        //set custom csv settings
        public CsvHelper.Configuration.Configuration csvconfig = new CsvHelper.Configuration.Configuration
        {
            Delimiter = "~",
            HasHeaderRecord = false
        };
        //read vars
        public string generic_id1;
        public string iguid1;
        public string generic_id2;
        public string iguid2;
        //write vars
        public CSV(int queryType, string ifilename)
        {
            query = queryType;
            filename = ifilename;
            //path construction
            string b = "";
            string[] dirparts = Directory.GetCurrentDirectory().Split('\\');
            for (int a = 0; a < dirparts.Length; a++)
            {
                if (dirparts[a] == "DataHandling")
                {
                    //end reconstruction on the ..\\..\\bin root (at DataHandling root directory)
                    b += dirparts[a];
                    //go forward to the Data folder
                    b += "\\Data";
                    break;
                }
                else
                {
                    //reconstruct path
                    b += dirparts[a] + "\\";
                }
            }
            this.root = b;
            Console.WriteLine(">>>" + b + "<<<");
            //global csv config setup
            //var csvconfig = new CsvHelper.Configuration.Configuration
            //{
            //     Delimiter = "~",
            //    HasHeaderRecord = false
            //};
        }
        /*
         * Data loader: update object refs to be written.
         * In: objs[]
         * Out: None
         */
        public void DataLoaderStack(List<Object> dataStack)
        {
            this.MyDataStack = dataStack;
        }
        //Get size of MyDataStack
        //public int DataStackSize()
        //{
        //    return this.MyDataStack.Length;
        //}
        /*
         * Set search or write cell location by 2 locators
         */
        public void SetSingleTargetData(string GENERIC_ID, string iGUID)
        {
            generic_id1 = GENERIC_ID;
            iguid1 = iGUID;
        }

        /*
         * Set search or write cell range by 2 locators
         */
        public void SetRangeTargetData(string GENERIC_ID1, string iGUID1, string GENERIC_ID2, string iGUID2)
        {
            generic_id1 = GENERIC_ID1;
            iguid1 = iGUID1;
            generic_id2 = GENERIC_ID2;
            iguid2 = iGUID2;
        }
        /*
         * Change file path (root)
         */
        public void SetIOPathTo(string newpath)
        {
            root = newpath;
        }

        /*
         * Funct - Check the type of the queried I/O request.
         * In: queryType
         * Out: None
         */
        public void ProcessQuery()
        {
            string filepath = root + "\\" + this.filename;
            switch (this.query)
            {
                case 0:
                    this.ReadSingle(filepath, this.generic_id1, this.iguid1);
                    break;
                case 1:
                    this.ReadRange(filepath, this.generic_id1, this.iguid1, this.generic_id2, this.iguid2);
                    break;
                case 2:
                    this.ReadAll(filepath);
                    break;
                case 3:
                    this.WriteSingle(filepath, this.generic_id1, this.iguid1);
                    break;
                case 4:
                    this.WriteRange(filepath, this.generic_id1, this.iguid1, this.generic_id2, this.iguid2);
                    break;
                case 5:
                    this.WriteAll(filepath);
                    break;
                default:
                    Console.WriteLine("Query Unrecognized");
                    break;
            }
        }
        /*
        * Read - Find single target by GENERIC_ID, iGUID
        * In: file being read, tile locators
        * Out: none
        * 
        * PERHAPS "READ <TYPE> over READ SINGLE?
        */
        public void ReadSingle(string filepath, string GENERIC_ID, string iGUID)
        {

        }
        /*
        * Read - Find all in range and read to obejct buffer
        * In: file being read, tile start locators, tile end locators
        * Out: none
        */
        public void ReadRange(string filepath, string GENERIC_ID1, string GENERIC_ID2, string iGUID1, string iGUID2)
        {

        }
        /*
            * Read - Read all in file
            * In: file being read, tile locators
            * Out: none
            */
        public void ReadAll(string filepath)
        {
            Type type = null;
            Boolean valid = false;
            Helpers.HashBuilder hasher = new Helpers.HashBuilder();
            //instanciate csv reader
            using (var reader = new StreamReader(filepath))
            using (var csv = new CsvReader(reader, this.csvconfig))
            {
                //register the Tile Map that exists for the TileObjectBase base
                csv.Configuration.RegisterClassMap<Helpers.TileMap>();
                //while there are lines to read in the file, read them one-by-one
                while (csv.Read())
                {
                    //gen_id
                    Int32 genID = Int32.Parse(csv.GetField(0));
                    /*
                     * Process Data for instancing
                     */
                    //GEN_ID : Console.WriteLine(csv.GetField(0));
                    //iGUID : Console.WriteLine(csv.GetField(1));
                    //get coded position
                    string rawV3 = csv.GetField(2);
                    Vector3 pos = hasher.DecodeFromHex(rawV3);//position
                    rawV3 = csv.GetField(3);
                    Vector3 rot = hasher.DecodeFromHex(rawV3);//rotation
                    rawV3 = csv.GetField(4);
                    Vector3 scl = hasher.DecodeFromHex(rawV3);//scale
                    //Create colors from hexYhex data block
                    Color[] maps = hasher.HexToColorArray(csv.GetField(5));//color maps
                    //create emissive color(s)
                    Color[] emisArr = hasher.HexToColorArray(csv.GetField(6));
                    //grab first color. Later on when more emissive colors are present this will change.
                    Color emis = emisArr[0];
                    //get current animation frame (float)
                    float animFrame = hasher.FloatFromHexString(csv.GetField(7));//Animation Frame
                    string decal = hasher.DecalStringDecoder(csv.GetField(8));
                    Console.WriteLine(decal);
                    //case prefixing
                    //find type by generic id
                    valid = ID_Dict.GenIDs.TryGetValue(genID, out type);
                    //if the GENERIC_ID exists in the dictionary
                    if (valid)////
                    {
                        //reflexively call constructor
                        /*
                         * ALL TILE CONSTRUCTORS MUST HAVE SAME PARAMS (or additional params must have defaults)
                         * otherwise, those different constructors need to be handled differently, with some
                         * way of knowing that constructor is different.
                         */
                        try
                        {
                            //TileObjectBase Constructor Param Types
                            ConstructorInfo conInfo = type.GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Color[])
                            , typeof(Color), typeof(float), typeof(string) });
                            //TileObjectBase Params
                            //grab the rest of the data
                            object instance = conInfo.Invoke(new object[] { pos, rot, scl, maps, emis, animFrame, decal });
                            //then, create the appropriate object
                            instance.GetType().GetMethod("setIGUID").Invoke(instance, new object[] { Guid.Parse(csv.GetField(1)) }); //needs to be a Guid
                        }
                        catch (ArgumentException ae)
                        {
                            Console.WriteLine(ae);
                        }
                        catch (ReflectionTypeLoadException re)
                        {
                            Console.WriteLine(re);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Generic Expection:", e);
                        }
                    }
                    else
                    {
                        Console.WriteLine("GENERIC ID NOT FOUND IN DICTIONARY!");
                    }
                }


            }
            //Open csv and read in data line 1

        }
        /*
            * Write - Write one hash space
        * In: file being read, tile locators
        * Out: none
        */
        public void WriteSingle(string filepath, string GENERIC_ID, string iGUID)
        {

        }
        /* 
        * Write - Write over/in a range of hash spaces
        * In: file being read, tile start locators, tile end locators
        * Out: none
        */
        public void WriteRange(string filepath, string GENERIC_ID1, string GENERIC_ID2, string iGUID1, string iGUID2)
        {

        }
        /* 
        * Write - Write over whole file/write new file
        * In: file being read
        * Out: none
        */
        public void WriteAll(string filepath)
        {
            /*
            * DATA PREP
            */
            //Init control vars
            Helpers.HashBuilder hasher = new Helpers.HashBuilder();
            int size = 0;
            //int total = 0;
            Object objref;
            string V3hashString = "";
            //string dataString = "";
            //List<string> buffer = new List<string>();
            List<string> Singleresult = new List<string>();
            List<List<string>> WritingBuffer = new List<List<string>>();

            //instanciate new CSV StreamWriter
            using (var writer = new System.IO.StreamWriter(filepath))
            using (var csv = new CsvWriter(writer, this.csvconfig))
            {
            head:
                //Reset Control Vars
                size = 0;
                V3hashString = "";
                //dataString = "";
                Singleresult.Clear();
                WritingBuffer.Clear();
                do
                {
                    Singleresult.Clear();
                    //get next data set from stack
                    objref = this.MyDataStack[0];

                    //V3Hashing list
                    Vector3[] v3list = {(Vector3)objref.GetType().GetMethod("ObjectPos").Invoke(objref, null)
                            , (Vector3)objref.GetType().GetMethod("ObjectRot").Invoke(objref, null)
                            , (Vector3)objref.GetType().GetMethod("ObjectScl").Invoke(objref, null)};
                    //encode position rotation scale V3s
                    V3hashString = hasher.V3Hash(v3list);
                    //prefix identifiers
                    hasher.TypeHash(V3hashString, objref, ref Singleresult);
                    //add colormaps and emissive
                    hasher.CM_EM_Concat((Color[])objref.GetType().GetMethod("GetCmaps").Invoke(objref, null)
                        , (Color[])objref.GetType().GetMethod("GetEmaps").Invoke(objref, null), ref Singleresult);
                    //Encode remaining data (frame number and detail name string
                    hasher.EncodeToHex((float)objref.GetType().GetMethod("AnimFrame").Invoke(objref, null)
                        , (string)objref.GetType().GetMethod("Decor").Invoke(objref, null)
                        , ref Singleresult);
                    //Add EOF marker
                    Singleresult.Add("ENDOFFIELD");//used to ensure proper fieldwritting
                    //add to buffer list
                    WritingBuffer.Add(
                        //Fix overwrite/reference issue by seperating data/ref from Singleresult.
                        Singleresult.ToList<string>()
                        );
                    //buffer.Add(dataString);
                    //delete written index (it will be at 0)
                    this.MyDataStack.RemoveAt(0);
                    size++;
                } while (this.MyDataStack.Count != 0 && size < 15);
                /*
                 * DATA WRITING
                 */
                //register the Tile Map that exists for the TileObjectBase base
                csv.Configuration.RegisterClassMap<Helpers.TileMap>();
                foreach (List<string> a in WritingBuffer)
                {
                    foreach (string b in a)
                    {
                        if (b != "ENDOFFIELD")
                        {
                            //write data to file
                            csv.WriteField(b);
                            //Console.WriteLine("Written: " + b);
                        }
                        if (b == "ENDOFFIELD")
                        {
                            csv.NextRecord();
                        }
                    }
                }
                //clean out csvhelper writing buffer
                writer.Flush();
                //Auto close file on exiting 'Using{}'
                //if there are still more files to write: Goto write head
                if (this.MyDataStack.Count != 0)
                {
                    goto head;
                }
            }
        }
    }
}