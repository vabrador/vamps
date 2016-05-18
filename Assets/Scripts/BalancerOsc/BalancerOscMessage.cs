using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BalancerOscMessage {
    public BalancerOscDatagram datagram = null;
    public BalancerOscDatagram Datagram {
        get {
            if (datagram != null) return datagram;
            datagram = new BalancerOscDatagram(); return datagram;
        }
        set { datagram = value; }
    }

    public BalancerOscMessage(string address) { Datagram.Append(address); }

    public void Add(List<float> values) {
        Datagram.Append(GetTypeTag(values));
        Datagram.Append(BalancerOscDatagram.FromFloats(values, values.Count).bytes);
    }
    
    public void Add(int value) {
        Datagram.Append("i");
        Datagram.Append(BalancerOscDatagram.FromInt32(value).bytes);
    }

    public string GetTypeTag(List<float> values) {
        StringBuilder sb = new StringBuilder();
        sb.Append(",");
        for (int i = 0; i < values.Count; i++) {
            sb.Append("f");
        }
        return sb.ToString();
    }
}