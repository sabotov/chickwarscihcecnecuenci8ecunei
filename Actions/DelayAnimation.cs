using System;
using System.Collections.Generic;
using NewAssets.Scripts.UtilScripts;
using ServiceLocator;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class DelayAnimation : BitActionAnimation
	{
		private readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		private readonly float _delay;

		private IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		public DelayAnimation(float delay)
		{
			_delay = delay;
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			_delayedActionsHandler.WaitForProcedure(_delay, delegate
			{
				foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
				{
					item.Key();
				}
				onEnded();
			});
		}
	}
}
