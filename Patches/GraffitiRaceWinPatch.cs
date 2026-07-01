using BombRushMP.Plugin;
using BombRushMP.Plugin.Gamemodes;
using HarmonyLib;
using System;

namespace BRCGambling.Patches
{
    [HarmonyPatch(typeof(GraffitiRace), "OnEnd")]
    internal class GraffitiRaceWinPatch
    {
        [HarmonyPostfix]
        private static void Postfix(GraffitiRace __instance)
        {
            ClientController instance = ClientController.Instance;
            if (instance == null) return;

            var lobby = instance.ClientLobbyManager?.CurrentLobby;
            if (lobby == null) return;

            ushort localID = instance.LocalID;
            int maxScore = GamblingManager.MaxPossibleScore;
            if (maxScore <= 0) return;

            float playerScore = 0f;
            bool won = false;

            if (__instance.TeamBased)
            {
                if (lobby.LobbyState.Players.ContainsKey(localID))
                {
                    byte team = lobby.LobbyState.Players[localID].Team;
                    float teamScore = lobby.LobbyState.GetScoreForTeam(team);
                    won = teamScore >= maxScore && teamScore > 0f;
                }
            }
            else
            {
                if (lobby.LobbyState.Players.ContainsKey(localID))
                {
                    playerScore = lobby.LobbyState.Players[localID].Score;
                    won = playerScore >= maxScore && playerScore > 0f;
                }
            }

            ChatUI chatUI = ChatUI.Instance;

            if (won)
            {
                GamblingManager.ConsecutiveLosses = 0;

                int lobbyPlayerCount = lobby.LobbyState.Players.Count;
                int bonusRep = 0;

                if (lobbyPlayerCount == 2) bonusRep = 50;
                else if (lobbyPlayerCount == 3) bonusRep = 75;
                else if (lobbyPlayerCount >= 4) bonusRep = 100;

                GamblingManager.Rep += 25 + bonusRep;

                if (chatUI != null && GamblingPlugin.ShowChatMessages.Value)
                {
                    if (bonusRep > 0)
                        chatUI.AddMessage($"<color=blue>+{25 + bonusRep} REP for gracing with {lobbyPlayerCount} players! Total REP: {GamblingManager.Rep}</color>");
                    else
                        chatUI.AddMessage($"<color=green>+25 REP for winning a GRACE! Total REP: {GamblingManager.Rep}</color>");
                }
            }
            else
            {
                GamblingManager.ConsecutiveLosses++;

                if (GamblingManager.ConsecutiveLosses >= 7)
                {
                    int tags = (int)playerScore;
                    int pityRep = 0;

                    if (tags < 6) pityRep = 15;
                    else if (tags >= 6 && tags < 8) pityRep = 25;
                    else if (tags >= 8) pityRep = 40;

                    GamblingManager.Rep += pityRep;

                    if (chatUI != null && GamblingPlugin.ShowChatMessages.Value)
                        chatUI.AddMessage($"<color=yellow>[Pity] +{pityRep} REP for {tags} tags after {GamblingManager.ConsecutiveLosses} losses! Total REP: {GamblingManager.Rep}</color>");
                }
            }

            GamblingManager.MaxPossibleScore = 0;
            EffectTriggerManager.PlayOneShot(EffectTrigger.OnGraceEnd);
        }
    }
}