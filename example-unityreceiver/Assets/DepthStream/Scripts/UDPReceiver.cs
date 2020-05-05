using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace depth {

    public class UDPReceiver {
        public const int DEFAULT_PORT = 4445;

        // receiving Thread
        Thread receiveThread = null;
    
        // udpclient object
        UdpClient client;
        int port = DEFAULT_PORT;
        bool keepGoing;
        Action<byte[]> packetCallback = null;
        byte[] lastPacket = null;

        public byte[] LastPacket { get { return lastPacket; }}

        public UDPReceiver(Action<byte[]> packetCallback, int port=DEFAULT_PORT) {
            this.port = port;
            this.packetCallback = packetCallback;
        }

        // // start from shell
        // private static void Main()
        // {
        //     UDPReceive receiveObj=new UDPReceive();
        //     receiveObj.init();
        
        //     string text="";
        //     do
        //     {
        //         text = Console.ReadLine();
        //     }
        //     while(!text.Equals("exit"));
        // }
    
        // // OnGUI
        // void OnGUI()
        // {
        //     Rect rectObj=new Rect(40,10,200,400);
        //         GUIStyle style = new GUIStyle();
        //             style.alignment = TextAnchor.UpperLeft;
        //     GUI.Box(rectObj,"# UDPReceive\n127.0.0.1 "+port+" #\n"
        //                 + "shell> nc -u 127.0.0.1 : "+port+" \n"
        //                 + "\nLast Packet: \n"+ lastReceivedUDPPacket
        //                 + "\n\nAll Messages: \n"+allReceivedUDPPackets
        //             ,style);
        // }

        // init
        public void Start()
        {
            receiveThread = new Thread(
                new ThreadStart(ThreadFunc));
            receiveThread.IsBackground = true;
            keepGoing = true;
            receiveThread.Start();
        }

        public void Stop(bool joinThread=true) {
            keepGoing = false;

            if (receiveThread != null) {
                receiveThread.Abort();
                if (joinThread)
                    receiveThread.Join();

                receiveThread = null;
            }
        }

        private void ThreadFunc()
        {
            client = new UdpClient(port);
            
            try {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

                while (keepGoing) {
                    // Bytes empfangen.
                    lastPacket = client.Receive(ref anyIP);

                    // Bytes mit der UTF8-Kodierung in das Textformat kodieren.
                    // string text = Encoding.UTF8.GetString(data);
    
                    // Den abgerufenen Text anzeigen.
                    // print(">> " + text);

                    // latest UDPpacket
                    // lastReceivedUDPPacket=text;
                }
            }
            catch (Exception err) {
                Debug.LogError(err.ToString());
            }
        }
    }
}