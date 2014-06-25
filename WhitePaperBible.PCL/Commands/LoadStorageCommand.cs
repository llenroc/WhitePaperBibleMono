using MonkeyArms;
using WhitePaperBible.Core.Invokers;
using WhitePaperBible.Core.Models;
using System.Threading.Tasks;
using PCLStorage;
using System.Xml.Serialization;
using System.IO;

namespace WhitePaperBible.Core.Commands
{
	public class LoadStorageCommand : Command
	{
		[Inject]
		public AppModel AM;

		[Inject]
		public PapersReceivedInvoker PapersReceived;

		public override void Execute (InvokerArgs args)
		{
			var loadStore = LoadStore ();
		}

		public async Task LoadStore()
		{
			IFolder rootFolder = FileSystem.Current.LocalStorage;
			IFolder folder = await rootFolder.GetFolderAsync("WhitePaperBible");
			IFile file = await folder.GetFileAsync("app_model.dat");

			var serializer = new XmlSerializer (AM.GetType());
			var fileText = await file.ReadAllTextAsync ();
			var stringReader = new StringReader (fileText);
			AM = (AppModel)serializer.Deserialize (stringReader);

		}
	}
}
