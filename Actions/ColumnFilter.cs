using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ColumnFilter : BitStaticFilter
	{
		private bool _isFriends = true;

		private bool _ignoreImmune;

		private int _num;

		public ColumnFilter(BitStaticFilter prevFilter = null, int num = 0, bool isFriend = true, bool ignoreImmune = true)
			: base(prevFilter)
		{
			_isFriends = isFriend;
			_ignoreImmune = ignoreImmune;
			_num = num;
		}

		public override bool CheckFilter(Vector2 pos, FieldMonster mon, FieldElement affectedParameter, SkillType skill, BitFilter requester, bool ignoreImmune = true)
		{
			int num = 0;
			if (requester.side == ArmySide.Left)
			{
				if (_isFriends)
				{
					num = _num;
				}
				else
				{
					switch (_num)
					{
					case 2:
						num = 3;
						break;
					case 1:
						num = 4;
						break;
					case 0:
						num = 5;
						break;
					}
				}
			}
			else if (_isFriends)
			{
				switch (_num)
				{
				case 2:
					num = 3;
					break;
				case 1:
					num = 4;
					break;
				case 0:
					num = 5;
					break;
				}
			}
			else
			{
				num = _num;
			}
			if (mon.coords.x == (float)num)
			{
				return base.CheckFilter(pos, mon, affectedParameter, skill, requester, _ignoreImmune);
			}
			return false;
		}
	}
}
