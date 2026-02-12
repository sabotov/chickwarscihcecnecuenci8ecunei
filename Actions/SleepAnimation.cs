using System;
using System.Collections.Generic;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class SleepAnimation : BitActionAnimation
	{
		private static GameObject _sleepPrefab;

		private readonly BitActionAnimation _innerAnimation;

		private static GameObject SleepPrefab => _sleepPrefab ?? (_sleepPrefab = Resources.Load<GameObject>("Prefabs/BattlePrefabs/BattleStaticEffects/SleepEffectPrefab"));

		public SleepAnimation(BitActionAnimation innerAnimation)
		{
			_innerAnimation = innerAnimation;
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
			{
				string str = item.Key();
				dictionary.Add(() => str, item.Value);
				if (!(item.Value is FieldMonsterVisual) || item.Value == null)
				{
					Debug.LogError("Trying to attach sleep animation to something that appears not to be FieldMonsterVisual");
					continue;
				}
				int turns = int.Parse(str);
				GameObject gameObject = UnityEngine.Object.Instantiate(SleepPrefab);
				gameObject.transform.localScale = SleepPrefab.transform.localScale;
				gameObject.transform.parent = item.Value.transform.parent;
				gameObject.transform.localPosition = item.Value.transform.localPosition;
				gameObject.GetComponent<SleepStaticAnimation>().Init((FieldMonsterVisual)item.Value, turns);
			}
			if (_innerAnimation != null)
			{
				_innerAnimation.Animate(dictionary, onEnded);
			}
			else
			{
				onEnded();
			}
		}
	}
}
