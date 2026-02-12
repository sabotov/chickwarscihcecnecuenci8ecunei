using System.Collections.Generic;
using System.Linq;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class RandomRunesEmptyFilter : BitStaticFilter
	{
		private readonly int _count;

		public override bool monsterFilter => false;

		public RandomRunesEmptyFilter(BitStaticFilter prevFilter = null, int count = 1)
			: base(prevFilter)
		{
			_count = count;
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			int num = requester.parameters.AllRunesAndEmptyPlaces().Count();
			if (num == 0)
			{
				yield break;
			}
			List<int> randNums = new List<int>();
			for (int i = 0; i < _count && num - i >= 0; i++)
			{
				int j;
				for (j = requester.random.GetRange(0, num - i); randNums.Contains(j); j++)
				{
				}
				randNums.Add(j);
			}
			int counter = 0;
			foreach (Vector2 item in requester.parameters.AllRunesAndEmptyPlaces())
			{
				if (randNums.Contains(counter))
				{
					yield return new KeyValuePair<Vector2, FieldMonster>(item, null);
				}
				counter++;
			}
		}
	}
}
