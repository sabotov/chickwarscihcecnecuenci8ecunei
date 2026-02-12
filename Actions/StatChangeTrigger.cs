using System;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class StatChangeTrigger : BitStaticTrigger
	{
		private string _stat;

		private string _comparison;

		private int _value;

		private bool _isPercentValue;

		private void Init(string paramType, string paramValue)
		{
			_stat = paramType;
			if (_stat != "no_shield")
			{
				int startIndex = 2;
				_comparison = paramValue.Substring(0, 2);
				if (_comparison != ">=" && _comparison != "<=")
				{
					startIndex = 1;
					_comparison = paramValue.Substring(0, 1);
					if (_comparison != ">" && _comparison != "<" && _comparison != "=")
					{
						throw new Exception("Invalid comparison type " + _comparison);
					}
				}
				string text = paramValue.Substring(startIndex);
				_isPercentValue = text.Contains("%");
				if (!int.TryParse(paramValue.Substring(startIndex).Replace("%", ""), out _value))
				{
					throw new Exception("Cant parse comparison value " + paramValue);
				}
			}
			else
			{
				_comparison = "";
				_value = 0;
				_isPercentValue = false;
			}
		}

		public StatChangeTrigger(string paramType, string paramValue, BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
			Init(paramType, paramValue);
		}

		public StatChangeTrigger(string paramType, string paramValue, TriggerType trigger)
			: base(trigger)
		{
			Init(paramType, paramValue);
		}

		private float GetPercentValue(int current, int max)
		{
			if (max == 0)
			{
				if (current == 0)
				{
					return 0f;
				}
				return float.MaxValue;
			}
			return (float)current / (float)max;
		}

		private bool CheckMonster(FieldMonster mon)
		{
			string stat = _stat;
			if (!(stat == "atk"))
			{
				if (stat == "hp")
				{
					float num = (_isPercentValue ? ((float)(int)mon.Health / (float)(int)mon.MaxHealth) : ((float)(int)mon.Health));
					float num2 = (_isPercentValue ? ((float)_value / 100f) : ((float)_value));
					switch (_comparison)
					{
					case "<":
						return num < num2;
					case ">":
						return num > num2;
					case "=":
						return num == num2;
					case ">=":
						return num >= num2;
					case "<=":
						return num <= num2;
					default:
						return false;
					}
				}
				return false;
			}
			float num3 = (_isPercentValue ? GetPercentValue(mon.Attack, mon.data.attack) : ((float)mon.Attack));
			float num4 = (_isPercentValue ? ((float)_value / 100f) : ((float)_value));
			switch (_comparison)
			{
			case "<":
				return num3 < num4;
			case ">":
				return num3 > num4;
			case "=":
				return num3 == num4;
			case ">=":
				return num3 >= num4;
			case "<=":
				return num3 <= num4;
			default:
				return false;
			}
		}

		private bool CheckMonster(FieldMonster mon, object param)
		{
			if (mon.ShouldDie && _stat != "no_shield")
			{
				return false;
			}
			if (!(param is StatChangeData statChangeData))
			{
				return false;
			}
			switch (_stat)
			{
			case "no_shield":
				if (statChangeData.prevSnapshot.hasDivineShield)
				{
					return !statChangeData.curSnapshot.hasDivineShield;
				}
				return false;
			case "atk":
			{
				float num4 = (_isPercentValue ? GetPercentValue(statChangeData.prevSnapshot.attackSnapshot, mon.data.attack) : ((float)statChangeData.prevSnapshot.attackSnapshot));
				float num5 = (_isPercentValue ? GetPercentValue(statChangeData.curSnapshot.attackSnapshot, mon.data.attack) : ((float)statChangeData.curSnapshot.attackSnapshot));
				float num6 = (_isPercentValue ? ((float)_value / 100f) : ((float)_value));
				switch (_comparison)
				{
				case "<":
					if (num5 < num6)
					{
						return num4 >= num6;
					}
					return false;
				case ">":
					if (num5 > num6)
					{
						return num4 <= num6;
					}
					return false;
				case "=":
					if (num5 == num6)
					{
						return num4 != num6;
					}
					return false;
				case ">=":
					if (num5 >= num6)
					{
						return num4 < num6;
					}
					return false;
				case "<=":
					if (num5 <= num6)
					{
						return num4 > num6;
					}
					return false;
				default:
					return false;
				}
			}
			case "hp":
			{
				float num = (_isPercentValue ? statChangeData.prevSnapshot.hpPercentSnapshot : ((float)statChangeData.prevSnapshot.hpSnapshot));
				float num2 = (_isPercentValue ? statChangeData.curSnapshot.hpPercentSnapshot : ((float)statChangeData.curSnapshot.hpSnapshot));
				float num3 = (_isPercentValue ? ((float)_value / 100f) : ((float)_value));
				switch (_comparison)
				{
				case "<":
					if (num2 < num3)
					{
						return num >= num3;
					}
					return false;
				case ">":
					if (num2 > num3)
					{
						return num <= num3;
					}
					return false;
				case "=":
					if (num2 == num3)
					{
						return num != num3;
					}
					return false;
				case ">=":
					if (num2 >= num3)
					{
						return num < num3;
					}
					return false;
				case "<=":
					if (num2 <= num3)
					{
						return num > num3;
					}
					return false;
				default:
					return false;
				}
			}
			default:
				return false;
			}
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
			{
				if (trigger != TriggerType.StatChange)
				{
					return CheckMonster(monster as FieldMonster);
				}
				return CheckMonster(monster as FieldMonster, param);
			}
			return false;
		}
	}
}
