namespace YuLeLux
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using YuLeLibrary;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class Lux
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private Spell E, Q, R, W, Qcol;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private Vector3 Epos = Vector3.Zero;
        private float DragonDmg = 0;
        private double DragonTime = 0;

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 1175);
            Qcol = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1075);
            R = new Spell(SpellSlot.R, 3000);

            Qcol.SetSkillshot(0.25f, 70f, 1200f, true, SkillshotType.SkillshotLine);
            Q.SetSkillshot(0.25f, 70f, 1200f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 110f, 1200f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.3f, 250f, 1050f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1f, 110f, float.MaxValue, false, SkillshotType.SkillshotLine);

            Config.SubMenu("Q 设置").AddItem(new MenuItem("autoQ", "自动 Q", true).SetValue(true)); 
            Config.SubMenu("Q 设置").AddItem(new MenuItem("harrasQ", "骚扰 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("harrasQmana", "骚扰 Q|最低蓝量比", true).SetValue(new Slider(60, 0, 100)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("jungleQ", "清野 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("jungleQMana", "清野 Q | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("gapQ", "反突进 Q", true).SetValue(true));
            Config.SubMenu("Q 设置").AddItem(new MenuItem("gapQlist", "  使用对象", true));
            foreach (var enemy in HeroManager.Enemies)
                Config.SubMenu("Q 设置").AddItem(new MenuItem("Qgap" + enemy.ChampionName, enemy.ChampionName).SetValue(true));

            Config.SubMenu("W 设置").AddItem(new MenuItem("autoW", "启动!", true).SetValue(true));
            Config.SubMenu("W 设置").AddItem(new MenuItem("Wdmg", "抵挡伤害|跟目标血量的比例", true).SetValue(new Slider(10, 100, 0)));
            Config.SubMenu("W 设置").AddItem(new MenuItem("autoWlist", "  使用友军对象", true));
            foreach (var ally in HeroManager.Allies)
            {
                Config.SubMenu("W 设置").AddItem(new MenuItem("useWWW" + ally.ChampionName, ally.ChampionName, true).SetValue(true));
            }

            Config.SubMenu("E 设置").AddItem(new MenuItem("autoE", "自动 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("harrasE", "骚扰 E", true).SetValue(false));
            Config.SubMenu("E 设置").AddItem(new MenuItem("harrasQmana", "骚扰 E|最低蓝量比", true).SetValue(new Slider(60, 0, 100)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("farmE", "清线 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("Mana", "清线 E | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("LCminions", "清线 E | 最低命中数", true).SetValue(new Slider(2, 10, 0)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("jungleE", "清野 E", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("jungleEMana", "清野 E | 最低蓝量比", true).SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("E 设置").AddItem(new MenuItem("autoEcc", "自动 E | 被控制到的敌人", true).SetValue(false));
            Config.SubMenu("E 设置").AddItem(new MenuItem("autoEslow", "自动 E | 减速敌人", true).SetValue(true));
            Config.SubMenu("E 设置").AddItem(new MenuItem("autoEdet", "仅敌人在范围内且能命中", true).SetValue(false));

            Config.SubMenu("R 设置").AddItem(new MenuItem("autoR", "智能 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rcc", "快速击杀 R ", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("RaoeCount", "自动 R | 最低命中数", true).SetValue(new Slider(3, 5, 0)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("hitchanceR", "R命中率(3最高)", true).SetValue(new Slider(2, 3, 0)));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rjungle", "抢野怪 R", true).SetValue(true));
            Config.SubMenu("R 设置").AddItem(new MenuItem("Rally", "抢野怪 R | 抢掉友军的", true).SetValue(false));
            Config.SubMenu("R 设置").AddItem(new MenuItem("useR", "手动R按键", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space   

            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWard", "启动", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoBuy", "lv9自动买灯泡", true).SetValue(false));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoPink", "自动真眼扫描", true).SetValue(true));
            Config.SubMenu("自动眼位").AddItem(new MenuItem("AutoWardCombo", "仅连招模式启动 ", true).SetValue(true));
            new AutoWard().Load();
            new Tracker().Load();

            var SkinMenu = Config.AddSubMenu(new Menu("换肤设置", "换肤设置"));
            {
                SkinMenu.AddItem(new MenuItem("EnableSkin", "启动换肤").SetValue(false));
                SkinMenu.AddItem(new MenuItem("SkinSelect", "选择皮肤").SetValue(new StringList(new[] { "经典", "奥术光辉", "游侠法师", "ODST地狱伞兵", "星际迷航", "钢铁军团", "魔法少女" })));
            }

            Config.SubMenu("显示设置").AddItem(new MenuItem("qRange", "Q 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("wRange", "W 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("eRange", "E 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRange", "R 范围", true).SetValue(false));
            Config.SubMenu("显示设置").AddItem(new MenuItem("rRangeMini", "R 小地图范围", true).SetValue(true));

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Q.IsReady() && gapcloser.Sender.IsValidTarget(Q.Range) && Config.Item("gapQ", true).GetValue<bool>() && Config.Item("Qgap" + gapcloser.Sender.ChampionName).GetValue<bool>())
                Q.Cast(gapcloser.Sender);
        }


        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "LuxLightStrikeKugel")
            {
                Epos = args.End;
           }
        }

        private void setbool()
        {
            AutoWard.Enable = Config.GetBool("AutoWard");
            AutoWard.AutoBuy = Config.GetBool("AutoBuy");
            AutoWard.AutoPink = Config.GetBool("AutoPink");
            AutoWard.OnlyCombo = Config.GetBool("AutoWardCombo");
            AutoWard.InComboMode = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
        }

        private void Skin()
        {
            if (Config.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, Config.Item("SkinSelect").GetValue<StringList>().SelectedIndex);
            }
            else if (!Config.Item("EnableSkin").GetValue<bool>())
            {
                ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, 0);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            setbool();
            Skin();

            if (R.IsReady() )
            {
                if (Config.Item("Rjungle", true).GetValue<bool>())
                {
                    KsJungle();
                }
                
                if (Config.Item("useR", true).GetValue<KeyBind>().Active)
                {
                    var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                    if (t.IsValidTarget())
                        R.Cast(t, true, true);
                }
            }
            else
                DragonTime = 0; 


            if (Program.LagFree(0))
            {
                SetMana();
                Jungle();
            }

            if ((Program.LagFree(4) || Program.LagFree(1) || Program.LagFree(3)) && W.IsReady() && !Player.IsRecalling() && Config.Item("autoW", true).GetValue<bool>())
                LogicW();
            if (Program.LagFree(1) && Q.IsReady() && Config.Item("autoQ", true).GetValue<bool>())
                LogicQ();
            if (Program.LagFree(2) && E.IsReady() && Config.Item("autoE", true).GetValue<bool>())
                LogicE();
            if (Program.LagFree(3) && R.IsReady())
                LogicR();
        }

        private void LogicW()
        {
            foreach (var ally in HeroManager.Allies.Where(ally => !ally.IsDead && ally.IsHPBarRendered && Config.Item("useWWW" + ally.ChampionName, true).GetValue<bool>() && Player.ServerPosition.Distance(ally.ServerPosition) < W.Range))
            {
                double dmg = Common.GetIncomingDamage(ally);

                int nearEnemys = ally.CountEnemiesInRange(800);

                if (dmg == 0 && nearEnemys == 0)
                    continue;

                int sensitivity = 20;
                
                double HpPercentage = (dmg * 100) / ally.Health;
                double shieldValue = 65 + W.Level * 25 + 0.35 * Player.FlatMagicDamageMod;

                if (nearEnemys > 0 && HardCC(ally))
                {
                    W.CastOnUnit(ally);
                }

                if (ally.HasBuffOfType(BuffType.Poison))
                {
                    W.Cast(W.GetPrediction(ally).CastPosition);
                }

                nearEnemys = (nearEnemys == 0) ? 1 : nearEnemys;

                if (dmg > shieldValue)
                    W.Cast(W.GetPrediction(ally).CastPosition);
                else if (dmg > 100 + Player.Level * sensitivity)
                    W.Cast(W.GetPrediction(ally).CastPosition);
                else if (ally.Health - dmg < nearEnemys * ally.Level * sensitivity)
                    W.Cast(W.GetPrediction(ally).CastPosition);
                else if (HpPercentage >= Config.Item("Wdmg", true).GetValue<Slider>().Value)
                    W.Cast(W.GetPrediction(ally).CastPosition);
            }
        }

        private void LogicQ()
        {
            foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && E.GetDamage(enemy) + Q.GetDamage(enemy) + BonusDmg(enemy) > enemy.Health))
            {
                CastQ(enemy);
                return;
            }

            var t = Orbwalker.GetTarget() as Obj_AI_Hero;
            if (!t.IsValidTarget())
                t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                if (Program.Combo && Player.Mana > RMANA + QMANA)
                    CastQ(t);
                else if (Program.Farm  && Config.Item("harrasQ", true).GetValue<bool>() && Player.Mana > Config.Item("harrasQmana", true).GetValue<Slider>().Value && Player.Mana > RMANA + EMANA + WMANA + EMANA)
                    CastQ(t);
                else if(Common.GetKsDamage(t,Q) > t.Health)
                    CastQ(t);

                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !Common.CanMove(enemy)))
                    CastQ(enemy);
            }
        }
        
        private void CastQ(Obj_AI_Base t)
        {
            var poutput = Qcol.GetPrediction(t);
            var col = poutput.CollisionObjects.Count(ColObj => ColObj.IsEnemy && ColObj.IsMinion && !ColObj.IsDead); 
     
            if ( col < 4)
                Program.CastSpell(Q, t);
        }

        private void LogicE()
        {
            if (Player.HasBuff("LuxLightStrikeKugel") && !Program.None)
            {
                int eBig = Epos.CountEnemiesInRange(350);
                if (Config.Item("autoEslow", true).GetValue<bool>())
                {
                    int detonate = eBig - Epos.CountEnemiesInRange(160);

                    if (detonate > 0 || eBig > 1)
                        E.Cast();
                }
                else if (Config.Item("autoEdet", true).GetValue<bool>())
                {
                    if (eBig > 0)
                        E.Cast();
                }
                else
                {
                    E.Cast();
                }
            }
            else
            {
                var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget() )
                {
                    if (!Config.Item("autoEcc", true).GetValue<bool>())
                    {
                        if (Program.Combo && Player.Mana > RMANA + EMANA)
                            Program.CastSpell(E, t);
                        else if (Program.Farm && Config.Item("harrasE", true).GetValue<bool>() && Player.Mana > Config.Item("harrasEmana", true).GetValue<Slider>().Value && Player.Mana > RMANA + EMANA + EMANA + RMANA)
                            Program.CastSpell(E, t);
                        else if (Common.GetKsDamage(t, E) > t.Health)
                            Program.CastSpell(E, t);
                    }

                    foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !Common.CanMove(enemy)))
                        E.Cast(enemy, true);
                }
                else if (Program.LaneClear && Player.ManaPercent > Config.Item("Mana", true).GetValue<Slider>().Value && Config.Item("farmE", true).GetValue<bool>() && Player.Mana > RMANA + WMANA)
                {
                    var minionList = Cache.GetMinions(Player.ServerPosition, E.Range);
                    var farmPosition = E.GetCircularFarmLocation(minionList, E.Width);

                    if (farmPosition.MinionsHit > Config.Item("LCminions", true).GetValue<Slider>().Value)
                        E.Cast(farmPosition.Position);
                }
            }
        }

        private void LogicR()
        {
            if (Config.Item("autoR", true).GetValue<bool>() )
            {
                foreach (var target in HeroManager.Enemies.Where(target => target.IsValidTarget(R.Range) && target.CountAlliesInRange(600) < 2 && Common.ValidUlt(target)))
                {
                    float predictedHealth = target.Health + target.HPRegenRate * 2;
                    float Rdmg = Common.GetKsDamage(target, R);

                    if (Items.HasItem(3155, target))
                    {
                        Rdmg = Rdmg - 250;
                    }

                    if (Items.HasItem(3156, target))
                    {
                        Rdmg = Rdmg - 400;
                    }

                    if (target.HasBuff("luxilluminatingfraulein"))
                    {
                        Rdmg +=  (float)Player.CalcDamage(target, Damage.DamageType.Magical,10 + (8 * Player.Level) + 0.2 * Player.FlatMagicDamageMod);
                    }

                    if (Player.HasBuff("itemmagicshankcharge"))
                    {
                        if (Player.GetBuff("itemmagicshankcharge").Count == 100)
                        {
                            Rdmg += (float)Player.CalcDamage(target, Damage.DamageType.Magical, 100 + 0.1 * Player.FlatMagicDamageMod);
                        }
                    }

                    if (Rdmg > predictedHealth )
                    {
                        castR(target);
                    }
                    else if (!Common.CanMove(target) && Config.Item("Rcc", true).GetValue<bool>() && target.IsValidTarget(E.Range))
                    {
                        float dmgCombo = Rdmg;

                        if (E.IsReady())
                        {
                            var eDmg = E.GetDamage(target);
                            
                            if (eDmg > predictedHealth)
                                return;
                            else
                                dmgCombo += eDmg;
                        }

                        if (target.IsValidTarget(800))
                            dmgCombo += BonusDmg(target);

                        if (dmgCombo > predictedHealth)
                        {
                            R.CastIfWillHit(target, 2);
                            R.Cast(target);
                        }

                    }
                    else if (Program.Combo && Config.Item("RaoeCount", true).GetValue<Slider>().Value > 0)
                    {
                        R.CastIfWillHit(target, Config.Item("RaoeCount", true).GetValue<Slider>().Value);
                    }
                }
            }
        }

        private float BonusDmg(Obj_AI_Hero target)
        {
            float damage = 10 + (Player.Level) * 8 + 0.2f * Player.FlatMagicDamageMod;
            if (Player.HasBuff("lichbane"))
            {
                damage += (Player.BaseAttackDamage * 0.75f) + ((Player.BaseAbilityDamage + Player.FlatMagicDamageMod) * 0.5f);
            }

            return (float)(Player.GetAutoAttackDamage(target) + Player.CalcDamage(target, Damage.DamageType.Magical, damage));
        }

        private void castR(Obj_AI_Hero target)
        {
            var inx = Config.Item("hitchanceR", true).GetValue<Slider>().Value;
            if (inx == 0)
            {
                R.Cast(R.GetPrediction(target).CastPosition);
            }
            else if (inx == 1)
            {
                R.Cast(target);
            }
            else if (inx == 2)
            {
                Program.CastSpell(R, target);
            }
            else if (inx == 3)
            {
                List<Vector2> waypoints = target.GetWaypoints();
                if ((Player.Distance(waypoints.Last<Vector2>().To3D()) - Player.Distance(target.Position)) > 400)
                {
                    Program.CastSpell(R, target);
                }
            }
        }

        private void Jungle()
        {
            if (Program.LaneClear && Player.Mana > RMANA + WMANA + RMANA + WMANA)
            {
                var mobs = Cache.GetMinions(Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (Q.IsReady() && Config.Item("jungleQ", true).GetValue<bool>() && Player.ManaPercent >= Config.Item("jungleQMana", true).GetValue<Slider>().Value)
                    {
                        Q.Cast(mob.ServerPosition);
                        return;
                    }
                    if (E.IsReady() && Config.Item("jungleE", true).GetValue<bool>() && Player.ManaPercent >= Config.Item("jungleEMana", true).GetValue<Slider>().Value)
                    {
                        E.Cast(mob.ServerPosition);
                        return;
                    }
                }
            }
        }

        private void KsJungle()
        {
            var mobs = Cache.GetMinions(Player.ServerPosition, R.Range, MinionTeam.Neutral);

            foreach (var mob in mobs)
            {
                if (((mob.SkinName == "SRU_Dragon") || (mob.SkinName == "SRU_Baron") || (mob.SkinName == "SRU_Red") || (mob.SkinName == "SRU_Blue")) && (mob.CountAlliesInRange(1000) == 0 || Config.Item("Rally", true).GetValue<bool>()) && mob.Health < mob.MaxHealth && mob.Distance(Player.Position) > 1000)
                {
                    if (DragonDmg == 0)
                        DragonDmg = mob.Health;

                    if (Game.Time - DragonTime > 3)
                    {
                        if (DragonDmg - mob.Health > 0)
                        {
                            DragonDmg = mob.Health;
                        }
                        DragonTime = Game.Time;
                    }
                    else
                    {
                        var DmgSec = (DragonDmg - mob.Health) * (Math.Abs(DragonTime - Game.Time) / 3);

                        if (DragonDmg - mob.Health > 0)
                        {
                            var timeTravel = R.Delay;
                            var timeR = (mob.Health - R.GetDamage(mob)) / (DmgSec / 3);
                            if (timeTravel > timeR)
                                R.Cast(mob.Position);
                        }
                        else
                            DragonDmg = mob.Health;
                    }
                }
            }
        }


        private bool HardCC(Obj_AI_Hero target)
        {
            
            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                target.IsStunned)
            {
                return true;

            }
            else
                return false;
        }

        private void SetMana()
        {
            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }

        public static void drawLine(Vector3 pos1, Vector3 pos2, int bold, System.Drawing.Color color)
        {
            var wts1 = Drawing.WorldToScreen(pos1);
            var wts2 = Drawing.WorldToScreen(pos2);

            Drawing.DrawLine(wts1[0], wts1[1], wts2[0], wts2[1], bold, color);
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (Config.Item("rRangeMini", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Aqua, 1, 20, true);
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {

            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Q.IsReady())
                    Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }

            if (Config.Item("wRange", true).GetValue<bool>())
            {
                if (W.IsReady())
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Orange, 1, 1);
            }

            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (E.IsReady())
                    Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Yellow, 1, 1);
            }

            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (R.IsReady())
                    Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Gray, 1, 1);
            }
        }
    }
}
