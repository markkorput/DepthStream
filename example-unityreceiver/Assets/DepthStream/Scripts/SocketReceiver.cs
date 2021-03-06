using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using UnityEngine;

namespace depth {
  public class SocketReceiver {
    public const int DEFAULT_BUFFER_SIZE = 131072;
    public const string DEFAULT_HOST = "127.0.0.1";
    public const int DEFAULT_PORT = 4445;

    public bool IsConnected { get { return IsConnected; }}

    Socket socket = null;
    Thread receiveThread = null;
    byte[] receiveBuffer = null;
    int lastPacketLength = 0;
    Action<int, byte[]> callback;
    string error = null;
    bool waitForNext = true;
    bool readyForNext = true;
    bool isConnected = false;

    public SocketReceiver(Socket socket, Action<int, byte[]> callback, int bufferSize=DEFAULT_BUFFER_SIZE) {
      this.socket = socket;
      this.callback = callback;
      receiveBuffer = new byte[bufferSize];
    }

    public void Start()
    {
        receiveThread = new Thread(
            new ThreadStart(ThreadFunc));
        receiveThread.IsBackground = true;

        receiveThread.Start();
    }

    public void Stop(bool joinThread=true, bool closeSocket=true) {
      if (receiveThread != null) {
          receiveThread.Abort();
          if (joinThread)
              receiveThread.Join();

          receiveThread = null;
      }

      if (this.socket != null && closeSocket) CloseSocket(this.socket);
      this.socket = null;
    }

    public void ReadyForNext() {
      readyForNext = true;
    }

    private void ThreadFunc()
    {   
      isConnected = true;
  
      // try {
        int len;
        
        while (socket != null) {
          if (!readyForNext) {
            Thread.Sleep(10);
            continue;
          }

          len = readHeader(socket, receiveBuffer);
          
          if (len <= 0) break;
          
          if (len > receiveBuffer.Length) {
            Debug.LogWarning("Header announced "+len+"-byte packat, which is too big for our buffer ("+receiveBuffer.Length+" bytes), ignoring packet");
            skipBody(socket, receiveBuffer, len);
            continue;
          }

          if (readBody(socket, receiveBuffer, len) != len) break;

          lastPacketLength = len;
          if (this.callback != null) this.callback.Invoke(len, receiveBuffer);

          if (waitForNext) readyForNext = false;
        }
      // }
      // catch (Exception err) {
      //     error = err.ToString();
      // }

        isConnected = false;
    }

    private static int readHeader(Socket socket, byte[] buffer) {
      int len = socket.Receive(buffer, 4, 0);
      if (len != 4) return -1;
      return headerToInt(buffer);
    }

    private static int headerToInt(byte[] buf) {
      int b0 = (int)(0x0ff & buf[0]);
      int b1 = (int)(0x0ff & buf[1]);
      int b2 = (int)(0x0ff & buf[2]);
      int b3 = (int)(0x0ff & buf[3]);
      return ((b0 << 24) | (b1 << 16) | (b2 << 8) | b3);
    }

    private static int readBody(Socket socket, byte[] buffer, int size) {
      int count=0;

      while(count < size)
        count += socket.Receive(buffer, count, Math.Min(buffer.Length-count, size-count), 0);

      return count;
    }

    private static int skipBody(Socket socket, byte[] buffer, int size) {
      int len=0;

      while(len < size)
        len += socket.Receive(buffer, 0, Math.Min(buffer.Length, size-len), 0);

      return len;
    }

    public static Socket Connect(string server, int port) {
      Socket tempSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Unspecified);
      try {
        tempSocket.Connect(server, port);
      }
      catch(SocketException e) {
        Debug.LogWarning("Failed to connect to DepthStream socket: "+server+":"+port+", error: "+e.Message);
      }

      return tempSocket.Connected ? tempSocket : null;
    }

    public void CloseSocket(Socket sock) {
      try {
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
      } catch(SocketException e) {

      }
    }
  }
}