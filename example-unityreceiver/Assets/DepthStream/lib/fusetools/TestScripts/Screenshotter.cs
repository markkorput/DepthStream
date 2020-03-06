using System.Collections;
using UnityEngine;

namespace FuseTools.Test {
	/// <summary>
    /// Screenshotter class which facilitates taking and exporting a series of
	/// sequentially numbers screenshots to a timestamped folder (by default
	/// the user's Desktop folder, unless the SCREENSHOTS_FOLDER environment
	/// variable is set
    /// </summary>
	public class Screenshotter
    {
		private Opts opts = null;

		public Screenshotter(Opts opts = null) {
			this.opts = opts != null ? opts : new Opts();
		}

        private string folderTimestamp_ = null;
        private string folderTimestamp
        {
            get
            {
                if (folderTimestamp_ == null)
                    folderTimestamp_ = System.DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss");
                return folderTimestamp_;
            }
        }

        private string screenshotsFolder
        {
            get
            {
                if (this.opts.screenshotsFolder != null) return this.opts.screenshotsFolder;
                string path = System.Environment.GetEnvironmentVariable("SCREENSHOTS_FOLDER");
                if (path == null) path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                return path;
            }
        }

		private string fileFolderPrefix { get { return this.opts.folderPrefix != null ? this.opts.folderPrefix : "Screenshots"; }}
        private string fileFolderName { get { return this.fileFolderPrefix + "." + folderTimestamp; } }
        private string fileFolderPath { get { return System.IO.Path.Combine(this.screenshotsFolder, fileFolderName); } }
        private string GetFilePath(string filename)
        {
            return System.IO.Path.Combine(fileFolderPath, filename);
        }

        private int counter = 0;

        public IEnumerator Take(string fileName)
        {
            string filePath;

            if (this.opts.addCountPrefix) {
                string countPrefix = counter.ToString().PadLeft(2, '0');
                filePath = this.GetFilePath(countPrefix + "-" + fileName);
            } else {
                filePath = this.GetFilePath(fileName);
            }

            counter += 1;
            // return Actions.TakeScreenshot(filePath);         
			//public static IEnumerator TakeScreenshot(string filePath)
            //{
            //    Debug.Log("Saving screenshot to " + filePath);
            //    var promise = FuseTools.Screenshot.SavePNGTo(filePath);
            //    yield return RSG.Wait.WhilePending(promise);
            //}

			Debug.Log("Saving screenshot #" + this.counter.ToString() + ": " + filePath);
            var promise = FuseTools.Screenshot.SavePNGTo(filePath, true);
            yield return RSG.Wait.WhilePending(promise);         
        }

        public IEnumerator Moment(string name) {
            //Debug.Log("Moment: "+name+" (enabled: "+this.opts.isEnabled.ToString()+")" );
            if (!this.opts.isEnabled) yield break;
            yield return Take(name+".png");
        }

        public void SetEnabled(bool v) {
            this.opts.Enabled(v);
        }

		public class Opts {

            public Opts() {
                this.isEnabled = true;
                this.screenshotsFolder = null;
                this.folderPrefix = null;
            }

			public string folderPrefix { get; private set; }
			public Opts FolderPrefix(string prefix) { this.folderPrefix = prefix; return this; }

			public string screenshotsFolder { get; private set; }
			public Opts ScreenshotsFolder(string v) { this.screenshotsFolder = v; return this; }

            public bool addCountPrefix { get; private set; }
            public Opts AddCountPrefix(bool v) { this.addCountPrefix = v; return this; }

            public bool isEnabled { get; private set; }
            public Opts Enabled(bool v) { this.isEnabled = v; return this; }
		}
    }              
}
