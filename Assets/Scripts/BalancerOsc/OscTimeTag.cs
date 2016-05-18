using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class OscTimeTag {
    public byte[] timeBytes;

    public OscTimeTag() {
        CalcTime();
    }

    public void CalcTime() {
        timeBytes = new byte[8];
        //UInt64 timetag = DateTimeToTimetag(DateTime.Now.ToUniversalTime());
        //byte[] reversedBytes =  BitConverter.GetBytes(timetag);
        //timeBytes = new byte[8];
        //for (int i = 0; i < reversedBytes.Length; i++) {
        //    timeBytes[7 - i] = reversedBytes[i];
        //}
    }

    // OSC uses NTP time
    // https://github.com/ValdemarOrn/SharpOSC/blob/master/SharpOSC/Utils.cs
    //public static UInt64 DateTimeToTimetag(DateTime value) {
    //    Debug.Log("seconds should be " + (value - DateTime.Parse("1900-01-01 00:00:00.000")).TotalSeconds);
    //    UInt64 seconds = (UInt32)(value - DateTime.Parse("1900-01-01 00:00:00.000")).TotalSeconds;
    //    Debug.Log("Seconds: " + seconds);
    //    UInt64 fraction = (UInt32)(0xFFFFFFFF * ((double)value.Millisecond / 1000));
    //    Debug.Log("fraction: " + fraction);

    //    UInt64 output = (seconds << 32) + fraction;
    //    return output;
    //}
}
