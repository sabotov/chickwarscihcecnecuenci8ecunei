using System;
using System.Collections.Generic;
using BattlefieldScripts.Actions;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.Data_Helpers;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts
{
	public static class SkillFabric
	{
		public static ActionBit CreateSkill(SkillStaticData staticData, string count, Common.BoolDelegate rangeDelegate, ParamIntDelegate attackDel, ParamIntDelegate healthDelegate, FieldRandom random, bool withoutAnimation = false)
		{
			ActionBitSignature signature = new ActionBitSignature(staticData.skill, staticData.trigger, staticData.strId, staticData.strId, (staticData.skill == SkillType.AddSkill) ? staticData.value : "", staticData.isNegative);
			BitTrigger trigger = new BitTrigger(staticData.triggerBit);
			BitTrigger affectedTrigger = ((staticData.triggerAffectedBit == null) ? null : new BitTrigger(staticData.triggerAffectedBit));
			ParamIntValueClass paramIntValueClass = new ParamIntValueClass(() => 0);
			ParamIntValueClass paramIntValueClass2 = new ParamIntValueClass(() => 0);
			int num = 0;
			int intValue2;
			if (int.TryParse(count, out var intValue))
			{
				paramIntValueClass = new ParamIntValueClass(() => intValue);
				paramIntValueClass2 = new ParamIntValueClass(() => -intValue);
				num = intValue;
			}
			else if (count.Split('-').Length == 2 && int.TryParse(count.Split('-')[0], out intValue) && int.TryParse(count.Split('-')[1], out intValue2))
			{
				paramIntValueClass = new ParamIntValueClass(() => random.GetRange(intValue, intValue2 + 1));
				paramIntValueClass2 = new ParamIntValueClass(() => -random.GetRange(intValue, intValue2 + 1));
				num = intValue;
			}
			else if (count.Length >= 10 && count.Substring(0, 10) == "turnNumber")
			{
				bool multiplication = false;
				int multiplicator = 0;
				int adding = 0;
				bool flag = true;
				bool flag2 = true;
				string text = count.Replace(" ", "").Replace("turnNumber", "");
				for (int num2 = 0; num2 < text.Length; num2++)
				{
					char c = text[num2];
					switch (c)
					{
					case '*':
						multiplication = true;
						break;
					case '/':
						multiplication = false;
						break;
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						if (flag)
						{
							multiplicator *= 10;
							multiplicator += int.Parse(c.ToString() ?? "");
						}
						else
						{
							adding *= 10;
							adding += int.Parse(c.ToString() ?? "");
						}
						break;
					case '+':
						flag = false;
						break;
					case '-':
						flag2 = false;
						flag = false;
						break;
					}
				}
				if (!flag2)
				{
					adding *= -1;
				}
				paramIntValueClass = new ParamIntValueClass((int x) => (multiplication ? (x * multiplicator) : ((x - x % multiplicator) / multiplicator)) + adding);
				paramIntValueClass2 = new ParamIntValueClass((int x) => -(multiplication ? (x * multiplicator) : ((x - x % multiplicator) / multiplicator)) - adding);
			}
			if (staticData.valueBit != null)
			{
				BitFilter multiplierFilter = new BitFilter(staticData.valueBit);
				paramIntValueClass.AttachFiler(multiplierFilter, staticData.filterMode);
				paramIntValueClass2.AttachFiler(multiplierFilter, staticData.filterMode);
			}
			signature.levelSkill = staticData.levelSkill;
			signature.value = num;
			signature.strValue = count;
			if (staticData.filterBit == null)
			{
				Debug.LogError("Call Grisha! Problems in skill " + staticData.strId);
			}
			BitFilter filter = new BitFilter(staticData.filterBit, num);
			BitActionAnimation bitActionAnimation = new BitActionAnimation();
			if (!withoutAnimation)
			{
				bitActionAnimation = CreateAnimation(staticData);
			}
			BitAction bitAction = null;
			if (staticData.filterMode == ValueFilterMode.PercentForEachMonster || staticData.filterMode == ValueFilterMode.PercentForOneMonster)
			{
				paramIntValueClass.SetPercentParamValue(staticData.percentParameter, num, staticData.filterMode);
				paramIntValueClass2.SetPercentParamValue(staticData.percentParameter, -num, staticData.filterMode);
			}
			switch (staticData.skill)
			{
			case SkillType.Summon:
				if (staticData.filterMode == ValueFilterMode.No)
				{
					MonsterData singletoneMonster = MonsterDataUtils.GetSingletoneMonster(int.Parse(staticData.value), intValue);
					singletoneMonster.isLevelMonster = true;
					bitAction = new SummonAction(bitActionAnimation, singletoneMonster);
					break;
				}
				if (staticData.filterMode == ValueFilterMode.Monster || staticData.filterMode == ValueFilterMode.MonsterData)
				{
					bitAction = new SummonAction(bitActionAnimation, paramIntValueClass);
					break;
				}
				throw new NotImplementedException("Filter modes except No and Monster not supported for SSummon action");
			case SkillType.Reborn:
				bitAction = new RebornAction(bitActionAnimation);
				break;
			case SkillType.SummonRune:
			{
				RuneData runeByName = RuneDataHelper.GetRuneByName(staticData.value);
				bitAction = new SummonRuneAction(bitActionAnimation, runeByName, count);
				break;
			}
			case SkillType.Purify:
				bitAction = new PurifyAction(bitActionAnimation);
				break;
			case SkillType.Damage:
			case SkillType.Return:
				bitAction = new ChangeParamAction(bitActionAnimation, ParamType.Health, paramIntValueClass2);
				break;
			case SkillType.ChainAttack:
				if (!withoutAnimation)
				{
					bitActionAnimation = new ChainAttackAnimation(staticData.effectName);
				}
				bitAction = new ChangeParamAction(bitActionAnimation, ParamType.Health, paramIntValueClass2);
				break;
			case SkillType.Pierce:
				bitAction = new ChangeParamAction(bitActionAnimation, ParamType.Health, paramIntValueClass2);
				break;
			case SkillType.Bleeding:
				bitAction = new BleedingAction(bitActionAnimation, paramIntValueClass2);
				break;
			case SkillType.SplashAttack:
				bitAction = new ChangeParamAction(bitActionAnimation, ParamType.Health, paramIntValueClass2);
				break;
			case SkillType.RangeAttack:
				return CreateSplitAttack(trigger, rangeDelegate, signature, attackDel, filter, withoutAnimation);
			case SkillType.Heal:
				if (!withoutAnimation)
				{
					bitActionAnimation = new HealAnimation(bitActionAnimation);
				}
				bitAction = new ChangeParamAction(bitActionAnimation, ParamType.Health, paramIntValueClass);
				break;
			case SkillType.InstantKill:
				bitAction = new InstantKillAction(bitActionAnimation);
				break;
			case SkillType.Transform:
				if (staticData.filterMode == ValueFilterMode.No)
				{
					Func<MonsterData> transformTo = delegate
					{
						MonsterData singletoneMonster2 = MonsterDataUtils.GetSingletoneMonster(int.Parse(staticData.value), intValue);
						singletoneMonster2.isLevelMonster = true;
						return singletoneMonster2;
					};
					bitAction = new TransformAction(bitActionAnimation, transformTo);
				}
				else
				{
					if (staticData.filterMode != ValueFilterMode.Monster)
					{
						throw new NotImplementedException("Filter modes except No and Monster not supported for SSummon action");
					}
					bitAction = new TransformAction(bitActionAnimation, paramIntValueClass);
				}
				break;
			case SkillType.Regen:
				bitAction = new ChangeParamAction(bitActionAnimation, ParamType.Health, paramIntValueClass);
				break;
			case SkillType.Weakness:
				bitAction = new ChangeParamTempAction(bitActionAnimation, ParamType.Attack, paramIntValueClass2, staticData.durTrigger, staticData.duration);
				break;
			case SkillType.HealToMax:
				if (!withoutAnimation)
				{
					bitActionAnimation = new HealAnimation(bitActionAnimation);
				}
				bitAction = new HealToMaxAction(bitActionAnimation);
				break;
			case SkillType.ClearHealToMax:
				bitAction = new HealToMaxAction(bitActionAnimation);
				break;
			case SkillType.SetAttack:
				bitAction = new SetParamAction(bitActionAnimation, ParamType.Attack, paramIntValueClass);
				break;
			case SkillType.SetHealth:
				bitAction = new SetParamAction(bitActionAnimation, ParamType.Health, paramIntValueClass);
				break;
			case SkillType.SetHealthTemp:
				bitAction = new SetParamTempAction(bitActionAnimation, ParamType.Health, paramIntValueClass, staticData.durTrigger, staticData.duration);
				break;
			case SkillType.SetAttackTemp:
				bitAction = new SetParamTempAction(bitActionAnimation, ParamType.Attack, paramIntValueClass, staticData.durTrigger, staticData.duration);
				break;
			case SkillType.BuffAttack:
				bitAction = new ChangeParamTempAction(bitActionAnimation, ParamType.Attack, paramIntValueClass, staticData.durTrigger, staticData.duration);
				break;
			case SkillType.BuffHealth:
				bitAction = new ChangeParamTempAction(bitActionAnimation, ParamType.Health, paramIntValueClass, staticData.durTrigger, staticData.duration);
				break;
			case SkillType.Sleep:
				if (!withoutAnimation)
				{
					bitActionAnimation = new FreezeAnimation(bitActionAnimation, TriggerType.NewTurn);
				}
				bitAction = new AddSkillTempAction(bitActionAnimation, SkillDataHelper.GetSkillByName("AttackBlocker"), count, TriggerType.NewTurn, num);
				break;
			case SkillType.Stun:
				if (!withoutAnimation)
				{
					bitActionAnimation = new FreezeAnimation(bitActionAnimation);
				}
				bitAction = new AddSkillTempAction(bitActionAnimation, SkillDataHelper.GetSkillByName("AttackBlocker"), count, TriggerType.TurnEnded, num);
				break;
			case SkillType.GoldFreeze:
				if (!withoutAnimation)
				{
					bitActionAnimation = new FreezeAnimation(bitActionAnimation, TriggerType.TurnEnded, isGold: true);
				}
				bitAction = new AddSkillTempAction(bitActionAnimation, SkillDataHelper.GetSkillByName("GoldenFreeze"), count, TriggerType.TurnEnded, num);
				break;
			case SkillType.Silence:
				bitAction = new SilenceAction(bitActionAnimation, SkillStaticData.GetSkillByString(staticData.value));
				break;
			case SkillType.Cleanse:
				bitAction = new CleanseAction(bitActionAnimation);
				break;
			case SkillType.StealAttack:
				bitAction = new StealAction(bitActionAnimation, ParamType.Attack, paramIntValueClass2);
				break;
			case SkillType.Vampiric:
				bitAction = new VampiricAction(bitActionAnimation, ParamType.Health, paramIntValueClass2);
				break;
			case SkillType.AddSkill:
			{
				SkillStaticData skillByName6 = SkillDataHelper.GetSkillByName(staticData.value);
				if (skillByName6 != null)
				{
					bitAction = ((staticData.valueBit == null) ? new AddSkillAction(bitActionAnimation, skillByName6, count) : new AddSkillAction(bitActionAnimation, skillByName6, count, paramIntValueClass));
				}
				break;
			}
			case SkillType.AddSkillTemporary:
			{
				SkillStaticData skillByName5 = SkillDataHelper.GetSkillByName(staticData.value);
				if (skillByName5 != null)
				{
					bitAction = new AddSkillTempAction(bitActionAnimation, skillByName5, count, staticData.durTrigger, staticData.duration);
				}
				break;
			}
			case SkillType.RemoveSkill:
			{
				SkillStaticData skillByName4 = SkillDataHelper.GetSkillByName(staticData.value);
				if (skillByName4 != null)
				{
					bitAction = ((staticData.valueBit == null) ? new RemoveSkillAction(bitActionAnimation, skillByName4) : new RemoveSkillAction(bitActionAnimation, skillByName4));
				}
				break;
			}
			case SkillType.RemoveSkillTemporary:
			{
				SkillStaticData skillByName3 = SkillDataHelper.GetSkillByName(staticData.value);
				if (skillByName3 != null)
				{
					bitAction = new RemoveSkillTempAction(bitActionAnimation, skillByName3, staticData.durTrigger, staticData.duration);
				}
				break;
			}
			case SkillType.ChangeWarlordSkill:
			{
				SkillStaticData skillByName2 = SkillDataHelper.GetSkillByName(staticData.value);
				if (skillByName2 != null)
				{
					bitAction = new ChangeWarlordSkill(bitActionAnimation, skillByName2, count);
				}
				break;
			}
			case SkillType.ExtraAttack:
				if (staticData.triggerTarget.Replace(" ", "") == "self" && signature.trigger == TriggerType.AttackPerformed)
				{
					signature.trigger = TriggerType.Attack;
				}
				return CreateAttack(trigger, rangeDelegate, signature, attackDel, withoutAnimation);
			case SkillType.Attack:
				return CreateCounterAttack(trigger, rangeDelegate, signature, attackDel, staticData.filterBit, withoutAnimation);
			case SkillType.SwapStats:
				bitAction = new SwapStatsAction(bitActionAnimation);
				break;
			case SkillType.StealSkills:
			case SkillType.KillStealAttack:
			case SkillType.KillStealHealth:
			case SkillType.KillStealStats:
			case SkillType.KillStealSkills:
			case SkillType.KillStealAll:
			case SkillType.KillStealAttackFromHp:
			case SkillType.KillStealHpFromAttack:
				bitAction = new KillStealAction(bitActionAnimation, staticData.skill);
				break;
			case SkillType.AddWarlordSkill:
			{
				SkillStaticData skillByName = SkillDataHelper.GetSkillByName(staticData.value);
				if (skillByName != null)
				{
					bitAction = ((staticData.valueBit == null) ? new AddWarlordSkillAction(bitActionAnimation, skillByName, count) : new AddWarlordSkillAction(bitActionAnimation, skillByName, count, paramIntValueClass));
				}
				break;
			}
			case SkillType.ShuffleHand:
				bitAction = new ShuffleHandAction(bitActionAnimation);
				break;
			}
			if (bitAction == null || bitActionAnimation == null)
			{
				return null;
			}
			ActionBit actionBit = ((staticData.counter != -1) ? new ActionBit(signature, trigger, filter, bitAction, affectedTrigger, staticData.counter + 1) : new ActionBit(signature, trigger, filter, bitAction, affectedTrigger));
			actionBit.performCount = staticData.action_count;
			actionBit.repeatDelay = staticData.repeatDelay;
			if (staticData.rechargeConditionType != ConditionType.NoCondition)
			{
				BitFilter condition = new BitFilter(staticData.rechargeConditionBit);
				actionBit.InitRecharge(staticData.rechargeConditionType, staticData.rechargeConditionValue, condition);
			}
			return actionBit;
		}

		public static ActionBit CreateAttack(BitTrigger trigger, Common.BoolDelegate rangeDelegate, ActionBitSignature signature, ParamIntDelegate attackDel, bool withoutAnimation = false)
		{
			BitActionAnimation meleeAnimation;
			BitActionAnimation rangedAnimation;
			if (withoutAnimation)
			{
				meleeAnimation = new BitActionAnimation();
				rangedAnimation = new BitActionAnimation();
			}
			else
			{
				meleeAnimation = new MeleeAnimation();
				rangedAnimation = new RangedAnimation();
			}
			if (SkillStaticData.AttackFilter == null)
			{
				Debug.LogError("Call Grisha! Problems in SkillStaticData.AttackFilter ");
			}
			BitFilter filter = new BitFilter(SkillStaticData.AttackFilter);
			ParamIntValueClass val = new ParamIntValueClass(() => -attackDel());
			BitAction action = new AttackAction(meleeAnimation, rangedAnimation, val, rangeDelegate);
			return new ActionBit(signature, trigger, filter, action);
		}

		private static ActionBit CreateSplitAttack(BitTrigger trigger, Common.BoolDelegate rangeDelegate, ActionBitSignature signature, ParamIntDelegate attackDel, BitFilter filter, bool withoutAnimation = false)
		{
			BitActionAnimation meleeAnimation;
			BitActionAnimation rangedAnimation;
			if (withoutAnimation)
			{
				meleeAnimation = new BitActionAnimation();
				rangedAnimation = new BitActionAnimation();
			}
			else
			{
				meleeAnimation = new MeleeAnimation();
				rangedAnimation = new SplitRangedAnimation();
			}
			ParamIntValueClass val = new ParamIntValueClass(() => -attackDel());
			BitAction action = new AttackAction(meleeAnimation, rangedAnimation, val, rangeDelegate);
			return new ActionBit(signature, trigger, filter, action);
		}

		public static ActionBit CreateCounterAttack(BitTrigger trigger, Common.BoolDelegate rangeDelegate, ActionBitSignature signature, ParamIntDelegate attackDel, BitStaticFilter filter, bool withoutAnimation = false)
		{
			BitActionAnimation meleeAnimation;
			BitActionAnimation rangedAnimation;
			if (withoutAnimation)
			{
				meleeAnimation = new BitActionAnimation();
				rangedAnimation = new BitActionAnimation();
			}
			else
			{
				meleeAnimation = new MeleeStaticAnimation();
				rangedAnimation = new RangedAnimation();
			}
			if (SkillStaticData.AttackFilter == null)
			{
				Debug.LogError("Call Grisha! Problems in SkillStaticData.AttackFilter ");
			}
			ParamIntValueClass val = new ParamIntValueClass(() => -attackDel());
			BitFilter filter2 = new BitFilter(filter);
			BitAction action = new CounterAttackAction(meleeAnimation, rangedAnimation, val, rangeDelegate);
			return new ActionBit(signature, trigger, filter2, action);
		}

		public static PerkBit CreatePerk(SkillStaticData staticData, string count, bool withoutAnimation = false, int duration = -1, TriggerType durationTrigger = TriggerType.NoTrigger)
		{
			ActionBitSignature signature = new ActionBitSignature(staticData.skill, TriggerType.NoTrigger, staticData.strId, staticData.strId, (staticData.skill == SkillType.AddSkill) ? staticData.value : "", staticData.isNegative);
			BitActionAnimation onAttachedAnimation = null;
			BitActionAnimation onWorkedAnimation = null;
			PerkStat perkStat = null;
			switch (staticData.skill)
			{
			case SkillType.Immune:
			case SkillType.ClearImmune:
			case SkillType.ImmuneToFriend:
			case SkillType.ImmuneToEnemy:
			case SkillType.ImmuneToPositive:
			case SkillType.ImmuneToNegative:
				if (staticData.value == "")
				{
					perkStat = new ImmuneExeptStat(new List<SkillType> { SkillType.Attack }, staticData.skill);
				}
				else
				{
					List<SkillType> list = new List<SkillType>();
					string[] array = staticData.value.Replace(" ", "").Split(',');
					for (int num = 0; num < array.Length; num++)
					{
						list.Add(SkillStaticData.GetSkillByString(array[num]));
					}
					perkStat = new ImmuneStat(list, staticData.skill);
				}
				if (!withoutAnimation && staticData.skill != SkillType.ClearImmune)
				{
					onAttachedAnimation = ((duration != -1 && durationTrigger != TriggerType.NoTrigger) ? new ImmuneAnimation(null, durationTrigger, duration) : ((staticData.duration == -1 || staticData.durTrigger == TriggerType.NoTrigger) ? new ImmuneAnimation(null) : new ImmuneAnimation(null, staticData.durTrigger, staticData.duration)));
				}
				signature.value = 10000;
				signature.levelSkill = staticData.levelSkill;
				break;
			case SkillType.Miss:
			{
				if (!int.TryParse(count, out var result4))
				{
					result4 = 100;
				}
				if (result4 > 100)
				{
					result4 = 100;
				}
				perkStat = new MissStat(result4);
				signature.value = result4;
				signature.strValue = count;
				signature.levelSkill = staticData.levelSkill;
				break;
			}
			case SkillType.Accuracy:
			{
				if (!int.TryParse(count, out var result2))
				{
					result2 = 100;
				}
				if (result2 > 100)
				{
					result2 = 100;
				}
				perkStat = new AccuracyStat(result2);
				signature.value = result2;
				signature.strValue = count;
				signature.levelSkill = staticData.levelSkill;
				break;
			}
			case SkillType.EvadeMelee:
			case SkillType.EvadeRanged:
			{
				if (!int.TryParse(count, out var result3))
				{
					result3 = 100;
				}
				if (result3 > 100)
				{
					result3 = 100;
				}
				perkStat = new EvadeChanceStat(result3);
				signature.value = result3;
				signature.strValue = count;
				signature.levelSkill = staticData.levelSkill;
				break;
			}
			case SkillType.Block:
			{
				Common.IntDelegate intDelegate3 = () => 0;
				if (int.TryParse(count, out var intValue))
				{
					intDelegate3 = () => intValue;
				}
				perkStat = new IntModulateStat(intDelegate3);
				signature.value = intDelegate3();
				signature.strValue = count;
				signature.levelSkill = staticData.levelSkill;
				if (!withoutAnimation)
				{
					onWorkedAnimation = CreateAnimation(staticData);
				}
				break;
			}
			case SkillType.ClearBlock:
			{
				Common.IntDelegate intDelegate = () => 0;
				if (int.TryParse(count, out var blockValue))
				{
					intDelegate = () => blockValue;
				}
				perkStat = new IntModulateStat(intDelegate);
				signature.value = intDelegate();
				signature.strValue = count;
				signature.levelSkill = staticData.levelSkill;
				break;
			}
			case SkillType.DivineShield:
			{
				Common.IntDelegate intDelegate2 = () => 10000;
				if (!withoutAnimation)
				{
					onAttachedAnimation = new DivineShieldAnimation(null);
				}
				perkStat = new IntModulateStat(intDelegate2);
				signature.value = intDelegate2();
				signature.levelSkill = staticData.levelSkill;
				break;
			}
			case SkillType.AttackBlockedPerk:
			case SkillType.AttackBlocked:
			case SkillType.Mercy:
				perkStat = new PerkStat();
				break;
			case SkillType.Strong:
			{
				perkStat = new PerkStat();
				if (!int.TryParse(count, out var result))
				{
					throw new Exception("Parse exception: value cannot be " + count);
				}
				result = Mathf.Clamp(result, 0, 100);
				signature.value = result;
				signature.strValue = count;
				break;
			}
			}
			if (perkStat == null)
			{
				return null;
			}
			return new PerkBit(signature, perkStat)
			{
				onAttachedAnimation = onAttachedAnimation,
				onWorkedAnimation = onWorkedAnimation
			};
		}

		private static BitActionAnimation CreateAnimation(SkillStaticData data)
		{
			BitActionAnimation bitActionAnimation = new BitActionAnimation();
			if (data.effectName != "" && data.soundName != "")
			{
				bitActionAnimation = new SoundedEffectAnimation(data.effectName, data.soundName);
			}
			else if (data.effectName != "")
			{
				bitActionAnimation = new EffectAnimation(data.effectName);
			}
			else if (data.soundName != "")
			{
				bitActionAnimation = new SoundAnimation(data.soundName);
			}
			else if (data.skill == SkillType.Summon || data.skill == SkillType.SummonRune)
			{
				bitActionAnimation = new DelayAnimation(TimeDebugController.instance.summonDelayTime / TimeDebugController.totalTimeMultiplier);
			}
			else if (data.skill == SkillType.Reborn)
			{
				bitActionAnimation = new DelayAnimation(TimeDebugController.instance.deathDelay / TimeDebugController.totalTimeMultiplier);
			}
			if (data.skill == SkillType.Pierce || data.skill == SkillType.SplashAttack || data.skill == SkillType.Return || data.skill == SkillType.Damage || data.skill == SkillType.Bleeding)
			{
				bitActionAnimation = new LabelDamageAnimation(bitActionAnimation);
			}
			if (!string.IsNullOrEmpty(data.selfEffectName))
			{
				bitActionAnimation = new SelfEffectAnimation(bitActionAnimation, data.selfEffectName);
			}
			if (data.value.Contains("each:"))
			{
				bitActionAnimation = new ValueCheckAnimation(bitActionAnimation);
			}
			return bitActionAnimation;
		}
	}
}
