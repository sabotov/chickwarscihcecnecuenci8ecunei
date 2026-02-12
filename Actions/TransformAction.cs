using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class TransformAction : BitAction
	{
		private Func<MonsterData> _transformTo;

		private ParamIntValueClass _monsterGetter;

		public TransformAction(BitActionAnimation animation, Func<MonsterData> transformTo)
			: base(animation)
		{
			_transformTo = transformTo;
		}

		public TransformAction(BitActionAnimation animation, ParamIntValueClass monsterGetter)
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
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			FieldElement aMon = null;
			foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
			{
				if (monster.Value.ShouldDie)
				{
					continue;
				}
				FieldMonster cur = monster.Value;
				FieldVisual visualElement = monster.Value.visualElement;
				FieldMonster monsterParam = null;
				if (_monsterGetter != null)
				{
					monsterParam = _monsterGetter.GetMonsterParam(_affected);
					if (monsterParam == null)
					{
						continue;
					}
				}
				dictionary.Add(delegate
				{
					if (_monsterGetter == null)
					{
						FieldMonsterVisual visualMonster = cur.VisualMonster;
						if ((bool)visualMonster)
						{
							cur.Transform(_transformTo(), visualMonster.tweenOutTime);
						}
						else
						{
							cur.Transform(_transformTo());
						}
						return "";
					}
					cur.Transform(monsterParam.data);
					if (_monsterGetter.filterMode == ValueFilterMode.Monster)
					{
						cur.CopyStatus(monsterParam);
					}
					return "";
				}, visualElement);
				if (aMon == null)
				{
					aMon = monster.Value;
				}
			}
			_animation.Animate(dictionary, delegate
			{
				onCompleted(arg1: true, aMon);
			});
		}
	}
}
