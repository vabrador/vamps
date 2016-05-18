using System.Collections.Generic;

public class OscData {
    public byte[] bytes;
    public uint numBytes;
    public OscData(byte[] bytes) {
        this.bytes = bytes;
        numBytes = (uint)bytes.Length;
    }

    public static OscData operator +(OscData data, byte[] moreData) {
        byte[] newBytes = new byte[data.bytes.Length + moreData.Length];
        for (int i = 0; i < newBytes.Length; i++) {
            if (i < data.bytes.Length) {
                newBytes[i] = data.bytes[i];
            }
            else {
                newBytes[i] = moreData[i - data.bytes.Length];
            }
        }
        return new OscData(newBytes);
    }

    public static byte[] GetBytes(List<OscData> data, out uint numBytes) {
        uint totalBytes = 0;
        for (int i = 0; i < data.Count; i++) {
            OscData datum = data[i];
            totalBytes += datum.numBytes;
        }
        numBytes = totalBytes;
        byte[] finalBytes = new byte[totalBytes];
        int cur = 0;
        foreach (OscData datum in data) {
            foreach (byte b in datum.bytes) {
                finalBytes[cur++] = b;
            }
        }
        return finalBytes;
    }
}