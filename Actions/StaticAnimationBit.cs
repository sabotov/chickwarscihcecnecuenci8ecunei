using NewAssets.Scripts.UtilScripts;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class StaticAnimationBit : MonoBehaviourExt
	{
		protected FieldMonsterVisual _curMonster;

		protected bool _shouldDeleteAnim;

		public bool GetShouldDeleteAnim()
		{
			return _shouldDeleteAnim;
		}

		public void Init(FieldMonsterVisual monster, bool shouldDeleteAnim = false)
		{
			_curMonster = monster;
			_curMonster.AttachStaticAnimation(this);
			_shouldDeleteAnim = shouldDeleteAnim;
		}

		public virtual void InformTrigger(TriggerType trigger)
		{
			Debug.LogError("InformTrigger should be overwritten");
			_curMonster.DeattachStaticAnimation(this);
			Object.Destroy(base.gameObject);
		}

		public virtual void InformCleanse()
		{
		}

		public void InformDeath()
		{
			InformTrigger(TriggerType.ClearParams);
		}
	}
}
