using BombRushMP.Plugin;
using CommonAPI.Phone;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BRCGambling
{
    public class AppGambling : CustomApp
    {
        private static Sprite IconSprite = null;
        private PhoneButton repDisplayButton;
        private bool isOpening = false;

        enum AppScreen
        {
            MainMenu,
            CaseOpening,
            Inventory,
            ItemConfirm,
            SellConfirm,
            Result
        }

        AppScreen currentScreen = AppScreen.MainMenu;
        EffectDefinition currentConfirmDef;

        public static void Initialize()
        {
            PhoneAPI.RegisterApp<AppGambling>("Gambling!", GamblingPlugin.AppIcon);
        }

        public override void OnAppInit()
        {
            base.OnAppInit();
            CreateTitleBar("Monkey Casino", IconSprite);
            ScrollView = PhoneScrollView.Create(this);
            ShowMainMenu();
        }

        void ShowMainMenu()
        {
            currentScreen = AppScreen.MainMenu;
            ScrollView.RemoveAllButtons();

            repDisplayButton = PhoneUIUtility.CreateSimpleButton($"REP: {GamblingManager.Rep}");
            ScrollView.AddButton(repDisplayButton);

            var spinButton = PhoneUIUtility.CreateSimpleButton("Open Spray Case");
            spinButton.OnConfirm += () => ShowCaseOpening();
            ScrollView.AddButton(spinButton);

            var inventoryButton = PhoneUIUtility.CreateSimpleButton("Inventory");
            inventoryButton.OnConfirm += () => ShowInventory();
            ScrollView.AddButton(inventoryButton);
        }

        void ShowCaseOpening()
        {
            currentScreen = AppScreen.CaseOpening;
            ScrollView.RemoveAllButtons();

            var backButton = PhoneUIUtility.CreateSimpleButton("Back");
            backButton.OnConfirm += () => ShowMainMenu();
            ScrollView.AddButton(backButton);

            var costLabel = PhoneUIUtility.CreateSimpleButton($"A case costs {GamblingManager.SpinCost} REP");
            ScrollView.AddButton(costLabel);

            if (GamblingManager.Rep >= GamblingManager.SpinCost)
            {
                var confirmButton = PhoneUIUtility.CreateSimpleButton("Purchase? YES");
                confirmButton.OnConfirm += () =>
                {
                    GamblingManager.Rep -= GamblingManager.SpinCost;
                    ScrollView.RemoveAllButtons();
                    var openingLabel = PhoneUIUtility.CreateSimpleButton("Opening...");
                    ScrollView.AddButton(openingLabel);
                    StartCoroutine(LaunchOverlay());
                };
                ScrollView.AddButton(confirmButton);

                var declineButton = PhoneUIUtility.CreateSimpleButton("Purchase? NO");
                declineButton.OnConfirm += () => ShowMainMenu();
                ScrollView.AddButton(declineButton);
            }
            else
            {
                var noRepButton = PhoneUIUtility.CreateSimpleButton("Not enough REP, boss up and get some.");
                ScrollView.AddButton(noRepButton);
            }

            // Multi open option
            if (GamblingManager.Rep >= 1000)
            {
                var multiLabel = PhoneUIUtility.CreateSimpleButton("── Multi Open ──");
                ScrollView.AddButton(multiLabel);

                var multiButton = PhoneUIUtility.CreateSimpleButton("Open x10 Cases (1000 REP)");
                multiButton.OnConfirm += () =>
                {
                    GamblingManager.Rep -= 1000;
                    ScrollView.RemoveAllButtons();
                    var openingLabel = PhoneUIUtility.CreateSimpleButton("Opening 10 cases...");
                    ScrollView.AddButton(openingLabel);
                    StartCoroutine(LaunchMultiOverlay());
                };
                ScrollView.AddButton(multiButton);
            }
        }

        IEnumerator LaunchOverlay()
        {
            yield return null;
            CaseOpeningOverlay.Create(this, (result, effect) =>
            {
                isOpening = false;
                ShowResult(result, effect);
            });
        }

        void ShowResult(RewardTier result, EffectDefinition effect)
        {
            currentScreen = AppScreen.Result;
            ScrollView.RemoveAllButtons();

            // Back button at top
            var backButton = PhoneUIUtility.CreateSimpleButton("Back");
            backButton.OnConfirm += () => ShowMainMenu();
            ScrollView.AddButton(backButton);

            string itemName = effect != null ? effect.DisplayName : "Unknown Item";

            string tierText = result switch
            {
                RewardTier.Common => $"COMMON - {itemName}",
                RewardTier.Rare => $"RARE - {itemName}",
                RewardTier.Epic => $"EPIC - {itemName}",
                RewardTier.Legendary => $"LEGENDARY - {itemName}",
                _ => "???"
            };

            var resultLabel = PhoneUIUtility.CreateSimpleButton(tierText);
            ScrollView.AddButton(resultLabel);

            if (effect != null)
            {
                bool alreadyOwned = GamblingSaveData.Instance.OwnedEffectIds.Contains(effect.Id);
                int sellValue = GamblingManager.GetSellValue(effect.Rarity);

                if (alreadyOwned)
                {
                    var duplicateLabel = PhoneUIUtility.CreateSimpleButton("You already own this effect!");
                    ScrollView.AddButton(duplicateLabel);

                    var sellButton = PhoneUIUtility.CreateSimpleButton($"Sell for {sellValue} REP");
                    sellButton.OnConfirm += () =>
                    {
                        GamblingManager.Rep += sellValue;
                        ShowMainMenu();
                    };
                    ScrollView.AddButton(sellButton);

                    var keepButton = PhoneUIUtility.CreateSimpleButton("Keep (no duplicate stored)");
                    keepButton.OnConfirm += () => ShowMainMenu();
                    ScrollView.AddButton(keepButton);
                }
                else
                {
                    GamblingSaveData.Instance.AddEffect(effect.Id);

                    var inventoryLabel = PhoneUIUtility.CreateSimpleButton("Added to inventory!");
                    ScrollView.AddButton(inventoryLabel);
                }
            }

            if (result == RewardTier.Epic || result == RewardTier.Legendary)
            {
                string chatMsg = result == RewardTier.Legendary
                    ? $"You opened a <color=yellow>LEGENDARY {itemName}</color>"
                    : $"You opened a <color=red>EPIC {itemName}</color>";

                ChatUI instance = ChatUI.Instance;
                if (instance != null && GamblingPlugin.ShowChatMessages.Value)
                    instance.AddMessage(chatMsg);
            }
        }

        string GetRarityColor(RewardTier rarity)
        {
            switch (rarity)
            {
                case RewardTier.Legendary: return "#FFD700"; // yellow/gold
                case RewardTier.Epic: return "#FF4444"; // red
                case RewardTier.Rare: return "#CC44FF"; // purple
                case RewardTier.Common: return "#4488FF"; // blue
                default: return "#FFFFFF";
            }
        }

        void ShowInventory()
        {
            currentScreen = AppScreen.Inventory;
            ScrollView.RemoveAllButtons();

            // Back button at top
            var backButton = PhoneUIUtility.CreateSimpleButton("Back");
            backButton.OnConfirm += () => ShowMainMenu();
            ScrollView.AddButton(backButton);

            var header = PhoneUIUtility.CreateSimpleButton($"Equipped: {GamblingSaveData.Instance.EquippedEffectIds.Count}/{GamblingSaveData.MaxEquipped}");
            ScrollView.AddButton(header);

            if (GamblingSaveData.Instance.OwnedEffectIds.Count == 0)
            {
                var emptyLabel = PhoneUIUtility.CreateSimpleButton("No items owned yet. Open a case!");
                ScrollView.AddButton(emptyLabel);
            }
            else
            {
                var sorted = GamblingSaveData.Instance.OwnedEffectIds
                    .Select(id => EffectRegistry.Get(id))
                    .Where(def => def != null)
                    .OrderBy(def => def.Rarity switch
                    {
                        RewardTier.Legendary => 0,
                        RewardTier.Epic => 1,
                        RewardTier.Rare => 2,
                        RewardTier.Common => 3,
                        _ => 4
                    });

                foreach (var def in sorted)
                {
                    bool equipped = GamblingSaveData.Instance.IsEquipped(def.Id);
                    string color = GetRarityColor(def.Rarity);
                    string equippedTag = equipped ? " <color=#44FF44>(Equipped)</color>" : "";
                    string label = $"<color={color}>[{def.Rarity}] {def.DisplayName}</color>{equippedTag}";

                    var itemButton = PhoneUIUtility.CreateSimpleButton(label);
                    itemButton.OnConfirm += () => ShowItemConfirm(def);
                    ScrollView.AddButton(itemButton);
                }
            }
        }

        void ShowItemConfirm(EffectDefinition def)
        {
            currentScreen = AppScreen.ItemConfirm;
            currentConfirmDef = def;
            ScrollView.RemoveAllButtons();

            // Back button at top
            var backButton = PhoneUIUtility.CreateSimpleButton("Back");
            backButton.OnConfirm += () => ShowInventory();
            ScrollView.AddButton(backButton);

            bool equipped = GamblingSaveData.Instance.IsEquipped(def.Id);
            string color = GetRarityColor(def.Rarity);
            var nameLabel = PhoneUIUtility.CreateSimpleButton($"<color={color}>[{def.Rarity}] {def.DisplayName}</color>");
            ScrollView.AddButton(nameLabel);

            if (equipped)
            {
                var unequipButton = PhoneUIUtility.CreateSimpleButton("Unequip");
                unequipButton.OnConfirm += () =>
                {
                    GamblingSaveData.Instance.Unequip(def.Id);
                    ShowInventory();
                };
                ScrollView.AddButton(unequipButton);

                var lockedSellLabel = PhoneUIUtility.CreateSimpleButton("Unequip to sell this item.");
                ScrollView.AddButton(lockedSellLabel);
            }
            else
            {
                bool canEquip = GamblingSaveData.Instance.EquippedEffectIds.Count < GamblingSaveData.MaxEquipped;

                if (canEquip)
                {
                    var equipButton = PhoneUIUtility.CreateSimpleButton("Equip");
                    equipButton.OnConfirm += () =>
                    {
                        GamblingSaveData.Instance.TryEquip(def.Id);
                        ShowInventory();
                    };
                    ScrollView.AddButton(equipButton);
                }
                else
                {
                    var fullLabel = PhoneUIUtility.CreateSimpleButton("You need to unequip an effect first.");
                    ScrollView.AddButton(fullLabel);
                }

                int sellValue = GamblingManager.GetSellValue(def.Rarity);
                var sellButton = PhoneUIUtility.CreateSimpleButton($"Sell for {sellValue} REP");
                sellButton.OnConfirm += () => ShowSellConfirm(def, sellValue);
                ScrollView.AddButton(sellButton);
            }
        }

        void ShowSellConfirm(EffectDefinition def, int sellValue)
        {
            currentScreen = AppScreen.SellConfirm;
            currentConfirmDef = def;
            ScrollView.RemoveAllButtons();

            // Back button at top
            var backButton = PhoneUIUtility.CreateSimpleButton("Back");
            backButton.OnConfirm += () => ShowItemConfirm(def);
            ScrollView.AddButton(backButton);

            var warningLabel = PhoneUIUtility.CreateSimpleButton($"Sell {def.DisplayName} for {sellValue} REP? This cannot be undone.");
            ScrollView.AddButton(warningLabel);

            var confirmButton = PhoneUIUtility.CreateSimpleButton("Confirm Sell");
            confirmButton.OnConfirm += () =>
            {
                GamblingSaveData.Instance.RemoveEffect(def.Id);
                GamblingManager.Rep += sellValue;
                ShowInventory();
            };
            ScrollView.AddButton(confirmButton);

            var cancelButton = PhoneUIUtility.CreateSimpleButton("Cancel");
            cancelButton.OnConfirm += () => ShowItemConfirm(def);
            ScrollView.AddButton(cancelButton);
        }

        IEnumerator LaunchMultiOverlay()
        {
            yield return null;
            CaseOpeningOverlay.CreateMulti(this, (results) =>
            {
                ShowMultiResult(results);
            });
        }

        void ShowMultiResult(List<(RewardTier tier, EffectDefinition effect)> results)
        {
            currentScreen = AppScreen.Result;
            ScrollView.RemoveAllButtons();

            var backButton = PhoneUIUtility.CreateSimpleButton("Back");
            backButton.OnConfirm += () => ShowMainMenu();
            ScrollView.AddButton(backButton);

            // Separate duplicates from new items
            var newItems = new List<EffectDefinition>();
            var duplicates = new List<EffectDefinition>();

            foreach (var (tier, effect) in results)
            {
                if (effect == null) continue;
                bool alreadyOwned = GamblingSaveData.Instance.OwnedEffectIds.Contains(effect.Id);
                bool alreadyInNewItems = newItems.Any(e => e.Id == effect.Id);

                if (alreadyOwned || alreadyInNewItems)
                    duplicates.Add(effect);
                else
                    newItems.Add(effect);
            }

            // Add new items to inventory
            foreach (var item in newItems)
                GamblingSaveData.Instance.AddEffect(item.Id);

            // Show duplicate sell prompt if any
            if (duplicates.Count > 0)
            {
                int totalSellValue = 0;
                foreach (var dup in duplicates)
                    totalSellValue += GamblingManager.GetSellValue(dup.Rarity);

                var dupLabel = PhoneUIUtility.CreateSimpleButton($"You rolled {duplicates.Count} duplicate(s)!");
                ScrollView.AddButton(dupLabel);

                var sellDupsButton = PhoneUIUtility.CreateSimpleButton($"Sell all duplicates for {totalSellValue} REP");
                sellDupsButton.OnConfirm += () =>
                {
                    GamblingManager.Rep += totalSellValue;
                    ShowMultiSummary(newItems);
                };
                ScrollView.AddButton(sellDupsButton);

                var keepDupsButton = PhoneUIUtility.CreateSimpleButton("Keep (duplicates not saved)");
                keepDupsButton.OnConfirm += () => ShowMultiSummary(newItems);
                ScrollView.AddButton(keepDupsButton);
            }
            else
            {
                ShowMultiSummary(newItems);
            }
        }

        void ShowMultiSummary(List<EffectDefinition> newItems)
        {
            ScrollView.RemoveAllButtons();

            var backButton = PhoneUIUtility.CreateSimpleButton("Back");
            backButton.OnConfirm += () => ShowMainMenu();
            ScrollView.AddButton(backButton);

            var summaryLabel = PhoneUIUtility.CreateSimpleButton($"Added {newItems.Count} new item(s) to inventory!");
            ScrollView.AddButton(summaryLabel);

            foreach (var item in newItems)
            {
                string color = GetRarityColor(item.Rarity);
                var itemLabel = PhoneUIUtility.CreateSimpleButton($"<color={color}>[{item.Rarity}] {item.DisplayName}</color>");
                ScrollView.AddButton(itemLabel);
            }
        }
    }
}