using System.Collections.Generic;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.Data_Helpers;

namespace BattlefieldScripts
{
	public class SimulateFieldRune : FieldRune
	{
		protected override bool animated => false;

		public void InitFromOriginal(ArmyControllerCore curController, FieldRune original, FieldParameters fieldParameters, FieldRandom random, IteratorCore iterator)
		{
			_random = random;
			_curController = curController;
			_iterator = iterator;
			parameters = fieldParameters;
			base.data = original.data;
			_isStillExist = true;
			_actions = new List<ActionBit>();
			foreach (ActionBitSignature skill in original.skills)
			{
				ActionBit item = GenerateSkillFromSignature(skill);
				_actions.Add(item);
			}
		}

		private ActionBit GenerateSkillFromSignature(ActionBitSignature signature)
		{
			SkillStaticData skillByName = SkillDataHelper.GetSkillByName(signature.name);
			string strValue = signature.strValue;
			ActionBit actionBit = SkillFabric.CreateSkill(skillByName, strValue, () => false, () => 0, () => 0, _random, withoutAnimation: true);
			actionBit.Init(this, _curController, parameters, () => coords, _random, withoutDelay: true);
			return actionBit;
		}

		private List<ActionBit> GenerateSkills()
		{
			List<ActionBit> list = new List<ActionBit>();
			for (int i = 0; i < base.data.skills.Count; i++)
			{
				SkillStaticData staticData = base.data.skills[i];
				string count = string.Concat(base.data.skillValues[i]);
				ActionBit actionBit = SkillFabric.CreateSkill(staticData, count, () => false, () => 0, () => 0, _random, withoutAnimation: true);
				if (actionBit != null)
				{
					actionBit.Init(this, _curController, parameters, () => coords, _random, withoutDelay: true);
					list.Add(actionBit);
				}
			}
			return list;
		}
	}
}
