using CustomExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BalancerOscDatagram {

    internal List<OscData> data = new List<OscData>();
    internal int dataLength = 0;
    public BalancerOscDatagram() { }
    public BalancerOscDatagram(byte[] initData) { Append(initData); }

    public void Append(byte[] value) {
        data.Add(new OscData(value));
    }
    public void Append(string str) { Append(FromString(str).bytes); }
    public void Append(List<float> floats, int numFloats) { Append(FromFloats(floats, numFloats).bytes); }

    public static OscData FromString(string str) {
        byte[] strBytes = Encoding.ASCII.GetBytes(str);
        int length = strBytes.Length;

        int toPad = 4 - (length % 4);
        byte[] padding = new byte[toPad];
        for (int i = 0; i < toPad; i++) { padding[i] = 0x00; }

        return new OscData(strBytes) + padding;
    }

    public static OscData FromFloats(List<float> values, int numValues) {
        byte[] allFloatBytes = new byte[numValues * 4];
        for (int float_idx = 0; float_idx < numValues; float_idx++) {
            byte[] floatBytes = BitConverter.GetBytes(values[float_idx]);
            allFloatBytes[float_idx*4 + 0] = floatBytes[3];
            allFloatBytes[float_idx*4 + 1] = floatBytes[2];
            allFloatBytes[float_idx*4 + 2] = floatBytes[1];
            allFloatBytes[float_idx*4 + 3] = floatBytes[0];
        }
        return new OscData(allFloatBytes);
    }

public static OscData FromInt32(int value) {
        byte[] intb = BitConverter.GetBytes(value);
        byte[] revb = new byte[4];
        revb[0] = intb[3];
        revb[1] = intb[2];
        revb[2] = intb[1];
        revb[3] = intb[0];
        return new OscData(revb);
    }
}
