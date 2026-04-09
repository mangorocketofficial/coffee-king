using CoffeeKing.Core;
using CoffeeKing.Util;
using UnityEngine;

namespace CoffeeKing.View
{
    public sealed class GrayboxSceneContext
    {
        public Transform Root { get; set; }
        public Transform InteractiveRoot { get; set; }
        public Transform CustomerLayer { get; set; }
        public Transform OverlayLayer { get; set; }
        public Transform CupRoot { get; set; }
        public Transform ShotGlassRoot { get; set; }
        public Transform SteamWandRoot { get; set; }
        public Vector3 MachineSlotPosition { get; set; }
        public Vector3 CupAnchorPosition { get; set; }
        public Vector3 ServingAreaPosition { get; set; }
        public Vector3 GrinderPosition { get; set; }
        public Vector3 PortafilterWorkbenchPosition { get; set; }
        public Vector3 TamperPosition { get; set; }
        public Vector3 ShotGlassPosition { get; set; }
        public Vector3 SteamWandSpawnPosition { get; set; }
        public Vector3 SteamWandSnapPosition { get; set; }
        public SpriteRenderer MachineRenderer { get; set; }
        public SpriteRenderer MachineSlotRenderer { get; set; }
        public SpriteRenderer CupRenderer { get; set; }
        public SpriteRenderer ShotGlassRenderer { get; set; }
        public SpriteRenderer ServingAreaRenderer { get; set; }
        public SpriteRenderer ExtractionButtonRenderer { get; set; }
        public SpriteRenderer GrinderRenderer { get; set; }
        public SpriteRenderer TamperRenderer { get; set; }
        public SpriteRenderer WaterBottleRenderer { get; set; }
        public SpriteRenderer PitcherRenderer { get; set; }
        public SpriteRenderer SyrupBottleRenderer { get; set; }
        public SpriteRenderer SteamWandRenderer { get; set; }
        public TextMesh InstructionText { get; set; }
        public TextMesh StatusText { get; set; }
        public TextMesh QueueText { get; set; }
        public TextMesh ActiveOrderText { get; set; }
        public TextMesh ScoreText { get; set; }
        public TextMesh RoundText { get; set; }
        public TextMesh FeedbackText { get; set; }
        public GaugeView GaugeView { get; set; }

        public void SetCupVisual(string assetName, Vector2 size, Color tint)
        {
            if (CupRenderer == null)
            {
                return;
            }

            CupRenderer.sprite = SpriteFactory.Load(assetName, size, tint);
            CupRenderer.color = tint;
        }

        public void SetShotGlassVisual(string assetName, Vector2 size, Color tint)
        {
            if (ShotGlassRenderer == null)
            {
                return;
            }

            ShotGlassRenderer.sprite = SpriteFactory.Load(assetName, size, tint);
            ShotGlassRenderer.color = tint;
        }

        public void SetMachineVisual(string assetName, Vector2 size, Color tint)
        {
            if (MachineRenderer == null)
            {
                return;
            }

            MachineRenderer.sprite = SpriteFactory.Load(assetName, size, tint);
            MachineRenderer.color = tint;
        }

        public void SetInstruction(string message)
        {
            if (InstructionText != null)
            {
                InstructionText.text = message;
            }
        }

        public void SetStatus(string message)
        {
            if (StatusText != null)
            {
                StatusText.text = message;
            }
        }

        public void SetQueueSummary(string message)
        {
            if (QueueText != null)
            {
                QueueText.text = message;
            }
        }

        public void SetActiveOrder(string message)
        {
            if (ActiveOrderText != null)
            {
                ActiveOrderText.text = message;
            }
        }

        public void SetScore(string message)
        {
            if (ScoreText != null)
            {
                ScoreText.text = message;
            }
        }

        public void SetRound(string message)
        {
            if (RoundText != null)
            {
                RoundText.text = message;
            }
        }

        public void SetFeedback(string message, Color color)
        {
            if (FeedbackText != null)
            {
                FeedbackText.text = message;
                FeedbackText.color = color;
            }
        }
    }

