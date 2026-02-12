using System;
using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class KillStealAction : BitAction
	{
		private SkillType _skill;

		private bool _stealAttack;

		private bool _stealHp;

		private bool _stealSkills;

		private bool _kill;

		private bool _inversed;

		public KillStealAction(BitActionAnimation animation, SkillType skill)
			: base(animation)
		{
			_skill = skill;
			_stealAttack = skill == SkillType.KillStealAttack || skill == SkillType.KillStealStats || skill == SkillType.KillStealAll || skill == SkillType.KillStealHpFromAttack;
			_stealHp = skill == SkillType.KillStealHealth || skill == SkillType.KillStealStats || skill == SkillType.KillStealAll || skill == SkillType.KillStealAttackFromHp;
			_stealSkills = skill == SkillType.StealSkills || skill == SkillType.KillStealAll || skill == SkillType.KillStealSkills;
			_kill = skill != SkillType.StealSkills;
			_inversed = skill == SkillType.KillStealHpFromAttack || skill == SkillType.KillStealAttackFromHp;
		}

		public override void PerformAction(IEnumerable<KeyValuePair<Vector2, FieldMonster>> monsters, Action<bool, FieldElement> onCompleted)
		{
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			FieldElement aMon = null;
			FieldMonster myMonster = _myMonster as FieldMonster;
			if (myMonster == null)
			{
				Debug.LogError("Kill steal skill found on something that is not a FieldMonster!");
				onCompleted(arg1: false, null);
				return;
			}
			foreach (KeyValuePair<Vector2, FieldMonster> monster in monsters)
			{
				FieldMonster cur = monster.Value;
				if (cur == null)
				{
					Debug.LogError(string.Concat("Call Grisha Please               ", _myMonster.coords, " "));
					continue;
				}
				FieldMonsterVisual visualMonster = cur.VisualMonster;
				dictionary.Add(delegate
				{
					if (!_kill || !cur.PerformDivineShield(_skill))
					{
						if (_stealAttack && cur.Attack > 0)
						{
							myMonster.Buff(myMonster, _inversed ? ParamType.Health : ParamType.Attack, cur.Attack, TriggerType.NoTrigger, -1);
						}
						if (_stealHp)
						{
							myMonster.Buff(myMonster, (!_inversed) ? ParamType.Health : ParamType.Attack, cur.Health, TriggerType.NoTrigger, -1);
						}
						if (_stealSkills)
						{
							myMonster.CopySkills(cur);
						}
						if (_kill)
						{
							cur.Kill();
						}
					}
					return "";
				}, visualMonster);
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
