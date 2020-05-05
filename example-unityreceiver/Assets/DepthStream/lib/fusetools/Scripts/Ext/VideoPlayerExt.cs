using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace FuseTools
{
	/// <summary>
	/// 
	/// </summary>
	/// 
	[AddComponentMenu("FuseTools/VideoPlayerExt")]
	public class VideoPlayerExt : MonoBehaviour
	{
		[System.Serializable]
		public class DoubleEvent : UnityEvent<double> {}

		[System.Serializable]
        public class Evts
        {
            public DoubleEvent Time;
						public UnityEvent LoopPointReached;
        }

		[Tooltip("Defaults to first PlayableDirector found on this GameObject")]
		public UnityEngine.Video.VideoPlayer VideoPlayer;      
		public Evts Events;

		void Start()
		{
			if (VideoPlayer == null) this.VideoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
			if (this.VideoPlayer != null) {
				this.VideoPlayer.loopPointReached += this.OnLoopPointReached;
			}
		}

		private void OnDestroy()
		{
			if (this.VideoPlayer != null) {
				this.VideoPlayer.loopPointReached -= this.OnLoopPointReached;
			}
		}

		private void OnLoopPointReached(UnityEngine.Video.VideoPlayer player) {
			this.Events.LoopPointReached.Invoke();
		}

		public double GetTime(){
			return this.VideoPlayer.time;
		}

		public void SetTime(double t) {
			this.VideoPlayer.time = t;
		}

		public double GetDur() {
			if (this.VideoPlayer.clip != null) {
				return VideoPlayer.clip.length;
			}

			if (VideoPlayer.frameRate < float.Epsilon) return 0.0f;
			return (VideoPlayer.frameCount / VideoPlayer.frameRate);	
		}

		public void SetFrame(long frame) {
			this.VideoPlayer.frame = frame;
		}
      
		#region Public Methods
		public void InvokeTime() {
			if (this.VideoPlayer != null) this.Events.Time.Invoke(this.VideoPlayer.time);
		}

		public void DiscardRenderTextureContent() {
			if (this.VideoPlayer != null && this.VideoPlayer.targetTexture != null)
				this.VideoPlayer.targetTexture.DiscardContents();
		}
		#endregion
	}
}