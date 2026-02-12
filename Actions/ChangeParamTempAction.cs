using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class ChangeParamTempAction : BitAction
	{
		private ParamType _param;

		private ParamIntValueClass _val;

		private TriggerType _trigger;

		private int _count;

		public ChangeParamTempAction(BitActionAnimation animation, ParamType param, ParamIntValueClass val, TriggerType trigger, int count)
			: base(animation)
		{
			_param = param;
			_val = val;
			_trigger = trigger;
			_count = count;
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
			foreach (KeyValuePair<Vector2, FieldMonster> mon in monsters)
			{
				FieldMonster cur = mon.Value;
				FieldVisual visualElement = mon.Value.visualElement;
				int curVal = 0;
				if (_val.filterMode != ValueFilterMode.PercentForEachMonster)
				{
					curVal = _val.GetValue(_affected);
				}
				dictionary.Add(delegate
				{
					if (_val.filterMode == ValueFilterMode.PercentForEachMonster)
					{
						curVal = _val.GetValue(_affected, mon.Value);
					}
					return string.Concat(cur.Buff(_myMonster as FieldMonster, _param, curVal, _trigger, _count));
				}, visualElement);
				if (aMon == null)
				{
					aMon = mon.Value;
				}
			}
			_animation.Animate(dictionary, delegate
			{
				onCompleted(arg1: true, aMon);
			});
		}
	}
}
