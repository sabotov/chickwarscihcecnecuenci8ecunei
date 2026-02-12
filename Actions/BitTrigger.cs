using System;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class BitTrigger
	{
		protected BitStaticTrigger _data;

		public ArmySide side;

		public FieldParameters parameters;

		public Func<Vector2> placeDelegate;

		public ArmySide enemySide
		{
			get
			{
				if (side != ArmySide.Left)
				{
					return ArmySide.Left;
				}
				return ArmySide.Right;
			}
		}

		public BitTrigger(BitStaticTrigger data)
		{
			_data = data;
		}

		public void Init(ArmySide thisSide, FieldParameters thisParameters, Func<Vector2> positionDelegate)
		{
			side = thisSide;
			parameters = thisParameters;
			placeDelegate = positionDelegate;
		}

		public virtual bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, object param = null)
		{
			try
			{
				return _data.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, this, param);
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
