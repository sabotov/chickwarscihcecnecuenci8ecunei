using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class BitAction
	{
		protected FieldElement _myMonster;

		protected ArmyControllerCore _myController;

		protected FieldParameters _parameters;

		protected FieldElement _affected;

		protected BitActionAnimation _animation;

		public virtual bool ShouldCheckIfFilterNotEmpty => true;

		public BitActionAnimation Animation => _animation;

		public BitAction(BitActionAnimation animation)
		{
			_animation = animation;
		}

		public FieldElement GetMonster()
		{
			return _myMonster;
		}

		public virtual void Init(FieldElement myMonster, FieldParameters parameters, ArmyControllerCore controller, FieldRandom random, Common.BoolDelegate isRanged)
		{
			_myController = controller;
			_myMonster = myMonster;
			_parameters = parameters;
			if (myMonster is FieldMonster fieldMonster)
			{
				_animation.Init(fieldMonster.visualElement);
			}
		}

		public void SetAffected(FieldElement affected)
		{
			_affected = affected;
		}

		public virtual void PerformAction(IEnumerable<KeyValuePair<Vector2, FieldMonster>> monsters, Action<bool, FieldElement> onCompleted)
		{
			_animation.Animate(new Dictionary<Common.StringDelegate, FieldVisual>(), delegate
			{
				onCompleted(arg1: true, null);
			});
		}
	}
}
