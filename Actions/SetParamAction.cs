using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class SetParamAction : BitAction
	{
		private readonly ParamType _param;

		private readonly ParamIntValueClass _val;

		public SetParamAction(BitActionAnimation animation, ParamType param, ParamIntValueClass val)
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
			int value = _val.GetValue(_affected);
			foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
			{
				int curVal = value;
				FieldMonster cur = monster.Value;
				FieldVisual visualElement = cur.visualElement;
				if (curVal != 0)
				{
					dictionary.Add(delegate
					{
						if (_param == ParamType.Health)
						{
							curVal = cur.PerformDivineShield(curVal);
						}
						curVal = cur.ForceSetParam(_myMonster as FieldMonster, _param, curVal, TriggerType.NoTrigger, 0);
						return (curVal < 0) ? string.Concat(-curVal) : string.Concat(curVal);
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
