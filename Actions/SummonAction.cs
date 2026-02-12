using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class SummonAction : BitAction
	{
		private MonsterData _monster;

		private ParamIntValueClass _monsterGetter;

		public SummonAction(BitActionAnimation animation, MonsterData monsterToPlace)
			: base(animation)
		{
			_monster = monsterToPlace;
		}

		public SummonAction(BitActionAnimation animation, ParamIntValueClass monsterGetter)
			: base(animation)
		{
			_monsterGetter = monsterGetter;
		}

		public override void Init(FieldElement myMonster, FieldParameters parameters, ArmyControllerCore controller, FieldRandom random, Common.BoolDelegate isRanged)
		{
			base.Init(myMonster, parameters, controller, random, isRanged);
			if (_monsterGetter != null)
			{
				_monsterGetter.Init(myMonster, parameters, random, isRanged);
			}
		}

		public override void PerformAction(IEnumerable<KeyValuePair<Vector2, FieldMonster>> monsters, Action<bool, FieldElement> onCompleted)
		{
			FieldMonster fieldMonster = _myMonster as FieldMonster;
			bool flag = false;
			if (fieldMonster != null && fieldMonster.CheckHasSignature(SkillType.Reborn))
			{
				foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
				{
					if (monster.Key == fieldMonster.coords)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				onCompleted(arg1: false, null);
				return;
			}
			List<Action> actions = new List<Action>();
			FieldMonster monsterParam = null;
			if (_monsterGetter != null)
			{
				monsterParam = _monsterGetter.GetMonsterParam(_affected);
				_monster = monsterParam.data;
				if (_monsterGetter.filterMode == ValueFilterMode.MonsterData)
				{
					monsterParam = null;
				}
			}
			int i = 0;
			foreach (KeyValuePair<Vector2, FieldMonster> monster2 in monsters)
			{
				Vector2 tempPlace = monster2.Key;
				Action item = delegate
				{
					_animation.Animate(new Dictionary<Common.StringDelegate, FieldVisual> { 
					{
						delegate
						{
							_myController.PlaceMonster(_monster, tempPlace, delegate
							{
								i++;
								if (i >= actions.Count)
								{
									onCompleted(arg1: false, null);
								}
								else
								{
									actions[i]();
								}
							}, monsterParam);
							return "";
						},
						null
					} }, delegate
					{
					});
				};
				actions.Add(item);
			}
			if (i >= actions.Count)
			{
				onCompleted(arg1: false, null);
			}
			else
			{
				actions[i]();
			}
		}
	}
}
