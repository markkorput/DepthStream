using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Computils;

namespace depth {
    public class Populator : MonoBehaviour
    {
        public ComputeBufferFacade vectorBufferFacade;

        public UnityEvent OnNewFrameSize = new UnityEvent();


        Vector3[] vec3s = null;
        int width;
        int height;

        System.Action<byte[], Vector3[], int, int> populatorFunc = null;


        public void PopulateVector3(Frame f) {
            bool newSize = false;

            if (vec3s == null || vec3s.Length != f.Size) {
                if (f.Size == (1280*720*2)) {
                    Debug.Log("Allocating new vector buffer for 16bit 1280x720 stream");
                    width = 1280;
                    height = 720;
                    populatorFunc = populateVector3_16bit;
                    vec3s = new Vector3[f.Size];
                    newSize = true;
                } else {
                    Debug.LogWarning("Unsupported frame size: "+f.Size);
                    return;
                }
            }

            populatorFunc.Invoke(f.Data, vec3s, width, height);

            Populate(vec3s);
            // Debug.Log("Populated "+width+"x"+height+" depth pixels");
            if (newSize) OnNewFrameSize.Invoke();
        }

        void populateVector3_16bit(byte[] data, Vector3[] vector3s, int width, int height) {
            int idx_in=0;
            int idx_out=0;

            // ushort[] values = (ushort[])data;

            for(int y=0; y<height; y++) {
            for(int x=0; x<width; x++) {
                byte b1 = data[idx_in];
                byte b2 = data[idx_in+1];

                ushort z = (ushort)(b1 | (b2 << 8));
                vector3s[idx_out] = new Vector3(x,y,(float)z);

                idx_in += 2;
                idx_out++;
            }}
        }

        private void Populate(Vector3[] data, int offset=0)
		{
			var buf = vectorBufferFacade.GetValid();         
			buf = Computils.Populators.Utils.UpdateOrCreate(buf, data, offset);
			vectorBufferFacade.Set(buf);
		}
    }
}