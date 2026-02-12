using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class DivineShieldStaticAnimation : StaticAnimationBit
	{
		public override void InformTrigger(TriggerType trigger)
		{
			if (trigger == TriggerType.DivineShieldPerformed || trigger == TriggerType.ClearParams)
			{
				_curMonster.DeattachStaticAnimation(this);
				Object.Destroy(base.gameObject);
			}
		}
	}
}
