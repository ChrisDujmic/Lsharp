using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace STBBL
{
	class Program
	{
		static void Main(string[] args)
		{
			CustomEvents.Game.OnGameLoad += OnGameLoad;
		}

		private static void OnGameLoad(EventArgs args)
		{
			var plugin = Type.GetType("STBBL.ChampionPlugins." + ObjectManager.Player.ChampionName);
			if(plugin == null)
			{
				Game.PrintChat("<font color = \"#CC0000\">STBBL-" + ObjectManager.Player.ChampionName + " Cooming soon or not :>!</font>");
				return;
			}
			else
			{
				Game.PrintChat("<font color = \"#3399CC\">STBBL-" + ObjectManager.Player.ChampionName + " Loaded!</font>");
			}
			Activator.CreateInstance(plugin);
		}

		
	}
}
