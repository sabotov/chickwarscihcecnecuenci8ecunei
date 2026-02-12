using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class WarlordFilter : BitStaticFilter
	{
		public WarlordFilter(BitStaticFilter prevFilter = null)
			: base(prevFilter)
		{
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			FieldMonster warlord = requester.parameters.GetWarlord(requester.enemySide);
			FieldMonster fWarlord = requester.parameters.GetWarlord(requester.side);
			yield return new KeyValuePair<Vector2, FieldMonster>(warlord.coords, warlord);
			yield return new KeyValuePair<Vector2, FieldMonster>(fWarlord.coords, fWarlord);
		}
	}
}
