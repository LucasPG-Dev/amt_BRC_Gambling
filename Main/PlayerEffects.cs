using BombRushMP.Plugin;
using UnityEngine;
using Reptile;

namespace BRCGambling
{
    public static class PlayerEffects
    {
        public static void SpawnLegendaryEffect(float duration)
        {
            UnityEngine.Debug.Log("[BRCGambling] SpawnLegendaryEffect called");

            Transform playerTransform = GetLocalPlayerTransform();
            if (playerTransform == null)
            {
                UnityEngine.Debug.Log("[BRCGambling] Player transform is null, aborting effect");
                return;
            }

            UnityEngine.Debug.Log($"[BRCGambling] Got player transform at {playerTransform.position}");

            try
            {
                GameObject effectObj = new GameObject("LegendaryFireEffect");
                UnityEngine.Debug.Log("[BRCGambling] Created GameObject");

                effectObj.transform.SetParent(playerTransform);
                effectObj.transform.localPosition = new Vector3(0, 0.1f, 0); // just above feet
                UnityEngine.Debug.Log("[BRCGambling] Set parent and position");

                ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();

                var main = ps.main;
                main.duration = duration;
                main.loop = true;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f); // small like halo
                main.startColor = new ParticleSystem.MinMaxGradient(
                    new Color(1f, 0.3f, 0f),
                    new Color(1f, 0.8f, 0f)
                );
                main.gravityModifier = -0.3f; // float upward slowly
                main.simulationSpace = ParticleSystemSimulationSpace.World;

                var emission = ps.emission;
                emission.rateOverTime = 25f;

                // Flat circle at feet, not a cone
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.4f;
                shape.rotation = new Vector3(90f, 0f, 0f); // flat like the halo

                // Fade out over lifetime
                var colorOverLifetime = ps.colorOverLifetime;
                colorOverLifetime.enabled = true;
                Gradient grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0.8f, 0f), 1f)
                    },
                    new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
                    }
                );
                colorOverLifetime.color = grad;

                // Try to assign a basic particle material
                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

                UnityEngine.Debug.Log("[BRCGambling] Added ParticleSystem component");
                ps.Play();
                UnityEngine.Debug.Log("[BRCGambling] Particle system playing");

                GamblingPlugin.DestroyAfter(effectObj, duration);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log($"[BRCGambling] Effect failed: {e.Message}");
            }
        }

        public static void SpawnEpicEffect(float duration)
        {
            Transform playerTransform = GetLocalPlayerTransform();
            if (playerTransform == null) return;

            GameObject effectObj = new GameObject("EpicHaloEffect");
            effectObj.transform.SetParent(playerTransform);
            effectObj.transform.localPosition = new Vector3(0, 2.2f, 0); // above head

            ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = duration;
            main.loop = true;
            main.startLifetime = 1f;
            main.startSpeed = 0f;
            main.startSize = 0.1f;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.5f, 0.2f, 1f, 1f),   // purple
                new Color(0.8f, 0.4f, 1f, 0.5f)  // light purple
            );
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 40f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
            shape.rotation = new Vector3(90f, 0f, 0f); // flat ring

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.7f, 0.3f, 1f), 0f),
                    new GradientColorKey(new Color(0.5f, 0.2f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = grad;

            ps.Play();
            GamblingPlugin.DestroyAfter(effectObj, duration);
        }

        public static void SpawnEffectByName(string prefabName, Transform parent, Vector3 localOffset, float duration)
        {
            if (GamblingPlugin.EffectsBundle == null)
            {
                UnityEngine.Debug.Log("[BRCGambling] EffectsBundle is null, cannot spawn effect");
                return;
            }

            GameObject prefab = GamblingPlugin.EffectsBundle.LoadAsset<GameObject>(prefabName);
            if (prefab == null)
            {
                UnityEngine.Debug.Log($"[BRCGambling] Prefab '{prefabName}' not found in bundle");
                return;
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            instance.transform.localPosition = localOffset;

            GamblingPlugin.DestroyAfter(instance, duration);
        }

        public static Transform GetLocalPlayerTransform()
        {
            try
            {
                var player = Reptile.WorldHandler.instance?.GetCurrentPlayer();
                if (player != null)
                {
                    UnityEngine.Debug.Log($"[BRCGambling] Found player via WorldHandler at {player.transform.position}");
                    return player.transform;
                }
                UnityEngine.Debug.Log("[BRCGambling] WorldHandler returned null player");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log($"[BRCGambling] WorldHandler error: {e.Message}");
            }
            return null;
        }

        public static GameObject SpawnPersistentEffectByName(string prefabName, Transform parent, Vector3 localOffset)
        {
            if (GamblingPlugin.EffectsBundle == null) return null;

            GameObject prefab = GamblingPlugin.EffectsBundle.LoadAsset<GameObject>(prefabName);
            if (prefab == null)
            {
                UnityEngine.Debug.Log($"[BRCGambling] Prefab '{prefabName}' not found in bundle");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            instance.transform.localPosition = localOffset;
            return instance;
        }
    }
}