using System;
using System.Collections.Generic;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class DivineShieldAnimation : BitActionAnimation
	{
		private static GameObject _shieldPreffab;

		private static GameObject _shieldWarlordPreffab;

		private readonly BitActionAnimation _innerAnimation;

		private static GameObject ShieldPrefab => _shieldPreffab ?? (_shieldPreffab = Resources.Load<GameObject>("Prefabs/BattlePrefabs/BattleStaticEffects/DivineShieldPrefab"));

		private static GameObject ShieldWarlordPrefab => _shieldWarlordPreffab ?? (_shieldWarlordPreffab = Resources.Load<GameObject>("Prefabs/BattlePrefabs/BattleStaticEffects/DivineShieldWarlordPrefab"));

		public DivineShieldAnimation(BitActionAnimation innerAnimation)
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
				FieldMonsterVisual fieldMonsterVisual = item.Value as FieldMonsterVisual;
				if (fieldMonsterVisual == null || item.Value == null)
				{
					Debug.LogError("Trying to attach DivineShieldAnimation to something that appears not to be FieldMonsterVisual");
					continue;
				}
				GameObject obj = (fieldMonsterVisual.IsWarlord ? UnityEngine.Object.Instantiate(ShieldWarlordPrefab) : UnityEngine.Object.Instantiate(ShieldPrefab));
				obj.transform.localScale = ShieldPrefab.transform.localScale;
				obj.transform.parent = item.Value.transform;
				obj.transform.localPosition = new Vector3(0f, 0f, -1f);
				DivineShieldStaticAnimation component = obj.GetComponent<DivineShieldStaticAnimation>();
				FieldMonsterVisual monster = (FieldMonsterVisual)item.Value;
				component.Init(monster);
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
