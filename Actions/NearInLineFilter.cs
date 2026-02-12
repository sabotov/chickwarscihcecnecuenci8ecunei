using System.Collections.Generic;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class NearInLineFilter : BitStaticFilter
	{
		private int _xShift;

		private int _yShift;

		public override bool monsterFilter => false;

		public NearInLineFilter(BitStaticFilter prevFilter = null, int xShift = 0, int yShift = 0)
			: base(prevFilter)
		{
			_xShift = xShift;
			_yShift = yShift;
		}

		public override IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters(FieldElement affectedParameter, SkillType skill, BitFilter requester)
		{
			Vector2 vector = requester.placeDelegate();
			vector = ((requester.parameters.GetWarlord(requester.side).Side != ArmySide.Right) ? new Vector2(vector.x + (float)_xShift, vector.y + (float)_yShift) : new Vector2(vector.x - (float)_xShift, vector.y + (float)_yShift));
			if (!requester.parameters.GetMonsters(requester.side).ContainsKey(vector) && !requester.parameters.GetMonsters(requester.enemySide).ContainsKey(vector))
			{
				yield return new KeyValuePair<Vector2, FieldMonster>(vector, null);
			}
		}
	}
}
