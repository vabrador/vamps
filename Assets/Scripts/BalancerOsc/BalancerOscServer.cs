using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

class BalancerOscServer : MonoBehaviour {

    Thread receiveThread;
    UdpState udpState;

    [Header("Readonly")]
    public string host = "127.0.0.1";
    public int port = 1237;

    public delegate void callback(List<float> data);
    public callback function;

    private void Start() {  } // must have blank Start() for AddComponent<BalancerOscServer>()
    public void Start(string host, int port, callback function) {
        this.host = host;
        this.port = port;
        this.function = function;

        udpState = new UdpState();
        udpState.endpoint = new IPEndPoint(IPAddress.Parse(host), port);
        udpState.client = new UdpClient(udpState.endpoint);

        receiveThread = new Thread(new ThreadStart(StartListening));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    public byte[] lastReceivedUDPPacket;

    //void OnGUI() {
    //    Rect rectObj=new Rect(40,10,200,400);
    //    GUIStyle style = new GUIStyle();
    //    style.alignment = TextAnchor.UpperLeft;
    //    GUI.Box(rectObj, "# UDPReceive\n127.0.0.1 " + port + " #\n"
    //                   + "\nLast Packet: \n" + lastReceivedUDPPacket
    //            , style);
    //}

    public bool listening = true;
    private void StartListening() {
        //Debug.Log("Looking for data.......");
        if (!listening) return;
        udpState.client.BeginReceive(new AsyncCallback(ProcessReceive), udpState);
    }
    private void ProcessReceive(IAsyncResult dataResult) {
        UdpClient client = ((UdpState)(dataResult.AsyncState)).client;
        IPEndPoint remoteEP = ((UdpState)(dataResult.AsyncState)).endpoint;
        byte[] data = client.EndReceive(dataResult, ref remoteEP);
        receiveThread = new Thread(new ThreadStart(StartListening));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        lastReceivedUDPPacket = data;
        List<float> dataAfterAddressAsFloats = ParseOscDatagram(data);
        function(dataAfterAddressAsFloats);
    }
    private class UdpState { public UdpClient client; public IPEndPoint endpoint; }


    private string ParseTypeTag(byte[] datagram, int startingOffset, out int finalOffset) {
        finalOffset = startingOffset;
        int strLen = 0;
        while (datagram[finalOffset] != '\0') { strLen = finalOffset = finalOffset + 1; }
        //Debug.Log("Skipping " + (4 - (strLen % 4)) + " more, strLen is " + strLen);
        finalOffset += 4 - (strLen % 4);
        strLen -= 1;
        //Debug.Log(ListOp.ListToString(datagram.Skip(finalOffset).Take(12).ToList()));
        string toReturn = new string(Encoding.ASCII.GetChars(datagram.Skip(startingOffset).SkipWhile(x => x == ',').Take(strLen).ToArray()));
        return toReturn;
    }

    private float ParseFloat(byte[] datagram, int startingOffset, out int finalOffset) {
        finalOffset = startingOffset + 4;
        byte[] floatBuffer = new byte[4];
        floatBuffer[3] = datagram[startingOffset + 0];
        floatBuffer[2] = datagram[startingOffset + 1];
        floatBuffer[1] = datagram[startingOffset + 2];
        floatBuffer[0] = datagram[startingOffset + 3];
        return BitConverter.ToSingle(floatBuffer, 0);
    }

    private int ParseInt(byte[] datagram, int startingOffset, out int finalOffset) {
        finalOffset = startingOffset + 4;
        byte[] intBuffer = new byte[4];
        intBuffer[3] = datagram[startingOffset + 0];
        intBuffer[2] = datagram[startingOffset + 1];
        intBuffer[1] = datagram[startingOffset + 2];
        intBuffer[0] = datagram[startingOffset + 3];
        //Debug.Log("[BalancerOscServer] bytes for int: " + ListOp.ListToString(intBuffer.ToList()));
        return BitConverter.ToInt32(intBuffer, 0);
    }

    public List<float> ParseOscDatagram(byte[] oscDatagram) {
        List<float> valuesInDatagram = new List<float>();

        int offset = 0;
        string typeTag = ParseTypeTag(oscDatagram, offset, out offset);
        //Debug.Log(typeTag);
        foreach (char typeChar in typeTag.ToCharArray()) {
            switch (typeChar) {
                case 'i':
                    int decodedInt = ParseInt(oscDatagram, offset, out offset);
                    valuesInDatagram.Add(decodedInt);
                    //Debug.Log("[BalancerOscServer] decodedInt: " + decodedInt);
                    break;
                case 'f':
                    float decodedFloat = ParseFloat(oscDatagram, offset, out offset);
                    valuesInDatagram.Add(decodedFloat);
                    break;
                default:
                    Debug.LogError("[NetworkResponseProcessor] Decoding message: Unsupported type: " + typeChar);
                    break;
            }
        }

        //Debug.Log("[NetworkResponseProcessor] Finished parsing datagram.");
        return valuesInDatagram;
    }


    void OnApplicationQuit() {
        Debug.Log("Trying to stop thread...");
        listening = false;
        Thread.Sleep(50);
        Debug.Log("ThreadState: " + receiveThread.ThreadState);
    }

}