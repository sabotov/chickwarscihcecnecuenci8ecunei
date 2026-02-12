using System;
using System.Collections.Generic;
using System.Linq;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class RandomColumnFilter : BitStaticFilter
	{
		private int _num = 1;

		public RandomColumnFilter(BitStaticFilter prevFilter, int num)
			: base(prevFilter)
		{
			_num = num;
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			List<int> randomLines = new List<int>();
			int num = (((BitStaticFilter.IsEnemySideToPlayEffect ? requester.side.OtherSide() : requester.side) == ArmySide.Right) ? 3 : 0);
			int item = requester.random.GetRange(0, 3) + num;
			randomLines.Add(item);
			if (_num > 1)
			{
				for (int i = 0; i < _num - 1; i++)
				{
					do
					{
						item = new System.Random((int)DateTime.Now.Ticks).Next(0, 3) + num;
					}
					while (randomLines.Contains(item));
					randomLines.Add(item);
				}
			}
			base.ColList = randomLines;
			if (base.GetRightMonsters(affectedParameter, skill, requester).Count() == 0)
			{
				yield break;
			}
			foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in base.GetRightMonsters(affectedParameter, skill, requester))
			{
				if (randomLines.Contains((int)rightMonster.Key.x))
				{
					yield return rightMonster;
				}
			}
		}
	}
}
