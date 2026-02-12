using System;
using System.Collections.Generic;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class PurifyAction : BitAction
	{
		public PurifyAction(BitActionAnimation animation)
			: base(animation)
		{
		}

		public override void PerformAction(IEnumerable<KeyValuePair<Vector2, FieldMonster>> monsters, Action<bool, FieldElement> onCompleted)
		{
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
			{
				Vector2 curElem = monster.Key;
				if (_parameters.GetRunes(_myController.Side).ContainsKey(monster.Key))
				{
					dictionary.Add(delegate
					{
						_myController.DestroyRune(_parameters.GetRunes(_myController.Side)[curElem]);
						return "";
					}, _parameters.GetRunes(_myController.Side)[monster.Key].visualElement);
				}
				else if (_parameters.GetRunes(_myController.EnemySide).ContainsKey(monster.Key))
				{
					dictionary.Add(delegate
					{
						_myController.DestroyRune(_parameters.GetRunes(_myController.EnemySide)[curElem]);
						return "";
					}, _parameters.GetRunes(_myController.EnemySide)[monster.Key].visualElement);
				}
			}
			_animation.Animate(dictionary, delegate
			{
				onCompleted(arg1: true, null);
			});
		}
	}
}
