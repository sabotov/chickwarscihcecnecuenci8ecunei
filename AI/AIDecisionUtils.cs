using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.AI
{
	public static class AIDecisionUtils
	{
		public static AIDecision GetDecisionByIQStatic(List<AIDecision> sortedDecisions, float IQ)
		{
			if (sortedDecisions == null)
			{
				Debug.LogError("GetDecisionByIQStatic. sortedDecisions == null");
				return null;
			}
			if (sortedDecisions.Count == 0)
			{
				Debug.LogError("GetDecisionByIQStatic. sortedDecisions.Count == 0");
				return null;
			}
			if (sortedDecisions.Any((AIDecision dec) => dec.isDefeat))
			{
				return sortedDecisions[0];
			}
			foreach (AIDecision sortedDecision in sortedDecisions)
			{
				if (sortedDecision.isWin)
				{
					return sortedDecision;
				}
			}
			int num = (int)((1f - IQ) * (float)(sortedDecisions.Count - 1));
			int randomInt = Common.GetRandomInt(0, num + 1);
			return sortedDecisions[randomInt];
		}

		public static List<AIDecision> GetBestDecisions(List<AIDecision> decisions)
		{
			if (decisions.Count <= TestUtilFunctions.GetBestDecisionsCount())
			{
				return decisions;
			}
			return decisions.GetRange(0, TestUtilFunctions.GetBestDecisionsCount());
		}
	}
}
