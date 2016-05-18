using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using CustomExtensions;
using UnityEngine;

public class BalancerOscClient {
    private UdpClient udpClient;
    public BalancerOscClient(string ip, int port) {
        udpClient = new UdpClient(ip, port);
    }

    public void Send(BalancerOscMessage message) {
        uint numBytes = 0;
        byte[] bytes = OscData.GetBytes(message.datagram.data, out numBytes);
        udpClient.Send(bytes, (int)numBytes);
    }

    public void Send(BalancerOscBundle bundle) {
        int numBytes = 0;
        byte[] bytes = bundle.GetBytes(out numBytes);
        udpClient.Send(bytes, numBytes);
    }
    
}