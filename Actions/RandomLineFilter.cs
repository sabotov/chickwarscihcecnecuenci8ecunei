using System;
using System.Collections.Generic;
using System.Linq;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class RandomLineFilter : BitStaticFilter
	{
		private int _num = 1;

		public override bool monsterFilter => true;

		public RandomLineFilter(BitStaticFilter prevFilter, int num)
			: base(prevFilter)
		{
			_num = num;
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			List<int> randomLines = new List<int>();
			int range = requester.random.GetRange(0, 4);
			randomLines.Add(range);
			if (_num > 1)
			{
				for (int i = 0; i < _num - 1; i++)
				{
					do
					{
						range = new System.Random((int)DateTime.Now.Ticks).Next(0, 4);
					}
					while (randomLines.Contains(range));
					randomLines.Add(range);
				}
			}
			base.LinesList = randomLines;
			if (base.GetRightMonsters(affectedParameter, skill, requester).Count() == 0)
			{
				yield break;
			}
			foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in base.GetRightMonsters(affectedParameter, skill, requester))
			{
				if (randomLines.Contains((int)rightMonster.Key.y))
				{
					yield return rightMonster;
				}
			}
		}
	}
}
