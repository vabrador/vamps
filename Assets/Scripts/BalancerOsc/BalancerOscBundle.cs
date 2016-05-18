using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BalancerOscBundle {

    OscTimeTag timeTag;
    List<BalancerOscMessage> messages = new List<BalancerOscMessage>();

    public BalancerOscBundle() {
        timeTag = new OscTimeTag();
    }

    public void Add(BalancerOscMessage message) {
        messages.Add(message);
    }

    public OscTimeTag GetOscTimeTag() {
        return timeTag;
    }

    public byte[] GetBytes(out int numBytes) {
        // OSC-string "#bundle"
        OscData bundleIDData = BalancerOscDatagram.FromString("#bundle");
        byte[] bundleID = bundleIDData.bytes;
        int bundleIDLen = (int)bundleIDData.numBytes;
        // OSC time tag
        byte[] bundleTimeTag = timeTag.timeBytes;
        int timeTagLen = 8;
        // Messages (prefixed by their length in bytes)
        List<byte[]> msgs = new List<byte[]>();
        List<UInt32> msgLens = new List<UInt32>();
        int totalMsgsLen = 0;
        for (int i = 0; i < messages.Count; i++) {
            UInt32 msgLen = 0;
            byte[] msgData = OscData.GetBytes(messages[i].datagram.data, out msgLen);
            msgs.Add(msgData);
            msgLens.Add(msgLen);
            totalMsgsLen += (int)msgLen + 4; // extra 4 bytes for the int32 prefix for message length in bytes
        }
        // Construct output
        numBytes = bundleIDLen + timeTagLen + totalMsgsLen;
        byte[] data = new byte[numBytes];
        int idx = 0;
        for (int j = 0; j < bundleIDLen; j++) {
            data[idx++] = bundleID[j];
        }
        for (int j = 0; j < 8; j++) {
            data[idx++] = bundleTimeTag[j];
        }
        for (int j = 0; j < msgs.Count; j++) {
            byte[] lenBytes = BitConverter.GetBytes(msgLens[j]);
            for (int k = 0; k < 4; k++) {
                data[idx++] = lenBytes[k];
            }
            for (int k = 0; k < msgLens[j]; k++) {
                data[idx++] = msgs[j][k];
            }
        }
        return data;
    }
}