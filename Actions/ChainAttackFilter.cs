using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ChainAttackFilter : BitStaticFilter
	{
		private int _distance = 1;

		public ChainAttackFilter(BitStaticFilter prevFilter = null, int distance = 1)
			: base(prevFilter)
		{
			_distance = distance;
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in base.GetRightMonsters(affectedParameter, skill, requester))
			{
				yield return rightMonster;
			}
		}
	}
}
