using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	public class ObjectSwitcher : MonoBehaviour
	{
		[Tooltip("Defaults to the children of this GameObject")]
		public GameObject[] Objects;

		[System.Serializable]
		public class ObjectEvent : UnityEvent<GameObject> {}
      
		[System.Serializable]
		public class Evts {
			public UnityEvent Switch;
			public ObjectEvent Object;
		}
      
		public Evts Events;
		private GameObject activeObject = null;

		private GameObject[] ResolvedObjects { get {
				return this.Objects == null || this.Objects.Length == 0 ? this.DefaultObjects : this.Objects;
			}}
      
		private GameObject[] DefaultObjects { get {
				var objs = new GameObject[this.transform.childCount];
                for (int i = 0; i < this.transform.childCount; i++) objs[i] = this.transform.GetChild(i).gameObject;
				return objs;
			}}

		private void Start()
		{
			if (this.Objects == null || this.Objects.Length == 0) this.Objects = this.DefaultObjects;
		}
      
		#region Public Methods
		public void Switch(int idx) {
			if (idx < 0 || idx >= this.ResolvedObjects.Length) {
				Debug.Log("[ObjectSwitcher] invalid idx: " + idx.ToString());
				this.Switch(null);
				return;
			}

			this.Switch(this.ResolvedObjects[idx]);         
		}
      
		public void Switch(GameObject obj)
		{
			for (int i = 0; i < this.ResolvedObjects.Length; i++)
			{
				this.ResolvedObjects[i].SetActive(this.ResolvedObjects[i] == obj);
			}
         
			if (obj != this.activeObject)
            {
                this.activeObject = obj;
                this.Events.Switch.Invoke();
				this.Events.Object.Invoke(this.activeObject);
            }         
		}

		public void SwitchRandom() {
			Switch(new System.Random().Next(this.ResolvedObjects.Length));
		}
		#endregion
	}
}