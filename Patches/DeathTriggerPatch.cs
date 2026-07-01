using HarmonyLib;
using Reptile;

namespace BRCGambling.Patches
{
    [HarmonyPatch(typeof(DieAbility), "OnStartAbility")]
    internal class DeathTriggerPatch
    {
        static void Postfix(DieAbility __instance)
        {
            EffectTriggerManager.PlayOneShot(EffectTrigger.OnDeath);
        }
    }
}