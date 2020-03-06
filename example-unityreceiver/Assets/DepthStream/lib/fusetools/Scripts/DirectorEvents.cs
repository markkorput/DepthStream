using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace FuseTools
{
	/// <summary>
	/// Simple component that triggers lifecyce (start, enable, disable) events.
    /// </summary>
	/// 
	[System.Obsolete("Use DirectorExt")]
	[AddComponentMenu("FuseTools/DirectorEvents")]
	public class DirectorEvents : MonoBehaviour
	{
		[Tooltip("Defaults to first PlayableDirector found on this GameObject")]
		public PlayableDirector Director;
      
		[System.Serializable]
		public class Evts
		{
			public UnityEvent Played;
			public UnityEvent Paused;
			public UnityEvent Stopped;
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
      
		private void OnPlayed(PlayableDirector dir) {
            this.Events.Played.Invoke();
		}
      
		private void OnPaused(PlayableDirector dir)
        {
            this.Events.Paused.Invoke();
        }
      
		private void OnStopped(PlayableDirector dir)
        {
            this.Events.Stopped.Invoke();
        }
	}
}