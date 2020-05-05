using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace FuseTools
{
	[AddComponentMenu("FuseTools/AudioSourceExt")]
	public class AudioSourceExt : MonoBehaviour
	{
		[Tooltip("Defaults to AudioSource component found on this GameObject")]
		public AudioSource AudioSource;
		public float LengthToEndToConsiderFinished = 0.2f;

		[System.Serializable]
		public class Evts
		{
			public UnityEvent Started;
			public UnityEvent Stopped;
			public UnityEvent Finished;
			public FloatEvent Duration;
		}

		public Evts Events;
      
		public AudioSource ResolvedAudioSource { get { return this.AudioSource != null ? this.AudioSource : this.GetComponent<AudioSource>(); }}

		private bool isPlaying = false;
		private float lastPlayingTime;


		void Update() {
			if (this.ResolvedAudioSource == null) return;

			var p = this.ResolvedAudioSource.isPlaying;

			if (p != isPlaying) {
				this.isPlaying = p;
				(this.isPlaying ? this.Events.Started : this.Events.Stopped).Invoke();
				
				if (!isPlaying) {
					if ((this.ResolvedAudioSource.clip.length - this.lastPlayingTime) < LengthToEndToConsiderFinished)
						this.Events.Finished.Invoke();
				}
			}

			if (p) {
				this.lastPlayingTime = this.ResolvedAudioSource.time;
			}
		}

		#region Public Methods
		public void InvokeDuration() {
			this.Events.Duration.Invoke(this.ResolvedAudioSource.clip.length);
		}
		#endregion
	}
}