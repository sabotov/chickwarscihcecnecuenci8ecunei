using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class NoRaceFilter : BitStaticFilter
	{
		private Race _race;

		private bool _ignoreImmune;

		public NoRaceFilter(Race race, BitStaticFilter prevFilter = null, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_race = race;
			_ignoreImmune = ignoreImmune;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			if (base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune))
			{
				return mon.data.staticInnerData.race != _race;
			}
			return false;
		}
	}
}
