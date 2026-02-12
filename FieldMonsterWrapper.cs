using System.Collections.Generic;
using Assets.Scripts.UI_Scripts.UI_Controller_Scripts.BattleControllers;
using DG.Tweening;
using MyVisual;
using NGUI.Scripts.UI;
using NewAssets.Scripts.UtilScripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace BattlefieldScripts
{
	public class FieldMonsterWrapper : MonoBehaviourExt
	{
		public UILabel monsterClassLabel;

		public UILabel hpLabel;

		public UILabel goldLabel;

		public UILabel atkLabel;

		public UISprite hpGoodGlance;

		public UISprite hpBadGlance;

		public UISprite atkGoodGlance;

		public UISprite atkBadGlance;

		public Transform hpContainer;

		public Transform goldContainer;

		public Transform atkContainer;

		public bool TryShowNewSkills;

		public List<UISprite> skills;

		public SkillsDescriptionController SkillsDescriptionController;

		public UISprite hpSprite;

		public UISprite meleeSprite;

		public UISprite bowSprite;

		public UISprite shadow;

		public Transform collideElement;

		public AnimatedMonster image;

		public EventAnimatorElement infoIcon;

		[Header("Death gold animation")]
		public Transform EffectsContainer;

		public float animationDelay;

		[FormerlySerializedAs("fadeTime")]
		public float shakeTime;

		public float fadeTime;

		public float fadeDelay;

		public Ease shakeEase;

		public float shakeMult;

		public float shakeXAmpl;

		public float shakeYAmpl;

		public float effectLifeTime;

		public float effectScale;
	}
}
