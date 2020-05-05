using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace depth {
    public class Frame {
        int size;
        byte[] data;

        public int Size { get { return size; }}
        public byte[] Data { get { return data; }}

        public Frame(int s, byte[] d) {
            size = s;
            data = d;
        }
    }
}