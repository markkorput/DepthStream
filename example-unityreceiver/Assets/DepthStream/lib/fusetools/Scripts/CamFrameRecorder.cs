
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using System.Text.RegularExpressions;

namespace FuseTools
{
    public class CamFrameRecorder : MonoBehaviour
    {
        [Header("Capture Settings")]		
        [Tooltip("Required; determines the resolution of the recording")]
        public RenderTexture RenderTexture;
		[Tooltip("Set to -1 to disable capture framerate")]
		public int CaptureFramerate = 60;
        [Header("Export Settings")]
        [Tooltip("When left empty, will default to the current user's desktop folder")]
        public string Folder = "{DesktopPath}";
        public bool CreateTimestampFolder = true;
        [Tooltip("When left empty will default to \"f{FrameNumber:5}.png\"")]
        public string FileNameFormat = "f{FrameNumber:5}.png";
        [Header("Behaviour Settings")]
        public bool StartAtAwake = false;
        public bool Verbose = false;

		/// <summary>
        /// Optional func to register custom texture-saving logic
        /// </summary>
		public System.Action<Texture2D> SaveTextureFunc = null;
        public System.Action<byte[]> SavePNGFunc = null;
      
        [System.Serializable]
        public class Evts
        {
            public UnityEvent RecordingStarted;
            public UnityEvent RecordingStopped;
            public UnityEvent FrameRecorded;
        }
      
        public Evts Events;

#if UNITY_EDITOR
        [System.Serializable]
        public class Dinfo
        {
            public bool IsRecording = false;
            public int FrameCount = 0;
        }

        public Dinfo DebugInfo;
#endif
      
        public bool IsRecording { get; private set; }
        public Camera Camera { get; private set; }

        private Texture2D texture2D = null;
        private List<CamSpy> camSpies = new List<CamSpy>();
        private string framesFolder;
		private int frameCount = 0;
        private int stopAtFrameCount = -1;

        #region Unity Methods
		void Start()
		{
			this.IsRecording = false;
			this.Camera = null;

			if (this.StartAtAwake) this.StartRecording();

			if (this.Events == null) this.Events = new Evts();
			if (this.Events.RecordingStarted == null) this.Events.RecordingStarted = new UnityEvent();
			if (this.Events.RecordingStopped == null) this.Events.RecordingStopped = new UnityEvent();

#if UNITY_EDITOR
			if (this.DebugInfo == null) this.DebugInfo = new Dinfo();
#endif
        }

        private void Update()
        {
            if (IsRecording)
            {
                var cam = Camera.main;
                if (cam == null) cam = FindObjectOfType<Camera>();

                if (cam != null && cam != this.Camera)
                {
                    this.Camera = cam;
                    var spy = CamSpy.ActivateFor(cam.gameObject, this.AfterRender, this.RenderTexture);
                    if (spy != null) this.camSpies.Add(spy);
                }
            }
        }
        #endregion

        #region Private Methods
        /// This method is invoked by CamSpy instances attached to a GameObject which also has a Camera instance,
        /// which means the CamSpy component's OnPostRender method is invoked by unity, where it will invoke the
        /// callback we gave it, which calls this AfterRender method.
        void AfterRender()
        {
            if (!this.IsRecording) return;         
            if (this.RenderTexture == null) return;
         
            if (this.texture2D == null)
            {
                RenderTexture.active = this.RenderTexture;
                this.texture2D = new Texture2D(this.RenderTexture.width, this.RenderTexture.height, TextureFormat.RGB24, false);
            }
         
            if (this.texture2D == null)
            {
				Debug.Log("[FuseTools.CamFrameRecorder] Failed to create 2D texture");
                return;
            }

            this.texture2D.ReadPixels(new Rect(0, 0, this.RenderTexture.width, this.RenderTexture.height), 0, 0);
            RenderTexture.active = null;

			if (this.SaveTextureFunc != null) {
                // custom texture-saving logic? Invoke it
				this.SaveTextureFunc.Invoke(this.texture2D);
            } else {
                // convert to PNG
                byte[] bytes = this.texture2D.EncodeToPNG();
                // Invoke custom PNG saver func if one is registered
                if (this.SavePNGFunc != null) this.SavePNGFunc.Invoke(bytes);
                // Invoke our default PNG exporter func
			    else this.Export(this.NextFrameFilePath, bytes);
            }

			this.frameCount += 1;
#if UNITY_EDITOR
			this.DebugInfo.FrameCount = this.frameCount;
#endif

            this.Events.FrameRecorded.Invoke();

            if (this.stopAtFrameCount > 0 && this.frameCount >= this.stopAtFrameCount) this.StopRecording();
        }

