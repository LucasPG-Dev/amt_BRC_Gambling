using System.Collections.Generic;
using UnityEngine;

namespace BRCGambling
{
    public static class EffectRegistry
    {
        public static readonly Dictionary<string, EffectDefinition> All = new Dictionary<string, EffectDefinition>();

        public static void Register(EffectDefinition def)
        {
            All[def.Id] = def;
        }

        public static EffectDefinition Get(string id)
        {
            return All.TryGetValue(id, out var def) ? def : null;
        }

        public static List<EffectDefinition> GetPool(RewardTier rarity)
        {
            var list = new List<EffectDefinition>();
            foreach (var def in All.Values)
                if (def.Rarity == rarity)
                    list.Add(def);
            return list;
        }

        public static void InitializeDefaults()
        {
            // ===================== COMMON =====================
            Register(new EffectDefinition("common_skull_head", "Skull Head", "CFXR2 Skull Head Alt 1", RewardTier.Common, EffectPosition.Torso, EffectTrigger.Looping));
            Register(new EffectDefinition("common_ground_hit", "Ground Hit", "CFXR2 Ground Hit 1", RewardTier.Common, EffectPosition.Feet, EffectTrigger.OnLand));
            Register(new EffectDefinition("common_bubble_breath", "Bubble Breath", "CFXR4 Bubbles Breath Underwater Loop 1", RewardTier.Common, EffectPosition.Feet, EffectTrigger.Looping));
            Register(new EffectDefinition("common_flash", "Flash", "CFXR Flash 1", RewardTier.Common, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("common_magic_poof", "Magic Poof", "CFXR Magic Poof 1", RewardTier.Common, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("common_smoke_source", "Smoke Source", "CFXR Smoke Source 3D 1", RewardTier.Common, EffectPosition.Feet, EffectTrigger.Looping));
            Register(new EffectDefinition("common_poison_cloud", "Poison Cloud", "CFXR2 Poison Cloud 1", RewardTier.Common, EffectPosition.Torso, EffectTrigger.Looping));
            Register(new EffectDefinition("common_ambient_glows", "Ambient Glows", "CFXR3 Ambient Glows 1", RewardTier.Common, EffectPosition.Feet, EffectTrigger.Looping));
            Register(new EffectDefinition("common_wallplant_smoke", "Smoke Hit", "CFXR3 Hit Misc F Smoke 1", RewardTier.Common, EffectPosition.Torso, EffectTrigger.OnWallPlant));

            // ===================== RARE =====================
            Register(new EffectDefinition("rare_fire_hit", "Fire Hit", "CFXR3 Hit Fire B (Air) 1", RewardTier.Rare, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("rare_sun", "Sun", "CFXR4 Sun 1", RewardTier.Rare, EffectPosition.Feet, EffectTrigger.Looping));
            Register(new EffectDefinition("rare_electric_hit", "Electric Hit", "CFXR3 Hit Electric C (Air) 1", RewardTier.Rare, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("rare_explosion", "Explosion", "CFXR Explosion Smoke 2 Solo (HDR) 1", RewardTier.Rare, EffectPosition.Torso, EffectTrigger.OnGraceEnd));
            Register(new EffectDefinition("rare_firework_shoot", "Firework Shoot", "CFXR4 Firework HDR Shoot Single (Random Color) 1", RewardTier.Rare, EffectPosition.Feet, EffectTrigger.OnSpray));
            Register(new EffectDefinition("rare_water_splash", "Water Splash", "CFXR Water Splash (Smaller) 1", RewardTier.Rare, EffectPosition.Feet, EffectTrigger.OnLand));
            Register(new EffectDefinition("rare_broken_heart", "Broken Heart", "CFXR2 Broken Heart 1", RewardTier.Rare, EffectPosition.Torso, EffectTrigger.OnDeath));
            Register(new EffectDefinition("rare_smoke_hit", "Smoke Hit", "CFXR3 Hit Misc A 1", RewardTier.Rare, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("rare_rain_fall", "Rain Fall", "CFXR4 Rain Falling 1", RewardTier.Rare, EffectPosition.AboveHead, EffectTrigger.Looping));

            // Hit effects ice
            Register(new EffectDefinition("rare_ice_trail_wide", "Ice Trail (Wide)", "CFXR4 Sword Trail ICE (360 Thin Spiral) 1", RewardTier.Rare, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("rare_ice_trail_small", "Ice Trail (Small)", "CFXR4 Sword Trail ICE (360 Spiral) 1", RewardTier.Rare, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("rare_ice_hit", "Ice Hit", "CFXR4 Sword Hit ICE (Cross) 1", RewardTier.Rare, EffectPosition.Torso, EffectTrigger.OnSpray));

            // Hit effects fire
            Register(new EffectDefinition("rare_fire_trail_wide", "Fire Trail (Wide)", "CFXR4 Sword Trail FIRE (360 Thin Spiral) 1", RewardTier.Rare, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("rare_fire_trail_small", "Fire Trail (Small)", "CFXR4 Sword Trail FIRE (360 Spiral) 1", RewardTier.Rare, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("rare_fire_sword_hit", "Fire Hit (Sword)", "CFXR4 Sword Hit FIRE (Cross) 1", RewardTier.Rare, EffectPosition.Torso, EffectTrigger.OnSpray));

            // ===================== EPIC =====================
            Register(new EffectDefinition("epic_big_explosion_smoke", "Big Explosion Smoke", "CFXR Explosion 2", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnGraceEnd));
            Register(new EffectDefinition("epic_fire_explosion", "Fire Explosion", "CFXR3 Fire Explosion B 1", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("epic_firework", "Firework", "CFXR4 Firework 1 Cyan-Purple (HDR) 1", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("epic_hit_ice", "Hit Ice", "CFXR3 Hit Ice B (Air) 1", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnBoostTrick));
            Register(new EffectDefinition("epic_falling_stars", "Falling Stars", "CFXR4 Falling Stars 1", RewardTier.Epic, EffectPosition.AboveHead, EffectTrigger.Looping));
            Register(new EffectDefinition("epic_cartoon_fight", "Cartoon Fight", "CFXR2 Cartoon Fight (Loop) 1", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnSpray)); // toggled on/off via spray

            // Word effects
            Register(new EffectDefinition("epic_word_wow", "WOW", "CFXR3 _WOW_ 1", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnBoostTrick));
            Register(new EffectDefinition("epic_word_wham", "WHAM", "CFXR2 _WHAM_ 4", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnBoostTrick));
            Register(new EffectDefinition("epic_word_cursed", "CURSED", "CFXR2 _CURSED_ 1", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("epic_word_slash", "SLASH", "CFXR _SLASH_ 1", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("epic_word_pow", "POW", "CFXR _POW_ 1", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnWallPlant));
            Register(new EffectDefinition("epic_word_boom", "BOOM", "CFXR _BOOM_ 1", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnGraceEnd));
            Register(new EffectDefinition("epic_word_boing", "BOING", "CFXR _BOING_ 1", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnJump));

            Register(new EffectDefinition("epic_hit_leaves", "Hit Leaves", "CFXR3 Hit Leaves A (Lit) 1", RewardTier.Epic, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("epic_water_ripples", "Water Ripples", "CFXR Water Ripples 1", RewardTier.Epic, EffectPosition.Feet, EffectTrigger.OnLand));

            // ===================== LEGENDARY =====================
            Register(new EffectDefinition("legendary_fire", "Fire", "CFXR Fire 1", RewardTier.Legendary, EffectPosition.Feet, EffectTrigger.Looping));
            Register(new EffectDefinition("legendary_souls_escape", "Souls Escape", "CFXR2 Souls Escape 1", RewardTier.Legendary, EffectPosition.Feet, EffectTrigger.Looping));
            Register(new EffectDefinition("legendary_purple_explosion", "Purple Explosion", "CFXR2 WW Enemy Explosion 1", RewardTier.Legendary, EffectPosition.Torso, EffectTrigger.OnGraceEnd));
            Register(new EffectDefinition("legendary_electrified", "Electrified", "CFXR Electrified 4", RewardTier.Legendary, EffectPosition.Torso, EffectTrigger.Looping));
            Register(new EffectDefinition("legendary_cartoon_explosion", "Explosion", "CFXR2 WW Explosion 1", RewardTier.Legendary, EffectPosition.Torso, EffectTrigger.OnGraceEnd));
            Register(new EffectDefinition("legendary_yellow_hit", "Yellow Hit", "CFXR Hit D 3D (Yellow) 1", RewardTier.Legendary, EffectPosition.Torso, EffectTrigger.OnBoostTrick)); // also wallplant/spray
            Register(new EffectDefinition("legendary_glowing_impact", "Glowing Impact", "CFXR Impact Glowing HDR (Blue) 1", RewardTier.Legendary, EffectPosition.Torso, EffectTrigger.OnEmote));
            Register(new EffectDefinition("legendary_hit_light", "Hit Light", "CFXR3 Hit Light B (Air) 1", RewardTier.Legendary, EffectPosition.Torso, EffectTrigger.OnSpray));
            Register(new EffectDefinition("legendary_light_glow", "Light Glow", "CFXR3 LightGlow A (Loop) 1", RewardTier.Legendary, EffectPosition.Torso, EffectTrigger.Looping));
            Register(new EffectDefinition("legendary_magic_aura", "Magic Aura", "CFXR3 Magic Aura A (Runic) 1", RewardTier.Legendary, EffectPosition.Feet, EffectTrigger.Looping));
            Register(new EffectDefinition("legendary_shiny_aura", "Shiny Aura", "CFXR2 Shiny Item (Loop) 1", RewardTier.Legendary, EffectPosition.Torso, EffectTrigger.Looping));
            Register(new EffectDefinition("legendary_leaves_shield", "Leaves Shield", "CFXR3 Shield Leaves A (Lit) 1", RewardTier.Legendary, EffectPosition.Feet, EffectTrigger.Looping));
            Register(new EffectDefinition("legendary_bouncing_glow_bubble", "Bouncing Glow Bubble", "CFXR4 Bouncing Glows Bubble (Blue Purple) 1", RewardTier.Legendary, EffectPosition.Torso, EffectTrigger.Looping));
            Register(new EffectDefinition("legendary_fire_breath", "Fire Breath", "CFXR Fire Breath 1", RewardTier.Legendary, EffectPosition.Torso, EffectTrigger.Looping));
        }
    }
}