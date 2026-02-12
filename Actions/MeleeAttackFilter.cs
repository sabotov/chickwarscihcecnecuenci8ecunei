using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class MeleeAttackFilter : RangedAttackFilter
	{
		public MeleeAttackFilter(BitStaticFilter prevFilter = null)
			: base(prevFilter)
		{
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			Vector2 pos = requester.placeDelegate();
			foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in base.GetRightMonsters(affectedParameter, skill, requester))
			{
				int num = (int)Mathf.Sign(rightMonster.Key.x - pos.x);
				bool flag = false;
				foreach (KeyValuePair<Vector2, FieldMonster> monster in requester.parameters.GetMonsters(requester.side))
				{
					if (monster.Key.y == pos.y && monster.Key.x * (float)num > pos.x * (float)num)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					yield return rightMonster;
				}
			}
		}
	}
}
