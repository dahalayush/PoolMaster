// ============================================================================
// PoolMaster - Object Pooling System for Unity
// Copyright (c) 2026 Max Thomas Coates
// https://github.com/mistyuk/PoolMaster
// Licensed under MIT License (see LICENSE file for details)
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PoolMaster.Examples
{
    /// <summary>
    /// Professional demo navigator showing one demo at a time with clean UI
    /// Displays title, description, features, and navigation controls
    /// </summary>
    public class DemoNavigator : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField]
        private List<DemoMetadata> demos = new List<DemoMetadata>();

        [Header("UI References")]
        private Canvas canvas;
        private Text titleText;
        private Text descriptionText;
        private Text featuresText;
        private Text pageIndicatorText;
        private Button prevButton;
        private Button nextButton;
        private Image accentBar;

        private int currentDemoIndex = 0;

        private void Start()
        {
            SetupDemos();
            CreateUI();
            ShowDemo(0);
        }

        private void SetupDemos()
        {
            // Find demo components
            var basicDemo = FindObjectsByType<BasicPoolingDemo>(FindObjectsSortMode.None)[0];
            var projectileDemo = FindObjectsByType<ProjectileDemo>(FindObjectsSortMode.None)[0];
            var vfxDemo = FindObjectsByType<VFXDemo>(FindObjectsSortMode.None)[0];
            var batchDemo = FindObjectsByType<BatchSpawningDemo>(FindObjectsSortMode.None)[0];
            var particleDemo = FindObjectsByType<ParticleBurstDemo>(FindObjectsSortMode.None)[0];
            var commandDemo = FindObjectsByType<CommandBufferDemo>(FindObjectsSortMode.None)[0];
            var collectionDemo = FindObjectsByType<CollectionPoolingDemo>(FindObjectsSortMode.None)[
                0
            ];

            // Configure practical, useful demos
            demos = new List<DemoMetadata>
            {
                new DemoMetadata
                {
                    title = "Projectile Pooling",
                    description =
                        "The most common pooling use-case in games: bullets, arrows, and projectiles.",
                    features =
                        "• High-frequency spawning (multiple projectiles/second)\n• Automatic despawn after lifetime\n• Zero garbage collection overhead\n• Perfect for: Shooting games, tower defense, action games",
                    demoComponent = projectileDemo,
                    accentColor = new Color(1f, 0.3f, 0.2f), // Red
                },
                new DemoMetadata
                {
                    title = "VFX & Particle Pooling",
                    description =
                        "Reuse visual effects instead of destroying them. Essential for polished games.",
                    features =
                        "• Event-driven spawning (explosions, impacts, collectibles)\n• Scale, fade, and animate effects\n• Reduce instantiation lag spikes\n• Perfect for: Particle systems, UI effects, environment details",
                    demoComponent = vfxDemo,
                    accentColor = new Color(1f, 0.9f, 0.2f), // Yellow
                },
                new DemoMetadata
                {
                    title = "Enemy & NPC Pooling",
                    description =
                        "Spawn waves of enemies efficiently. Used in every action/arcade game.",
                    features =
                        "• Batch spawning for waves\n• Circular/grid spawn patterns\n• Dynamic pool expansion\n• Perfect for: Wave spawners, enemy AI, traffic systems",
                    demoComponent = batchDemo,
                    accentColor = new Color(0.3f, 1f, 0.3f), // Green
                },
                new DemoMetadata
                {
                    title = "Particle Burst Effects",
                    description =
                        "Spawn dozens/hundreds of particles instantly without frame drops.",
                    features =
                        "• Burst spawning (50+ objects at once)\n• Physics-based motion\n• Dynamic pool sizing\n• Perfect for: Explosions, confetti, debris, shatter effects",
                    demoComponent = particleDemo,
                    accentColor = new Color(0.9f, 0.9f, 0.9f), // White
                },
                new DemoMetadata
                {
                    title = "Basic Object Pooling",
                    description =
                        "The foundation: simple spawn and despawn cycles for any GameObject.",
                    features =
                        "• Simple API: Get() and Return()\n• Automatic initialization\n• Configurable pool sizes\n• Perfect for: Pickups, platforms, UI elements, generic objects",
                    demoComponent = basicDemo,
                    accentColor = new Color(0.2f, 0.8f, 1f), // Cyan
                },
                new DemoMetadata
                {
                    title = "Thread-Safe Command Buffer",
                    description =
                        "Queue spawn commands from background threads (advanced performance).",
                    features =
                        "• Thread-safe pooling operations\n• Deferred execution on main thread\n• No blocking, no locks\n• Perfect for: Procedural generation, pathfinding results, async spawning",
                    demoComponent = commandDemo,
                    accentColor = new Color(1f, 0.8f, 0.2f), // Orange
                },
                new DemoMetadata
                {
                    title = "Collection Pooling (Zero Alloc)",
                    description =
                        "Reuse List/Dictionary/HashSet to eliminate ALL garbage collection.",
                    features =
                        "• Pool C# collections, not just GameObjects\n• Zero allocations in hot paths\n• 300+ operations/second with no GC\n• Perfect for: Data processing, pathfinding, queries, temporary buffers",
                    demoComponent = collectionDemo,
                    accentColor = new Color(0.6f, 0.4f, 1f), // Purple
                },
            };
        }

        private void CreateUI()
        {
            // Create canvas with proper scaling
            GameObject canvasObj = new GameObject("Demo Navigator UI");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Info panel (bottom - compact)
            GameObject panelObj = new GameObject("Info Panel");
            panelObj.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.anchoredPosition = new Vector2(0, 20);
            panelRect.sizeDelta = new Vector2(-40, 180);

            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);

            // Accent bar (top of panel)
            GameObject accentObj = new GameObject("Accent Bar");
            accentObj.transform.SetParent(panelObj.transform, false);

            RectTransform accentRect = accentObj.AddComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0, 1);
            accentRect.anchorMax = new Vector2(1, 1);
            accentRect.pivot = new Vector2(0.5f, 1);
            accentRect.offsetMin = new Vector2(0, -6);
            accentRect.offsetMax = new Vector2(0, 0);

            accentBar = accentObj.AddComponent<Image>();
            accentBar.color = Color.cyan;

            // Title (left side, top)
            titleText = CreateText(
                panelObj.transform,
                "Title",
                new Vector2(0, 1),
                new Vector2(0.4f, 1),
                new Vector2(0, 1),
                new Vector2(20, -15),
                new Vector2(-10, -15),
                28,
                FontStyle.Bold,
                TextAnchor.UpperLeft
            );

            // Description (left side, middle)
            descriptionText = CreateText(
                panelObj.transform,
                "Description",
                new Vector2(0, 1),
                new Vector2(0.4f, 1),
                new Vector2(0, 1),
                new Vector2(20, -55),
                new Vector2(-10, -55),
                14,
                FontStyle.Normal,
                TextAnchor.UpperLeft
            );

            // Features (right side, all)
            featuresText = CreateText(
                panelObj.transform,
                "Features",
                new Vector2(0.4f, 1),
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(10, -15),
                new Vector2(-150, -15),
                13,
                FontStyle.Normal,
                TextAnchor.UpperLeft
            );

            // Navigation buttons (bottom right)
            prevButton = CreateNavButton(
                panelObj.transform,
                "◄ PREVIOUS",
                new Vector2(1, 0),
                new Vector2(1, 0),
                new Vector2(-140, 10),
                new Vector2(130, 45),
                () => Navigate(-1)
            );

            nextButton = CreateNavButton(
                panelObj.transform,
                "NEXT ►",
                new Vector2(1, 0),
                new Vector2(1, 0),
                new Vector2(-10, 10),
                new Vector2(130, 45),
                () => Navigate(1)
            );

            // Page indicator (bottom right, above buttons)
            pageIndicatorText = CreateText(
                panelObj.transform,
                "1 / 7",
                new Vector2(1, 0),
                new Vector2(1, 0),
                new Vector2(1, 0),
                new Vector2(-140, 65),
                new Vector2(200, 30),
                16,
                FontStyle.Bold,
                TextAnchor.MiddleRight
            );
        }

        private Text CreateText(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 offsetMin,
            Vector2 offsetMax,
            int fontSize,
            FontStyle style,
            TextAnchor alignment
        )
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Text text = obj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = alignment;

            return text;
        }

        private Button CreateNavButton(
            Transform parent,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 position,
            Vector2 size,
            System.Action onClick
        )
        {
            GameObject buttonObj = new GameObject(label);
            buttonObj.transform.SetParent(parent, false);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.pivot = anchorMin;
            buttonRect.anchoredPosition = position;
            buttonRect.sizeDelta = size;

            Image buttonBg = buttonObj.AddComponent<Image>();
            buttonBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            Button button = buttonObj.AddComponent<Button>();

            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            button.colors = colors;

            button.onClick.AddListener(() => onClick());

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = label;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 14;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;

            return button;
        }

        private void Navigate(int direction)
        {
            int newIndex = currentDemoIndex + direction;

            if (newIndex < 0)
                newIndex = demos.Count - 1;
            if (newIndex >= demos.Count)
                newIndex = 0;

            ShowDemo(newIndex);
        }

        private void ShowDemo(int index)
        {
            // Disable all demos
            foreach (var demo in demos)
            {
                if (demo.demoComponent != null)
                    demo.demoComponent.enabled = false;
            }

            // Enable current demo
            currentDemoIndex = index;
            var currentDemo = demos[currentDemoIndex];

            if (currentDemo.demoComponent != null)
                currentDemo.demoComponent.enabled = true;

            // Update UI
            titleText.text = currentDemo.title;
            descriptionText.text = currentDemo.description;
            featuresText.text = currentDemo.features;
            accentBar.color = currentDemo.accentColor;
            pageIndicatorText.text = $"{currentDemoIndex + 1} / {demos.Count}";

            Debug.Log($"PoolMaster Demo: Now showing '{currentDemo.title}'");
        }
    }
}
