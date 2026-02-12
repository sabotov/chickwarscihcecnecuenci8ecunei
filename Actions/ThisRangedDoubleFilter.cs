using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ThisRangedDoubleFilter : BitStaticFilter
	{
		private BitStaticFilter _filter1;

		private BitStaticFilter _filter2;

		public ThisRangedDoubleFilter(BitStaticFilter filter1, BitStaticFilter filter2)
			: base(filter1)
		{
			_filter1 = filter1;
			_filter2 = filter2;
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			if (requester.isRanged())
			{
				return _filter1.GetRightMonsters(affectedParameter, skill, requester);
			}
			return _filter2.GetRightMonsters(affectedParameter, skill, requester);
		}
	}
}
