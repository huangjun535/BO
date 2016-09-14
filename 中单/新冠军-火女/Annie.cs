namespace YuLeAnnie
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using YuLeLibrary;

    class Annie
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell Q, W, E, R, FR;
        public float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        private SpellSlot flash;

        public Obj_AI_Base Tibbers;
        public float TibbersTimer = 0;
        private bool HaveStun = false;
        private Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 550f);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 625f);
            FR = new Spell(SpellSlot.R, 1000f );

            Q.SetTargetted(0.25f, 1400f);
            W.SetSkillshot(0.3f, 80f, float.MaxValue, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 180f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            FR.SetSkillshot(0.25f, 180f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            flash = Player.GetSpellSlot("summonerflash");

            Config.SubMenu("Q技能设置").AddItem(new MenuItem("autoQ", "自动Q", true).SetValue(true));
            Config.SubMenu("Q技能设置").AddItem(new MenuItem("harrasQ", "骚扰Q", true).SetValue(true));

            Config.SubMenu("W技能设置").AddItem(new MenuItem("autoW", "自动W", true).SetValue(true));
            Config.SubMenu("W技能设置").AddItem(new MenuItem("harrasW", "骚扰W", true).SetValue(true));

            Config.SubMenu("E技能设置").AddItem(new MenuItem("autoE", "自动E攒晕", true).SetValue(true));

            Config.SubMenu("R技能设置").AddItem(new MenuItem("autoRks", "自动R", true).SetValue(true));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("autoRcombo", "假如有晕自动R", true).SetValue(true));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("rCount", "自动R敌人数", true).SetValue(new Slider(2, 2, 5)));
            Config.SubMenu("R技能设置").AddItem(new MenuItem("tibers", "自动R跟随敌人", true).SetValue(true));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu("R技能设置").AddItem(new MenuItem("UM" + enemy.ChampionName, enemy.ChampionName, true).SetValue(new StringList(new[] { "Normal", "Always", "Never", "Always Stun" }, 0)));

            if (flash != SpellSlot.Unknown)
            {
                Config.SubMenu("R技能设置").AddItem(new MenuItem("rCountFlash", "自动闪现+R爆发敌人", true).SetValue(new Slider(3, 2, 5)));
            }

            Config.SubMenu("清线清野").AddItem(new MenuItem("farmQ", "清线Q", true).SetValue(true));
            Config.SubMenu("清线清野").AddItem(new MenuItem("farmW", "清线W", true).SetValue(false));
            Config.SubMenu("清线清野").AddItem(new MenuItem("Mana", "清线最低蓝量比", true).SetValue(new Slider(40, 100, 0)));
            Config.SubMenu("清线清野").AddItem(new MenuItem("havestuns", "有被动禁止技能清线", true).SetValue(false));
            Config.SubMenu("清线清野").AddItem(new MenuItem("farmQA", "清野Q", true).SetValue(true));
            Config.SubMenu("清线清野").AddItem(new MenuItem("farmWA", "清野W", true).SetValue(true));
            Config.SubMenu("清线清野").AddItem(new MenuItem("ManaA", "清野最低蓝量比", true).SetValue(new Slider(20, 100, 0)));


            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Config.AddSubMenu(new Menu("换肤设置", "换肤设置"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "哥特萝莉", "小红帽", "梦游仙境", "舞会公主", "冰霜烈焰", "安伯斯与提妮", "科学怪熊的新娘", "你见过我的熊猫吗", "甜心宝贝", "海克斯科技" })));
            }

            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));

            Config.AddItem(new MenuItem("supportMode", "辅助模式", true).SetValue(false));

            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsValid && obj.IsAlly && obj is Obj_AI_Minion && obj.Name.ToLower() == "tibbers")
            {
                Tibbers = obj as Obj_AI_Base ;
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.HasBuff("Recall"))
                return;

            HaveStun = Player.HasBuff("pyromania_particle");

            SetMana();

            if (R.IsReady() && (Program.LagFree(1) || Program.LagFree(3)) && !HaveTibers)
            {
                var realRange = R.Range;

                if (flash.IsReady())
                    realRange = FR.Range;

                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(realRange) && Common.ValidUlt(enemy)))
                {
                    if (enemy.IsValidTarget(R.Range))
                    {
                        int Rmode = Config.Item("UM" + enemy.ChampionName, true).GetValue<StringList>().SelectedIndex;

                        if (Rmode == 2)
                            continue;

                        var poutput = R.GetPrediction(enemy, true);
                        var aoeCount = poutput.AoeTargetsHitCount;

                        if (Rmode == 1)
                            R.Cast(poutput.CastPosition);

                        if (Rmode == 3 && HaveStun)
                            R.Cast(poutput.CastPosition);

                        if (aoeCount >= Config.Item("rCount", true).GetValue<Slider>().Value && Config.Item("rCount", true).GetValue<Slider>().Value > 0)
                            R.Cast(poutput.CastPosition);
                        else if (Program.Combo && HaveStun && Config.Item("autoRcombo", true).GetValue<bool>())
                            R.Cast(poutput.CastPosition);
                        else if (Config.Item("autoRks", true).GetValue<bool>())
                        {
                            var comboDmg = Common.GetKsDamage(enemy, R);

                            if (W.IsReady() && RMANA + WMANA < Player.Mana)
                                comboDmg += W.GetDamage(enemy);

                            if (Q.IsReady() && RMANA + WMANA + QMANA < Player.Mana)
                                comboDmg += Q.GetDamage(enemy);

                            if (enemy.Health < comboDmg)
                                R.Cast(poutput.CastPosition);
                        }
                    }
                    else if(HaveStun && flash.IsReady())
                    {
                        var poutputFlas = FR.GetPrediction(enemy, true);
                        var aoeCountFlash = poutputFlas.AoeTargetsHitCount;
                        if (HaveStun && aoeCountFlash >= Config.Item("rCountFlash", true).GetValue<Slider>().Value && Config.Item("rCountFlash", true).GetValue<Slider>().Value > 0)
                        {
                            Player.Spellbook.CastSpell(flash, poutputFlas.CastPosition);
                            R.Cast(poutputFlas.CastPosition);
                        }
                    }
                }
            }

            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget() && Program.LagFree(2))
            {
                if (Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                {
                    if (Program.Combo && RMANA + WMANA < Player.Mana)
                        Q.Cast(t);
                    else if (Program.Farm && RMANA + WMANA + QMANA < Player.Mana && Config.Item("harrasQ", true).GetValue<bool>())
                        Q.Cast(t);
                    else
                    {
                        var qDmg = Common.GetKsDamage(t, Q);
                        var wDmg = W.GetDamage(t);
                        if (qDmg > t.Health)
                            Q.Cast(t);
                        else if (qDmg + wDmg > t.Health && Player.Mana > QMANA + WMANA)
                            Q.Cast(t);
                    }
                }
                if (W.IsReady() && Config.Item("autoW", true).GetValue<bool>() && t.IsValidTarget(W.Range))
                {
                    var poutput = W.GetPrediction(t, true);
                    var aoeCount = poutput.AoeTargetsHitCount;

                    if (Program.Combo && RMANA + WMANA < Player.Mana)
                        W.Cast(poutput.CastPosition);
                    else if (Program.Farm && RMANA + WMANA + QMANA < Player.Mana && Config.Item("harrasW", true).GetValue<bool>())
                        W.Cast(poutput.CastPosition);
                    else
                    {
                        var wDmg = Common.GetKsDamage(t, W);
                        var qDmg = Q.GetDamage(t);
                        if (wDmg > t.Health)
                            W.Cast(poutput.CastPosition);
                        else if (qDmg + wDmg > t.Health && Player.Mana > QMANA + WMANA)
                            W.Cast(poutput.CastPosition);
                    }
                }
            }
            else if(Q.IsReady() || W.IsReady())
            {
                if (Config.Item("supportMode", true).GetValue<bool>())
                {
                    if (Program.LaneClear && Player.Mana > RMANA + QMANA)
                        farm();
                }
                else
                {
                    if ((!HaveStun || Program.LaneClear) && Program.Farm)
                        farm();
                }
            }

            if (Program.LagFree(3))
            {
                if (!HaveStun)
                {
                    if (E.IsReady() && !Program.LaneClear && Config.Item("autoE", true).GetValue<bool>() && Player.Mana > RMANA + EMANA + QMANA + WMANA)
                        E.Cast();
                    else if (W.IsReady() && Player.InFountain())
                        W.Cast(Player.Position);
                }
                if (R.IsReady())
                {
                    if (Config.Item("tibers", true).GetValue<bool>() && HaveTibers && Tibbers != null && Tibbers.IsValid)
                    {
                        var enemy = HeroManager.Enemies.Where(x => x.IsValidTarget() && Tibbers.Distance(x.Position) < 1000 && !x.UnderTurret(true)).OrderBy(x => x.Distance(Tibbers)).FirstOrDefault();
                        if(enemy != null)
                        {

                            if (Tibbers.Distance(enemy.Position) > 200)
                                Player.IssueOrder(GameObjectOrder.MovePet, enemy);
                            else
                                Tibbers.IssueOrder(GameObjectOrder.AttackUnit, enemy);
                        }
                        else
                        {
                            var annieTarget = Orbwalker.GetTarget() as Obj_AI_Base;
                            if (annieTarget != null)
                            {
                                if (Tibbers.Distance(annieTarget.Position) > 200)
                                    Player.IssueOrder(GameObjectOrder.MovePet, annieTarget);
                                else
                                    Tibbers.IssueOrder(GameObjectOrder.AttackUnit, annieTarget);
                            }
                            else if (Tibbers.UnderTurret(true))
                            {
                                Player.IssueOrder(GameObjectOrder.MovePet, Player);
                            }
                        }
                    }
                    else
                    {
                        Tibbers = null;
                    }
                }
            }
        }

        private void farm()
        {
            if(Program.LaneClear)
            { 
                var mobs = Cache.GetMinions(Player.ServerPosition, Q.Range, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (W.IsReady() && Config.Item("farmWA", true).GetValue<bool>() && Player.ManaPercent > Config.Item("ManaA", true).GetValue<Slider>().Value)
                        W.Cast(mob);
                    else if (Q.IsReady() && Config.Item("farmQA", true).GetValue<bool>() && Player.ManaPercent > Config.Item("ManaA", true).GetValue<Slider>().Value)
                        Q.Cast(mob);
                }
            }

            if (Config.Item("havestuns", true).GetValue<bool>() && Player.HasBuff("Energized"))
            {
                return;
            }

            if (Config.Item("supportMode", true).GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                return;
            }

            var minionsList = Cache.GetMinions(Player.ServerPosition, Q.Range);
            if (Q.IsReady() && Config.Item("farmQ", true).GetValue<bool>() && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value)
            {
                var minion = minionsList.Where(x => YuLeLibrary.HealthPrediction.LaneClearHealthPrediction(x, 250, 50) < Q.GetDamage(x) && x.Health > Player.GetAutoAttackDamage(x)).FirstOrDefault();
                Q.Cast(minion);
            }
            else if (Program.LaneClear && W.IsReady() && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmW", true).GetValue<bool>())
            {
                var farmLocation = W.GetCircularFarmLocation(minionsList, W.Width);
                if (farmLocation.MinionsHit > 1)
                    W.Cast(farmLocation.Position);
            }
        }

        private bool HaveTibers
        {
            get { return Player.HasBuff("infernalguardiantimer"); }
        }

        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady() || HaveTibers)
                RMANA = 0;
            else 
                RMANA = R.Instance.ManaCost;
        }

        private void Drawing_OnDraw(EventArgs args)
        {

            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Q.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }
            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (W.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
            }

            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range + R.Width / 2, System.Drawing.Color.Gray, 1, 1);
            }

        }
    }
}
