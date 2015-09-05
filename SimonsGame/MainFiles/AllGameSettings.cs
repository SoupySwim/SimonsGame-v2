using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SimonsGame.GlobalGameSettings
{
	public static class AllGameSettings
	{
		public static string GameSettingsLocation = Environment.CurrentDirectory + @"\Settings\";
		public static string GameSettingsPathName = GameSettingsLocation + @"GameSettings.xml"; // Perhaps later I will zip and lock this
		public static object fileLock = new object();

		#region GameSettings
		public static bool MenuEditor_ShowGrid = true;

		#endregion

		public static void Initialize()
		{
			XDocument GameSettingsDoc = null;
			try
			{
				lock (fileLock)
				{
					GameSettingsDoc = XDocument.Load(GameSettingsPathName);
				}
			}
			catch (Exception)// Don't care, if it's faulty, I make a new one.
			{
				GameSettingsDoc = new XDocument();
			}
		}
	}
}
