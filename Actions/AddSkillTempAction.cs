using System;
using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class AddSkillTempAction : BitAction
	{
		private SkillStaticData _skill;

		private string _skillValue;

		private TriggerType _trigger;

		private int _count;

		public AddSkillTempAction(BitActionAnimation animation, SkillStaticData skill, string value, TriggerType trigger, int count)
			: base(animation)
		{
			_skill = skill;
			_skillValue = value;
			_trigger = trigger;
			_count = count;
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
					cur.AddSkillTemporary(_skill, _skillValue, _trigger, _count);
					return string.Concat(_count);
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
