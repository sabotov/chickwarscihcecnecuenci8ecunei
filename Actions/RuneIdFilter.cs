using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class RuneIdFilter : BitStaticFilter
	{
		private int _id;

		private bool _ignoreImmune;

		public override bool monsterFilter => false;

		public RuneIdFilter(BitStaticFilter prevFilter = null, int id = 0, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_id = id;
			_ignoreImmune = ignoreImmune;
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			if (_lowerFilter != null)
			{
				foreach (KeyValuePair<Vector2, FieldMonster> elem in base.GetRightMonsters(affectedParameter, skill, requester))
				{
					if (!requester.parameters.GetRunes(requester.side).ContainsKey(elem.Key))
					{
						continue;
					}
					Dictionary<Vector2, FieldRune> runes = requester.parameters.GetRunes(requester.side);
					if (runes.Count == 0)
					{
						continue;
					}
					foreach (KeyValuePair<Vector2, FieldRune> item in runes)
					{
						if (item.Value.data.id == _id)
						{
							yield return elem;
						}
					}
				}
				yield break;
			}
			foreach (Vector2 item2 in requester.parameters.AllEmptyPlaces())
			{
				if (!requester.parameters.GetRunes(requester.side).ContainsKey(item2))
				{
					continue;
				}
				Dictionary<Vector2, FieldRune> runes2 = requester.parameters.GetRunes(requester.side);
				if (runes2.Count == 0)
				{
					continue;
				}
				foreach (KeyValuePair<Vector2, FieldRune> item3 in runes2)
				{
					if (item3.Value.data.id == _id)
					{
						yield return new KeyValuePair<Vector2, FieldMonster>(item3.Key, null);
					}
				}
			}
		}
	}
}
