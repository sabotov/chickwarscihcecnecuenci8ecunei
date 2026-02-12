using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class VampiricAction : BitAction
	{
		private readonly ParamType _param;

		private readonly ParamIntValueClass _val;

		public VampiricAction(BitActionAnimation animation, ParamType param, ParamIntValueClass val)
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
			foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
			{
				FieldMonster cur = monster.Value;
				FieldVisual visualElement = monster.Value.visualElement;
				dictionary.Add(delegate
				{
					int num = _val.GetValue(_affected);
					if (_param == ParamType.Health)
					{
						num = cur.PerformDivineShield(num);
					}
					int num2 = cur.ChangeParam(_myMonster as FieldMonster, _param, num);
					if (_myMonster is FieldMonster fieldMonster)
					{
						fieldMonster.ChangeParam(fieldMonster, _param, -num2);
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
