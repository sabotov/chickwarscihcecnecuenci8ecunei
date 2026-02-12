using System;
using System.Collections.Generic;

namespace BattlefieldScripts.Core
{
	public class IteratorCore
	{
		public virtual void IterateOnActions<T>(IEnumerable<T> elements, Action<T, Action> action, Action onCompleted)
		{
			foreach (T element in elements)
			{
				action(element, delegate
				{
				});
			}
			onCompleted();
		}

		public virtual void Break()
		{
		}
	}
}
