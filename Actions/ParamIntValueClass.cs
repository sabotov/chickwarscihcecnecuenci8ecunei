using System;
using System.Collections.Generic;
using System.Linq;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class ParamIntValueClass
	{
		public ValueFilterMode filterMode;

		private readonly ParamIntDelegate _paramDelegate;

		private readonly Func<int, int> _turnBasedDelegate;

		private BitFilter _multiplierFilter;

		private ArmySide _side;

		private FieldParameters _parameters;

		private PercentParamValueClass _percentParamValue;

		public ParamIntValueClass(ParamIntDelegate paramDelegate)
		{
			_paramDelegate = paramDelegate;
			_turnBasedDelegate = null;
		}

		public ParamIntValueClass(Func<int, int> turnBasedDelegate)
		{
			_paramDelegate = null;
			_turnBasedDelegate = turnBasedDelegate;
		}

		public void SetPercentParamValue(ParamType type, int value, ValueFilterMode mode)
		{
			filterMode = mode;
			_percentParamValue = new PercentParamValueClass(type, value);
		}

		public void AttachFiler(BitFilter multiplierFilter, ValueFilterMode mode)
		{
			_multiplierFilter = multiplierFilter;
			filterMode = mode;
		}

		public void Init(FieldElement myMonster, FieldParameters parameters, FieldRandom random, Common.BoolDelegate isRanged)
		{
			_parameters = parameters;
			_side = myMonster.Side;
			if (_multiplierFilter != null)
			{
				_multiplierFilter.Init(myMonster.Side, parameters, () => myMonster.coords, random, isRanged);
			}
		}

		public int GetValue(FieldElement affected = null, FieldMonster monster = null)
		{
			int num = ((_paramDelegate == null) ? _turnBasedDelegate(_parameters.GetTurn(visual: true)) : _paramDelegate());
			int num2 = 0;
			int num3 = num;
			switch (filterMode)
			{
			case ValueFilterMode.No:
				return num;
			case ValueFilterMode.Multiplier:
				return num * _multiplierFilter.GetRightMonsters(affected, SkillType.NoSkill).Count();
			case ValueFilterMode.Attack:
				foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in _multiplierFilter.GetRightMonsters(affected, SkillType.NoSkill))
				{
					num2 += rightMonster.Value.Attack;
				}
				return num2 * num3;
			case ValueFilterMode.Health:
				foreach (KeyValuePair<Vector2, FieldMonster> rightMonster2 in _multiplierFilter.GetRightMonsters(affected, SkillType.NoSkill))
				{
					num2 += (int)rightMonster2.Value.Health;
				}
				return num2 * num3;
			case ValueFilterMode.Monster:
			case ValueFilterMode.MonsterData:
				if (_multiplierFilter.GetRightMonsters(affected, SkillType.NoSkill).Count() == 0)
				{
					return -1;
				}
				return _multiplierFilter.GetRightMonsters(affected, SkillType.NoSkill).First().Value.data.monster_id;
			case ValueFilterMode.PercentForEachMonster:
				return _percentParamValue.GetCalculatedValue(monster);
			case ValueFilterMode.PercentForOneMonster:
			{
				foreach (KeyValuePair<Vector2, FieldMonster> rightMonster3 in _multiplierFilter.GetRightMonsters(affected, SkillType.NoSkill))
				{
					num2 = _percentParamValue.GetCalculatedValue(rightMonster3.Value);
				}
				return num2;
			}
			default:
				return num;
			}
		}

		public int GetValueMultiplier(FieldElement affected = null)
		{
			if (_multiplierFilter != null && filterMode == ValueFilterMode.Multiplier)
			{
				return _multiplierFilter.GetRightMonsters(affected, SkillType.NoSkill).Count();
			}
			Debug.LogError("Trying to get value multiplier with null filter. Returned 1.");
			return 1;
		}

		public FieldMonster GetMonsterParam(FieldElement affected = null)
		{
			if (filterMode == ValueFilterMode.Monster)
			{
				if (_multiplierFilter.GetRightMonsters(affected, SkillType.NoSkill).Count() == 0)
				{
					return null;
				}
				return _multiplierFilter.GetRightMonsters(affected, SkillType.NoSkill).First().Value;
			}
			return null;
		}
	}
}
