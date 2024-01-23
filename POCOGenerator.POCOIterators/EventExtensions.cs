using System.Linq;
using POCOGenerator.POCOIterators;

namespace System
{
	internal static class EventExtensions
	{
		public static TEventArgs Raise<TEventArgs>(this EventHandler<TEventArgs> handler,
												   object sender,
												   Func<TEventArgs> argsHandler)
			where TEventArgs : EventArgs
		{
			if (handler != null)
			{
				TEventArgs args = argsHandler();
				foreach (EventHandler<TEventArgs> listener in handler.GetInvocationList()
																	 .Cast<EventHandler<TEventArgs>>())
				{
					listener.Invoke(sender, args);
					if (args is IStopGenerating { Stop: true })
					{
						return args;
					}
				}
				return args;
			}
			return null;
		}

		public static TEventArgs RaiseAsync<TEventArgs>(this EventHandler<TEventArgs> handler,
														object sender,
														Func<TEventArgs> argsHandler)
			where TEventArgs : EventArgs
		{
			if (handler != null)
			{
				TEventArgs args = argsHandler();
				foreach (EventHandler<TEventArgs> listener in handler.GetInvocationList()
																	 .Cast<EventHandler<TEventArgs>>())
				{
					listener.BeginInvoke(sender, args, ar =>
													   {
														   try
														   {
															   listener.EndInvoke(ar);
														   }
														   catch { }
													   },
										 null);
				}
				return args;
			}
			return null;
		}
	}
}
