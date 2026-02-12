using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class AddWarlordSkillAction : BitAction
	{
		private SkillStaticData _skill;

		private string _skillValue;

		private ParamIntValueClass _val;

		public AddWarlordSkillAction(BitActionAnimation animation, SkillStaticData skill, string value, ParamIntValueClass val = null)
			: base(animation)
		{
			_skill = skill;
			_skillValue = value;
			_val = val;
		}

		public override void Init(FieldElement myMonster, FieldParameters parameters, ArmyControllerCore controller, FieldRandom random, Common.BoolDelegate isRanged)
		{
			base.Init(myMonster, parameters, controller, random, isRanged);
			if (_val != null)
			{
				_val.Init(myMonster, parameters, random, isRanged);
			}
		}

		public override void PerformAction(IEnumerable<KeyValuePair<Vector2, FieldMonster>> monsters, Action<bool, FieldElement> onCompleted)
		{
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			FieldElement aMon = null;
			foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
			{
				FieldMonster cur = monster.Value;
				FieldVisual curVisual = monster.Value.visualElement;
				if (!cur.data.is_warlord)
				{
					continue;
				}
				string skillValStr = _skillValue;
				if (_val != null)
				{
					int result2;
					if (_val.filterMode == ValueFilterMode.Multiplier)
					{
						if (int.TryParse(skillValStr, out var result))
						{
							result *= _val.GetValueMultiplier(_affected);
							if (result <= 0)
							{
								continue;
							}
							skillValStr = result.ToString();
						}
					}
					else if (int.TryParse(skillValStr, out result2))
					{
						result2 *= _val.GetValue(_affected);
						if (result2 <= 0)
						{
							continue;
						}
						skillValStr = result2.ToString();
					}
				}
				dictionary.Add(delegate
				{
					cur.AddWarlordSkill(_skill, skillValStr);
					if (curVisual as FieldMonsterVisual != null)
					{
						(curVisual as FieldMonsterVisual).SetFlyingStatus();
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
