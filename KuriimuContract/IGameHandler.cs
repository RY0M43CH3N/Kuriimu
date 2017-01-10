﻿using System.Drawing;
using System.Text;

namespace KuriimuContract
{
	public interface IGameHandler
	{
		// Information
		string Name { get; }
		Image Icon { get; }

		// Feature Support
		bool HandlerCanGeneratePreviews { get; } // Is this handler capable of generating previews?

		// Features
		string GetKuriimuString(string rawString);
		string GetRawString(string kuriimuString);
		Bitmap GeneratePreview(string rawString);
	}

	class DefaultGameHandler : IGameHandler
	{
		public string Name => "No Game";
		public Image Icon { get; set; }

		public bool HandlerCanGeneratePreviews => false;
		public string GetKuriimuString(string rawString) => rawString;
		public string GetRawString(string kuriimuString) => kuriimuString;
		public Bitmap GeneratePreview(string rawString) => null;
	}
}