using System.IO;
using System.Collections.Generic;
using CommonAPI;

namespace BRCGambling
{
    public class GamblingSaveData : CustomSaveData
    {
        public static GamblingSaveData Instance { get; private set; }

        public int Rep = 0;
        public List<string> OwnedEffectIds = new List<string>();
        public List<string> EquippedEffectIds = new List<string>();

        public const int MaxEquipped = 5;

        public bool IsEquipped(string effectId) => EquippedEffectIds.Contains(effectId);

        public GamblingSaveData() : base("BRCGambling", "gambling_save_{0}.dat")
        {
            Instance = this;
        }

        public static void Register()
        {
            new GamblingSaveData();
        }

        public override void Read(BinaryReader reader)
        {
            Rep = reader.ReadInt32();
            UnityEngine.Debug.Log($"[BRCGambling] Read called, loaded Rep={Rep}");

            int count = reader.ReadInt32();
            OwnedEffectIds = new List<string>();
            for (int i = 0; i < count; i++)
                OwnedEffectIds.Add(reader.ReadString());

            int equippedCount = reader.ReadInt32();
            EquippedEffectIds = new List<string>();
            for (int i = 0; i < equippedCount; i++)
                EquippedEffectIds.Add(reader.ReadString());
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Rep);

            writer.Write(OwnedEffectIds.Count);
            foreach (var id in OwnedEffectIds)
                writer.Write(id);

            writer.Write(EquippedEffectIds.Count);
            foreach (var id in EquippedEffectIds)
                writer.Write(id);
        }

        public void AddEffect(string effectId)
        {
            if (!OwnedEffectIds.Contains(effectId))
                OwnedEffectIds.Add(effectId);
            Save();
        }

        public bool TryEquip(string effectId)
        {
            if (IsEquipped(effectId)) return true;
            if (EquippedEffectIds.Count >= MaxEquipped) return false;

            EquippedEffectIds.Add(effectId);
            Save();
            EffectTriggerManager.RefreshLoopingEffects();
            return true;
        }

        public void Unequip(string effectId)
        {
            EquippedEffectIds.Remove(effectId);
            Save();
            EffectTriggerManager.RefreshLoopingEffects();
        }

        public void RemoveEffect(string effectId)
        {
            OwnedEffectIds.Remove(effectId);
            EquippedEffectIds.Remove(effectId);
            Save();
            EffectTriggerManager.RefreshLoopingEffects();
        }
    }
}