    public sealed class GameSceneBuilder
    {
        public GrayboxSceneContext Build(GameConfig config)
        {
            var existingRoot = GameObject.Find("[GrayboxScene]");
            if (existingRoot != null)
            {
                Object.Destroy(existingRoot);
            }

            var context = new GrayboxSceneContext();
            var root = new GameObject("[GrayboxScene]");
            context.Root = root.transform;

            var backgroundRoot = CreateChild(root.transform, "Background");
            var propsRoot = CreateChild(root.transform, "Props");
            var interactiveRoot = CreateChild(root.transform, "Interactive");
            var customerLayer = CreateChild(root.transform, "Customers");
            var overlayLayer = CreateChild(root.transform, "Overlay");
            var textRoot = CreateChild(root.transform, "Text");

            context.InteractiveRoot = interactiveRoot;
            context.CustomerLayer = customerLayer;
            context.OverlayLayer = overlayLayer;

            CreateSpriteObject("Wall", SpriteAssetNames.BackgroundWall, backgroundRoot, config.WallPosition, config.WallSize, config.WallColor, -10);
            CreateSpriteObject("Counter", SpriteAssetNames.BackgroundCounter, backgroundRoot, config.CounterPosition, config.CounterSize, config.CounterColor, -9);
            CreateSpriteObject("CounterEdge", null, backgroundRoot, new Vector3(0f, -1.2f, 0f), new Vector2(18f, 0.45f), config.CounterEdgeColor, -8);

            context.MachineRenderer = CreateSpriteObject(
                "Machine",
                SpriteAssetNames.MachineEmpty,
                propsRoot,
                config.MachinePosition,
                config.MachineSize,
                config.MachineColor,
                0);

            context.MachineSlotRenderer = CreateSpriteObject(
                "MachineSlot",
                SpriteAssetNames.MachineSlot,
                propsRoot,
                config.MachineSlotPosition,
                config.MachineSlotSize,
                config.MachineSlotIdleColor,
                1);

            context.GrinderRenderer = CreateSpriteObject(
                "Grinder",
                SpriteAssetNames.Grinder,
                propsRoot,
                config.GrinderPosition,
                config.GrinderSize,
                config.IngredientTrayColor,
                1);
            CreateSpriteLabel("GrinderLabel", context.GrinderRenderer.transform, "G", ColorPalette.InstructionText, 0.42f, 3);

            context.TamperRenderer = CreateSpriteObject(
                "Tamper",
                SpriteAssetNames.Tamper,
                interactiveRoot,
                config.TamperPosition,
                config.TamperSize,
                config.MachineSlotSnapColor,
                12);
            CreateSpriteLabel("TamperLabel", context.TamperRenderer.transform, "T", ColorPalette.InstructionText, 0.34f, 14);
            context.TamperRenderer.gameObject.SetActive(false);

            context.WaterBottleRenderer = CreateSpriteObject(
                "WaterBottle",
                SpriteAssetNames.WaterBottle,
                interactiveRoot,
                config.WaterBottlePosition,
                config.WaterBottleSize,
                config.CupLatteColor,
                9);
            CreateSpriteLabel("WaterBottleLabel", context.WaterBottleRenderer.transform, "W", ColorPalette.InstructionText, 0.34f, 12);
            context.WaterBottleRenderer.gameObject.SetActive(false);

            context.CupRoot = new GameObject("Cup").transform;
            context.CupRoot.SetParent(interactiveRoot, false);
            context.CupRoot.position = config.CupPosition;
            context.CupRenderer = context.CupRoot.gameObject.AddComponent<SpriteRenderer>();
            context.SetCupVisual(SpriteAssetNames.CupPlasticEmpty, config.CupSize, config.CupEmptyColor);
            context.CupRenderer.sortingOrder = 10;
            CreateSpriteLabel("CupLabel", context.CupRenderer.transform, "C", ColorPalette.InstructionText, 0.34f, 13);
            context.CupRoot.gameObject.SetActive(false);
            context.CupAnchorPosition = config.CupPosition;

            context.ShotGlassRoot = new GameObject("ShotGlass").transform;
            context.ShotGlassRoot.SetParent(interactiveRoot, false);
            context.ShotGlassRoot.position = config.ShotGlassPosition;
            context.ShotGlassRenderer = context.ShotGlassRoot.gameObject.AddComponent<SpriteRenderer>();
            context.SetShotGlassVisual(SpriteAssetNames.ShotGlassEmpty, config.ShotGlassSize, config.CupEspressoColor);
            context.ShotGlassRenderer.sortingOrder = 11;
            context.ShotGlassRoot.gameObject.SetActive(false);
            context.ShotGlassPosition = config.ShotGlassPosition;

            context.PitcherRenderer = CreateSpriteObject(
                "Pitcher",
                SpriteAssetNames.Pitcher,
                interactiveRoot,
                config.PitcherPosition,
                config.PitcherSize,
                config.PitcherColor,
                8);
            context.PitcherRenderer.gameObject.SetActive(false);

            context.SyrupBottleRenderer = CreateSpriteObject(
                "SyrupBottle",
                SpriteAssetNames.TraySyrup,
                interactiveRoot,
                config.SyrupBottlePosition,
                config.SyrupBottleSize,
                config.SyrupBottleColor,
                8);
            context.SyrupBottleRenderer.gameObject.SetActive(false);

            context.SteamWandRoot = new GameObject("SteamWand").transform;
            context.SteamWandRoot.SetParent(interactiveRoot, false);
            context.SteamWandRoot.position = config.SteamWandSpawnPosition;
            context.SteamWandRenderer = context.SteamWandRoot.gameObject.AddComponent<SpriteRenderer>();
            context.SteamWandRenderer.sprite = SpriteFactory.Load(SpriteAssetNames.SteamWand, config.SteamWandSize, config.SteamWandColor);
            context.SteamWandRenderer.color = config.SteamWandColor;
            context.SteamWandRenderer.sortingOrder = 9;
            context.SteamWandRenderer.gameObject.SetActive(false);
            context.SteamWandSpawnPosition = config.SteamWandSpawnPosition;
            context.SteamWandSnapPosition = config.SteamWandSnapPosition;

            context.ServingAreaRenderer = CreateSpriteObject(
                "ServingArea",
                null,
                propsRoot,
                config.ServingAreaPosition,
                config.ServingAreaSize,
                config.ServingAreaIdleColor,
                -1);
            context.ServingAreaPosition = config.ServingAreaPosition;

            context.ExtractionButtonRenderer = CreateSpriteObject(
                "ExtractionButton",
                null,
                interactiveRoot,
                config.ExtractionButtonPosition,
                config.ExtractionButtonSize,
                config.ExtractionButtonIdleColor,
                9);
            context.ExtractionButtonRenderer.gameObject.SetActive(false);

            context.GaugeView = GaugeView.Create(propsRoot, config.GaugePosition, config.GaugeSize, config.GaugeFrameColor, config.GaugeBackgroundColor);

            context.InstructionText = CreateTextObject(
                "Instruction",
                textRoot,
                new Vector3(0f, 4.25f, 0f),
                "V2 Reboot",
                config.InstructionTextColor,
                0.10f,
                TextAnchor.MiddleCenter,
                TextAlignment.Center,
                50);

            context.StatusText = CreateTextObject(
                "Status",
                textRoot,
                new Vector3(0f, 3.55f, 0f),
                "Preparing stage.",
                config.StatusTextColor,
                0.085f,
                TextAnchor.MiddleCenter,
                TextAlignment.Center,
                50);

            context.RoundText = CreateTextObject(
                "Round",
                textRoot,
                new Vector3(-6.5f, 4.15f, 0f),
                string.Empty,
                config.SecondaryTextColor,
                0.075f,
                TextAnchor.MiddleLeft,
                TextAlignment.Left,
                50);

            context.ScoreText = CreateTextObject(
                "Score",
                textRoot,
                new Vector3(-6.5f, 3.25f, 0f),
                string.Empty,
                config.SecondaryTextColor,
                0.065f,
                TextAnchor.UpperLeft,
                TextAlignment.Left,
                50);

            context.QueueText = CreateTextObject(
                "Queue",
                textRoot,
                new Vector3(-6.5f, 1.95f, 0f),
                string.Empty,
                config.SecondaryTextColor,
                0.07f,
                TextAnchor.UpperLeft,
                TextAlignment.Left,
                50);

            context.ActiveOrderText = CreateTextObject(
                "ActiveOrder",
                textRoot,
                new Vector3(6.15f, -0.2f, 0f),
                string.Empty,
                config.SecondaryTextColor,
                0.075f,
                TextAnchor.UpperCenter,
                TextAlignment.Center,
                50);

            context.FeedbackText = CreateTextObject(
                "Feedback",
                textRoot,
                new Vector3(0f, 1.95f, 0f),
                string.Empty,
                config.SecondaryTextColor,
                0.10f,
                TextAnchor.MiddleCenter,
                TextAlignment.Center,
                50);

            context.MachineSlotPosition = config.MachineSlotPosition;
            context.GrinderPosition = config.GrinderPosition;
            context.PortafilterWorkbenchPosition = config.PortafilterWorkbenchPosition;
            context.TamperPosition = config.TamperPosition;
            return context;
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static SpriteRenderer CreateSpriteObject(
            string name,
            string assetName,
            Transform parent,
            Vector3 position,
            Vector2 size,
            Color color,
            int sortingOrder)
        {
            var spriteObject = new GameObject(name);
            spriteObject.transform.SetParent(parent, false);
            spriteObject.transform.position = position;

            var renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = assetName == null ? SpriteFactory.CreateRect(name, size, color) : SpriteFactory.Load(assetName, size, color);
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            return renderer;
        }

        private static TextMesh CreateTextObject(
            string name,
            Transform parent,
            Vector3 position,
            string content,
            Color color,
            float characterSize,
            TextAnchor anchor,
            TextAlignment alignment,
            int sortingOrder)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            textObject.transform.position = position;

            var textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = content;
            textMesh.color = color;
            textMesh.fontSize = 64;
            textMesh.characterSize = characterSize;
            textMesh.anchor = anchor;
            textMesh.alignment = alignment;

            var meshRenderer = textObject.GetComponent<MeshRenderer>();
            meshRenderer.sortingOrder = sortingOrder;

            return textMesh;
        }

        private static TextMesh CreateSpriteLabel(
            string name,
            Transform parent,
            string content,
            Color color,
            float characterSize,
            int sortingOrder)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = Vector3.zero;

            var textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = content;
            textMesh.color = color;
            textMesh.fontSize = 64;
            textMesh.characterSize = characterSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;

            var meshRenderer = textObject.GetComponent<MeshRenderer>();
            meshRenderer.sortingOrder = sortingOrder;

            return textMesh;
        }
    }
}
