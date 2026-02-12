using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class StealAction : BitAction
	{
		private readonly ParamType _param;

		private readonly ParamIntValueClass _val;

		public StealAction(BitActionAnimation animation, ParamType param, ParamIntValueClass val)
			: base(animation)
		{
			_param = param;
			_val = val;
		}

		public override void Init(FieldElement myMonster, FieldParameters parameters, ArmyControllerCore controller, FieldRandom random, Common.BoolDelegate isRanged)
		{
			base.Init(myMonster, parameters, controller, random, isRanged);
			_val.Init(myMonster, parameters, random, isRanged);
		}

		public override void PerformAction(IEnumerable<KeyValuePair<Vector2, FieldMonster>> monsters, Action<bool, FieldElement> onCompleted)
		{
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			FieldElement aMon = null;
			int curVal = 0;
			if (_val.filterMode != ValueFilterMode.PercentForEachMonster)
			{
				curVal = _val.GetValue(_affected);
			}
			foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
			{
				FieldMonster cur = monster.Value;
				FieldVisual visualElement = monster.Value.visualElement;
				if (_val.filterMode == ValueFilterMode.PercentForEachMonster)
				{
					curVal = _val.GetValue(_affected, monster.Value);
				}
				dictionary.Add(delegate
				{
					int num = cur.Buff(_myMonster as FieldMonster, _param, curVal, TriggerType.NoTrigger, -1);
					if (_myMonster is FieldMonster fieldMonster)
					{
						fieldMonster.Buff(fieldMonster, _param, -num, TriggerType.NoTrigger, -1);
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
