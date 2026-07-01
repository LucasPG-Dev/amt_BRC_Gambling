using Reptile;
using UnityEngine;

namespace BRCGambling
{
    public static class GamblingManager
    {
        //FOR TESTING PURPOSES ONLY - DELETE FOR FINAL BUILD - public const int SpinCost = 100;
        public static int SpinCost => 100;
        public static int StartingRep => 500;
        public static int Rep
        {
            get => GamblingSaveData.Instance?.Rep ?? 0;
            set
            {
                if (GamblingSaveData.Instance != null)
                {
                    GamblingSaveData.Instance.Rep = value;
                    GamblingSaveData.Instance.Save();
                }
            }
        }

        // Active buffs
        public static float ScoreMultiplier = 1f;
        public static float SpeedBonus = 0f;
        public static float BuffTimeRemaining = 0f;
        public static int MaxPossibleScore = 0;

        public static void ApplyReward(RewardTier tier)
        {
            switch (tier)
            {
                case RewardTier.Common:
                    ScoreMultiplier = 1.2f;
                    SpeedBonus = 0f;
                    BuffTimeRemaining = 30f;
                    break;
                case RewardTier.Rare:
                    ScoreMultiplier = 1.5f;
                    SpeedBonus = 0f;
                    BuffTimeRemaining = 45f;
                    break;
                case RewardTier.Epic:
                    ScoreMultiplier = 1.5f;
                    SpeedBonus = 2f;
                    BuffTimeRemaining = 45f;
                    break;
                case RewardTier.Legendary:
                    ScoreMultiplier = 2f;
                    SpeedBonus = 4f;
                    BuffTimeRemaining = 60f;
                    break;
            }
        }

        // Call this from a MonoBehaviour Update tick
        public static void Tick(float deltaTime)
        {
            if (BuffTimeRemaining <= 0f) return;
            BuffTimeRemaining -= deltaTime;
            if (BuffTimeRemaining <= 0f)
            {
                ScoreMultiplier = 1f;
                SpeedBonus = 0f;
            }
        }

        public static int GetSellValue(RewardTier rarity)
        {
            switch (rarity)
            {
                case RewardTier.Common: return 15;
                case RewardTier.Rare: return 60;
                case RewardTier.Epic: return 220;
                case RewardTier.Legendary: return 400;
                default: return 0;
            }
        }
    }




    public enum RewardTier { Common, Rare, Epic, Legendary }
}