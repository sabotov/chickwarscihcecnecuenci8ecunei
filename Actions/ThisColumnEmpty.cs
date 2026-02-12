using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ThisColumnEmpty : BitStaticFilter
	{
		public override bool monsterFilter => false;

		public ThisColumnEmpty(BitStaticFilter prevFilter = null)
			: base(prevFilter)
		{
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			if (_lowerFilter != null)
			{
				foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in base.GetRightMonsters(affectedParameter, skill, requester))
				{
					if (rightMonster.Key.x == requester.placeDelegate().x)
					{
						yield return rightMonster;
					}
				}
				yield break;
			}
			foreach (Vector2 item in requester.parameters.AllEmptyPlaces())
			{
				if (item.x == requester.placeDelegate().x)
				{
					yield return new KeyValuePair<Vector2, FieldMonster>(item, null);
				}
			}
		}
	}
}
