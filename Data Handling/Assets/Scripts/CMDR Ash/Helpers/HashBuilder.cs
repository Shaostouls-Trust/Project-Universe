using System;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
//using System.Drawing;
//fix ambiguity
using Vector3 = UnityEngine.Vector3;
using Color = UnityEngine.Color;

namespace DataHandling.Helpers
{
    class HashBuilder
    {

        public HashBuilder()
        {

        }

        /*
         * Convert a float to a hex string. Used for encoding position, rotation, scale.
         */
        public string FloatToHexString(float f)
        {
            var bytes = BitConverter.GetBytes(f);
            int num = BitConverter.ToInt32(bytes, 0);
            return "0x" + num.ToString("X8");
        }

        /*
         * Convert a hex string to a float. Used for deencoding position, rotation, scale.
         */
        public float FloatFromHexString(string s)
        {
            int num = Convert.ToInt32(s, 16);
            var bytes = BitConverter.GetBytes(num);
            return BitConverter.ToSingle(bytes, 0);
        }

        /*
         * Write - Prep Vector3s for writing
         * In: 3 V3 objects in one Vector3[]
         * Out: string of delimited hexes.
         */
        public string V3Hash(Vector3[] posRotScl)
        {
            string xHex;
            string yHex;
            string zHex;
            string compileString = "";
            for (int i = 0; i < posRotScl.Length; i++)
            {
                //grab x,y,z and convert to hex
                xHex = this.FloatToHexString(posRotScl[i].x);
                yHex = this.FloatToHexString(posRotScl[i].y);
                zHex = this.FloatToHexString(posRotScl[i].z);
                //combine hexes with char 'Z'. Will be split apart later
                compileString += xHex + "Z" + yHex + "Z" + zHex + "~";
            }
            return compileString;
        }

        /*
         * Write - Add Identifier
         * In: V3 hash from HashBuilder.V3Hash(), ref to obj being written.
         * Out: Prefixed V3 hash
        */
        public void TypeHash(string V3HashString, System.Object obj, ref List<string> dataList)
        {
            Int32 value = 0;
            Boolean valid = false;
            //try to get value from dictionary for type (key)
            //value is the GENERIC_ID
            valid = ID_Dict.TypeIDs.TryGetValue(obj.GetType(), out value);
            if (valid)
            {
                //Console.WriteLine(value);
                //Prefix GENERIC_ID
                dataList.Add(value.ToString());
                //case search for casting to class
                switch (value)
                {
                    //use reflection to get the unique GUID and append to final string
                    case (Int32)0x00000001: dataList.Add((string)obj.GetType().GetMethod("GetiGUID_string").Invoke(obj, null)); break;//+"~"

                    default: dataList.Add("0x0"); break;
                }
                //postfix V3HashString
                //split on ~
                string[] strings = V3HashString.Split('~');
                dataList.Add(strings[0]);
                dataList.Add(strings[1]);
                dataList.Add(strings[2]);
            }
            else
            {
                dataList.Add("Value Not Found");
            }
        }

        /*
         * Write - Add in colormap hexes
         * In: string[] of colormap hexes and string[] emissive states
         * out: Concat string
         */
        public void CM_EM_Concat(string[] maps, string[] emis, ref List<string> dataList)
        {
            string concat = "";
            foreach (string s in maps)
            {
                concat += s + "Y";
            }
            dataList.Add(concat);
            concat = "";
            foreach (string ss in emis)
            {
                concat += ss + "X";
            }
            dataList.Add(concat);
        }

        /*
         * Write - Add in colormap hexes
         * In: Color[] of colormap hexes and Color[] emissive states
         * out: Concat string
         */
        public void CM_EM_Concat(Color[] maps, Color[] emis, ref List<string> dataList)
        {
            string[] Cmaps = new string[maps.Length];
            string[] Emaps = new string[emis.Length];
            //convert to System.Drawing.Color from UnityEngine 
            //for (int cm = 0; cm < Cmaps.Length; cm++)
            //{
            //    Color unityBlankColor = new Color(maps[cm]., blankColor.G, blankColor.B, blankColor.A);
            //}

            for (int i = 0; i < Cmaps.Length; i++)
            {
                Cmaps[i] = "0x" + Convert.ToString(maps[i].ToString("X8"));//Convert.ToString(maps[i].ToArgb().ToString("X8")
            }
            for (int j = 0; j < Emaps.Length; j++)
            {
                Emaps[j] = "0x" + Convert.ToString(emis[j].ToString("X8"));

            }
            string concat = "";
            foreach (string s in Cmaps)
            {
                concat += s + "Y";
            }
            dataList.Add(concat);
            concat = "";
            foreach (string ss in Emaps)
            {
                concat += ss + "X";
            }
            dataList.Add(concat);
        }

        /*
         * Write - Encode all remaining data to Hex
         * In: int and string args from write-to pile. Might tkae in list if need arises.
         * exactly defined params. Normal Tiles will only have 1 int and 1 string.
         * Out: Delimited hex string
         */
        public void EncodeToHex(float floatargs, string stringargs, ref List<string> dataList)
        {
            //convert Frame float to hex
            dataList.Add(this.FloatToHexString(floatargs));
            byte[] bytes = { };
            //turn string into hex
            bytes = Encoding.Default.GetBytes(stringargs);
            //turn hex into a string format
            dataList.Add(BitConverter.ToString(bytes));
        }
        /*
        * Read - Decode From Hex
        * In: Joined XYZ float data in hex format
        * Out: V3 for position, rotation, or scale.
        */
        public Vector3 DecodeFromHex(string hex)
        {
            string[] parts = hex.Split('Z');
            //convert to float
            float x = FloatFromHexString(parts[0]);
            float y = FloatFromHexString(parts[0]);
            float z = FloatFromHexString(parts[0]);
            //return as Vector3
            return new Vector3(x, y, z);
        }
        /*
         * Read - Decode Colors from hex data and return the Colors[] array used in constructors
         * In: Hex string
         * Out: Color[] array
         */
        public Color[] HexToColorArray(string hexGroup)
        {
            string[] parts = hexGroup.Split('Y');
            Int32 num = 0;
            Color[] colors = new Color[6];
            Color c;
            //convert to Hex color
            //loop through hex colors
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] != "")
                {
                    //convert the string to an Int32
                    int.TryParse(parts[i].Substring(2), out num);
                    //get the color represented by the Int32
                    System.Drawing.Color intcolor = System.Drawing.Color.FromArgb(num);
                    c = new Color(intcolor.R, intcolor.G, intcolor.B, intcolor.A);
                    //c = Color.FromArgb(num);
                    //add it to the Color[] array
                    colors[i] = c;
                }
                else
                {
                    continue;
                }
            }
            return colors;
        }
        /*
         * Read - Convert a hex-hex-hex data string into a string
         * In: '-' delimed hex string
         * Out: string
         */
        public string DecalStringDecoder(string hex)
        {
            //Remove '-' breaks to get joined hex representation of string
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                //convert each two-count block to bytes and add to raw. Each character is now in byte form
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return Encoding.ASCII.GetString(raw);
        }
    }
}
