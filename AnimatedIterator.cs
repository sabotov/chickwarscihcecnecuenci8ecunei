using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;

namespace BattlefieldScripts
{
	public class AnimatedIterator : IteratorCore
	{
		private readonly List<Action> _breakActions = new List<Action>();

		public override void IterateOnActions<T>(IEnumerable<T> elements, Action<T, Action> action, Action onCompleted)
		{
			bool breakFlag = false;
			Action onComplete = delegate
			{
			};
			Action breakAction = delegate
			{
				breakFlag = true;
			};
			_breakActions.Add(breakAction);
			List<Action> actions = new List<Action>();
			int i = -1;
			onComplete = delegate
			{
				if (!breakFlag)
				{
					i++;
					if (actions.Count <= i)
					{
						onCompleted();
						_breakActions.Remove(breakAction);
					}
					else
					{
						actions[i]();
					}
				}
			};
			foreach (T element2 in elements)
			{
				T element1 = element2;
				actions.Add(delegate
				{
					action(element1, onComplete);
				});
			}
			onComplete();
		}

		public override void Break()
		{
			Action[] array = new Action[_breakActions.Count];
			_breakActions.CopyTo(array);
			Action[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i]();
			}
		}
	}
}
