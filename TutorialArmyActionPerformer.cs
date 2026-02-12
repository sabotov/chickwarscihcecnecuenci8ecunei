using System;
using System.Collections.Generic;
using System.Linq;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using DG.Tweening;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MapData;
using NewAssets.Scripts.DataClasses.MonsterParams;
using NewAssets.Scripts.UtilScripts;
using UnityEngine;

namespace BattlefieldScripts
{
	public class TutorialArmyActionPerformer : ArmyActionPerformer
	{
		private List<TutorialStepData> _steps;

		private int _curStep;

		private Action<TriggerType> _onSkill;

		private bool _breakFlag;

		public void AttachStepsInfo(List<TutorialStepData> steps)
		{
			_steps = steps;
			_curStep = 0;
		}

		public override void PerformPlacingChoose(List<MonsterData> hand, List<TriggerType> skills, Action<MonsterData, Vector2> onChosen, Action<TriggerType> onSkill, bool isAfterSkill = false, int currentTurn = 0)
		{
			_onSkill = onSkill;
			if ((hand.Count == 0 || !_parameters.GetClassedTiles(Class.NoClass, _side).Any()) && skills.Count == 0)
			{
				onChosen(null, Vector2.zero);
				return;
			}
			if (_steps.Count > _curStep)
			{
				TutorialStepData curStep = _steps[_curStep];
				if (curStep.isMonster)
				{
					MonsterData monsterData = hand.Find((MonsterData x) => x.monster_id == curStep.monId);
					bool flag = _parameters.GetClassedTiles(Class.NoClass, _side).Contains(curStep.position);
					if (monsterData != null && flag)
					{
						onChosen(monsterData, curStep.position);
						_curStep++;
						return;
					}
				}
				else
				{
					TriggerType triggerType = TriggerType.NoTrigger;
					switch (curStep.trNum)
					{
					case 1:
						triggerType = TriggerType.WarlordSkill1;
						break;
					case 2:
						triggerType = TriggerType.WarlordSkill2;
						break;
					case 3:
						triggerType = TriggerType.WarlordSkill3;
						break;
					case 4:
						triggerType = TriggerType.WarlordSkill4;
						break;
					}
					if (skills.Contains(triggerType))
					{
						AnimateSkillSelected(triggerType);
						_curStep++;
						return;
					}
				}
			}
			if (!_parameters.GetClassedTiles(Class.NoClass, _side).Any())
			{
				AnimateSkillSelected(skills[0]);
				return;
			}
			foreach (MonsterData item in hand)
			{
				foreach (Vector2 classedTile in _parameters.GetClassedTiles(Class.NoClass, _side))
				{
					if (_parameters.GetClassedTiles(item.monsterClass, _side).Contains(classedTile))
					{
						onChosen(item, classedTile);
						return;
					}
				}
			}
		}

		private void AnimateSkillSelected(TriggerType trigger)
		{
			if (_onSkill == null)
			{
				return;
			}
			SkillCard skillCard = SkillCard.CreateCard(_parameters.GetWarlord(_side).VisualMonster.transform.parent);
			skillCard.transform.localScale = new Vector3(0f, 0f, 1f);
			skillCard.transform.localPosition = _parameters.GetWarlord(_side).VisualMonster.transform.localPosition;
			for (int i = 0; i < _parameters.GetWarlord(_side).data.skills.Count && i < _parameters.GetWarlord(_side).data.skillValues.Count; i++)
			{
				if (_parameters.GetWarlord(_side).data.skills[i].trigger == trigger)
				{
					string value = GetValue(_parameters.GetWarlord(_side).data.skillValues[i]);
					skillCard.Init(_parameters.GetWarlord(_side).data.skills[i], value);
					break;
				}
			}
			float step = 0f;
			float scaleMult = 1.7f;
			float xPositionStart = skillCard.transform.position.x;
			float shift = ((_side == ArmySide.Left) ? 1 : (-1)) * 200;
			DOTween.To(() => step, delegate(float x)
			{
				step = x;
				skillCard.transform.localScale = new Vector3(step * scaleMult, step * scaleMult, 1f);
				skillCard.transform.position = new Vector3(xPositionStart + shift * step, skillCard.transform.position.y, skillCard.transform.position.z);
			}, 1f, TimeDebugController.instance.skillCardUsageTweenTime * TimeDebugController.totalTimeMultiplier).OnComplete(delegate
			{
				if (!_breakFlag)
				{
					Initializer.WaitForProcedure(TimeDebugController.instance.skillCardUsageTweenDelay / TimeDebugController.totalTimeMultiplier, delegate
					{
						skillCard.TweenAlpha(0f, 0.2f);
						Initializer.WaitForProcedure(0.5f, skillCard.Destroy);
					});
					_onSkill(trigger);
					_onSkill = null;
				}
			}).SetEase(Ease.InCubic)
				.Play();
		}

		public override void InformStop()
		{
			_breakFlag = true;
			base.InformStop();
		}
	}
}
