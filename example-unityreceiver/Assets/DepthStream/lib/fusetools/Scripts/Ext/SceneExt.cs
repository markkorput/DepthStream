using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace FuseTools
{
	public class SceneExt : MonoBehaviour
	{
		[Tooltip("Will default to this.gameObject.scene")]
		public string SceneName;
		public bool InvokeEventsWhenInactive = false;
      
		[System.Serializable]
		public class Evts {
			public UnityEvent OnLoad;
			public UnityEvent OnUnload;

			public UnityEvent IsOnlyScene;
			public UnityEvent IsNotOnlyScene;
		}

		public Evts Events;

		private List<System.Action> destroyFuncs = new List<System.Action>();
      
		void Start()      
		{
			SceneManager.sceneLoaded += this.OnLoaded;         
			SceneManager.sceneUnloaded += this.OnUnloaded;

			destroyFuncs.Add(() =>
			{
				SceneManager.sceneLoaded -= this.OnLoaded;
				SceneManager.sceneUnloaded -= this.OnUnloaded;            
			});
		}
      
		void OnDestroy()
		{
			foreach (var func in this.destroyFuncs) func.Invoke();
			this.destroyFuncs.Clear();
        }

		private void OnLoaded(Scene scene, LoadSceneMode mode)
		{
			if (this.isActiveAndEnabled || this.InvokeEventsWhenInactive) {
				this.Events.OnLoad.Invoke();
			}
		}
      
		private void OnUnloaded(Scene scene)
        {
            if (this.isActiveAndEnabled || this.InvokeEventsWhenInactive) {
                this.Events.OnUnload.Invoke();
            }
        }

		#region Public Action Methods
		public void LoadSingle()
        {
			SceneManager.LoadSceneAsync(this.SceneName, LoadSceneMode.Single);
        }

		public void LoadAdditive() {
			SceneManager.LoadSceneAsync(this.SceneName, LoadSceneMode.Additive);
		}
      
		public void Unload() {
			SceneManager.UnloadSceneAsync(this.SceneName);
		}

		public bool IsOnlyScene { get {
			var activeScene = SceneManager.GetActiveScene();
			return SceneManager.sceneCount == 1 && activeScene != null && activeScene.name.Equals(this.SceneName); }}

		public void InvokeIsOnlyScene() {
			// if (this.isActiveAndEnabled || this.InvokeEventsWhenInactive) {
				(this.IsOnlyScene ? this.Events.IsOnlyScene : this.Events.IsNotOnlyScene).Invoke();
            // }
		}
		#endregion
	}
}
