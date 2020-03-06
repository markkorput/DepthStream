using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using RSG; // Promise Library

namespace FuseTools
{
	/// <summary>
    /// Lets you assign groups PlayableDirectors to an ID, which
	/// can be started through public methods (using UnityActions).
	/// When the playback for a certain ID is requested, all directors
	/// associated with other IDs are stopped.
	/// Also provides a promise-based feedback mechanism for when playing
	/// directors stop playing.
    /// </summary>
	[AddComponentMenu("FuseTools/DirectorPool")]
	public class DirectorPool : MonoBehaviour
	{      
		[System.Serializable]
		public class Item
		{
			public int id;
			public PlayableDirector[] Directors;
		}

		public bool EvaluateStoppedDirectors = true;
		public Item[] items;

		private int playCounter = 0;

		public Promise Play(int id)
		{
			var directors = this.DirectorsForId(id);

			if (directors.Length == 0)
			{
				return new Promise((res, rej) =>
				{
					rej(new UnityException("[DirectorPool] got unknown id: " + id.ToString()));
				});
			}

			playCounter += 1;
			int curPlayCounterValue = this.playCounter;
         
			this.Stop();
			var promises = (from dir in directors
							select new Promise((resolve, reject) =>
							{
								dir.stopped += (dir_) =>
								{
									if (this.playCounter == curPlayCounterValue) resolve();
								};

								dir.Play();
							})).ToArray();

			return (Promise)Promise.All(promises);
		}

		public void PlayById(int id)
		{
			this.Play(id);
		}

		public void Stop()
		{
			// stop all (also the requested director)
			foreach (var it in items)
			{
				foreach (var d in it.Directors)
				{
					d.Stop();
					if (EvaluateStoppedDirectors) d.Evaluate();
				}
			}
		}

		private Item[] ItemsForId(int id)
		{
			return new List<Item>(this.items).FindAll((item) => { return item.id == id; }).ToArray();
		}

		private PlayableDirector[] DirectorsForId(int id)
		{
			List<PlayableDirector> directors = new List<PlayableDirector>();
			foreach (var it in this.ItemsForId(id))
			{
				foreach (var d in it.Directors) directors.Add(d);
			}

			return directors.ToArray();
		}
	}
}