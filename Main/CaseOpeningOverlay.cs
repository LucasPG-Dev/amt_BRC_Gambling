using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BRCGambling
{
    public class CaseOpeningOverlay : MonoBehaviour
    {
        const int VisibleCount = 5;
        const float ItemWidth = 80f;
        const float ItemHeight = 100f;
        const float ItemSpacing = 10f;
        const float PanelWidth = (ItemWidth + ItemSpacing) * VisibleCount;
        const float PanelHeight = ItemHeight + 40f;

        static readonly Color ColorCommon = new Color(0.4f, 0.6f, 1f);
        static readonly Color ColorRare = new Color(0.6f, 0.2f, 0.8f);
        static readonly Color ColorEpic = new Color(0.9f, 0.2f, 0.2f);
        static readonly Color ColorLegendary = new Color(1f, 0.8f, 0f);

        GameObject overlayRoot;
        RectTransform reelStrip;
        RectTransform dividerRect;
        System.Action<RewardTier, EffectDefinition> onComplete;
        bool isSpinning = false;

        static readonly List<RewardTier> ReelPool = new List<RewardTier>
        {
            RewardTier.Common, RewardTier.Common, RewardTier.Common, RewardTier.Common,
            RewardTier.Common, RewardTier.Common, RewardTier.Common, RewardTier.Common,
            RewardTier.Rare, RewardTier.Rare, RewardTier.Rare,
            RewardTier.Epic,
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

        void Build()
        {
            // ---- Root canvas ----
            overlayRoot = new GameObject("CaseCanvas");
            Canvas canvas = overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            overlayRoot.AddComponent<CanvasScaler>();
            overlayRoot.AddComponent<GraphicRaycaster>();

            // ---- Panel (bottom right) ----
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(overlayRoot.transform, false);
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(1, 0);
            panelRt.anchorMax = new Vector2(1, 0);
            panelRt.pivot = new Vector2(1, 0);
            panelRt.sizeDelta = new Vector2(PanelWidth + 20f, PanelHeight + 20f);
            panelRt.anchoredPosition = new Vector2(-20f, 20f);

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);

            // ---- Viewport ----
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(panel.transform, false);
            RectTransform vpRt = viewport.AddComponent<RectTransform>();
            vpRt.anchorMin = new Vector2(0.5f, 0.5f);
            vpRt.anchorMax = new Vector2(0.5f, 0.5f);
            vpRt.pivot = new Vector2(0.5f, 0.5f);
            vpRt.sizeDelta = new Vector2(PanelWidth, ItemHeight + 20f);
            vpRt.anchoredPosition = Vector2.zero;
            viewport.AddComponent<RectMask2D>();

            // ---- Reel strip ----
            GameObject strip = new GameObject("ReelStrip");
            strip.transform.SetParent(viewport.transform, false);
            reelStrip = strip.AddComponent<RectTransform>();
            reelStrip.anchorMin = new Vector2(0, 0.5f);
            reelStrip.anchorMax = new Vector2(0, 0.5f);
            reelStrip.pivot = new Vector2(0, 0.5f);
            reelStrip.anchoredPosition = Vector2.zero;

            // ---- Single center selector line ----
            GameObject dividerObj = CreateLine(panel, Vector2.zero, new Vector2(2f, ItemHeight + 24f));
            dividerRect = dividerObj.GetComponent<RectTransform>();

            StartCoroutine(SpinReel());
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
    }
}