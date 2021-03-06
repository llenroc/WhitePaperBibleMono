using System;
using MonkeyArms;
using WhitePaperBible.Core.Models;
using WhitePaperBible.Core.Invokers;

namespace WhitePaperBible.Core.Commands
{
	public class ConfigureInvokersCommand : Command
	{
		public override void Execute (InvokerArgs args)
		{
//			base.Execute (args);

			DI.MapSingleton<PapersReceivedInvoker> ();
			DI.MapSingleton<PaperDetailsReceivedInvoker> ();
			DI.MapSingleton<TagsReceivedInvoker> ();
			DI.MapSingleton<PapersByTagReceivedInvoker> ();
			DI.MapSingleton<LogInInvoker> ();
			DI.MapSingleton<LoggedInInvoker> ();
		}
	}
}

