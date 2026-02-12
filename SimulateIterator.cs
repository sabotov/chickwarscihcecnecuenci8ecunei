using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;

namespace BattlefieldScripts
{
	public class SimulateIterator : IteratorCore
	{
		private bool _breakFlag;

		public override void IterateOnActions<T>(IEnumerable<T> elements, Action<T, Action> action, Action onCompleted)
		{
			IEnumerator<T> enumerator = elements.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					action(enumerator.Current, delegate
					{
					});
					if (_breakFlag)
					{
						enumerator.Dispose();
						return;
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("IterateOnActions " + ex);
			}
			finally
			{
				enumerator.Dispose();
			}
			onCompleted();
		}

		public override void Break()
		{
			_breakFlag = true;
		}
	}
}
