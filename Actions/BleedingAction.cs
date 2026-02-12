using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class BleedingAction : BitAction
	{
		private readonly ParamIntValueClass _val;

		public BleedingAction(BitActionAnimation animation, ParamIntValueClass val)
			: base(animation)
		{
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
						curVal = cur.PerformDivineShield(curVal);
						int num = cur.ChangeParam(_myMonster as FieldMonster, ParamType.Health, curVal);
						return (curVal < 0) ? string.Concat(-num) : string.Concat(num);
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
