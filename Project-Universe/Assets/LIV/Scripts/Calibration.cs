using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace LIV.SDK.Unity
{
    public static class Calibration
    {
        const string ConfigFileName = "externalcamera.cfg";

        public static event EventHandler Changed;


        public static float X;
        public static float Y;
        public static float Z;

        public static float Yaw;
        public static float Pitch;
        public static float Roll;

        public static float FieldOfVision;
        public static float NearClip;
        public static float FarClip;
        public static float HMDOffset;
        public static float NearOffset;


        public static Vector3 PositionOffset
        {
            get { return new Vector3(X, Y, Z); }
        }

        public static Quaternion RotationOffset
        {
            get { return Quaternion.Euler(Pitch, Yaw, Roll); }
        }


        static readonly FileSystemWatcher ConfigWatcher;

        static Calibration()
        {
#if !UNITY_METRO
            try
            {
                var configFileInfo = new FileInfo(ConfigFileName);

                ConfigWatcher = new FileSystemWatcher(configFileInfo.DirectoryName ?? "", configFileInfo.Name) { NotifyFilter = NotifyFilters.LastWrite };
                ConfigWatcher.Changed += (o, e) => Read();
                ConfigWatcher.EnableRaisingEvents = true;
            }
            catch
            {
                // File doesn't exist, most likely. We Read() anyway.
            }

            Read();
#endif
        }

        static void Reset()
        {
            X = Y = Z = 0;
            Pitch = Yaw = Roll = 0;

            FieldOfVision = 60;
            NearClip = 0.01f;
            FarClip = 1000f;

            HMDOffset = 0;
            NearOffset = 0;
        }

        public static void Read()
        {
            Reset();

            var configLines = new string[0];

            try
            {
                configLines = File.ReadAllLines(ConfigFileName);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("LIV: Failed to read camera calibration from disk. Error: {0}", e);
            }

            foreach (var line in configLines)
            {
                var configLine = line.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                if (configLine.Length != 2) continue;

                switch (configLine[0].ToLowerInvariant())
                {
                    case "x": TryParseInvariantFloat(configLine[1], out X); break;
                    case "y": TryParseInvariantFloat(configLine[1], out Y); break;
                    case "z": TryParseInvariantFloat(configLine[1], out Z); break;

                    case "rx": TryParseInvariantFloat(configLine[1], out Pitch); break;
                    case "ry": TryParseInvariantFloat(configLine[1], out Yaw); break;
                    case "rz": TryParseInvariantFloat(configLine[1], out Roll); break;

                    case "fov": TryParseInvariantFloat(configLine[1], out FieldOfVision); break;
                    case "near": TryParseInvariantFloat(configLine[1], out NearClip); break;
                    case "far": TryParseInvariantFloat(configLine[1], out FarClip); break;
                    case "hmdoffset": TryParseInvariantFloat(configLine[1], out HMDOffset); break;
                    case "nearoffset": TryParseInvariantFloat(configLine[1], out NearOffset); break;
                }
            }

            if (Changed != null)
                Changed(null, EventArgs.Empty);
        }

        /// <summary>
        /// Tries to parse a float using invariant culture.
        /// </summary>
        /// <param name="number">The string containing the float to parse.</param>
        /// <param name="result">The parsed float, if successful.</param>
        /// <returns>True on success, false on failure.</returns>
        static bool TryParseInvariantFloat(string number, out float result)
        {
            return float.TryParse(
                number,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out result
            );
        }
    }
}
