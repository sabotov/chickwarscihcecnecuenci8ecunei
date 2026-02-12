using System;
using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class RemoveSkillAction : BitAction
	{
		private SkillStaticData _skill;

		public RemoveSkillAction(BitActionAnimation animation, SkillStaticData skill)
			: base(animation)
		{
			_skill = skill;
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
					cur.RemoveSkillOrPerk(_skill);
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
