using UnityEngine;

namespace BRCGambling
{
    public enum EffectTrigger
    {
        Looping,        // always on while equipped
        OnSpray,
        OnJump,
        OnLand,
        OnWallPlant,
        OnBoostTrick,
        OnSlide,
        OnGraceEnd,
        OnDeath,
        OnEmote
    }

    public enum EffectPosition
    {
        Feet,
        Torso,
        AboveHead
    }

    public class EffectDefinition
    {
        public string Id;              // unique key, e.g. "legendary_fire"
        public string DisplayName;     // shown to player, e.g. "Fire"
        public string PrefabName;      // exact name in the AssetBundle
        public RewardTier Rarity;
        public EffectPosition Position;
        public EffectTrigger Trigger;
        public Vector3 PositionOffset = Vector3.zero;

        public EffectDefinition(string id, string displayName, string prefabName, RewardTier rarity, EffectPosition position, EffectTrigger trigger, Vector3 offset = default)
        {
            Id = id;
            DisplayName = displayName;
            PrefabName = prefabName;
            Rarity = rarity;
            Position = position;
            Trigger = trigger;
            PositionOffset = offset;
        }
    }
}