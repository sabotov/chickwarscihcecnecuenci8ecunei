using System.Linq;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class MonsterIdFilter : BitStaticFilter
	{
		private int[] _id;

		private bool _ignoreImmune;

		public MonsterIdFilter(BitStaticFilter prevFilter = null, int[] id = null, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_id = id;
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune))
			{
				return _id.Contains(mon.data.monster_id);
			}
			return false;
		}
	}
}
