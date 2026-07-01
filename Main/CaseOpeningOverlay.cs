using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BRCGambling
{
    public class CaseOpeningOverlay : MonoBehaviour
    {
        // Update constants at the top of the class for better scaling
        const int VisibleCount = 7;
        const float ItemWidth = 120f;
        const float ItemHeight = 140f;
        const float ItemSpacing = 12f;
        const float PanelWidth = (ItemWidth + ItemSpacing) * VisibleCount;
        const float PanelHeight = ItemHeight + 60f;

        // Multi open constants
        const float MultiItemWidth = 100f;
        const float MultiItemHeight = 80f;
        const float MultiItemSpacing = 8f;
        const int MultiVisibleCount = 7;
        const float MultiPanelWidth = (MultiItemWidth + MultiItemSpacing) * MultiVisibleCount;
        const float MultiColumnGap = 20f;
        const float MultiStaggerDelay = 0.4f;
        bool isMultiOpen = false;
        System.Action<List<(RewardTier, EffectDefinition)>> onMultiComplete;
        List<RectTransform> multiReelStrips = new List<RectTransform>();
        List<RectTransform> multiDividers = new List<RectTransform>();

        static readonly Color ColorCommon = new Color(0.27f, 0.53f, 1f);    // blue
        static readonly Color ColorRare = new Color(0.8f, 0.27f, 1f);     // purple
        static readonly Color ColorEpic = new Color(1f, 0.27f, 0.27f);    // red
        static readonly Color ColorLegendary = new Color(1f, 0.84f, 0f);       // yellow/gold

        GameObject overlayRoot;
        RectTransform reelStrip;
        RectTransform reelContainerRt;
        RectTransform dividerRect;
        System.Action<RewardTier, EffectDefinition> onComplete;
        bool isSpinning = false;

        static readonly List<RewardTier> ReelPool = new List<RewardTier>
        {
            RewardTier.Common, RewardTier.Common, RewardTier.Common, RewardTier.Common,
            RewardTier.Common, RewardTier.Common, RewardTier.Common, RewardTier.Common,
            RewardTier.Rare, RewardTier.Rare, RewardTier.Rare, RewardTier.Rare,
            RewardTier.Epic, RewardTier.Epic,
            RewardTier.Legendary
        };

        public static CaseOpeningOverlay Create(MonoBehaviour host, System.Action<RewardTier, EffectDefinition> onComplete)
        {
            GameObject go = new GameObject("CaseOpeningOverlay");
            CaseOpeningOverlay overlay = go.AddComponent<CaseOpeningOverlay>();
            overlay.onComplete = onComplete;
            overlay.Build();
            return overlay;
        }

        public static CaseOpeningOverlay CreateMulti(MonoBehaviour host, System.Action<List<(RewardTier, EffectDefinition)>> onMultiComplete)
        {
            GameObject go = new GameObject("CaseOpeningOverlayMulti");
            CaseOpeningOverlay overlay = go.AddComponent<CaseOpeningOverlay>();
            overlay.onMultiComplete = onMultiComplete;
            overlay.isMultiOpen = true;
            overlay.BuildMulti();
            return overlay;
        }

        void Build(int rollCount = 1)
        {
            // ---- Root canvas ----
            overlayRoot = new GameObject("CaseCanvas");
            Canvas canvas = overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            // Scale canvas to 1920x1080 reference resolution
            CanvasScaler scaler = overlayRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            overlayRoot.AddComponent<GraphicRaycaster>();

            // ---- Panel (centered) ----
            // Height scales with roll count for multi-roll later
            float totalPanelHeight = (PanelHeight + 10f) * rollCount + 20f;

            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(overlayRoot.transform, false);
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(PanelWidth + 40f, totalPanelHeight);
            panelRt.anchoredPosition = Vector2.zero;

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);

            // ---- Reel container (vertical layout for multiple reels) ----
            GameObject reelContainer = new GameObject("ReelContainer");
            reelContainer.transform.SetParent(panel.transform, false);
            RectTransform containerRt = reelContainer.AddComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0.5f, 0.5f);
            containerRt.anchorMax = new Vector2(0.5f, 0.5f);
            containerRt.pivot = new Vector2(0.5f, 0.5f);
            containerRt.sizeDelta = new Vector2(PanelWidth, totalPanelHeight - 20f);
            containerRt.anchoredPosition = Vector2.zero;

            // Store container reference for multi-roll
            reelContainerRt = containerRt;

            // ---- Build first reel (single roll for now) ----
            BuildReel(reelContainer, 0);

            StartCoroutine(SpinReel());
        }

        void BuildReel(GameObject container, int reelIndex)
        {
            float reelSlotHeight = PanelHeight + 10f;
            float yOffset = -(reelIndex * reelSlotHeight);

            // ---- Viewport ----
            GameObject viewport = new GameObject($"Viewport_{reelIndex}");
            viewport.transform.SetParent(container.transform, false);
            RectTransform vpRt = viewport.AddComponent<RectTransform>();
            vpRt.anchorMin = new Vector2(0.5f, 1f);
            vpRt.anchorMax = new Vector2(0.5f, 1f);
            vpRt.pivot = new Vector2(0.5f, 1f);
            vpRt.sizeDelta = new Vector2(PanelWidth, ItemHeight + 20f);
            vpRt.anchoredPosition = new Vector2(0, yOffset);
            viewport.AddComponent<RectMask2D>();

            // ---- Reel strip ----
            GameObject strip = new GameObject($"ReelStrip_{reelIndex}");
            strip.transform.SetParent(viewport.transform, false);
            RectTransform stripRt = strip.AddComponent<RectTransform>();
            stripRt.anchorMin = new Vector2(0, 0.5f);
            stripRt.anchorMax = new Vector2(0, 0.5f);
            stripRt.pivot = new Vector2(0, 0.5f);
            stripRt.anchoredPosition = Vector2.zero;

            // Store reference for single roll — multi-roll will use a list
            reelStrip = stripRt;

            // ---- Center divider line ----
            GameObject dividerObj = new GameObject("Divider");
            dividerObj.transform.SetParent(container.transform, false);
            RectTransform divRt = dividerObj.AddComponent<RectTransform>();
            divRt.anchorMin = new Vector2(0.5f, 1f);
            divRt.anchorMax = new Vector2(0.5f, 1f);
            divRt.pivot = new Vector2(0.5f, 0.5f);
            divRt.sizeDelta = new Vector2(2f, ItemHeight + 24f);
            divRt.anchoredPosition = new Vector2(0, yOffset - (ItemHeight * 0.5f) - 10f);
            dividerObj.AddComponent<Image>().color = Color.white;

            dividerRect = divRt;
        }

        GameObject CreateLine(GameObject parent, Vector2 pos, Vector2 size)
        {
            GameObject line = new GameObject("Line");
            line.transform.SetParent(parent.transform, false);
            RectTransform rt = line.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            Image img = line.AddComponent<Image>();
            img.color = Color.white;
            return line;
        }

        IEnumerator SpinReel()
        {
            if (isSpinning) yield break;
            isSpinning = true;

            float stepX = ItemWidth + ItemSpacing;

            // 1. Pick result first
            RewardTier result = ReelPool[Random.Range(0, ReelPool.Count)];

            // 2. Build randomized strip
            List<RewardTier> items = new List<RewardTier>();
            int totalItems = 60;
            for (int i = 0; i < totalItems; i++)
                items.Add(ReelPool[Random.Range(0, ReelPool.Count)]);

            // 3. Force result at landing index
            int landingIndex = totalItems - 11;
            items[landingIndex] = result;

            // 4. Populate strip visually
            for (int i = 0; i < items.Count; i++)
            {
                GameObject item = CreateReelItem(items[i]);
                item.transform.SetParent(reelStrip, false);
                item.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * stepX, 0);
            }

            reelStrip.sizeDelta = new Vector2(items.Count * stepX, ItemHeight);

            // 5. Calculate targetX
            float targetX = (PanelWidth / 2f) - (landingIndex * stepX) - ItemWidth;

            // 6. Animate
            float duration = 8f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                reelStrip.anchoredPosition = new Vector2(Mathf.Lerp(0f, targetX, eased), 0);
                yield return null;
            }

            reelStrip.anchoredPosition = new Vector2(targetX, 0);
            Canvas.ForceUpdateCanvases();

            // 7. Use world positions to find which item is actually under the divider
            float dividerWorldX = dividerRect.position.x;
            float minDist = float.MaxValue;
            int closestIndex = 0;

            for (int i = 0; i < reelStrip.childCount; i++)
            {
                RectTransform child = reelStrip.GetChild(i) as RectTransform;
                float dist = Mathf.Abs(child.position.x - dividerWorldX);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestIndex = i;
                }
            }

            RewardTier visualResult = items[closestIndex];
            UnityEngine.Debug.Log($"[BRCGambling] Divider worldX={dividerWorldX} closest index={closestIndex} tier={visualResult}");

            // 8. CS:GO-style winner reveal
            yield return StartCoroutine(PlayWinReveal(closestIndex));

            // 9. Apply reward
            GamblingManager.ApplyReward(visualResult);

            var pool = EffectRegistry.GetPool(visualResult);
            EffectDefinition wonEffect = pool.Count > 0
                ? pool[Random.Range(0, pool.Count)]
                : null;

            

            yield return new WaitForSeconds(1.5f);

            onComplete?.Invoke(visualResult, wonEffect);
            Destroy(overlayRoot);
            Destroy(gameObject);
        }

        IEnumerator PlayWinReveal(int winnerIndex)
        {
            float duration = 0.55f;
            float elapsed = 0f;

            int count = reelStrip.childCount;
            RectTransform[] children = new RectTransform[count];
            CanvasGroup[] groups = new CanvasGroup[count];
            Vector3[] startScales = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                children[i] = reelStrip.GetChild(i) as RectTransform;
                startScales[i] = children[i].localScale;

                groups[i] = children[i].GetComponent<CanvasGroup>();
                if (groups[i] == null)
                    groups[i] = children[i].gameObject.AddComponent<CanvasGroup>();
            }

            Vector3 winnerTargetScale = new Vector3(1.18f, 1.18f, 1f);
            Vector3 surroundTargetScale = new Vector3(0.82f, 0.82f, 1f);

            // Add gold highlight border to the winner card
            Transform winnerChild = reelStrip.GetChild(winnerIndex);
            Transform winnerCard = winnerChild?.Find("Card");

            if (winnerCard != null)
                CreateOutlineFrame(winnerCard, 3f, new Color(1f, 0.85f, 0f, 0f));

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float e = 1f - Mathf.Pow(1f - t, 3f);

                for (int i = 0; i < count; i++)
                {
                    if (i == winnerIndex)
                    {
                        children[i].localScale = Vector3.Lerp(startScales[i], winnerTargetScale, e);
                        groups[i].alpha = 1f;
                    }
                    else
                    {
                        children[i].localScale = Vector3.Lerp(startScales[i], surroundTargetScale, e);
                        groups[i].alpha = Mathf.Lerp(1f, 0.28f, e);
                    }
                }

                if (winnerCard != null)
                {
                    Color edgeColor = new Color(1f, 0.85f, 0f, Mathf.Lerp(0f, 1f, e));
                    foreach (Image edge in winnerCard.GetComponentsInChildren<Image>())
                    {
                        if (edge.gameObject.name == "OutlineEdge")
                            edge.color = edgeColor;
                    }
                }

                yield return null;
            }

            yield return new WaitForSeconds(1f);
        }

        // Returns the top edge Image as a handle for alpha; all four share the same color so driving one drives all.
        Image CreateOutlineFrame(Transform parent, float thickness, Color startColor)
        {
            Image firstEdge = null;

            // top, bottom, left, right
            (Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)[] edges =
            {
                (new Vector2(0,1), new Vector2(1,1), new Vector2(-thickness, 0),          new Vector2(thickness, thickness)),
                (new Vector2(0,0), new Vector2(1,0), new Vector2(-thickness, -thickness), new Vector2(thickness, 0)),
                (new Vector2(0,0), new Vector2(0,1), new Vector2(-thickness, -thickness), new Vector2(0,          thickness)),
                (new Vector2(1,0), new Vector2(1,1), new Vector2(0,          -thickness), new Vector2(thickness,  thickness)),
            };

            foreach (var edge in edges)
            {
                GameObject strip = new GameObject("OutlineEdge");
                strip.transform.SetParent(parent, false);
                RectTransform rt = strip.AddComponent<RectTransform>();
                rt.anchorMin = edge.anchorMin;
                rt.anchorMax = edge.anchorMax;
                rt.offsetMin = edge.offsetMin;
                rt.offsetMax = edge.offsetMax;
                Image img = strip.AddComponent<Image>();
                img.color = startColor;
                if (firstEdge == null) firstEdge = img;
            }

            return firstEdge;
        }

        GameObject CreateReelItem(RewardTier tier)
        {
            Color rarityColor = tier switch
            {
                RewardTier.Common => ColorCommon,
                RewardTier.Rare => ColorRare,
                RewardTier.Epic => ColorEpic,
                RewardTier.Legendary => ColorLegendary,
                _ => Color.white
            };

            string label = tier switch
            {
                RewardTier.Common => "COM",
                RewardTier.Rare => "RARE",
                RewardTier.Epic => "EPIC",
                RewardTier.Legendary => "LEG",
                _ => "?"
            };

            // Item root
            GameObject item = new GameObject($"Item_{tier}");
            RectTransform itemRt = item.AddComponent<RectTransform>();
            itemRt.sizeDelta = new Vector2(ItemWidth, ItemHeight);

            // Card background
            GameObject card = new GameObject("Card");
            card.transform.SetParent(item.transform, false);
            RectTransform cardRt = card.AddComponent<RectTransform>();
            cardRt.anchorMin = Vector2.zero;
            cardRt.anchorMax = Vector2.one;
            cardRt.offsetMin = Vector2.zero;
            cardRt.offsetMax = new Vector2(0, -20f);
            Image cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            // Label
            GameObject txt = new GameObject("Label");
            txt.transform.SetParent(card.transform, false);
            RectTransform txtRt = txt.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            TMPro.TextMeshProUGUI tmp = txt.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontSize = 18;
            tmp.color = Color.white;

            // Rarity bar
            GameObject bar = new GameObject("RarityBar");
            bar.transform.SetParent(item.transform, false);
            RectTransform barRt = bar.AddComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0, 0);
            barRt.anchorMax = new Vector2(1, 0);
            barRt.pivot = new Vector2(0.5f, 0);
            barRt.sizeDelta = new Vector2(0, 18f);
            barRt.anchoredPosition = Vector2.zero;
            Image barImg = bar.AddComponent<Image>();
            barImg.color = rarityColor;

            return item;
        }

        void BuildMulti()
        {
            // ---- Root canvas ----
            overlayRoot = new GameObject("CaseCanvas");
            Canvas canvas = overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            CanvasScaler scaler = overlayRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            overlayRoot.AddComponent<GraphicRaycaster>();

            float columnWidth = MultiPanelWidth + 40f;
            float totalWidth = columnWidth * 2f + MultiColumnGap;
            float reelSlotHeight = MultiItemHeight + 30f;
            float totalHeight = reelSlotHeight * 5f + 40f;

            // ---- Main panel (centered) ----
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(overlayRoot.transform, false);
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(totalWidth, totalHeight);
            panelRt.anchoredPosition = Vector2.zero;
            panel.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 0.95f);

            // Build left column (reels 0-4) and right column (reels 5-9)
            for (int col = 0; col < 2; col++)
            {
                float colX = col == 0
                    ? -(columnWidth * 0.5f + MultiColumnGap * 0.5f)
                    : (columnWidth * 0.5f + MultiColumnGap * 0.5f);

                for (int row = 0; row < 5; row++)
                {
                    int reelIndex = col * 5 + row;
                    float rowY = (totalHeight * 0.5f) - 20f - (row * reelSlotHeight) - reelSlotHeight * 0.5f;

                    // Viewport
                    GameObject viewport = new GameObject($"Viewport_{reelIndex}");
                    viewport.transform.SetParent(panel.transform, false);
                    RectTransform vpRt = viewport.AddComponent<RectTransform>();
                    vpRt.anchorMin = new Vector2(0.5f, 0.5f);
                    vpRt.anchorMax = new Vector2(0.5f, 0.5f);
                    vpRt.pivot = new Vector2(0.5f, 0.5f);
                    vpRt.sizeDelta = new Vector2(MultiPanelWidth, MultiItemHeight + 10f);
                    vpRt.anchoredPosition = new Vector2(colX, rowY);
                    viewport.AddComponent<RectMask2D>();

                    // Reel strip
                    GameObject strip = new GameObject($"ReelStrip_{reelIndex}");
                    strip.transform.SetParent(viewport.transform, false);
                    RectTransform stripRt = strip.AddComponent<RectTransform>();
                    stripRt.anchorMin = new Vector2(0, 0.5f);
                    stripRt.anchorMax = new Vector2(0, 0.5f);
                    stripRt.pivot = new Vector2(0, 0.5f);
                    stripRt.anchoredPosition = Vector2.zero;
                    multiReelStrips.Add(stripRt);

                    // Divider
                    GameObject divObj = new GameObject($"Divider_{reelIndex}");
                    divObj.transform.SetParent(panel.transform, false);
                    RectTransform divRt = divObj.AddComponent<RectTransform>();
                    divRt.anchorMin = new Vector2(0.5f, 0.5f);
                    divRt.anchorMax = new Vector2(0.5f, 0.5f);
                    divRt.pivot = new Vector2(0.5f, 0.5f);
                    divRt.sizeDelta = new Vector2(2f, MultiItemHeight + 14f);
                    divRt.anchoredPosition = new Vector2(colX, rowY);
                    divObj.AddComponent<Image>().color = Color.white;
                    multiDividers.Add(divRt);
                }
            }

            StartCoroutine(SpinMultiReel());
        }
        IEnumerator SpinMultiReel()
        {
            float stepX = MultiItemWidth + MultiItemSpacing;
            int totalItems = 40;
            int landingIndex = totalItems - 8;

            var results = new List<(RewardTier tier, EffectDefinition effect)>();
            var reelCoroutines = new List<Coroutine>();

            // Pre-roll all 10 results
            var rolledTiers = new List<RewardTier>();
            var rolledEffects = new List<EffectDefinition>();

            for (int i = 0; i < 10; i++)
            {
                RewardTier tier = ReelPool[Random.Range(0, ReelPool.Count)];
                var pool = EffectRegistry.GetPool(tier);
                EffectDefinition effect = pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : null;
                rolledTiers.Add(tier);
                rolledEffects.Add(effect);
            }

            // Spin each reel with stagger
            bool[] reelDone = new bool[10];

            for (int i = 0; i < 10; i++)
            {
                int capturedI = i;
                StartCoroutine(SpinSingleMultiReel(
                    multiReelStrips[capturedI],
                    multiDividers[capturedI],
                    rolledTiers[capturedI],
                    stepX,
                    totalItems,
                    landingIndex,
                    () => reelDone[capturedI] = true
                ));

                yield return new WaitForSeconds(MultiStaggerDelay);
            }

            // Wait for all reels to finish
            bool allDone = false;
            while (!allDone)
            {
                allDone = true;
                for (int i = 0; i < 10; i++)
                    if (!reelDone[i]) { allDone = false; break; }
                yield return null;
            }

            yield return new WaitForSeconds(3f);

            // Build result list
            for (int i = 0; i < 10; i++)
                results.Add((rolledTiers[i], rolledEffects[i]));

            onMultiComplete?.Invoke(results);
            Destroy(overlayRoot);
            Destroy(gameObject);
        }

        IEnumerator SpinSingleMultiReel(RectTransform strip, RectTransform divider, RewardTier result, float stepX, int totalItems, int landingIndex, System.Action onDone)
        {
            List<RewardTier> items = new List<RewardTier>();
            for (int i = 0; i < totalItems; i++)
                items.Add(ReelPool[Random.Range(0, ReelPool.Count)]);
            items[landingIndex] = result;

            for (int i = 0; i < items.Count; i++)
            {
                GameObject item = CreateMultiReelItem(items[i]);
                item.transform.SetParent(strip, false);
                item.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * stepX, 0);
            }

            strip.sizeDelta = new Vector2(items.Count * stepX, MultiItemHeight);

            float targetX = (MultiPanelWidth / 2f) - (landingIndex * stepX) - MultiItemWidth;
            float duration = 5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                strip.anchoredPosition = new Vector2(Mathf.Lerp(0f, targetX, eased), 0);
                yield return null;
            }

            strip.anchoredPosition = new Vector2(targetX, 0);
            onDone?.Invoke();
        }

        GameObject CreateMultiReelItem(RewardTier tier)
        {
            Color rarityColor = tier switch
            {
                RewardTier.Common => new Color(0.27f, 0.53f, 1f),
                RewardTier.Rare => new Color(0.8f, 0.27f, 1f),
                RewardTier.Epic => new Color(1f, 0.27f, 0.27f),
                RewardTier.Legendary => new Color(1f, 0.84f, 0f),
                _ => Color.white
            };

            string label = tier switch
            {
                RewardTier.Common => "COM",
                RewardTier.Rare => "RARE",
                RewardTier.Epic => "EPIC",
                RewardTier.Legendary => "LEG",
                _ => "?"
            };

            GameObject item = new GameObject($"Item_{tier}");
            item.AddComponent<RectTransform>().sizeDelta = new Vector2(MultiItemWidth, MultiItemHeight);

            GameObject card = new GameObject("Card");
            card.transform.SetParent(item.transform, false);
            RectTransform cardRt = card.AddComponent<RectTransform>();
            cardRt.anchorMin = Vector2.zero;
            cardRt.anchorMax = Vector2.one;
            cardRt.offsetMin = Vector2.zero;
            cardRt.offsetMax = new Vector2(0, -14f);
            card.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);

            GameObject txt = new GameObject("Label");
            txt.transform.SetParent(card.transform, false);
            RectTransform txtRt = txt.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            TMPro.TextMeshProUGUI tmp = txt.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontSize = 14;
            tmp.color = Color.white;

            GameObject bar = new GameObject("RarityBar");
            bar.transform.SetParent(item.transform, false);
            RectTransform barRt = bar.AddComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0, 0);
            barRt.anchorMax = new Vector2(1, 0);
            barRt.pivot = new Vector2(0.5f, 0);
            barRt.sizeDelta = new Vector2(0, 12f);
            barRt.anchoredPosition = Vector2.zero;
            bar.AddComponent<Image>().color = rarityColor;

            return item;
        }
    }
}