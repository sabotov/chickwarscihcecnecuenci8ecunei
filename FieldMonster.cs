using System;
using System.Collections.Generic;
using System.Linq;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using NewAssets.Scripts.Data_Helpers;
using NewAssets.Scripts.UtilScripts;
using ServiceLocator;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts
{
	public class FieldMonster : FieldElement
	{
		public class StatSnapshot
		{
			public bool isDirty;

			public bool hasDivineShield;

			public int hpSnapshot;

			public float hpPercentSnapshot;

			public int maxHpSnapshot;

			public int attackSnapshot;

			public StatSnapshot()
			{
				isDirty = true;
			}

			public StatSnapshot Clone()
			{
				return new StatSnapshot
				{
					isDirty = isDirty,
					hpSnapshot = hpSnapshot,
					maxHpSnapshot = maxHpSnapshot,
					attackSnapshot = attackSnapshot,
					hpPercentSnapshot = hpPercentSnapshot,
					hasDivineShield = hasDivineShield
				};
			}
		}

		private readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		protected bool _instantKill;

		protected MonsterParam _attack;

		protected MonsterParam _maxHealth;

		protected ObscuredInt _health;

		protected ActionBit _standartAttack;

		protected List<PerkBit> _perks;

		protected List<MonsterDelayedSkill> _delayedSkills;

		protected List<MonsterDelayedSkill> _delayedAddedSkills;

		protected List<MonsterDelayedPerk> _delayedPerks;

		protected List<MonsterDelayedPerk> _delayedAddedPerks;

		protected FieldMonster _killer;

		protected StatSnapshot _statSnapshot;

		protected SilenceData _silenceData = new SilenceData();

		public string guid;

		public MonsterData data;

		protected FieldRandom _random;

		protected bool _evaded;

		protected FieldMonster _evadedAttacker;

		private bool _deathPerformed;

		private IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		public FieldMonsterVisual VisualMonster => visualElement as FieldMonsterVisual;

		public StatSnapshot SnapshotClone => _statSnapshot.Clone();

		protected virtual bool Animated => true;

		public bool CanCounter { get; set; }

		public bool CanEvade
		{
			get
			{
				foreach (PerkBit perk in _perks)
				{
					if (perk.GetSignature().signature == SkillType.EvadeMelee || perk.GetSignature().signature == SkillType.EvadeRanged)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool CanEvadeMelee
		{
			get
			{
				foreach (PerkBit perk in _perks)
				{
					if (perk.GetSignature().signature == SkillType.EvadeMelee)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool CanEvadeRanged
		{
			get
			{
				foreach (PerkBit perk in _perks)
				{
					if (perk.GetSignature().signature == SkillType.EvadeRanged)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasDoubleAttack
		{
			get
			{
				for (int i = 0; i < _perks.Count; i++)
				{
					if (_perks[i].GetSignature().signature == SkillType.ExtraAttack)
					{
						return true;
					}
				}
				for (int j = 0; j < base.Actions.Count; j++)
				{
					if (base.Actions[j].GetSignature().signature == SkillType.ExtraAttack)
					{
						return true;
					}
				}
				return false;
			}
		}

		public int Attack
		{
			get
			{
				if (data.monsterClass != Class.Building)
				{
					return _attack.GetValue();
				}
				return 0;
			}
		}

		public ObscuredInt Health => _health;

		public ObscuredInt MaxHealth => _maxHealth.GetValue();

		public MonsterParam AttackClone => _attack.Clone();

		public MonsterParam MaxHealthClone => _maxHealth.Clone();

		public bool canAttack
		{
			get
			{
				if (!ShouldDie && Attack > 0 && _perks.All((PerkBit x) => x.GetSignature().signature != SkillType.AttackBlockedPerk && x.GetSignature().signature != SkillType.AttackBlocked))
				{
					return !data.is_pet;
				}
				return false;
			}
		}

		public bool HaveAccuracy
		{
			get
			{
				foreach (PerkBit perk in _perks)
				{
					if (perk.GetSignature().signature == SkillType.Accuracy)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HaveMiss
		{
			get
			{
				foreach (PerkBit perk in _perks)
				{
					if (perk.GetSignature().signature == SkillType.Miss)
					{
						return true;
					}
				}
				return false;
			}
		}

		public List<ActionBitSignature> Perks => _perks.ConvertAll((PerkBit x) => x.GetSignature());

		public List<ActionBitSignature> Skills => _actions.ConvertAll((ActionBit x) => x.GetSignature());

		public List<MonsterDelayedSkill.MonsterDelayedSkillSignature> DelayedSkills => _delayedSkills.ConvertAll((MonsterDelayedSkill x) => new MonsterDelayedSkill.MonsterDelayedSkillSignature
		{
			count = x.count,
			reduceTrigger = x.reduceTrigger,
			skill = x.skill.GetSignature()
		});

		public List<MonsterDelayedSkill.MonsterDelayedSkillSignature> DelayedAddedSkills => _delayedAddedSkills.ConvertAll((MonsterDelayedSkill x) => new MonsterDelayedSkill.MonsterDelayedSkillSignature
		{
			count = x.count,
			reduceTrigger = x.reduceTrigger,
			skill = x.skill.GetSignature()
		});

		public List<MonsterDelayedPerk.MonsterDelayedPerkSignature> DelayedPerks => _delayedPerks.ConvertAll((MonsterDelayedPerk x) => new MonsterDelayedPerk.MonsterDelayedPerkSignature
		{
			count = x.count,
			reduceTrigger = x.reduceTrigger,
			perk = x.perk.GetSignature()
		});

		public List<MonsterDelayedPerk.MonsterDelayedPerkSignature> DelayedAddedPerks => _delayedAddedPerks.ConvertAll((MonsterDelayedPerk x) => new MonsterDelayedPerk.MonsterDelayedPerkSignature
		{
			count = x.count,
			reduceTrigger = x.reduceTrigger,
			perk = x.perk.GetSignature()
		});

		public bool ShouldDie
		{
			get
			{
				if ((int)_health > 0)
				{
					return _instantKill;
				}
				return true;
			}
		}

		public int GetSkillValue(string skillName)
		{
			int num = 0;
			if (_actions != null)
			{
				foreach (ActionBit item in _actions.FindAll((ActionBit x) => x.GetSignature().skillId == skillName))
				{
					if (item != null)
					{
						num += item.GetSignature().value;
					}
				}
			}
			if (_perks != null)
			{
				foreach (PerkBit item2 in _perks.FindAll((PerkBit x) => x.GetSignature().skillId == skillName))
				{
					if (item2 != null)
					{
						num += item2.GetSignature().value;
					}
				}
			}
			return num;
		}

		public int GetRebornCount(out string skillId)
		{
			ActionBit actionBit = _actions.Find((ActionBit x) => x.GetSignature().signature == SkillType.Reborn);
			if (actionBit == null)
			{
				skillId = "";
				return 0;
			}
			skillId = actionBit.GetSignature().skillId;
			return actionBit.GetSignature().value;
		}

		public FieldMonster()
		{
			guid = MyUtil.GetNewGuidString();
		}

		public bool IsEqual(FieldMonster mon)
		{
			return guid == mon.guid;
		}

		public override string ToString()
		{
			return data.NameLog() + " " + guid;
		}

		public bool isMine()
		{
			return _curController.Side == ArmySide.Left;
		}

		public virtual void Init(ArmyControllerCore curController, MonsterData newData, FieldMonsterVisual visual, FieldParameters fParameters, FieldRandom random, IteratorCore iterator)
		{
			parameters = fParameters;
			_random = random;
			_iterator = iterator;
			visualElement = visual;
			_curController = curController;
			data = newData;
			_attack = new MonsterParam(data.attack);
			_maxHealth = new MonsterParam(data.health);
			_health = data.health;
			_perks = new List<PerkBit>();
			_perks.AddRange(GeneratePerks());
			if (VisualMonster != null)
			{
				VisualMonster.Init(newData, this);
			}
			_delayedSkills = new List<MonsterDelayedSkill>();
			_delayedAddedSkills = new List<MonsterDelayedSkill>();
			_delayedPerks = new List<MonsterDelayedPerk>();
			_delayedAddedPerks = new List<MonsterDelayedPerk>();
			_actions = new List<ActionBit>();
			if (newData.monsterClass != Class.Building)
			{
				_standartAttack = GenerateStandartAttackBit();
				_actions.Add(_standartAttack);
			}
			_actions.AddRange(GenerateSkills());
			_instantKill = false;
			_killer = null;
			_deathPerformed = false;
			_isStillExist = true;
			foreach (PerkBit perk in _perks)
			{
				if (perk.onAttachedAnimation == null)
				{
					continue;
				}
				if (perk.GetSignature().ShouldMirrorOnBattleField())
				{
					perk.onAttachedAnimation.Animate(new Dictionary<Common.StringDelegate, FieldVisual> { 
					{
						() => "",
						visualElement
					} }, delegate
					{
					}, base.Side);
				}
				else
				{
					perk.onAttachedAnimation.Animate(new Dictionary<Common.StringDelegate, FieldVisual> { 
					{
						() => "",
						visualElement
					} }, delegate
					{
					});
				}
			}
			if (VisualMonster != null)
			{
				VisualMonster.UpdateParameters();
				VisualMonster.UpdateSkills();
			}
			MakeStatSnapshot();
		}

		public void MakeStatSnapshot()
		{
			if (_statSnapshot == null)
			{
				_statSnapshot = new StatSnapshot();
			}
			if (_statSnapshot.isDirty)
			{
				_statSnapshot.isDirty = false;
				_statSnapshot.attackSnapshot = Attack;
				_statSnapshot.hpSnapshot = Health;
				_statSnapshot.hpPercentSnapshot = (float)(int)Health / (float)(int)MaxHealth;
				_statSnapshot.maxHpSnapshot = MaxHealth;
				_statSnapshot.hasDivineShield = _perks.Find((PerkBit x) => x.GetSignature().signature == SkillType.DivineShield) != null;
			}
		}

		public void ShuffleHand()
		{
			_curController.ShuffleHand(playerUseShuffle: false);
		}

		public void PerformStatDropTrigger(Action onPerformed)
		{
			StatSnapshot prevSnapshot = _statSnapshot.Clone();
			_statSnapshot.isDirty = true;
			MakeStatSnapshot();
			_curController.BroadcastAction(TriggerType.StatChange, SkillType.NoSkill, coords, this, this, delegate
			{
				onPerformed();
			}, new StatChangeData
			{
				prevSnapshot = prevSnapshot,
				curSnapshot = _statSnapshot.Clone()
			});
		}

		public void PerformSilenceTrigger(FieldElement performer, Action onPerformed)
		{
			SilenceData param = _silenceData.Clone();
			_silenceData.silencedSkills.Clear();
			_curController.BroadcastAction(TriggerType.Silence, SkillType.NoSkill, coords, performer, this, delegate
			{
				onPerformed();
			}, param);
		}

		public void Transform(MonsterData transformData, float delayTime = 0f)
		{
			data = transformData;
			if (VisualMonster != null)
			{
				VisualMonster.DeattachAllStaticAnimations();
			}
			_attack = new MonsterParam(data.attack);
			_maxHealth = new MonsterParam(data.health);
			_health = data.health;
			_delayedSkills = new List<MonsterDelayedSkill>();
			_delayedAddedSkills = new List<MonsterDelayedSkill>();
			_delayedPerks = new List<MonsterDelayedPerk>();
			_delayedAddedPerks = new List<MonsterDelayedPerk>();
			_actions = new List<ActionBit>();
			_perks = new List<PerkBit>();
			if (transformData.monsterClass != Class.Building)
			{
				_standartAttack = GenerateStandartAttackBit();
				_actions.Add(_standartAttack);
			}
			_actions.AddRange(GenerateSkills());
			_perks.AddRange(GeneratePerks());
			if (VisualMonster != null)
			{
				_delayedActionsHandler.WaitForProcedure(delayTime, delegate
				{
					VisualMonster.Transform(transformData);
				});
			}
			_instantKill = false;
			_killer = null;
			_isStillExist = true;
			if (VisualMonster != null)
			{
				VisualMonster.UpdateParameters();
			}
		}

		public bool CheckHasActionSignature(ActionBitSignature signature)
		{
			if (_actions.Find((ActionBit x) => x.GetSignature() == signature) == null)
			{
				return _perks.Find((PerkBit x) => x.GetSignature() == signature) != null;
			}
			return true;
		}

		public bool CheckHasActionName(string name)
		{
			if (_actions.Find((ActionBit x) => x.GetSignature().name == name) == null)
			{
				return _perks.Find((PerkBit x) => x.GetSignature().name == name) != null;
			}
			return true;
		}

		public bool CheckHasTrigger(TriggerType trigger)
		{
			return _actions.Find((ActionBit x) => x.GetSignature().trigger == trigger) != null;
		}

		public bool CheckHasSignature(SkillType name)
		{
			return _actions.Find((ActionBit x) => x.GetSignature().signature == name) != null;
		}

		public int ChangeParam(FieldMonster performer, ParamType param, int delta)
		{
			switch (param)
			{
			case ParamType.Attack:
				Buff(performer, ParamType.Attack, delta, TriggerType.NoTrigger, 0);
				break;
			case ParamType.Health:
			{
				int num = _health;
				delta = PerformStrongPerk(delta);
				if ((int)_health > 0 || delta < 0)
				{
					_health = (int)_health + delta;
				}
				_health = Mathf.Min(_health, _maxHealth.GetValue());
				if (num != (int)_health && VisualMonster != null)
				{
					VisualMonster.BounceHealth();
				}
				delta = (int)_health - num;
				if ((int)_health <= 0 && num > 0)
				{
					if (VisualMonster != null && !(VisualMonster is WarlordMonsterVisual))
					{
						VisualMonster.Animate(MonsterAnimationType.Death);
					}
					_killer = performer;
				}
				else if (delta < 0 && VisualMonster != null)
				{
					VisualMonster.Animate(MonsterAnimationType.Damaged);
					VisualMonster.AnimateDamage();
				}
				if (delta <= 0)
				{
					_curController.PerformMonsterHit(coords);
				}
				break;
			}
			}
			if (VisualMonster != null)
			{
				VisualMonster.UpdateParameters();
			}
			return delta;
		}

		public int ForceSetParam(FieldMonster performer, ParamType param, int delta, TriggerType trigger, int counter)
		{
			MonsterParam monsterParam = ((param == ParamType.Attack) ? _attack : _maxHealth);
			MonsterParam monsterParam2 = new MonsterParam(delta, monsterParam, trigger, counter);
			monsterParam2.MakeForce();
			int result = monsterParam2.GetValue() - monsterParam.GetValue();
			switch (param)
			{
			case ParamType.Attack:
			{
				int value = _attack.GetValue();
				_attack = monsterParam2;
				if (value != _attack.GetValue() && VisualMonster != null)
				{
					VisualMonster.BounceAttack();
				}
				break;
			}
			case ParamType.Health:
			{
				_maxHealth = monsterParam2;
				int num = _health;
				_health = _maxHealth.GetValue();
				if (num != (int)_health && VisualMonster != null)
				{
					VisualMonster.BounceHealth();
				}
				if ((int)_health <= 0 && num > 0)
				{
					if (VisualMonster != null && !(VisualMonster is WarlordMonsterVisual))
					{
						VisualMonster.Animate(MonsterAnimationType.Death);
					}
					_killer = performer;
				}
				break;
			}
			}
			if (VisualMonster != null)
			{
				VisualMonster.UpdateParameters();
			}
			return result;
		}

		public int Buff(FieldMonster performer, ParamType param, int delta, TriggerType trigger, int counter)
		{
			MonsterParam monsterParam = ((param == ParamType.Attack) ? _attack : _maxHealth);
			MonsterParam monsterParam2 = new MonsterParam(delta, monsterParam, trigger, counter);
			int result = monsterParam2.GetValue() - monsterParam.GetValue();
			switch (param)
			{
			case ParamType.Attack:
			{
				int value = _attack.GetValue();
				_attack = monsterParam2;
				if (value != _attack.GetValue() && VisualMonster != null)
				{
					VisualMonster.BounceAttack();
				}
				break;
			}
			case ParamType.Health:
			{
				_maxHealth = monsterParam2;
				int num = _health;
				_health = Mathf.Min(Mathf.Max(((int)_health > 0 || delta < 0) ? ((int)_health + delta) : ((int)_health), _health), _maxHealth.GetValue());
				if (num != (int)_health && VisualMonster != null)
				{
					VisualMonster.BounceHealth();
				}
				if ((int)_health <= 0 && num > 0)
				{
					_killer = performer;
				}
				break;
			}
			}
			if (VisualMonster != null)
			{
				VisualMonster.UpdateParameters();
			}
			return result;
		}

		public void AddWarlordSkill(SkillStaticData skill, string val)
		{
			if (data.is_warlord || skill.trigger == TriggerType.WarlordSkillSpecial)
			{
				ActionBit actionBit = _actions.Find((ActionBit x) => x.GetSignature().trigger == TriggerType.WarlordSkillSpecial);
				if (actionBit != null)
				{
					RemoveSkill(actionBit.GetSignature());
					_curController.OnSkillReplaced(TriggerType.WarlordSkillSpecial);
				}
				AddSkill(skill, val);
				_curController.AddCardsToHand(TriggerType.WarlordSkillSpecial, delegate
				{
				});
			}
		}

		public void AddSkill(SkillStaticData skill, string val)
		{
			if (!skill.IsPerk)
			{
				string count = val;
				ActionBit existingSkill = null;
				for (int i = 0; i < _actions.Count; i++)
				{
					ActionBit actionBit = _actions[i];
					if (actionBit.GetSignature().ShouldStack(skill, val))
					{
						existingSkill = actionBit;
						break;
					}
				}
				List<MonsterDelayedSkill> list = null;
				if (existingSkill != null)
				{
					list = _delayedAddedSkills.FindAll((MonsterDelayedSkill x) => x.skill == existingSkill);
					count = existingSkill.GetSignature().GetStackValue(val);
					_actions.Remove(existingSkill);
				}
				ActionBit actionBit2 = SkillFabric.CreateSkill(skill, count, IsRanged, () => _attack.GetValue(), () => _maxHealth.GetValue(), _random, !Animated);
				if (actionBit2 != null)
				{
					actionBit2.Init(this, _curController, parameters, () => coords, _random, !Animated);
					_actions.Add(actionBit2);
					if (list != null)
					{
						foreach (MonsterDelayedSkill delayedAddedSkill in _delayedAddedSkills)
						{
							delayedAddedSkill.skill = actionBit2;
						}
					}
				}
			}
			else
			{
				if (!skill.IsStackablePerk && _perks.Find((PerkBit x) => x.GetSignature().signature == skill.skill) != null)
				{
					return;
				}
				PerkBit perkBit = SkillFabric.CreatePerk(skill, val, !Animated);
				if (perkBit != null && ((perkBit.GetSignature().skillId == "Miss" && !HaveAccuracy) || perkBit.GetSignature().skillId != "Miss"))
				{
					perkBit.Init(this);
					if (perkBit.onAttachedAnimation != null)
					{
						perkBit.onAttachedAnimation.Animate(new Dictionary<Common.StringDelegate, FieldVisual> { 
						{
							() => "",
							visualElement
						} }, delegate
						{
						});
					}
					_perks.Add(perkBit);
				}
				if (HaveAccuracy && HaveMiss)
				{
					PerkBit perkBit2 = _perks.Find((PerkBit x) => x.GetSignature().signature == SkillType.Miss);
					if (perkBit2 != null)
					{
						RemovePerk(perkBit2.GetSignature());
					}
				}
			}
			if (VisualMonster != null)
			{
				VisualMonster.UpdateSkills();
			}
		}

		public void AddSkillTemporary(SkillStaticData skill, string val, TriggerType trigger, int counter)
		{
			if (!skill.IsPerk)
			{
				string count = val;
				ActionBit existingSkill = null;
				for (int i = 0; i < _actions.Count; i++)
				{
					ActionBit actionBit = _actions[i];
					if (actionBit.GetSignature().ShouldStack(skill, val))
					{
						existingSkill = actionBit;
						break;
					}
				}
				List<MonsterDelayedSkill> list = null;
				if (existingSkill != null)
				{
					list = _delayedAddedSkills.FindAll((MonsterDelayedSkill x) => x.skill == existingSkill);
					count = existingSkill.GetSignature().GetStackValue(val);
					_actions.Remove(existingSkill);
				}
				ActionBit actionBit2 = SkillFabric.CreateSkill(skill, count, IsRanged, () => _attack.GetValue(), () => _maxHealth.GetValue(), _random, !Animated);
				if (actionBit2 != null)
				{
					actionBit2.Init(this, _curController, parameters, () => coords, _random, !Animated);
					_actions.Add(actionBit2);
					if (list != null)
					{
						foreach (MonsterDelayedSkill delayedAddedSkill in _delayedAddedSkills)
						{
							delayedAddedSkill.skill = actionBit2;
						}
					}
				}
				MonsterDelayedSkill item = new MonsterDelayedSkill
				{
					skill = actionBit2,
					reduceTrigger = trigger,
					count = counter,
					skillVal = val
				};
				_delayedAddedSkills.Add(item);
			}
			else
			{
				PerkBit perkBit = SkillFabric.CreatePerk(skill, val, !Animated, counter, trigger);
				if (perkBit != null && ((perkBit.GetSignature().skillId == "Miss" && !HaveAccuracy) || perkBit.GetSignature().skillId != "Miss"))
				{
					perkBit.Init(this);
					if (perkBit.onAttachedAnimation != null)
					{
						perkBit.onAttachedAnimation.Animate(new Dictionary<Common.StringDelegate, FieldVisual> { 
						{
							() => "",
							visualElement
						} }, delegate
						{
						});
					}
					_perks.Add(perkBit);
					MonsterDelayedPerk item2 = new MonsterDelayedPerk
					{
						perk = perkBit,
						reduceTrigger = trigger,
						count = counter
					};
					_delayedAddedPerks.Add(item2);
					if (HaveAccuracy && HaveMiss)
					{
						PerkBit perkBit2 = _perks.Find((PerkBit x) => x.GetSignature().signature == SkillType.Miss);
						if (perkBit2 != null)
						{
							RemovePerk(perkBit2.GetSignature());
						}
					}
				}
			}
			if (VisualMonster != null)
			{
				VisualMonster.UpdateSkills();
			}
		}

		public void RemoveSkill(ActionBitSignature skill)
		{
			_actions = _actions.FindAll((ActionBit x) => x.GetSignature() != skill);
			if (VisualMonster != null)
			{
				VisualMonster.UpdateSkills();
			}
		}

		public void RemoveSkillOrPerk(SkillStaticData skill)
		{
			_actions = _actions.FindAll((ActionBit x) => x.GetSignature().skillId != skill.strId);
			_perks = _perks.FindAll(delegate(PerkBit x)
			{
				if (x.GetSignature().skillId == skill.strId)
				{
					VisualMonster.CleanStaticAnimation();
				}
				return x.GetSignature().skillId != skill.strId;
			});
			if (VisualMonster != null)
			{
				VisualMonster.UpdateSkills();
			}
		}

		public void RemovePerk(ActionBitSignature signature)
		{
			_perks = _perks.FindAll((PerkBit x) => x.GetSignature() != signature);
			if (VisualMonster != null)
			{
				VisualMonster.UpdateSkills();
			}
		}

		public void RemoveSkillTemporary(SkillStaticData skill, TriggerType trigger, int counter)
		{
			foreach (ActionBit item3 in _actions.FindAll((ActionBit x) => x.GetSignature().skillId == skill.strId))
			{
				_actions.Remove(item3);
				MonsterDelayedSkill item = new MonsterDelayedSkill
				{
					skill = item3,
					reduceTrigger = trigger,
					count = counter
				};
				_delayedSkills.Add(item);
			}
			foreach (PerkBit item4 in _perks.FindAll((PerkBit x) => x.GetSignature().skillId == skill.strId))
			{
				_perks.Remove(item4);
				VisualMonster.CleanStaticAnimation();
				MonsterDelayedPerk item2 = new MonsterDelayedPerk
				{
					perk = item4,
					reduceTrigger = trigger,
					count = counter
				};
				_delayedPerks.Add(item2);
			}
			if (VisualMonster != null)
			{
				VisualMonster.UpdateSkills();
			}
		}

		public void Silence(TriggerType trigger)
		{
			_silenceData.silencedSkills = _actions.FindAll((ActionBit x) => x.GetSignature().trigger == trigger).ConvertAll((ActionBit x) => x.GetSignature());
			_silenceData.silencedSkills.AddRange(_perks.FindAll((PerkBit x) => x.GetSignature().trigger == trigger).ConvertAll((PerkBit x) => x.GetSignature()));
			_actions = _actions.FindAll((ActionBit x) => x.GetSignature().trigger != trigger);
			_perks = _perks.FindAll((PerkBit x) => x.GetSignature().trigger != trigger);
			if (VisualMonster != null)
			{
				VisualMonster.UpdateSkills();
			}
		}

		public void Silence(SkillType skill)
		{
			_silenceData.silencedSkills = _actions.FindAll((ActionBit x) => x.GetSignature().signature == skill).ConvertAll((ActionBit x) => x.GetSignature());
			_silenceData.silencedSkills.AddRange(_perks.FindAll((PerkBit x) => x.GetSignature().signature == skill).ConvertAll((PerkBit x) => x.GetSignature()));
			_actions = _actions.FindAll((ActionBit x) => x.GetSignature().signature != skill);
			_perks = _perks.FindAll((PerkBit x) => x.GetSignature().signature != skill);
			_delayedSkills = _delayedSkills.FindAll((MonsterDelayedSkill x) => x.skill.GetSignature().signature != skill);
			_delayedPerks = _delayedPerks.FindAll((MonsterDelayedPerk x) => x.perk.GetSignature().signature != skill);
			if (VisualMonster != null)
			{
				VisualMonster.UpdateSkills();
			}
		}

		public void Silence()
		{
			_maxHealth = _maxHealth.GetTrigger(TriggerType.ClearParams);
			_health = Mathf.Min(_health, _maxHealth.GetValue());
			_attack = _attack.GetTrigger(TriggerType.ClearParams);
			_silenceData.silencedSkills = _actions.ConvertAll((ActionBit x) => x.GetSignature());
			_silenceData.silencedSkills.AddRange(_perks.ConvertAll((PerkBit x) => x.GetSignature()));
			_actions = new List<ActionBit>();
			_delayedSkills = new List<MonsterDelayedSkill>();
			_perks = new List<PerkBit>();
			_delayedPerks = new List<MonsterDelayedPerk>();
			if (VisualMonster != null)
			{
				VisualMonster.InformTrigger(TriggerType.ClearParams);
			}
			if (_standartAttack != null && data.monsterClass != Class.Building)
			{
				_actions.Add(_standartAttack);
			}
			if (VisualMonster != null)
			{
				VisualMonster.UpdateSkills();
			}
		}

		public bool IsNotImmune(SkillType skill, FieldMonster affected = null, BitFilter requester = null)
		{
			List<PerkBit>.Enumerator enumerator = _perks.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == null)
					{
						continue;
					}
					SkillType signature = enumerator.Current.GetSignature().signature;
					if ((signature == SkillType.Immune || signature == SkillType.ClearImmune) && !enumerator.Current.CheckSkill(skill))
					{
						enumerator.Dispose();
						return false;
					}
					if (affected != null || requester != null)
					{
						if ((signature == SkillType.ImmuneToFriend || signature == SkillType.ImmuneToEnemy) && !enumerator.Current.CheckSide(affected, requester))
						{
							enumerator.Dispose();
							return false;
						}
						if ((signature == SkillType.ImmuneToPositive || signature == SkillType.ImmuneToNegative) && !enumerator.Current.CheckBenefit(affected, skill))
						{
							enumerator.Dispose();
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("IsNotImmune " + ex);
			}
			finally
			{
				enumerator.Dispose();
			}
			return true;
		}

		public int PerformBlock(int damage)
		{
			int num = damage;
			foreach (PerkBit perk in _perks)
			{
				if (perk.GetSignature().signature != SkillType.Block && perk.GetSignature().signature != SkillType.ClearBlock)
				{
					continue;
				}
				num = perk.ModulateInt(num);
				if (perk.onWorkedAnimation != null)
				{
					perk.onWorkedAnimation.Animate(new Dictionary<Common.StringDelegate, FieldVisual> { 
					{
						() => "",
						visualElement
					} }, delegate
					{
					});
				}
			}
			return num;
		}

		public int PerformDivineShield(int damage)
		{
			if (damage >= 0)
			{
				return damage;
			}
			int num = damage;
			bool flag = false;
			foreach (PerkBit perk in _perks)
			{
				if (perk.GetSignature().signature == SkillType.DivineShield)
				{
					num = perk.ModulateInt(num);
					flag = true;
					break;
				}
			}
			if (flag)
			{
				_perks = _perks.FindAll((PerkBit x) => x.GetSignature().signature != SkillType.DivineShield);
				if (VisualMonster != null)
				{
					VisualMonster.InformTrigger(TriggerType.DivineShieldPerformed);
					VisualMonster.UpdateSkills();
				}
			}
			return num;
		}

		public bool PerformDivineShield(SkillType skill)
		{
			if (skill != SkillType.InstantKill && skill != SkillType.KillStealAttack && skill != SkillType.KillStealHealth && skill != SkillType.KillStealStats && skill != SkillType.KillStealAll && skill != SkillType.KillStealSkills && skill != SkillType.KillStealAttackFromHp && skill != SkillType.KillStealHpFromAttack)
			{
				return false;
			}
			bool flag = false;
			foreach (PerkBit perk in _perks)
			{
				if (perk.GetSignature().signature == SkillType.DivineShield)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				_perks = _perks.FindAll((PerkBit x) => x.GetSignature().signature != SkillType.DivineShield);
				if (VisualMonster != null)
				{
					VisualMonster.InformTrigger(TriggerType.DivineShieldPerformed);
				}
				return true;
			}
			return false;
		}

		private int PerformStrongPerk(int damage)
		{
			if (damage >= 0)
			{
				return damage;
			}
			List<PerkBit> list = _perks.FindAll((PerkBit x) => x.GetSignature().signature == SkillType.Strong);
			if (list.Count != 0)
			{
				int num = 0;
				foreach (PerkBit item in list)
				{
					int value = item.GetSignature().value;
					if (value > num)
					{
						num = value;
					}
				}
				damage = FieldUtils.GetValueRoundedUp((float)(damage * (100 - num)) / 100f);
			}
			return damage;
		}

		public bool ImmuneByEvasion(FieldMonster attacker)
		{
			if (_evaded && _evadedAttacker != null)
			{
				return attacker == _evadedAttacker;
			}
			return false;
		}

		public bool PerformEvade(FieldMonster attacker, bool ranged, int damage)
		{
			_evaded = false;
			_evadedAttacker = null;
			foreach (PerkBit perk in _perks)
			{
				bool flag = false;
				bool flag2 = false;
				if (perk.GetSignature().signature == SkillType.EvadeMelee)
				{
					flag2 = true;
					flag = !ranged && !attacker.CanEvadeMelee;
				}
				else if (perk.GetSignature().signature == SkillType.EvadeRanged)
				{
					flag = ranged;
				}
				if (!flag)
				{
					continue;
				}
				if (perk.ModulateInt(damage) == 0)
				{
					if (attacker.HaveAccuracy && !flag2)
					{
						return false;
					}
					_evaded = true;
					_evadedAttacker = attacker;
					return true;
				}
				return false;
			}
			return false;
		}

		public void AnimateVictory()
		{
			if (VisualMonster != null)
			{
				_delayedActionsHandler.WaitForProcedure(0.7f, delegate
				{
					VisualMonster.Animate(MonsterAnimationType.Victory);
				});
			}
			if (VisualMonster != null)
			{
				VisualMonster.DeattachAllStaticAnimations();
			}
		}

		public void AnimateUseSkill()
		{
			if (VisualMonster != null)
			{
				VisualMonster.Animate(MonsterAnimationType.MeleeHit);
			}
		}

		public void AnimateDamaged()
		{
			if (VisualMonster != null)
			{
				VisualMonster.Animate(MonsterAnimationType.Damaged, null, pet: true);
			}
		}

		public void AnimateDefeat()
		{
			if (VisualMonster.Data.isGoldMineMonster)
			{
				return;
			}
			if (VisualMonster.Data.isGoldMineWarlord)
			{
				VisualMonster.HideParams();
				VisualMonster.AnimateDeathGoldWarlord();
			}
			else if (VisualMonster != null)
			{
				VisualMonster.HideParams();
				if (VisualMonster.Data.skills.FindAll((SkillStaticData x) => x.skill.Equals(SkillType.Mercy)).Count < 1)
				{
					VisualMonster.Animate(MonsterAnimationType.Death);
				}
				VisualMonster.DeattachAllStaticAnimations();
			}
		}

		public void Kill()
		{
			_instantKill = true;
			if (VisualMonster != null && !(VisualMonster is WarlordMonsterVisual))
			{
				VisualMonster.Animate(MonsterAnimationType.Death);
			}
		}

		private float GetDeathTime()
		{
			if (VisualMonster != null && VisualMonster.Image != null)
			{
				return VisualMonster.Image.GetAnimationDuration("death");
			}
			return 0f;
		}

		public void CheckDeath(Action onCompleted)
		{
			if (ShouldDie && !_deathPerformed)
			{
				_curController.PerformMonsterDead(coords);
				if (VisualMonster != null)
				{
					VisualMonster.InformTrigger(TriggerType.ClearParams);
					VisualMonster.AnimateDeath();
				}
				if (visualElement != null && !(visualElement is WarlordMonsterVisual))
				{
					float time = Mathf.Max(TimeDebugController.instance.deathDelay / TimeDebugController.totalTimeMultiplier, GetDeathTime());
					_delayedActionsHandler.WaitForProcedure(time, delegate
					{
						visualElement.Destroy();
					});
				}
				if (VisualMonster != null)
				{
					VisualMonster.DeattachAllStaticAnimations();
				}
				Action onKillBroadcasted = delegate
				{
					if (!data.is_warlord)
					{
						BroadcastAction(TriggerType.Death, SkillType.NoSkill, coords, this, _killer, onCompleted);
					}
					else
					{
						onCompleted();
					}
				};
				Action onPerformed = delegate
				{
					_isStillExist = false;
					if (_killer != null && !data.is_warlord && _killer != this)
					{
						BroadcastAction(TriggerType.Kill, SkillType.NoSkill, _killer.coords, _killer, this, onKillBroadcasted);
					}
					else
					{
						onKillBroadcasted();
					}
				};
				Action<Action> deathRecheckDelegate = delegate(Action onCompl)
				{
					_curController.BroadcastDeathRecheck(base.Side, onCompl);
				};
				PerformAction(TriggerType.Death, SkillType.NoSkill, coords, this, _killer, onPerformed, deathRecheckDelegate);
				_deathPerformed = true;
			}
			else
			{
				onCompleted();
			}
		}

		protected override void PreActionAnimation(TriggerType trigger, Action onAnimation)
		{
			if (!Animated || VisualMonster == null)
			{
				base.PreActionAnimation(trigger, onAnimation);
			}
			else
			{
				VisualMonster.PreActionAnimation(trigger, onAnimation);
			}
		}

		protected override void PostActionAnimation(TriggerType trigger, Action onAnimation)
		{
			if (!Animated || VisualMonster == null)
			{
				base.PostActionAnimation(trigger, onAnimation);
			}
			else
			{
				VisualMonster.PostActionAnimation(trigger, onAnimation);
			}
		}

		public override void OnTriggerCompleted(Action onPerformed, TriggerType trigger, bool somethingPerformed)
		{
			_maxHealth = _maxHealth.GetTrigger(trigger);
			int num = _health;
			if ((int)_health <= 0 && num > 0)
			{
				_killer = null;
			}
			_attack = _attack.GetTrigger(trigger);
			bool flag = false;
			foreach (MonsterDelayedSkill elem in _delayedAddedSkills)
			{
				if (elem.reduceTrigger != trigger)
				{
					continue;
				}
				elem.count--;
				if (elem.count > 0 || elem.skill == null)
				{
					continue;
				}
				SkillStaticData staticData = SkillDataHelper.GetSkillByName(elem.skill.GetSignature().skillId);
				if (staticData != null)
				{
					ActionBit existingSkill = _actions.Find((ActionBit x) => x.GetSignature().ShouldStack(staticData, elem.skillVal));
					if (existingSkill == null)
					{
						continue;
					}
					List<MonsterDelayedSkill> list = _delayedAddedSkills.FindAll((MonsterDelayedSkill x) => x != elem && x.skill == existingSkill);
					_actions.Remove(existingSkill);
					int num2 = existingSkill.GetSignature().value - int.Parse(elem.skillVal);
					if (num2 > 0)
					{
						ActionBit actionBit = SkillFabric.CreateSkill(staticData, num2.ToString(), IsRanged, () => _attack.GetValue(), () => _maxHealth.GetValue(), _random, !Animated);
						if (actionBit != null)
						{
							actionBit.Init(this, _curController, parameters, () => coords, _random, !Animated);
							_actions.Add(actionBit);
							foreach (MonsterDelayedSkill item in list)
							{
								item.skill = actionBit;
							}
						}
					}
					else
					{
						foreach (MonsterDelayedSkill item2 in list)
						{
							item2.skill = null;
						}
					}
					flag = true;
				}
				else
				{
					Debug.LogError("Cant find skill " + elem.skill.GetSignature().skillId);
				}
			}
			_delayedAddedSkills = _delayedAddedSkills.FindAll((MonsterDelayedSkill x) => x.count > 0);
			foreach (MonsterDelayedSkill delayedSkill in _delayedSkills)
			{
				if (delayedSkill.reduceTrigger == trigger)
				{
					delayedSkill.count--;
					if (delayedSkill.count <= 0)
					{
						_actions.Add(delayedSkill.skill);
						flag = true;
					}
				}
			}
			_delayedSkills = _delayedSkills.FindAll((MonsterDelayedSkill x) => x.count > 0);
			foreach (MonsterDelayedPerk delayedPerk in _delayedPerks)
			{
				if (delayedPerk.reduceTrigger == trigger)
				{
					delayedPerk.count--;
					if (delayedPerk.count <= 0)
					{
						_perks.Add(delayedPerk.perk);
						flag = true;
					}
				}
			}
			_delayedPerks = _delayedPerks.FindAll((MonsterDelayedPerk x) => x.count > 0);
			foreach (MonsterDelayedPerk delayedAddedPerk in _delayedAddedPerks)
			{
				if (delayedAddedPerk.reduceTrigger == trigger)
				{
					delayedAddedPerk.count--;
					if (delayedAddedPerk.count <= 0)
					{
						_perks.Remove(delayedAddedPerk.perk);
						flag = true;
					}
				}
			}
			_delayedAddedPerks = _delayedAddedPerks.FindAll((MonsterDelayedPerk x) => x.count > 0);
			if (VisualMonster != null)
			{
				VisualMonster.InformTrigger(trigger);
				if (flag)
				{
					VisualMonster.UpdateSkills();
				}
			}
			onPerformed();
		}

		protected virtual ActionBit GenerateStandartAttackBit()
		{
			ActionBit actionBit = SkillFabric.CreateAttack(signature: new ActionBitSignature(SkillType.Attack, TriggerType.Attack, "standartAttack", "attack"), trigger: (data.monsterClass == Class.Building) ? new BitTrigger(SkillStaticData.BuildingAttackTrigger) : new BitTrigger(SkillStaticData.StandartAttackTrigger), rangeDelegate: IsRanged, attackDel: () => _attack.GetValue());
			if (_curController != null)
			{
				actionBit.Init(this, _curController, parameters, () => coords, _random, !Animated);
			}
			return actionBit;
		}

		public void Cleanse()
		{
			_attack = _attack.Cleanse();
			_maxHealth = _maxHealth.Cleanse();
			_silenceData.silencedSkills = new List<ActionBitSignature>();
			int num = 0;
			while (num < _actions.Count)
			{
				SkillStaticData skillByName = SkillDataHelper.GetSkillByName(_actions[num].GetSignature().skillId);
				if (skillByName != null && skillByName.isNegative)
				{
					_silenceData.silencedSkills.Add(_actions[num].GetSignature());
					_actions.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			num = 0;
			while (num < _perks.Count)
			{
				SkillStaticData skillByName2 = SkillDataHelper.GetSkillByName(_perks[num].GetSignature().skillId);
				if (skillByName2 != null && skillByName2.isNegative)
				{
					_silenceData.silencedSkills.Add(_perks[num].GetSignature());
					_perks.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			_delayedAddedSkills = _delayedAddedSkills.FindAll((MonsterDelayedSkill x) => _actions.Find((ActionBit y) => x.skill == y) != null);
			_delayedAddedPerks = _delayedAddedPerks.FindAll((MonsterDelayedPerk x) => _perks.Find((PerkBit y) => x.perk == y) != null);
			if (VisualMonster != null)
			{
				VisualMonster.InformCleanse();
				VisualMonster.UpdateSkills();
			}
		}

		public void UpdateMaxHealth()
		{
			if ((int)_health > (int)data.health)
			{
				_maxHealth = new MonsterParam(_health);
			}
			else
			{
				_maxHealth = new MonsterParam(data.health);
			}
		}

		public void HandleParamsChanged(ParamType param, int delta)
		{
			switch (param)
			{
			case ParamType.Attack:
			{
				int value = _attack.GetValue();
				_attack = new MonsterParam(value + delta);
				if (value != _attack.GetValue() && VisualMonster != null)
				{
					VisualMonster.BounceAttack();
				}
				break;
			}
			case ParamType.Health:
			{
				int num = _health;
				_health = num + delta;
				if (num != (int)_health && VisualMonster != null)
				{
					VisualMonster.BounceHealth();
				}
				break;
			}
			}
			if (VisualMonster != null)
			{
				VisualMonster.UpdateParameters();
			}
		}

		public void HandleEvolveBoost(int currentInnerId)
		{
			List<PerkBit> list = GeneratePerks();
			List<ActionBit> list2 = GenerateSkills();
			PerkBit perkBit = null;
			foreach (PerkBit perk in list)
			{
				perkBit = _perks.Find((PerkBit x) => x.GetSignature().name == perk.GetSignature().name);
				if (perkBit != null)
				{
					_perks.Remove(perkBit);
					_perks.Add(perk);
				}
			}
			ActionBit actionBit = null;
			foreach (ActionBit action in list2)
			{
				actionBit = _actions.Find((ActionBit x) => x.GetSignature().name == action.GetSignature().name);
				if (actionBit != null)
				{
					_actions.Remove(actionBit);
					_actions.Add(action);
				}
			}
			MonsterStaticInnerData innerStaticData = data.GetInnerStaticData(currentInnerId);
			MonsterData monsterData = MonsterDataUtils.CreateMonster(data.monster_id, data.totalPromoteNum - 1);
			int num = innerStaticData.attack - (int)monsterData.attack;
			int num2 = innerStaticData.health - (int)monsterData.health;
			ForceSetParam(this, ParamType.Health, (int)Health + num2, TriggerType.NoTrigger, -1);
			ForceSetParam(this, ParamType.Attack, Attack + num, TriggerType.NoTrigger, -1);
		}

		protected virtual List<ActionBit> GenerateSkills()
		{
			List<ActionBit> list = new List<ActionBit>();
			for (int i = 0; i < data.skills.Count; i++)
			{
				SkillStaticData skillStaticData = data.skills[i];
				if (skillStaticData.IsPerk)
				{
					continue;
				}
				string count = data.skillValues[i] ?? "";
				ActionBit actionBit = SkillFabric.CreateSkill(skillStaticData, count, IsRanged, () => _attack.GetValue(), () => _maxHealth.GetValue(), _random, !Animated);
				if (actionBit == null)
				{
					continue;
				}
				if (_curController != null)
				{
					actionBit.Init(this, _curController, parameters, () => coords, _random, !Animated);
				}
				list.Add(actionBit);
			}
			return list;
		}

		protected virtual List<PerkBit> GeneratePerks()
		{
			List<PerkBit> list = new List<PerkBit>();
			for (int i = 0; i < data.skills.Count; i++)
			{
				SkillStaticData staticData = data.skills[i];
				string count = data.skillValues[i] ?? "";
				PerkBit perkBit = SkillFabric.CreatePerk(staticData, count, !Animated);
				if (perkBit != null)
				{
					perkBit.Init(this);
					list.Add(perkBit);
				}
			}
			return list;
		}

		public bool IsRanged()
		{
			return data.monsterClass == Class.Ranged;
		}

		public bool Collided(Vector3 position)
		{
			return VisualMonster.Collided(position);
		}

		public void CopySkills(FieldMonster target)
		{
			foreach (ActionBit action in target._actions)
			{
				if (action.GetSignature().name == "standartAttack" && action.GetSignature().trigger == TriggerType.Attack)
				{
					continue;
				}
				ActionBit actionBit = SkillFabric.CreateSkill(SkillDataHelper.GetSkillByName(action.GetSignature().skillId), action.GetSignature().strValue, IsRanged, () => _attack.GetValue(), () => _maxHealth.GetValue(), _random, !Animated);
				if (actionBit == null)
				{
					continue;
				}
				if (_curController != null)
				{
					actionBit.Init(this, _curController, parameters, () => coords, _random, !Animated);
				}
				_actions.Add(actionBit);
			}
			foreach (PerkBit perk in target._perks)
			{
				PerkBit perkBit = SkillFabric.CreatePerk(SkillDataHelper.GetSkillByName(perk.GetSignature().skillId), perk.GetSignature().strValue, !Animated);
				if (perkBit != null)
				{
					perkBit.Init(this);
					_perks.Add(perkBit);
				}
			}
			foreach (MonsterDelayedSkill delayedSkill in target._delayedSkills)
			{
				_delayedSkills.Add(new MonsterDelayedSkill
				{
					count = delayedSkill.count,
					skillVal = delayedSkill.skillVal,
					reduceTrigger = delayedSkill.reduceTrigger,
					skill = delayedSkill.skill
				});
			}
			foreach (MonsterDelayedPerk delayedPerk in target._delayedPerks)
			{
				_delayedPerks.Add(new MonsterDelayedPerk
				{
					count = delayedPerk.count,
					reduceTrigger = delayedPerk.reduceTrigger,
					perk = delayedPerk.perk
				});
			}
			foreach (MonsterDelayedSkill delayedAddedSkill in target._delayedAddedSkills)
			{
				_delayedAddedSkills.Add(new MonsterDelayedSkill
				{
					count = delayedAddedSkill.count,
					skillVal = delayedAddedSkill.skillVal,
					reduceTrigger = delayedAddedSkill.reduceTrigger,
					skill = delayedAddedSkill.skill
				});
			}
			foreach (MonsterDelayedPerk delayedAddedPerk in target._delayedAddedPerks)
			{
				_delayedAddedPerks.Add(new MonsterDelayedPerk
				{
					count = delayedAddedPerk.count,
					reduceTrigger = delayedAddedPerk.reduceTrigger,
					perk = delayedAddedPerk.perk
				});
			}
			if (VisualMonster != null)
			{
				VisualMonster.UpdateSkills();
			}
		}

		public void CopyStatus(FieldMonster target)
		{
			_attack = target._attack.Clone();
			_health = target._health;
			_maxHealth = target._maxHealth.Clone();
			_actions = new List<ActionBit>();
			foreach (ActionBit action in target._actions)
			{
				if (action.GetSignature().name == "standartAttack")
				{
					_standartAttack = GenerateStandartAttackBit();
					_actions.Add(_standartAttack);
					continue;
				}
				ActionBit actionBit = SkillFabric.CreateSkill(SkillDataHelper.GetSkillByName(action.GetSignature().name), action.GetSignature().strValue, IsRanged, () => _attack.GetValue(), () => _maxHealth.GetValue(), _random, !Animated);
				if (actionBit == null)
				{
					continue;
				}
				if (_curController != null)
				{
					actionBit.Init(this, _curController, parameters, () => coords, _random, !Animated);
				}
				_actions.Add(actionBit);
			}
			_perks = new List<PerkBit>();
			foreach (PerkBit perk in target._perks)
			{
				PerkBit perkBit = SkillFabric.CreatePerk(SkillDataHelper.GetSkillByName(perk.GetSignature().skillId), perk.GetSignature().strValue, !Animated);
				if (perkBit != null)
				{
					perkBit.Init(this);
					_perks.Add(perkBit);
				}
			}
			_delayedSkills = new List<MonsterDelayedSkill>();
			foreach (MonsterDelayedSkill delayedSkill in target._delayedSkills)
			{
				_delayedSkills.Add(new MonsterDelayedSkill
				{
					count = delayedSkill.count,
					skillVal = delayedSkill.skillVal,
					reduceTrigger = delayedSkill.reduceTrigger,
					skill = delayedSkill.skill
				});
			}
			_delayedPerks = new List<MonsterDelayedPerk>();
			foreach (MonsterDelayedPerk delayedPerk in target._delayedPerks)
			{
				_delayedPerks.Add(new MonsterDelayedPerk
				{
					count = delayedPerk.count,
					reduceTrigger = delayedPerk.reduceTrigger,
					perk = delayedPerk.perk
				});
			}
			_delayedAddedSkills = new List<MonsterDelayedSkill>();
			foreach (MonsterDelayedSkill delayedAddedSkill in target._delayedAddedSkills)
			{
				_delayedAddedSkills.Add(new MonsterDelayedSkill
				{
					count = delayedAddedSkill.count,
					skillVal = delayedAddedSkill.skillVal,
					reduceTrigger = delayedAddedSkill.reduceTrigger,
					skill = delayedAddedSkill.skill
				});
			}
			_delayedAddedPerks = new List<MonsterDelayedPerk>();
			foreach (MonsterDelayedPerk delayedAddedPerk in target._delayedAddedPerks)
			{
				_delayedAddedPerks.Add(new MonsterDelayedPerk
				{
					count = delayedAddedPerk.count,
					reduceTrigger = delayedAddedPerk.reduceTrigger,
					perk = delayedAddedPerk.perk
				});
			}
			if (VisualMonster != null)
			{
				VisualMonster.UpdateParameters();
				VisualMonster.UpdateSkills();
			}
		}
	}
}