        string NextFrameFileName { get {
            var filename = this.FileNameFormat;

            filename = Regex.Replace(filename, @"\{FrameNumber\:(\d+)\}", (match) => {
                // Debug.Log("match.ToString: "+match.ToString()+ " capture[0].ToString(): "+match.Groups[0].Captures[0].ToString());

                int digitCount;
                if (!int.TryParse(match.Groups[0].Captures[0].ToString(), out digitCount))
                digitCount = 5;

                return this.frameCount.ToString().PadLeft(digitCount, '0');
            });

            filename = Regex.Replace(filename, @"{RuntimeFrameNumber\:(\d+)}", (match) => {
                int digitCount;
                if (!int.TryParse(match.Groups[0].Captures[0].ToString(), out digitCount))
                digitCount = 5;

                return Time.frameCount.ToString().PadLeft(digitCount, '0');
            });

            return filename;
        }}

        string NextFrameFilePath { get {
            return System.IO.Path.Combine(this.framesFolder, this.NextFrameFileName);
        }}

        private void Export(string path, byte[] bytes) {
            System.IO.File.WriteAllBytes(path, bytes);
            if (Verbose) Debug.Log("Wrote frame to: " + path);

			//if (Verbose)
            //        {
            //Debug.Log("Frame Timing Info:"
            //+ "\n - Time.time = " + Time.time.ToString()
            //+ "\n - Time.fixedTime = " + Time.fixedTime.ToString()
            //  + "\n - Time.realtimeSinceStartup" + Time.realtimeSinceStartup.ToString()
            //  + "\n - Time.deltaTime = " + Time.deltaTime.ToString()
            //  +"\n - Time.fixedDeltaTime = " + Time.fixedDeltaTime.ToString());
            //}
        }
        #endregion

        #region Public Action Methods
        public void StartRecording()
        {
            if (this.Folder == "") this.Folder = "{DesktopPath}"; //Application.persistentDataPath;

			// create frames folder?
            if (this.CreateTimestampFolder)
            {
                this.framesFolder = System.IO.Path.Combine(FuseTools.Converters.String.ConvertMacros(this.Folder), "Frames-" + System.DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss"));
                System.IO.Directory.CreateDirectory(this.framesFolder);
            }
            else
            {
                this.framesFolder = FuseTools.Converters.String.ConvertMacros(this.Folder);
            }

            // Set captureFramerate?
            if (this.CaptureFramerate > 0)
            {
                Time.captureFramerate = this.CaptureFramerate;
                Debug.Log("[Recorder] Starting Recording at " + this.CaptureFramerate.ToString() + "fps To Folder: " + this.framesFolder);
            }
            else
            {
                Debug.Log("[Recorder] Starting Recording To Folder: " + this.framesFolder);
            }

            // change state to "is recording"
            this.IsRecording = true;
            this.Events.RecordingStarted.Invoke();
			this.frameCount = 0;
#if UNITY_EDITOR
            this.DebugInfo.IsRecording = this.IsRecording;
			this.DebugInfo.FrameCount = this.frameCount;
#endif
            this.Update();
        }

        public void StopRecording()
        {
            if (this.IsRecording)
            {
                // TODO: cleanup CamSpy instances?
                this.IsRecording = false;
                this.Camera = null;
                this.Events.RecordingStopped.Invoke();
#if UNITY_EDITOR
                this.DebugInfo.IsRecording = this.IsRecording;
#endif
            }

            foreach (var spy in this.camSpies) spy.Dispose();
            this.camSpies.Clear();
        }

        public void RecordNumFrame(int count) {
            this.stopAtFrameCount = count;
            this.StartRecording();
        }
        #endregion
    }
   
    public class CamSpy : MonoBehaviour, System.IDisposable
    {
        public System.Action postFunc = null;
        private RenderTexture renderTexture = null;
        private Camera cam = null;
        private RenderTexture restoreRenderTex = null;
        private bool renderTexSet = false;

        void OnPostRender()
        {
            if (this.postFunc != null) this.postFunc.Invoke();
        }

        public void Dispose()
        {
            if (this.renderTexSet)
            {
                this.cam.targetTexture = this.restoreRenderTex;
                this.renderTexSet = false;
            }

            Destroy(this);

            this.cam = null;
            this.restoreRenderTex = null;
            this.renderTexture = null;
        }
      
        private void Setup(System.Action postRenderFunc, RenderTexture renderTexture = null)
        {
            this.postFunc = postRenderFunc;
            this.renderTexture = renderTexture;

            if (this.renderTexture != null)
            {
                this.cam = this.GetComponent<Camera>();
                if (this.cam != null)
                {
                    this.restoreRenderTex = this.cam.targetTexture;
                    this.cam.targetTexture = this.renderTexture;
                    this.renderTexSet = true;
                }
            }
        }

        public static CamSpy ActivateFor(GameObject gameObject, System.Action func, RenderTexture renderTex)
        {
            var existing = gameObject.GetComponent<CamSpy>();
            // if (existing != null) return null; // not creating new instance         
            if (existing != null) existing.Dispose();
            var spy = existing == null ? gameObject.gameObject.AddComponent<CamSpy>() : existing;
            spy.Setup(func, renderTex);
            return existing == null ? spy : null;
        }
    }
}