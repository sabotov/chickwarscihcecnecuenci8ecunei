using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class ChangeParamAction : BitAction
	{
		private readonly ParamType _param;

		private readonly ParamIntValueClass _val;

		public ChangeParamAction(BitActionAnimation animation, ParamType param, ParamIntValueClass val)
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
				if (_val.filterMode == ValueFilterMode.PercentForEachMonster)
				{
					curVal = _val.GetValue(_affected, monster.Value);
				}
				FieldMonster cur = monster.Value;
				if (cur == null)
				{
					Debug.LogError(string.Concat("Call Grisha Please               ", _myMonster.coords, " "));
					continue;
				}
				FieldVisual visualElement = monster.Value.visualElement;
				if (curVal != 0)
				{
					dictionary.Add(delegate
					{
						int num = curVal;
						if (_param == ParamType.Health)
						{
							num = cur.PerformDivineShield(num);
						}
						int num2 = cur.ChangeParam(_myMonster as FieldMonster, _param, num);
						return (num < 0) ? string.Concat(-num2) : string.Concat(num2);
					}, visualElement);
				}
				if (aMon == null)
				{
					aMon = monster.Value;
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
