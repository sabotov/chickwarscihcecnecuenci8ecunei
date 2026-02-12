using System;
using System.Collections.Generic;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class HealToMaxAction : BitAction
	{
		public HealToMaxAction(BitActionAnimation animation)
			: base(animation)
		{
		}

		public override void PerformAction(IEnumerable<KeyValuePair<Vector2, FieldMonster>> monsters, Action<bool, FieldElement> onCompleted)
		{
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			FieldElement aMon = null;
			foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
			{
				FieldMonster cur = monster.Value;
				if (cur == null)
				{
					Debug.LogError(string.Concat("Call Grisha Please               ", _myMonster.coords, " "));
					continue;
				}
				FieldVisual visualElement = monster.Value.visualElement;
				int value = (int)cur.MaxHealth - (int)cur.Health;
				if (value != 0)
				{
					dictionary.Add(delegate
					{
						cur.ChangeParam(_myMonster as FieldMonster, ParamType.Health, cur.MaxHealth);
						return string.Concat(value);
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
