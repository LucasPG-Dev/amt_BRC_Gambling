using HarmonyLib;
using Reptile;

namespace BRCGambling.Patches
{
    [HarmonyPatch(typeof(Player), "DoTrick")]
    internal class PlayerTrickTriggerPatch
    {
        static void Postfix(Player __instance, Player.TrickType type, string trickName, int trickNum)
        {
            bool isAI = Traverse.Create(__instance).Field("isAI").GetValue<bool>();
            if (isAI) return;

            switch (type)
            {
                case Player.TrickType.AIR_BOOST:
                case Player.TrickType.GROUND_BOOST:
                case Player.TrickType.GRIND_BOOST:
                    EffectTriggerManager.PlayOneShot(EffectTrigger.OnBoostTrick);
                    break;

                case Player.TrickType.SLIDE:
                    EffectTriggerManager.PlayOneShot(EffectTrigger.OnSlide);
                    break;

                case Player.TrickType.GRAFFITI_S:
                case Player.TrickType.GRAFFITI_M:
                case Player.TrickType.GRAFFITI_L:
                case Player.TrickType.GRAFFITI_XL:
                    EffectTriggerManager.PlayOneShot(EffectTrigger.OnSpray);
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(Player), "Jump")]
    internal class PlayerJumpTriggerPatch
    {
        static void Postfix(Player __instance)
        {
            bool isAI = Traverse.Create(__instance).Field("isAI").GetValue<bool>();
            if (isAI) return;
            EffectTriggerManager.PlayOneShot(EffectTrigger.OnJump);
        }
    }

    [HarmonyPatch(typeof(Player), "OnLanded")]
    internal class PlayerLandTriggerPatch
    {
        static void Postfix(Player __instance)
        {
            bool isAI = Traverse.Create(__instance).Field("isAI").GetValue<bool>();
            if (isAI) return;
            EffectTriggerManager.PlayOneShot(EffectTrigger.OnLand);
        }
    }

    [HarmonyPatch(typeof(DanceAbility), "OnStartAbility")]
    internal class EmoteTriggerPatch
    {
        static void Postfix(DanceAbility __instance)
        {
            EffectTriggerManager.PlayOneShot(EffectTrigger.OnEmote);
        }
    }
}