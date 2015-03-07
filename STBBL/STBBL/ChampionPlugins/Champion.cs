using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace STBBL.ChampionPlugins
{
	class Champion
	{
		public Menu Menu;
		public Obj_AI_Hero Player = ObjectManager.Player;
		public Orbwalking.Orbwalker Orbwalker;

		public Champion()
		{
			BasicMenu();
			ActivateCallbacks();
		}

		private void BasicMenu()
		{
			Menu = new Menu("ChrisD´s STBBL", "stbbl" + Player.ChampionName, true);

			var ts = new Menu("Target Selector", "stbblTS");
			TargetSelector.AddToMenu(ts);
			Menu.AddSubMenu(ts);

			var orbwalk = new Menu("Orbwalker", "stbblOrbwalker");
			Orbwalker = new Orbwalking.Orbwalker(orbwalk);
			Menu.AddSubMenu(orbwalk);

			var draw = new Menu("Draw", "Draw");
			Menu.AddSubMenu(draw);

			Menu.AddToMainMenu();
		}

		public enum MenuName
		{
			Combo,
			LaneClear,
			Lasthit,
			Harras,
			Misc,
			Passiv,
			Draw,
		}

		public void AddMenu(MenuName name)
		{
			var menu = new Menu(name.ToString(), name.ToString());
			Menu.AddSubMenu(menu);
		}

		public void AddItemtoMenu(MenuName name, MenuItem item)
		{
			var menu = Menu.SubMenu(name.ToString());
			menu.AddItem(item);
		}

		private void ActivateCallbacks()
		{
			Game.OnUpdate += OnUpdateMode;
			Game.OnUpdate += OnUpdate;
			Drawing.OnDraw += OnDraw;
			Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
		}

		public virtual void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
		{		
		}

		public virtual void OnDraw(EventArgs args)
		{
		}

		public virtual void OnUpdate(EventArgs args)
		{
		}

		private void OnUpdateMode(EventArgs args)
		{
			try
			{

				switch (Orbwalker.ActiveMode)
				{
					case Orbwalking.OrbwalkingMode.Combo:
						OnCombo();
						break;
					case Orbwalking.OrbwalkingMode.LaneClear:
						OnLaneClear();
						break;
					case Orbwalking.OrbwalkingMode.LastHit:
						OnLastHit();
						break;
					case Orbwalking.OrbwalkingMode.Mixed:
						OnHarras();
						break;
				}
			}
			catch
			{
			}
		}

		public virtual void OnHarras()
		{			
		}

		public virtual void OnLastHit()
		{			
		}

		public virtual void OnLaneClear()
		{
		}

		public virtual void OnCombo()
		{			
		}

		public bool CastCircleSkillshot(Spell spell, TargetSelector.DamageType type, HitChance hitChance, bool towerCheckenemy = false)
		{
			var target = TargetSelector.GetSelectedTarget() ?? TargetSelector.GetTarget(spell.Range + (spell.Width /2), type);
			if(target == null || !spell.IsReady())
				return false;
			if(towerCheckenemy && target.UnderTurret(true))
				return false;
			if(spell.GetPrediction(target).Hitchance < hitChance)
				return false;
			spell.Cast(target);
			return true;
		}
	}
}
