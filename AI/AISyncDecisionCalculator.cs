using System;
using System.Collections.Generic;
using NewAssets.Scripts.DataClasses.UserData;
using NewAssets.Scripts.UtilScripts;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.AI
{
	public class AISyncDecisionCalculator
	{
		private int _remainingDecisionCount;

		private AIDecisionMaker.DecisionListVoidDelegate _onChosen;

		private readonly List<AIDecision> _allDecisions = new List<AIDecision>();

		public void MakeDecision(List<AIDecisionMaker.DecisionStepDelegate> decisionDelegates, AIDecisionMaker.DecisionListVoidDelegate onChosen, bool inOneFrame)
		{
			_onChosen = onChosen;
			_allDecisions.Clear();
			if (decisionDelegates.Count == 0)
			{
				if (TestUtilFunctions.useDebugAI)
				{
					Debug.LogError("AI_TESTING: decisionDelegates.Count == 0");
				}
				OnAllDecisionReceived();
				return;
			}
			if (Constants.show_ai_logs && TestUtilFunctions.useDebugAI)
			{
				Debug.LogError("AI_TESTING: AISyncDecisionCalculator Created decisionDelegates");
			}
			_remainingDecisionCount = decisionDelegates.Count;
			List<Common.VoidDelegate> outerDelegates = new List<Common.VoidDelegate>();
			for (int num = decisionDelegates.Count - 1; num >= 0; num--)
			{
				AIDecisionMaker.DecisionVoidDelegate onComplete;
				if (num == decisionDelegates.Count - 1)
				{
					onComplete = RegisterDecisionCalculated;
				}
				else
				{
					int count = outerDelegates.Count - 1;
					onComplete = delegate(AIDecision curDecision)
					{
						RegisterDecisionCalculated(curDecision);
						if (inOneFrame)
						{
							outerDelegates[count]();
						}
						else
						{
							Initializer.WaitForOneFrame(delegate
							{
								outerDelegates[count]();
							});
						}
					};
				}
				int index = num;
				Common.VoidDelegate item = delegate
				{
					try
					{
						decisionDelegates[index](onComplete);
					}
					catch (Exception ex)
					{
						Debug.LogError("Error in AISyncDecisionCalculator. " + ex.Message + ".trace: " + ex.StackTrace);
						onComplete(null);
					}
				};
				outerDelegates.Add(item);
			}
			if (outerDelegates.Count <= 0)
			{
				if (Constants.show_ai_logs && TestUtilFunctions.useDebugAI)
				{
					Debug.LogError("AI_TESTING: AISyncDecisionCalculator outerDelegates == 0 execute onAllDecisionsReceived");
				}
				OnAllDecisionReceived();
				return;
			}
			if (Constants.show_ai_logs && TestUtilFunctions.useDebugAI)
			{
				Debug.LogError("AI_TESTING: AISyncDecisionCalculator outerDelegates > 0 execute outerDelegates");
			}
			if (inOneFrame)
			{
				outerDelegates[outerDelegates.Count - 1]();
				return;
			}
			Initializer.WaitForOneFrame(delegate
			{
				outerDelegates[outerDelegates.Count - 1]();
			});
		}

		private void OnAllDecisionReceived()
		{
			if (Constants.show_ai_logs && TestUtilFunctions.useDebugAI)
			{
				Debug.LogError("[AI_TESTING: AISyncDecisionCalculator onAllDecisionsReceived");
			}
			List<AIDecision> list = new List<AIDecision>(_allDecisions);
			list.Sort((AIDecision d1, AIDecision d2) => d2.profit.CompareTo(d1.profit));
			_onChosen(list);
		}

		private void RegisterDecisionCalculated(AIDecision decision)
		{
			_remainingDecisionCount--;
			if (decision != null)
			{
				_allDecisions.Add(decision);
			}
			if (_remainingDecisionCount <= 0)
			{
				OnAllDecisionReceived();
			}
		}
	}
}
