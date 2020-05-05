using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using System.Collections;

namespace FuseTools
{
	/// <summary>
	/// Simple component that triggers lifecyce (start, enable, disable) events.
	/// </summary>
	/// 
	[AddComponentMenu("FuseTools/DirectorExt")]
	public class DirectorExt : MonoBehaviour
	{
		[Tooltip("Defaults to first PlayableDirector found on this GameObject")]
		public PlayableDirector Director;

		[System.Serializable]
		public class Evts
		{
			public UnityEvent Played;
			public UnityEvent Paused;
			public UnityEvent Stopped;
			// public UnityEvent Ended;
		}
      
		public Evts Events;

		void Start()
		{
			if (Director == null) this.Director = GetComponent<PlayableDirector>();

			if (Director == null) return;
			this.Director.paused += this.OnPaused;
			this.Director.played += this.OnPlayed;
			this.Director.stopped += this.OnStopped;
		}

		private void OnDestroy()
		{
			if (this.Director != null) this.Director.stopped -= this.OnStopped;
		}

		private void OnPlayed(PlayableDirector dir)
		{
			this.Events.Played.Invoke();
		}

		private void OnPaused(PlayableDirector dir)
		{
			this.Events.Paused.Invoke();
		}

		private void OnStopped(PlayableDirector dir)
		{
			this.Events.Stopped.Invoke();         
			// if (dir.time >= dir.duration) this.Events.Ended.Invoke(); // time is already reset to zero at this point
		}

		#region Public Methods
		public void SetTime(float t) {
			if (this.Director != null) this.Director.time = t;
		}

		public void PlayBackwards() {
			StartCoroutine(this.PlayBackBackwardsCoro());
		}
		#endregion

		public static IEnumerator Play(PlayableDirector dir) {
			bool done = false;
			System.Action<PlayableDirector> callback = (_) => done = true;
			dir.stopped += callback;
			dir.Play();
			yield return new WaitUntil(() => done);
			dir.stopped -= callback;
		}

		private IEnumerator PlayBackBackwardsCoro(){
			this.Director.Pause();
			var t = this.Director.duration;

			while (t >= 0.0) {
				this.Director.time = t;
				this.Director.Evaluate();
				yield return null;
				t -= Time.deltaTime;
			}

			this.Director.Stop();
		}
	}
}