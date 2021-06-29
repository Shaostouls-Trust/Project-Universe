using System;
using System.Collections.Generic;

namespace ProjectUniverse.Audio
{
    public static class SoundLibA
    {
        public static Dictionary<string, string[]> soundParameters = new Dictionary<string, string[]>
    {
        {"WarpAlarm1",new string[]{ "Priority", "1.0f","0.25","10","50","45","78"} },
        {"AvoidShipAlarm1",new string[]{ "Priority", "1.0f", "0.25","100","500","180","75"} },
        {"BiohazardAlarm1",new string[]{ "Priority", "1.0f", "0.25","25","75","45","76"} },
        {"DistressSiren1",new string[]{ "Priority", "1.0f", "0.25","100","500","180","77"} },
        {"RedAlert",new string[]{"Hostiles", "1.0f", "0.5","10","50","15","81"} },
        {"SecurityAlert1",new string[]{ "Hostiles", "1.0f", "0.25","10","50","15","81"} },
        {"IncommingAlarm1",new string[]{ "Hostiles", "1.0f", "0.25","25","75","45","81"} },
        {"Intruder1",new string[]{ "Hostiles", "1.0f", "0.25","10","50","15","81"} },
        {"Intruder2",new string[]{ "Hostiles", "1.0f", "0.25","10","50","15","81"} },
        {"NuclearWarning1",new string[]{ "Nuclear", "1.0f", "0.25","25","75","25","80"} },
        {"NuclearLeak1",new string[]{ "Nuclear", "1.0f", "0.25","25","75","25","79"} },
        {"NuclearLeak2",new string[]{ "Nuclear", "1.0f", "0.25","25","75","25","79"} },
        {"A1WarningSlow",new string[]{ "Generic", "1.0f", "0.5","5","30","10","84"} },
        {"A1WarningFast",new string[]{ "Generic", "1.0f", "0.5","5","30","10","84"} },
        {"ClosureWarning",new string[]{ "Generic", "1.0f", "0.5","5","30","10","86"} },
        {"RadarSound1",new string[]{ "Generic", "1.0f", "0.25","25","50","90","83"} },
        {"SonarPing",new string[]{ "Generic", "1.0f", "0.25","50","100","180","83"} },
        {"FactoryBuzzer",new string[]{ "Generic", "1.0f", "0.25","5","50","10","86"} },
        {"FireAlarm1",new string[]{ "Generic", "1.0f", "0.5","20","60","10","82"} },
        {"ModernKlaxAlarm1",new string[]{ "Generic", "1.0f", "0.5","15","50","20","83"} },
        {"SystemAlarmB1",new string[]{ "Errors", "1.0f", "0.25","5","30","15","84"} },
        {"CommsError1",new string[]{ "Errors", "1.0f", "0.25","5","30","20","83"} },
        {"SystemError1",new string[]{ "Errors", "1.0f", "0.25","5","30","20","84"} },
        {"Default",new string[]{ "Generic", "1.0f", "0.5","10","50","45","250"} }
    };


    }
}