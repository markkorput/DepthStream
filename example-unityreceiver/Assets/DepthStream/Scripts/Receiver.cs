using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace depth {
    public class Receiver : MonoBehaviour
    {
        public string SenderHost = SocketReceiver.DEFAULT_HOST;
        public int SenderPort = SocketReceiver.DEFAULT_PORT;
        SocketReceiver receiver;

        string last = null;



        // Start is called before the first frame update
        void Start()
        {
            receiver = new SocketReceiver(SocketReceiver.Connect(SenderHost, SenderPort), onPacket);
            receiver.Start();
        }

        void OnDestroy() {
            if (receiver != null) {
                receiver.Stop();
                receiver = null;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (last != null) Debug.Log(last);
        }

        void onPacket(int len, byte[] packet) {
            var decompressed = Decompress(packet, len);
            last = "Got packet: "+len+" bytes, decompressed: "+decompressed.Length;
        }


        public static byte[] Decompress(byte[] data, int len) {
            MemoryStream input = new MemoryStream(data, 0, len);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress)) {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }
    }
}