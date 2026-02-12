using System;
using System.Collections.Generic;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;

namespace BattlefieldScripts
{
	public class FieldRune : FieldElement
	{
		protected FieldRandom _random;

		public RuneData data { get; protected set; }

		public List<ActionBitSignature> skills => _actions.ConvertAll((ActionBit x) => x.GetSignature());

		protected virtual bool animated => true;

		public void Init(ArmyControllerCore curController, RuneData rData, FieldRuneVisual visual, FieldParameters fParams, FieldRandom random, IteratorCore iterator)
		{
			parameters = fParams;
			_random = random;
			visualElement = visual;
			data = rData;
			if (visual != null)
			{
				visual.Init(data);
			}
			_curController = curController;
			_iterator = iterator;
			_isStillExist = true;
			_actions = new List<ActionBit>();
			_actions.AddRange(GenerateSkills());
		}

		private List<ActionBit> GenerateSkills()
		{
			List<ActionBit> list = new List<ActionBit>();
			for (int i = 0; i < data.skills.Count; i++)
			{
				SkillStaticData staticData = data.skills[i];
				string count = string.Concat(data.skillValues[i]);
				ActionBit actionBit = SkillFabric.CreateSkill(staticData, count, () => false, () => 0, () => 0, _random, !animated);
				if (actionBit != null)
				{
					actionBit.Init(this, _curController, parameters, () => coords, _random, !animated);
					list.Add(actionBit);
				}
			}
			return list;
		}

		public override void OnTriggerCompleted(Action onPerformed, TriggerType trigger, bool somethingPerformed)
		{
			if (data.destroyAfterUse && somethingPerformed)
			{
				_curController.DestroyRune(this);
			}
			base.OnTriggerCompleted(onPerformed, trigger, somethingPerformed);
		}
	}
}
