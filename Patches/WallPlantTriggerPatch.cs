using HarmonyLib;

namespace BRCGambling.Patches
{
    [HarmonyPatch(typeof(WallPlant.WallPlantAbility), "OnStartAbility")]
    internal class WallPlantTriggerPatch
    {
        static void Postfix(WallPlant.WallPlantAbility __instance)
        {
            EffectTriggerManager.PlayOneShot(EffectTrigger.OnWallPlant);
        }
    }
}