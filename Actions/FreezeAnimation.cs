using System;
using System.Collections.Generic;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class FreezeAnimation : BitActionAnimation
	{
		private static GameObject _freezePrefab;

		private bool _isGold;

		private readonly BitActionAnimation _innerAnimation;

		private TriggerType _trigger;

		private static GameObject FreezePrefab => _freezePrefab ?? (_freezePrefab = Resources.Load<GameObject>("Prefabs/BattlePrefabs/BattleStaticEffects/FreezeEffectPrefab"));

		public FreezeAnimation(BitActionAnimation innerAnimation, TriggerType trigger = TriggerType.TurnEnded, bool isGold = false)
		{
			_innerAnimation = innerAnimation;
			_trigger = trigger;
			_isGold = isGold;
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
					Debug.LogError("Trying to attach freeze animation to something that appears not to be FieldMonsterVisual");
					continue;
				}
				int turns = int.Parse(str);
				GameObject gameObject = UnityEngine.Object.Instantiate(FreezePrefab);
				gameObject.transform.localScale = FreezePrefab.transform.localScale;
				gameObject.transform.parent = item.Value.transform.parent;
				gameObject.transform.localPosition = item.Value.transform.localPosition;
				gameObject.GetComponent<FreezeStaticAnimation>().Init((FieldMonsterVisual)item.Value, turns, _trigger, _isGold);
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
