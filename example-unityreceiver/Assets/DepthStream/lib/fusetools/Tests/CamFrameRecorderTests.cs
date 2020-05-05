using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using NUnit.Framework;


namespace FuseTools.Tests
{
    public class CamFrameRecorderTests
    {
        [UnityTest]
        public IEnumerator LoadAndRecordScene()
        {
            // Create recorder
            var go = new GameObject("Recorder");
			var recorder = go.AddComponent<CamFrameRecorder>();
			recorder.CreateTimestampFolder = false;

            {   // uninitialized public Event attributes started
                // becoming an issue in Unity 2018.3
                recorder.Events = new CamFrameRecorder.Evts(); 
                recorder.Events.FrameRecorded = new UnityEvent();
                recorder.Events.RecordingStarted = new UnityEvent();
                recorder.Events.RecordingStopped = new UnityEvent();
            }

			int frameCount = 0;
			recorder.SaveTextureFunc = (tex) => { frameCount += 1; };
            recorder.RenderTexture = new RenderTexture(128, 128, 24);         
            yield return null;

            // "load scene"
            var sceneobj = new GameObject("SceneTimer");
            var camobj = new GameObject("Camera");
            var cam = camobj.AddComponent<Camera>();
            cam.gameObject.tag = "MainCamera";
            yield return null;
            Assert.AreNotEqual(cam.targetTexture, recorder.RenderTexture);
         
            // Start recording
			Assert.IsFalse(recorder.IsRecording);
            recorder.StartRecording();
            Assert.IsTrue(recorder.IsRecording);
         
            //Assert.AreNotEqual(cam.targetTexture, recorder.RenderTexture);
            yield return null;
            Assert.AreEqual(cam.targetTexture, recorder.RenderTexture);
			Assert.AreEqual(frameCount, 1);
         
            yield return new WaitForSeconds(1);
            Assert.IsTrue(recorder.IsRecording);
			Assert.Greater(frameCount, 10);

            Assert.IsTrue(recorder.IsRecording);
			int lastAmount = frameCount;
            yield return new WaitForSeconds(0.2f);
            Assert.IsTrue(recorder.IsRecording);         
			Assert.Greater(frameCount, lastAmount);
         
			lastAmount = frameCount;
			recorder.StopRecording();         

			Assert.IsFalse(recorder.IsRecording);
			yield return new WaitForSeconds(0.2f);
			Assert.AreEqual(lastAmount, frameCount);
         
            // cleanup
			Object.Destroy(go);
			Object.Destroy(sceneobj);
			Object.Destroy(camobj);
        }
    }
}