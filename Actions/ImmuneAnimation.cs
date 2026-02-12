using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class ImmuneAnimation : BitActionAnimation
	{
		private static GameObject _immunePrefab;

		private static GameObject _immuneWarlordPrefab;

		private static GameObject _immunePrefabFriend;

		private static GameObject _immunePrefabEnemy;

		private static GameObject _immunePrefabNeg;

		private static GameObject _immunePrefabPos;

		private TriggerType _trigger;

		private int _duration;

		private readonly BitActionAnimation _innerAnimation;

		private static GameObject ImmunePrefab => _immunePrefab ?? (_immunePrefab = Resources.Load<GameObject>("Prefabs/BattlePrefabs/BattleStaticEffects/ImmunePrefab"));

		private static GameObject ImmuneWarlordPrefab => _immuneWarlordPrefab ?? (_immuneWarlordPrefab = Resources.Load<GameObject>("Prefabs/BattlePrefabs/BattleStaticEffects/ImmuneWarlordPrefab"));

		private static GameObject ImmunePrefabFriend => _immunePrefab ?? (_immunePrefab = Resources.Load<GameObject>("Prefabs/BattlePrefabs/BattleStaticEffects/ImmunePrefabFriend"));

		private static GameObject ImmunePrefabEnemy => _immunePrefab ?? (_immunePrefab = Resources.Load<GameObject>("Prefabs/BattlePrefabs/BattleStaticEffects/ImmunePrefabEnemy"));

		private static GameObject ImmunePrefabNeg => _immunePrefab ?? (_immunePrefab = Resources.Load<GameObject>("Prefabs/BattlePrefabs/BattleStaticEffects/ImmunePrefabNeg"));

		private static GameObject ImmunePrefabPos => _immunePrefab ?? (_immunePrefab = Resources.Load<GameObject>("Prefabs/BattlePrefabs/BattleStaticEffects/ImmunePrefabPos"));

		public ImmuneAnimation(BitActionAnimation innerAnimation, TriggerType trigger = TriggerType.TurnEnded, int dur = 100)
		{
			_innerAnimation = innerAnimation;
			_trigger = trigger;
			_duration = dur;
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			Animate(monstersAction, onEnded, ArmySide.Left);
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded, ArmySide armySide = ArmySide.Left)
		{
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
			{
				string str = item.Key();
				dictionary.Add(() => str, item.Value);
				if (!(item.Value is FieldMonsterVisual) || item.Value == null)
				{
					Debug.LogError("Trying to attach ImmuneAnimation to something that appears not to be FieldMonsterVisual");
					continue;
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(ImmunePrefab);
				Vector3 localScale = ImmunePrefab.transform.localScale;
				gameObject.transform.localScale = new Vector3((armySide == ArmySide.Left) ? localScale.x : (0f - localScale.x), localScale.y, localScale.z);
				gameObject.transform.parent = item.Value.transform;
				gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
				gameObject.GetComponent<ImmuneStaticAnimation>().Init((FieldMonsterVisual)item.Value, _duration, _trigger);
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
