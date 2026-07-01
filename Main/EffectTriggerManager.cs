using System.Collections.Generic;
using UnityEngine;

namespace BRCGambling
{
    public static class EffectTriggerManager
    {
        // Currently active looping effect GameObjects, keyed by effect id
        static Dictionary<string, GameObject> activeLoopingEffects = new Dictionary<string, GameObject>();

        public static void RefreshLoopingEffects()
        {
            // Remove any active effects that are no longer equipped
            var toRemove = new List<string>();
            foreach (var kvp in activeLoopingEffects)
            {
                if (!GamblingSaveData.Instance.IsEquipped(kvp.Key))
                {
                    if (kvp.Value != null)
                        Object.Destroy(kvp.Value);
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var id in toRemove)
                activeLoopingEffects.Remove(id);

            // Spawn any equipped looping effects not currently active
            foreach (var effectId in GamblingSaveData.Instance.EquippedEffectIds)
            {
                var def = EffectRegistry.Get(effectId);
                if (def == null) continue;
                if (def.Trigger != EffectTrigger.Looping) continue;
                if (activeLoopingEffects.ContainsKey(effectId)) continue;

                Transform playerTransform = PlayerEffects.GetLocalPlayerTransform();
                if (playerTransform == null) continue;

                Vector3 offset = GetOffsetForPosition(def.Position) + def.PositionOffset;
                GameObject instance = PlayerEffects.SpawnPersistentEffectByName(def.PrefabName, playerTransform, offset);
                if (instance != null)
                    activeLoopingEffects[effectId] = instance;
            }
        }

        public static void PlayOneShot(EffectTrigger trigger)
        {
            foreach (var effectId in GamblingSaveData.Instance.EquippedEffectIds)
            {
                var def = EffectRegistry.Get(effectId);
                if (def == null) continue;
                if (def.Trigger != trigger) continue;

                Transform playerTransform = PlayerEffects.GetLocalPlayerTransform();
                if (playerTransform == null) continue;

                Vector3 offset = GetOffsetForPosition(def.Position) + def.PositionOffset;
                PlayerEffects.SpawnEffectByName(def.PrefabName, playerTransform, offset, 5f); // one-shot effects auto destroy after 5s
            }
        }

        public static Vector3 GetOffsetForPosition(EffectPosition pos)
        {
            switch (pos)
            {
                case EffectPosition.Feet: return new Vector3(0, 0.1f, 0);
                case EffectPosition.Torso: return new Vector3(0, 1.0f, 0);
                case EffectPosition.AboveHead: return new Vector3(0, 2.2f, 0);
                default: return Vector3.zero;
            }
        }

        public static void ClearAll()
        {
            foreach (var kvp in activeLoopingEffects)
                if (kvp.Value != null)
                    Object.Destroy(kvp.Value);
            activeLoopingEffects.Clear();
        }
    }
}