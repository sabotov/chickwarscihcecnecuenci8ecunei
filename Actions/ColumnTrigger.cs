using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ColumnTrigger : BitStaticTrigger
	{
		private int _colNum;

		private bool _isFriends = true;

		public ColumnTrigger(TriggerType trigger, int num = 0, bool fr = true)
			: base(trigger)
		{
			_colNum = num;
			_isFriends = fr;
		}

		public ColumnTrigger(BitStaticTrigger lowerTrigger = null, int num = 0, bool fr = true)
			: base(lowerTrigger)
		{
			_colNum = num;
			_isFriends = fr;
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			int num = 0;
			if (requester.side == ArmySide.Left)
			{
				if (_isFriends)
				{
					num = _colNum;
				}
				else
				{
					switch (_colNum)
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
				switch (_colNum)
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
				num = _colNum;
			}
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param))
			{
				return monster.coords.x == (float)num;
			}
			return false;
		}
	}
}
