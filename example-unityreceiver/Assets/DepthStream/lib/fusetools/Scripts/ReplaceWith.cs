using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools
{
	/// <summary>
    /// At Start this component replaces its GameObject by instantiating a
	/// new GameObject (specified by its Template attribute) and Destroying
	/// its own GameObject. There are various options for which properties
	/// should be transfered from this Component's GameObject to the Replacement.
    /// </summary>
	[AddComponentMenu("FuseTools/ReplaceWith")]
	public class ReplaceWith : MonoBehaviour
	{
		public const int APPLY_TRANSFORM = (1 >> 0);
		public const int APPLY_SIBLING_INDEX = (1 >> 1);
		public const int APPLY_NAME = (1 >> 2);
      
		public GameObject Template;
		public bool ApplyTransform = true;
		public bool ApplySiblingIndex = true;
		public bool ApplyName = false;

		void Start()
		{
			Replace(this.gameObject, this.Template, ToFlags(this.ApplyTransform, this.ApplySiblingIndex, this.ApplyName));
		}
      
		public static GameObject Replace(GameObject replaced, GameObject replacer, int flags) {
			int idx = replaced.transform.GetSiblingIndex();
         
            GameObject replacement;

			if ((flags & APPLY_TRANSFORM) != 0)
            {
				replacement = Instantiate(replacer, replaced.transform.position, replaced.transform.rotation,replaced .transform.parent);
				replacement.transform.localScale = replaced.transform.localScale;
            }
            else
            {
                replacement = Instantiate(replacer, replaced.transform.parent);
            }

			if ((flags & APPLY_NAME) != 0) replacement.name = replaced.gameObject.name;
			if ((flags & APPLY_SIBLING_INDEX) != 0) replacement.transform.SetSiblingIndex(idx);
            Destroy(replaced);

			return replacement;
		}

		public static int ToFlags(bool transform, bool sibling_index, bool name) {
			return
				(transform ? APPLY_TRANSFORM : 0)
				| (sibling_index ? APPLY_SIBLING_INDEX : 0)
				| (name ? APPLY_NAME : 0);
		}
	}
}