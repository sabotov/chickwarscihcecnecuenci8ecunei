using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using NGUI.Scripts.Internal;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class CounterAttackAction : BitAction
	{
		private readonly ParamIntValueClass _val;

		private readonly Common.BoolDelegate _rangeDelegate;

		private readonly BitActionAnimation _meleeAnimation;

		private readonly BitActionAnimation _rangedAnimation;

		public CounterAttackAction(BitActionAnimation meleeAnimation, BitActionAnimation rangedAnimation, ParamIntValueClass val, Common.BoolDelegate rangeDelegate)
			: base(meleeAnimation)
		{
			_meleeAnimation = meleeAnimation;
			_rangedAnimation = rangedAnimation;
			_rangeDelegate = rangeDelegate;
			_val = val;
		}

		public override void Init(FieldElement myMonster, FieldParameters parameters, ArmyControllerCore controller, FieldRandom random, Common.BoolDelegate isRanged)
		{
			base.Init(myMonster, parameters, controller, random, isRanged);
			if (myMonster is FieldMonster fieldMonster)
			{
				_meleeAnimation.Init(fieldMonster.visualElement);
				_rangedAnimation.Init(fieldMonster.visualElement);
			}
			_val.Init(myMonster, parameters, random, isRanged);
		}

		public override void PerformAction(IEnumerable<KeyValuePair<Vector2, FieldMonster>> monsters, Action<bool, FieldElement> onCompleted)
		{
			int value = _val.GetValue(_affected);
			if (!((FieldMonster)_myMonster).canAttack || !((FieldMonster)_myMonster).CanCounter)
			{
				onCompleted(arg1: false, null);
				return;
			}
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			FieldElement aMon = null;
			bool IHaveMiss = ((FieldMonster)_myMonster).HaveMiss;
			foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
			{
				FieldMonster cur = monster.Value;
				FieldVisual visualElement = monster.Value.visualElement;
				dictionary.Add(delegate
				{
					cur.CanCounter = false;
					if (cur.PerformEvade(_myMonster as FieldMonster, _rangeDelegate(), value) || IHaveMiss)
					{
						return Localization.Localize("#attack_miss");
					}
					int damage = cur.PerformDivineShield(value);
					int delta = cur.PerformBlock(damage);
					return string.Concat(-cur.ChangeParam(_myMonster as FieldMonster, ParamType.Health, delta));
				}, visualElement);
				if (aMon == null)
				{
					aMon = monster.Value;
				}
			}
			if (_rangeDelegate())
			{
				_rangedAnimation.Animate(dictionary, delegate
				{
					onCompleted(!IHaveMiss, aMon);
				});
			}
			else
			{
				_meleeAnimation.Animate(dictionary, delegate
				{
					onCompleted(!IHaveMiss, aMon);
				});
			}
		}
	}
}
