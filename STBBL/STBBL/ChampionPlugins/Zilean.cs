using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace STBBL.ChampionPlugins
{
	class Zilean : Champion
	{
		public  Spell Q, W, E, R;
 
		public Zilean()
		{
			SetUpSpells();
			SetUpMenu();
		}

		private void SetUpSpells()
		{
			Q = new Spell(SpellSlot.Q, 800);
			Q.SetSkillshot(40, 200, 2000, false, SkillshotType.SkillshotCircle);

			W = new Spell(SpellSlot.W);

			E = new Spell(SpellSlot.E, 550);

			R = new Spell(SpellSlot.R, 900);
		}

		private void SetUpMenu()
		{
			AddMenu(MenuName.Combo);
			AddItemtoMenu(MenuName.Combo, new MenuItem("useQ", "use Q", true).SetValue( true));
			AddItemtoMenu(MenuName.Combo, new MenuItem("useW", "use W", true).SetValue(true));
			AddItemtoMenu(MenuName.Combo, new MenuItem("useE", "use E", true).SetValue(true));
			
			AddMenu(MenuName.Harras);
			AddItemtoMenu(MenuName.Harras, new MenuItem("useQ", "use Q", true).SetValue(true));
			AddItemtoMenu(MenuName.Harras, new MenuItem("useE", "use E", true).SetValue(true));

			AddMenu(MenuName.Passiv);
			AddItemtoMenu(MenuName.Passiv, new MenuItem("useRme", "use R on Me", true).SetValue(true));
			AddItemtoMenu(MenuName.Passiv, new MenuItem("useRally", "use R on Ally", true).SetValue(true));
			AddItemtoMenu(MenuName.Passiv, new MenuItem("useRPercent", "use on Health %", true).SetValue(new Slider(20, 1)));
			foreach(var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
				AddItemtoMenu(MenuName.Passiv, new MenuItem("useRon" + hero.ChampionName, hero.ChampionName, true).SetValue(true).DontSave());

			AddMenu(MenuName.Draw);
			AddItemtoMenu(MenuName.Draw, new MenuItem("enableDraw", "Enable Drawing", true).SetValue(true));
			AddItemtoMenu(MenuName.Draw, new MenuItem("drawQ", "Draw Q", true).SetValue(true));
			AddItemtoMenu(MenuName.Draw, new MenuItem("drawE", "Draw E", true).SetValue(true));
			AddItemtoMenu(MenuName.Draw, new MenuItem("drawR", "Draw R", true).SetValue(true));
			
		}

		public override void OnDraw(EventArgs args)
		{
			if(!Menu.Item("enableDraw", true).GetValue<bool>())
				return;
			if(Menu.Item("drawQ", true).GetValue<bool>())
				Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
			if(Menu.Item("drawE", true).GetValue<bool>())
				Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
			if(Menu.Item("drawR", true).GetValue<bool>())
				Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
		}

		public override void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
		{
			if(!sender.IsMe)
				return;
			var slot = Player.GetSpellSlot(args.SData.Name);
			if(slot == SpellSlot.Q)
				Q.LastCastAttemptT = Environment.TickCount;
		}

		public override void OnUpdate(EventArgs args)
		{
			if(Player.HasBuff("Recall") || Player.InFountain() || !R.IsReady())
				return;
			var useultMe = Menu.Item("useRme").GetValue<bool>();
			var useultAlly = Menu.Item("useRally").GetValue<bool>();
			var minHPPercent = Menu.Item("useRPercent").GetValue<Slider>().Value;
			if(!useultAlly && !useultMe)
				return;

			foreach(var hero in from hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && hero.Distance(Player) <= R.Range)
								let useonhero = Menu.Item("useRon" + hero.ChampionName).GetValue<bool>()
								where (useonhero || hero.IsMe) && (hero.Health / hero.MaxHealth) * 100 <= minHPPercent && hero.CountEnemiesInRange(1000) > 0
								select hero)
			{
				if(!hero.IsMe && useultAlly)
				{
					R.Cast(hero);
					return;
				}
				if(!hero.IsMe || !useultMe)
					continue;
				R.Cast(hero);
				return;
			}
		}

		public override void OnCombo()
		{
			if(Menu.Item("useQ", true).GetValue<bool>() && Q.IsReady())
				CastCircleSkillshot(Q, TargetSelector.DamageType.Magical, HitChance.Medium);
			if(Menu.Item("useW", true).GetValue<bool>() && W.IsReady())
			{
				var target = TargetSelector.GetSelectedTarget() ??
								 TargetSelector.GetTarget(Q.Range + (Q.Width / 2), TargetSelector.DamageType.Magical);
				if(Environment.TickCount - Q.LastCastAttemptT < 1500 || Q.IsKillable(target))
				{
					if(target != null && Q.GetPrediction(target).Hitchance >= HitChance.High && EnoughManaCombo1())
						W.Cast();
				}
			}
			if(Menu.Item("useE", true).GetValue<bool>() && Q.IsReady())
			{
				var target = TargetSelector.GetSelectedTarget() ??
							TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
				if (target != null)
					E.Cast(target);
			}
		}

		public override void OnHarras()
		{
			if(Menu.Item("useQ", true).GetValue<bool>() && Q.IsReady())
				CastCircleSkillshot(Q, TargetSelector.DamageType.Magical, HitChance.Medium);
			if(Menu.Item("useE", true).GetValue<bool>() && Q.IsReady())
			{
				var target = TargetSelector.GetSelectedTarget() ??
							TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
				if(target != null)
					E.Cast(target);
			}
		}

		public bool EnoughManaCombo1()
		{
			if(Q.Level == 0 || W.Level == 0)
				return false;
			var manacost = 50 + (Q.Level * 10) + 50;
			return manacost < Player.Mana;
		}
	}
}
