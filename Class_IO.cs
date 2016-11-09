using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System.Threading;


using System.IO;
using Android.Util;

namespace Kommentator_160212
{
	public class Class_IO
	{
		int count = 0;
		//static readonly string Filename = "count";
		string path;
		string filename;

		public Class_IO ()
		{
		}

		async Task<int> loadFileAsync ()
		{

			if (File.Exists (filename)) {
				/*	using (var f = new StreamReader (OpenFileInput (Filename))) {
					string line;
					do {
						line = await f.ReadLineAsync ();
					} while (!f.EndOfStream);
					Console.WriteLine ("Load Finished");
					Log.Info ("---", "Loaded=" + line);
					return int.Parse (line);

				}*/
			}
			return 0;
		}

		async Task writeFileAsync ()
		{
			/*using (var f = new StreamWriter (OpenFileOutput (Filename, FileCreationMode.Append | FileCreationMode.WorldReadable))) {
				await f.WriteLineAsync (count.ToString ()).ConfigureAwait (false);
			}
			Console.WriteLine ("Save Finished!");*/
			Log.Info ("---", "Saved=" + count);
		}
	}
}

