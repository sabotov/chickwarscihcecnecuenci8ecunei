using System;
using System.Collections.Generic;
using NewAssets.Scripts.UtilScripts;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class AlphaAppearAnimation : BitActionAnimation
	{
		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
			{
				Debug.LogWarning("TopAppearAnimation isn't supported!");
				item.Key();
			}
			Initializer.WaitForProcedure(0.4f, delegate
			{
				onEnded();
			});
		}
	}
}
