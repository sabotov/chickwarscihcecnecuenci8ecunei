using System;
using System.Collections.Generic;
using NewAssets.Scripts.DataClasses.UserData;
using NewAssets.Scripts.UtilScripts;
using Threads;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.AI
{
	public class AIMultiThreadDecisionCalculator
	{
		private int _remainingDecisionCount;

		private AIDecisionMaker.DecisionListVoidDelegate _onChosen;

		private readonly List<AIDecision> _allDecisions = new List<AIDecision>();

		private object _remainingLock = new object();

		public void MakeDecision(List<AIDecisionMaker.DecisionStepDelegate> decisionDelegates, AIDecisionMaker.DecisionListVoidDelegate onChosen)
		{
			_onChosen = onChosen;
			if (decisionDelegates.Count == 0)
			{
				if (TestUtilFunctions.useDebugAI)
				{
					Debug.LogError("decisionDelegates.Count == 0");
				}
				OnAllDecisionReceived();
				return;
			}
			if (Constants.show_ai_logs && TestUtilFunctions.useDebugAI)
			{
				Debug.LogError("AI_TESTING: AIMultiThreadDecisionCalculator Created decisionDelegates");
			}
			_remainingDecisionCount = decisionDelegates.Count;
			int processorCount = Initializer.ProcessorCount;
			if (Constants.show_ai_logs && TestUtilFunctions.useDebugAI)
			{
				Debug.LogError($"AI_TESTING: AIMultiThreadDecisionCalculator More then one thread. threadsCount = {processorCount}");
			}
			int count = decisionDelegates.Count;
			processorCount = Mathf.Clamp(processorCount, 1, count);
			int num = count / processorCount;
			int num2 = count % processorCount;
			Action<List<AIDecisionMaker.DecisionStepDelegate>> threadOperation = delegate(List<AIDecisionMaker.DecisionStepDelegate> operations)
			{
				for (int i = 0; i < operations.Count; i++)
				{
					try
					{
						operations[i](RegisterDecisionCalculated);
					}
					catch (Exception ex)
					{
						Debug.LogError("Error in AIMultiThreadDecisionCalculator. " + ex.Message + ".trace: " + ex.StackTrace);
						RegisterDecisionCalculated(null);
					}
				}
			};
			for (int num3 = 0; num3 < processorCount; num3++)
			{
				int num4 = num3 * num;
				int b = count - num4;
				int count2 = Mathf.Min(num, b);
				List<AIDecisionMaker.DecisionStepDelegate> decisionSteps = decisionDelegates.GetRange(num4, count2);
				if (num3 < num2)
				{
					decisionSteps.Add(decisionDelegates[count - 1 - num3]);
				}
				CustomThreadPool.QueueUserTask(delegate
				{
					threadOperation(decisionSteps);
				}, delegate
				{
				});
			}
		}

		private void OnAllDecisionReceived()
		{
			if (Constants.show_ai_logs && TestUtilFunctions.useDebugAI)
			{
				Debug.LogError("AI_TESTING: AIMultiThreadDecisionCalculator onAllDecisionsReceived");
			}
			List<AIDecision> result = new List<AIDecision>(_allDecisions);
			result.Sort((AIDecision d1, AIDecision d2) => d2.profit.CompareTo(d1.profit));
			MainThreadDispatcher.Invoke((Common.VoidDelegate)delegate
			{
				_onChosen(result);
			});
		}

		private void RegisterDecisionCalculated(AIDecision decision)
		{
			lock (_remainingLock)
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
}
