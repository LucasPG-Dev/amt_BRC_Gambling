using BepInEx;
using BepInEx.Configuration;
using CommonAPI;
using System.IO;
using UnityEngine;

namespace BRCGambling
{
    [BepInPlugin("com.chimp.brcgambling", "BRC Gambling", "1.0.0")]
    [BepInDependency("CommonAPI")]
    [BepInDependency("BombRushMP.Plugin", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("WallPlant", BepInDependency.DependencyFlags.SoftDependency)]
    public class GamblingPlugin : BaseUnityPlugin
    {
        public static GamblingPlugin Instance;
        public static AssetBundle EffectsBundle;

        
        public static Sprite AppIcon;

        float effectRefreshTimer = 0f;

        void Awake()
        {
            Instance = this;

            
            GamblingSaveData.Register();
            LoadAppIcon();
            AppGambling.Initialize();
            LoadAssetBundle();
            EffectRegistry.InitializeDefaults();

            var harmony = new HarmonyLib.Harmony("com.chimp.brcgambling");
            harmony.PatchAll();

            Logger.LogInfo("[BRCGambling] Loaded.");
        }

        void Update()
        {
            effectRefreshTimer += Time.deltaTime;
            if (effectRefreshTimer >= 3f)
            {
                effectRefreshTimer = 0f;
                EffectTriggerManager.RefreshLoopingEffects();
            }
        }

        void LoadAppIcon()
        {
            string iconPath = Path.Combine(Path.GetDirectoryName(Info.Location), "icon_gambling.png");
            if (!File.Exists(iconPath)) return;

            byte[] data = File.ReadAllBytes(iconPath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);
            AppIcon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        public static void DestroyAfter(GameObject obj, float delay)
        {
            Instance.StartCoroutine(DestroyCoroutine(obj, delay));
        }

        static System.Collections.IEnumerator DestroyCoroutine(GameObject obj, float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            if (obj != null)
                Object.Destroy(obj);
        }

        void LoadAssetBundle()
        {
            string bundlePath = Path.Combine(Path.GetDirectoryName(Info.Location), "Bundles", "spraycasino");
            EffectsBundle = AssetBundle.LoadFromFile(bundlePath);

            if (EffectsBundle == null)
                Logger.LogError("[BRCGambling] Failed to load spraycasino AssetBundle!");
            else
                Logger.LogInfo("[BRCGambling] AssetBundle loaded successfully.");
        }
    }
}