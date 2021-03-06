﻿using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Drawing;
using System.Linq;

public class Xerath : Champion
{
    private Spell WCenter;
    private Items.Item BlueTrinket1;
    private Items.Item BlueTrinket2;
    
    private Obj_AI_Hero RTarget = null;
    private int RTime = 0;
    private int RWaitTime = 0;
    
    public Xerath() : base("Xerath")
	{
        BlueTrinket1 = ItemData.Scrying_Orb_Trinket.GetItem();
        BlueTrinket2 = ItemData.Farsight_Orb_Trinket.GetItem();
    }
    
    protected override void OnInitSkins()
    {
        Skins.Add("Xerath");
        Skins.Add("Runeborn Xerath");
        Skins.Add("Battlecast Xerath");
        Skins.Add("Scorched Earth Xerath");
    }
    
    protected override void OnInitSpells()
    {
        Q = new Spell(SpellSlot.Q, 1600f);
        W = new Spell(SpellSlot.W, 1000f);
        WCenter = new Spell(SpellSlot.W, 1000f);
        E = new Spell(SpellSlot.E, 1150f);
        R = new Spell(SpellSlot.R, 2950);

        Q.SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
        Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1550, 1.5f);
        W.SetSkillshot(0.7f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        WCenter.SetSkillshot(0.7f, 50f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        E.SetSkillshot(0.2f, 60, 1400f, true, SkillshotType.SkillshotLine);
        R.SetSkillshot(0.7f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);
    }
    
    protected override void OnInitMenu()
    {
        MenuWrapper.SubMenu comboMenu = Menu.MainMenu.AddSubMenu("连 招");
        BoolLinks.Add("combo_q", comboMenu.AddLinkedBool("使用 Q", true));
        BoolLinks.Add("combo_w", comboMenu.AddLinkedBool("使用 W", true));
        BoolLinks.Add("combo_e", comboMenu.AddLinkedBool("使用 E", true));

        MenuWrapper.SubMenu harassMenu = Menu.MainMenu.AddSubMenu("骚 扰");
        BoolLinks.Add("harass_q", harassMenu.AddLinkedBool("使用 Q", true));
        BoolLinks.Add("harass_w", harassMenu.AddLinkedBool("使用 W", false));
        BoolLinks.Add("harass_e", harassMenu.AddLinkedBool("使用 E", false));
        SliderLinks.Add("harass_mana", harassMenu.AddLinkedSlider("保持 蓝量", 200, 0, 500));

        MenuWrapper.SubMenu autoMenu = Menu.MainMenu.AddSubMenu("自 动");
        BoolLinks.Add("auto_q", autoMenu.AddLinkedBool("使用 Q", false));
        BoolLinks.Add("auto_w", autoMenu.AddLinkedBool("使用 W", false));
        BoolLinks.Add("auto_e", autoMenu.AddLinkedBool("使用 E", false));
        BoolLinks.Add("auto_e_interrupt", autoMenu.AddLinkedBool("使用 E （打断法术）", true));
        BoolLinks.Add("auto_e_slows", autoMenu.AddLinkedBool("使用 E （敌人减速）", false));
        BoolLinks.Add("auto_e_stuns", autoMenu.AddLinkedBool("使用 E （敌人眩晕）", true));
        BoolLinks.Add("auto_e_gapclosers", autoMenu.AddLinkedBool("使用 E （阻止突进）", true));
        SliderLinks.Add("auto_mana", autoMenu.AddLinkedSlider("保存 蓝量", 200, 0, 500));

        MenuWrapper.SubMenu drawingMenu = Menu.MainMenu.AddSubMenu("范 围");
        BoolLinks.Add("drawing_q", drawingMenu.AddLinkedBool("Q 范围", true));
        BoolLinks.Add("drawing_w", drawingMenu.AddLinkedBool("W 范围", true));
        BoolLinks.Add("drawing_e", drawingMenu.AddLinkedBool("E 范围", true));
        BoolLinks.Add("drawing_r", drawingMenu.AddLinkedBool("R 范围", true));
        BoolLinks.Add("drawing_r_map", drawingMenu.AddLinkedBool("地图 显示 R 范围", true));
        BoolLinks.Add("drawing_damage", drawingMenu.AddLinkedBool("显示 R 伤害", true));

        MenuWrapper.SubMenu miscMenu = Menu.MainMenu.AddSubMenu("杂 项");
        KeyLinks.Add("misc_e", miscMenu.AddLinkedKeyBind("使用 E 按键", 'T', KeyBindType.Press));
        BoolLinks.Add("misc_w", miscMenu.AddLinkedBool("使用 W centered", true));
        BoolLinks.Add("misc_r", miscMenu.AddLinkedBool("大招开启时自动 R", true));
        SliderLinks.Add("misc_r_min_delay", miscMenu.AddLinkedSlider("R 丨最小延迟", 800, 0, 1500));
        SliderLinks.Add("misc_r_max_delay", miscMenu.AddLinkedSlider("R 丨最大延迟", 1750, 1500, 3000));
        SliderLinks.Add("misc_r_dash", miscMenu.AddLinkedSlider("R 延迟丨 防止 闪现/突进", 500, 0, 2000));
        BoolLinks.Add("misc_r_blue", miscMenu.AddLinkedBool("使用R丨使用蓝色小饰品开视野", true));
    }
    
    protected override void OnCombo()
    {
        if (BoolLinks["combo_w"].Value && !BoolLinks["misc_w"].Value)
            Spells.CastSkillshot(W, TargetSelector.DamageType.Magical);
        if (BoolLinks["combo_w"].Value && BoolLinks["misc_w"].Value)
            Spells.CastSkillshot(WCenter, TargetSelector.DamageType.Magical);
        if (BoolLinks["combo_e"].Value)
            Spells.CastSkillshot(E, TargetSelector.DamageType.Magical);
        if (BoolLinks["combo_q"].Value)
            CastQ();
    }
    
    protected override void OnHarass()
    {
        if (BoolLinks["harass_w"].Value && !BoolLinks["misc_w"].Value && GetSpellData(SpellSlot.W).ManaCost + SliderLinks["harass_mana"].Value.Value <= Player.Mana)
            Spells.CastSkillshot(W, TargetSelector.DamageType.Magical);
        if (BoolLinks["harass_w"].Value && BoolLinks["misc_w"].Value && GetSpellData(SpellSlot.W).ManaCost + SliderLinks["harass_mana"].Value.Value <= Player.Mana)
            Spells.CastSkillshot(WCenter, TargetSelector.DamageType.Magical);
        if (BoolLinks["harass_e"].Value && GetSpellData(SpellSlot.E).ManaCost + SliderLinks["harass_mana"].Value.Value <= Player.Mana)
            Spells.CastSkillshot(E, TargetSelector.DamageType.Magical);
        if (BoolLinks["harass_q"].Value && (Q.IsCharging || GetSpellData(SpellSlot.Q).ManaCost + SliderLinks["harass_mana"].Value.Value <= Player.Mana))
            CastQ();
    }
    
    protected override void OnAuto()
    {
        if (BoolLinks["auto_w"].Value && !BoolLinks["misc_w"].Value && GetSpellData(SpellSlot.W).ManaCost + SliderLinks["auto_mana"].Value.Value <= Player.Mana)
            Spells.CastSkillshot(W, TargetSelector.DamageType.Magical);
        if (BoolLinks["auto_w"].Value && BoolLinks["misc_w"].Value && GetSpellData(SpellSlot.W).ManaCost + SliderLinks["auto_mana"].Value.Value <= Player.Mana)
            Spells.CastSkillshot(WCenter, TargetSelector.DamageType.Magical);
        if (BoolLinks["auto_e"].Value && GetSpellData(SpellSlot.E).ManaCost + SliderLinks["auto_mana"].Value.Value <= Player.Mana)
            Spells.CastSkillshot(E, TargetSelector.DamageType.Magical);
        if (BoolLinks["auto_q"].Value && (Q.IsCharging || GetSpellData(SpellSlot.Q).ManaCost + SliderLinks["auto_mana"].Value.Value <= Player.Mana))
            CastQ();
    }
    
    protected override void OnUpdate()
    {
        if (Q.IsCharging)
            Orbwalking.Attack = false;
        else
            Orbwalking.Attack = true;

        if (R.Level > 0)
            R.Range = 1750 + R.Level * 1200;

        if (E.IsReady())
        {
            if (KeyLinks["misc_e"].Value.Active)
                Spells.CastSkillshot(E, TargetSelector.DamageType.Magical);

            foreach (Obj_AI_Hero enemy in Enemies.Where(x => Player.Distance(x, false) <= E.Range))
            {
                if (BoolLinks["auto_e_stuns"].Value && enemy.HasBuffOfType(BuffType.Stun))
                    Spells.CastSkillshot(E, enemy);
                if (BoolLinks["auto_e_slows"].Value && enemy.HasBuffOfType(BuffType.Slow))
                    Spells.CastSkillshot(E, enemy);
            }
        }

        if (BoolLinks["misc_r"].Value && IsChargingUltimate())
            CastR();
        else
            ResetR();
    }
    
    protected override void OnDraw()
    {
        if (Q.IsCharging)
            Utility.DrawCircle(Player.Position, Q.Range, Color.FromArgb(100, 0, 255, 0));
        if (BoolLinks["drawing_q"].Value)
            Utility.DrawCircle(Player.Position, Q.ChargedMaxRange, Color.FromArgb(100, 0, 255, 0));
        if (BoolLinks["drawing_w"].Value)
            Utility.DrawCircle(Player.Position, W.Range, Color.FromArgb(100, 0, 255, 0));
        if (BoolLinks["drawing_e"].Value)
            Utility.DrawCircle(Player.Position, E.Range, Color.FromArgb(100, 0, 255, 0));
        if (BoolLinks["drawing_r"].Value)
            Utility.DrawCircle(Player.Position, R.Range, Color.FromArgb(100, 0, 255, 0));
        if (RTarget != null)
            Utility.DrawCircle(RTarget.Position, 100, Color.FromArgb(100, 255, 0, 0));

        Utility.HpBarDamageIndicator.Enabled = BoolLinks["drawing_damage"].Value;
        Utility.HpBarDamageIndicator.DamageToUnit = DamageCalculation;
    }

    protected override void OnEndScene()
    {
        if (BoolLinks["drawing_r_map"].Value)
            Utility.DrawCircle(Player.Position, R.Range, Color.FromArgb(255, 255, 255), 2, 30, true);
    }

    private void CastQ()
    {
        Obj_AI_Hero target = TargetSelector.GetTarget(Q.ChargedMaxRange, TargetSelector.DamageType.Magical);
        if (target == null || !target.IsValidTarget(Q.ChargedMaxRange))
            return;

        if (!Q.IsCharging)
            Q.StartCharging();
        else
        {
            if (Q.GetPrediction(target).Hitchance >= HitChance.VeryHigh)
                Q.Cast(target, IsPacketCastEnabled());
            else
            {
                float distance = Player.Distance(target) + target.BoundingRadius * 2;

                if (distance > Q.ChargedMaxRange)
                    distance = Q.ChargedMaxRange;

                if(Q.Range >= distance && Q.GetPrediction(target).Hitchance >= HitChance.High)
                    Q.Cast(target, IsPacketCastEnabled());
            }
        }
    }

    private void CastR()
    {
        Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
        if (target == null || !target.IsValidTarget(R.Range)) return;

        if (RTarget == null)
            RTarget = target;
        else if(RTarget.NetworkId != target.NetworkId)
        {
            if (RTime == 0)
                RTime = Environment.TickCount;

            int time = Environment.TickCount - RTime;
            float distance = RTarget.Distance(target);

            if(time > distance / 2.5)
            {
                time = 0;
                RTarget = target;
            }
        }

        if(Environment.TickCount >= RWaitTime)
        {
            if ((Player.LastCastedSpellName() == "summonerflash" && Player.LastCastedSpellT() > Environment.TickCount - 100) || RTarget.IsDashing())
                RWaitTime = Environment.TickCount + SliderLinks["misc_r_dash"].Value.Value;

            if (Player.LastCastedSpellT() < Environment.TickCount - SliderLinks["misc_r_min_delay"].Value.Value)
            {
                if (Player.LastCastedSpellT() < Environment.TickCount - SliderLinks["misc_r_max_delay"].Value.Value)
                    Spells.CastSkillshot(R, RTarget, HitChance.High);
                else
                    Spells.CastSkillshot(R, RTarget);
            }
        }
    }

    protected override void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
    {
        if (BoolLinks["auto_e_interrupt"].Value)
            Spells.CastSkillshot(E, unit);
    }

    protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
    {
        if (BoolLinks["auto_e_gapclosers"].Value)
            Spells.CastSkillshot(E, gapcloser.Sender);
    }

    private bool IsChargingUltimate()
    {
        return Player.HasBuff("XerathLocusOfPower2", true) || Player.LastCastedSpellName() == "XerathLocusOfPower2";
    }

    private float DamageCalculation(Obj_AI_Base hero)
    {
        if (R.Level > 0)
            return (float)Player.GetSpellDamage(hero, SpellSlot.R) * 3f;
        return 0f;
    }
    
    private void ResetR()
    {
        RTarget = null;
        RTime = 0;
        RWaitTime = 0;
    }

    protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
    {
        if (sender.Owner.IsMe && args.Slot == SpellSlot.R && BoolLinks["misc_r_blue"].Value)
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget(R.Range)) return;

            if (RTarget == null)
                RTarget = target;

            if ((BlueTrinket1.IsOwned() && BlueTrinket1.IsReady()) || (BlueTrinket2.IsOwned() && BlueTrinket2.IsReady()))
            {
                if (BlueTrinket1.IsOwned() && BlueTrinket1.IsReady() && (Player.Level >= 9 ? 3500f : BlueTrinket1.Range) >= Player.Distance(RTarget))
                {
                    BlueTrinket1.Cast(RTarget.Position);
                    Utility.DelayAction.Add(175, CastRCallback);
                }
                if (BlueTrinket2.IsOwned() && BlueTrinket2.IsReady() && BlueTrinket2.Range >= Player.Distance(RTarget))
                {
                    BlueTrinket2.Cast(RTarget.Position);
                    Utility.DelayAction.Add(175, CastRCallback);
                }
            }
            else
                args.Process = true;
        }
    }

    private void CastRCallback()
    {
        Spells.Cast(R);
    }
}
