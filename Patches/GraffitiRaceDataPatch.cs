using BombRushMP.Common.Packets;
using BombRushMP.Plugin.Gamemodes;
using HarmonyLib;

namespace BRCGambling.Patches
{
    [HarmonyPatch(typeof(GraffitiRace), "OnReceive_GraffitiRaceData")]
    internal class GraffitiRaceDataPatch
    {
        private static void Postfix(GraffitiRace __instance, ClientGraffitiRaceGSpots packet)
        {
            GamblingManager.MaxPossibleScore += packet.GraffitiSpots.Count;
            //UnityEngine.Debug.Log($"[BRCGambling] GraffitiRaceData received - added {packet.GraffitiSpots.Count} spots, total={GamblingManager.MaxPossibleScore}");
        }
    }
}