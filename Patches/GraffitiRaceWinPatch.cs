using BombRushMP.Plugin;
using BombRushMP.Plugin.Gamemodes;
using HarmonyLib;

namespace BRCGambling.Patches
{
    [HarmonyPatch(typeof(GraffitiRace), "OnEnd")]
    internal class GraffitiRaceWinPatch
    {
        [HarmonyPostfix]
        private static void Postfix(GraffitiRace __instance)
        {
            UnityEngine.Debug.Log("[BRCGambling] GraffitiRace.OnEnd fired!");

            ClientController instance = ClientController.Instance;
            if (instance == null)
            {
                UnityEngine.Debug.Log("[BRCGambling] ClientController is null");
                return;
            }

            var lobby = instance.ClientLobbyManager?.CurrentLobby;
            if (lobby == null)
            {
                UnityEngine.Debug.Log("[BRCGambling] No lobby found");
                return;
            }

            ushort localID = instance.LocalID;
            int maxScore = GamblingManager.MaxPossibleScore;
            UnityEngine.Debug.Log($"[BRCGambling] localID={localID} maxScore={maxScore}");

            if (maxScore <= 0)
            {
                UnityEngine.Debug.Log("[BRCGambling] maxScore is 0, aborting");
                return;
            }

            float playerScore = 0f;
            bool won = false;

            if (__instance.TeamBased)
            {
                if (lobby.LobbyState.Players.ContainsKey(localID))
                {
                    byte team = lobby.LobbyState.Players[localID].Team;
                    float teamScore = lobby.LobbyState.GetScoreForTeam(team);
                    won = teamScore >= maxScore && teamScore > 0f;
                    UnityEngine.Debug.Log($"[BRCGambling] Team mode - team={team} teamScore={teamScore} won={won}");
                }
            }
            else
            {
                if (lobby.LobbyState.Players.ContainsKey(localID))
                {
                    playerScore = lobby.LobbyState.Players[localID].Score;
                    won = playerScore >= maxScore && playerScore > 0f;
                    UnityEngine.Debug.Log($"[BRCGambling] Solo mode - playerScore={playerScore} maxScore={maxScore} won={won}");
                }
                else
                {
                    UnityEngine.Debug.Log($"[BRCGambling] LocalID {localID} not found in players");
                }
            }

            if (won)
            {
                GamblingManager.Rep += 25;

                int lobbyPlayerCount = lobby.LobbyState.Players.Count;
                int bonusRep = 0;

                if (lobbyPlayerCount == 2)
                    bonusRep = 25;
                else if (lobbyPlayerCount == 3)
                    bonusRep = 50;
                else if (lobbyPlayerCount >= 4)
                    bonusRep = 75;

                GamblingManager.Rep += bonusRep;

                ChatUI chatUI = ChatUI.Instance;
                if (chatUI != null)
                {
                    if (bonusRep > 0)
                        chatUI.AddMessage($"<color=blue>+{25 + bonusRep} REP for gracing with {lobbyPlayerCount} players! Total REP: {GamblingManager.Rep}</color>");
                    else
                        chatUI.AddMessage($"<color=yellow>+25 REP for winning a GRACE! Total REP: {GamblingManager.Rep}</color>");
                }
            }

            GamblingManager.MaxPossibleScore = 0;

            EffectTriggerManager.PlayOneShot(EffectTrigger.OnGraceEnd);
        }
    }
}