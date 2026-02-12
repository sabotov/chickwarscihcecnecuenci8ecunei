using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class RangedAttackFilter : BitStaticFilter
	{
		public RangedAttackFilter(BitStaticFilter prevFilter = null)
			: base(prevFilter)
		{
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			Vector2 vector = requester.placeDelegate();
			Vector2 key = new Vector2(-1000f, -1000f);
			if (requester.parameters.GetMonsters(requester.enemySide).Count == 0)
			{
				FieldMonster warlord = requester.parameters.GetWarlord(requester.enemySide);
				yield return new KeyValuePair<Vector2, FieldMonster>(warlord.coords, warlord);
				yield break;
			}
			FieldMonster fieldMonster = null;
			foreach (KeyValuePair<Vector2, FieldMonster> monster in requester.parameters.GetMonsters(requester.enemySide))
			{
				if (monster.Key.y == vector.y && Mathf.Abs(monster.Key.x - vector.x) < Mathf.Abs(key.x - vector.x))
				{
					key = monster.Key;
					fieldMonster = monster.Value;
				}
			}
			if (fieldMonster != null)
			{
				if (fieldMonster.IsNotImmune(SkillType.Attack))
				{
					yield return new KeyValuePair<Vector2, FieldMonster>(key, fieldMonster);
				}
				yield break;
			}
			fieldMonster = requester.parameters.GetWarlord(requester.enemySide);
			if (fieldMonster != null && fieldMonster.IsNotImmune(SkillType.Attack))
			{
				yield return new KeyValuePair<Vector2, FieldMonster>(fieldMonster.coords, fieldMonster);
			}
		}
	}
}
