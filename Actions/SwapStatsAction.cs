using System;
using System.Collections.Generic;
using NewAssets.Scripts.DataClasses.MonsterParams;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class SwapStatsAction : BitAction
	{
		public SwapStatsAction(BitActionAnimation animation)
			: base(animation)
		{
		}

		public override void PerformAction(IEnumerable<KeyValuePair<Vector2, FieldMonster>> monsters, Action<bool, FieldElement> onCompleted)
		{
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			FieldElement aMon = null;
			foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
			{
				FieldMonster cur = monster.Value;
				if (cur == null)
				{
					Debug.LogError(string.Concat("Call Grisha Please               ", _myMonster.coords, " "));
				}
				else
				{
					if (cur.ShouldDie)
					{
						continue;
					}
					FieldVisual visualElement = monster.Value.visualElement;
					FieldMonsterVisual curMonsterVisual = monster.Value.VisualMonster;
					Common.VoidDelegate dataChangeParams = delegate
					{
						int attack = cur.Attack;
						int delta = cur.Health;
						if (cur.data.monsterClass != Class.Building)
						{
							cur.ForceSetParam(_myMonster as FieldMonster, ParamType.Attack, delta, TriggerType.NoTrigger, 0);
							cur.ForceSetParam(_myMonster as FieldMonster, ParamType.Health, attack, TriggerType.NoTrigger, 0);
						}
						else
						{
							cur.ForceSetParam(_myMonster as FieldMonster, ParamType.Health, 0, TriggerType.NoTrigger, 0);
						}
					};
					dictionary.Add(delegate
					{
						if (curMonsterVisual != null)
						{
							curMonsterVisual.SetHpAttackTogether(together: true, delegate
							{
								dataChangeParams();
								curMonsterVisual.SetHpAttackTogether(together: false);
							});
						}
						else
						{
							dataChangeParams();
						}
						return "swapped";
					}, visualElement);
					if (aMon == null)
					{
						aMon = monster.Value;
					}
				}
			}
			if (dictionary.Count == 0)
			{
				onCompleted(arg1: false, null);
				return;
			}
			_animation.Animate(dictionary, delegate
			{
				onCompleted(arg1: true, aMon);
			});
		}
	}
}
