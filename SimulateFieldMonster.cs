using System.Collections.Generic;
using System.Linq;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using NewAssets.Scripts.Data_Helpers;

namespace BattlefieldScripts
{
	public class SimulateFieldMonster : FieldMonster
	{
		protected override bool Animated => false;

		public void InitFromOriginal(ArmyControllerCore curController, FieldMonster original, FieldParameters fieldParameters, FieldRandom random, IteratorCore iterator)
		{
			visualElement = null;
			_curController = curController;
			_iterator = iterator;
			data = original.data;
			guid = original.guid;
			_random = random;
			parameters = fieldParameters;
			_attack = original.AttackClone;
			_maxHealth = original.MaxHealthClone;
			_health = original.Health;
			_perks = new List<PerkBit>();
			_delayedSkills = new List<MonsterDelayedSkill>();
			_delayedAddedSkills = new List<MonsterDelayedSkill>();
			_delayedPerks = new List<MonsterDelayedPerk>();
			_delayedAddedPerks = new List<MonsterDelayedPerk>();
			_actions = new List<ActionBit>();
			List<ActionBitSignature> skills = original.Skills;
			List<ActionBitSignature> perks = original.Perks;
			foreach (ActionBitSignature item3 in skills)
			{
				if (!(item3.name == "standartAttack"))
				{
					ActionBit item = GenerateSkillFromSignature(item3);
					_actions.Add(item);
				}
			}
			foreach (ActionBitSignature item4 in perks)
			{
				PerkBit item2 = GeneratePerkFromSignature(item4);
				_perks.Add(item2);
			}
			if (original.data.monsterClass != Class.Building && skills.Any((ActionBitSignature x) => x.name == "standartAttack"))
			{
				_standartAttack = GenerateStandartAttackBit();
				_actions.Add(_standartAttack);
			}
			_instantKill = false;
			_statSnapshot = original.SnapshotClone;
			_killer = null;
		}

		public void InitFromOriginalWarlord(ArmyControllerCore curController, FieldMonster original, FieldParameters fParams, FieldRandom random, IteratorCore iterator)
		{
			visualElement = null;
			_curController = curController;
			_iterator = iterator;
			data = original.data;
			parameters = fParams;
			_random = random;
			_attack = original.AttackClone;
			_maxHealth = original.MaxHealthClone;
			_health = original.Health;
			_perks = new List<PerkBit>();
			_perks.AddRange(GeneratePerks());
			List<MonsterDelayedSkill.MonsterDelayedSkillSignature> delayedSkills = original.DelayedSkills;
			_delayedSkills = new List<MonsterDelayedSkill>();
			foreach (MonsterDelayedSkill.MonsterDelayedSkillSignature item in delayedSkills)
			{
				ActionBit skill = ((item.skill.name == "standartAttack") ? GenerateStandartAttackBit() : GenerateSkillFromSignature(item.skill));
				_delayedSkills.Add(new MonsterDelayedSkill
				{
					count = item.count,
					reduceTrigger = item.reduceTrigger,
					skill = skill
				});
			}
			List<MonsterDelayedPerk.MonsterDelayedPerkSignature> delayedPerks = original.DelayedPerks;
			_delayedPerks = new List<MonsterDelayedPerk>();
			foreach (MonsterDelayedPerk.MonsterDelayedPerkSignature item2 in delayedPerks)
			{
				PerkBit perk = GeneratePerkFromSignature(item2.perk);
				_delayedPerks.Add(new MonsterDelayedPerk
				{
					count = item2.count,
					reduceTrigger = item2.reduceTrigger,
					perk = perk
				});
			}
			_actions = new List<ActionBit>();
			List<ActionBitSignature> skills = original.Skills;
			List<ActionBitSignature> perks = original.Perks;
			List<MonsterDelayedSkill.MonsterDelayedSkillSignature> delayedAddedSkills = original.DelayedAddedSkills;
			_delayedAddedSkills = new List<MonsterDelayedSkill>();
			foreach (ActionBitSignature elem in skills)
			{
				if (!(elem.name == "standartAttack"))
				{
					ActionBit actionBit = GenerateSkillFromSignature(elem);
					_actions.Add(actionBit);
					MonsterDelayedSkill.MonsterDelayedSkillSignature monsterDelayedSkillSignature = delayedAddedSkills.Find((MonsterDelayedSkill.MonsterDelayedSkillSignature x) => x.skill == elem);
					if (monsterDelayedSkillSignature != null)
					{
						_delayedAddedSkills.Add(new MonsterDelayedSkill
						{
							count = monsterDelayedSkillSignature.count,
							reduceTrigger = monsterDelayedSkillSignature.reduceTrigger,
							skill = actionBit
						});
					}
				}
			}
			List<MonsterDelayedPerk.MonsterDelayedPerkSignature> delayedAddedPerks = original.DelayedAddedPerks;
			_delayedAddedPerks = new List<MonsterDelayedPerk>();
			foreach (ActionBitSignature elem2 in perks)
			{
				PerkBit perkBit = GeneratePerkFromSignature(elem2);
				_perks.Add(perkBit);
				MonsterDelayedPerk.MonsterDelayedPerkSignature monsterDelayedPerkSignature = delayedAddedPerks.Find((MonsterDelayedPerk.MonsterDelayedPerkSignature x) => x.perk == elem2);
				if (monsterDelayedPerkSignature != null)
				{
					_delayedAddedPerks.Add(new MonsterDelayedPerk
					{
						count = monsterDelayedPerkSignature.count,
						reduceTrigger = monsterDelayedPerkSignature.reduceTrigger,
						perk = perkBit
					});
				}
			}
			_standartAttack = null;
			_instantKill = false;
			_statSnapshot = original.SnapshotClone;
			_killer = null;
		}

		protected override ActionBit GenerateStandartAttackBit()
		{
			ActionBit obj = SkillFabric.CreateAttack(signature: new ActionBitSignature(SkillType.Attack, TriggerType.Attack, "standartAttack", "attack"), trigger: (data.monsterClass == Class.Building) ? new BitTrigger(SkillStaticData.BuildingAttackTrigger) : new BitTrigger(SkillStaticData.StandartAttackTrigger), rangeDelegate: base.IsRanged, attackDel: () => _attack.GetValue(), withoutAnimation: true);
			obj.Init(this, _curController, parameters, () => coords, _random, withoutDelay: true);
			return obj;
		}

		private ActionBit GenerateSkillFromSignature(ActionBitSignature signature)
		{
			SkillStaticData skillByName = SkillDataHelper.GetSkillByName(signature.name);
			string strValue = signature.strValue;
			ActionBit actionBit = SkillFabric.CreateSkill(skillByName, strValue, base.IsRanged, () => _attack.GetValue(), () => _maxHealth.GetValue(), _random, withoutAnimation: true);
			actionBit.Init(this, _curController, parameters, () => coords, _random, withoutDelay: true);
			return actionBit;
		}

		protected PerkBit GeneratePerkFromSignature(ActionBitSignature signature)
		{
			SkillStaticData skillByName = SkillDataHelper.GetSkillByName(signature.name);
			string strValue = signature.strValue;
			PerkBit perkBit = SkillFabric.CreatePerk(skillByName, strValue, withoutAnimation: true);
			perkBit.Init(this);
			return perkBit;
		}
	}
}
