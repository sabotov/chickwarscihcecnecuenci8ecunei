using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class SetParamTempAction : BitAction
	{
		private ParamType _param;

		private ParamIntValueClass _val;

		private TriggerType _trigger;

		private int _count;

		public SetParamTempAction(BitActionAnimation animation, ParamType param, ParamIntValueClass val, TriggerType trigger, int count)
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
			foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
			{
				FieldMonster cur = monster.Value;
				FieldVisual visualElement = monster.Value.visualElement;
				dictionary.Add(delegate
				{
					cur.ForceSetParam(_myMonster as FieldMonster, _param, _val.GetValue(_affected), _trigger, _count);
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
