using System;
using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class SilenceAction : BitAction
	{
		private SkillType _skill;

		public SilenceAction(BitActionAnimation animation, SkillType skill)
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
				FieldVisual curVisual = monster.Value.visualElement;
				dictionary.Add(delegate
				{
					if (_skill == SkillType.NoSkill)
					{
						cur.Silence();
						if (curVisual as FieldMonsterVisual != null)
						{
							(curVisual as FieldMonsterVisual).SetFlyingStatus();
						}
					}
					else
					{
						cur.Silence(_skill);
						if (curVisual as FieldMonsterVisual != null)
						{
							(curVisual as FieldMonsterVisual).SetFlyingStatus();
						}
					}
					return "";
				}, curVisual);
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
