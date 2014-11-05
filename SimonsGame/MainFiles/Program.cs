#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace SimonsGame
{
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		public static MainGame Game;
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			using (Game = new MainGame())
				Game.Run();
		}
	}
}
