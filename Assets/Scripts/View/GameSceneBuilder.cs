using CoffeeKing.Core;
using CoffeeKing.UI;
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
        public Transform LidRoot { get; set; }
        public Vector3 MachineSlotPosition { get; set; }
        public Vector3 CupAnchorPosition { get; set; }
        public Vector3 ServingAreaPosition { get; set; }
        public Vector3 GrinderPosition { get; set; }
        public Vector3 PortafilterWorkbenchPosition { get; set; }
        public Vector3 TamperPosition { get; set; }
        public Vector3 ShotGlassPosition { get; set; }
        public Vector3 SteamWandSpawnPosition { get; set; }
        public Vector3 SteamWandSnapPosition { get; set; }
        public Vector3 LidPosition { get; set; }
        public SpriteRenderer MachineRenderer { get; set; }
        public SpriteRenderer MachineSlotRenderer { get; set; }
        public SpriteRenderer CupRenderer { get; set; }
        public SpriteRenderer ShotGlassRenderer { get; set; }
        public SpriteRenderer ServingAreaRenderer { get; set; }
        public SpriteRenderer ExtractionButtonRenderer { get; set; }
        public SpriteRenderer ExtractionButtonRingRenderer { get; set; }
        public TextMesh ExtractionButtonLabel { get; set; }
        public SpriteRenderer GrinderRenderer { get; set; }
        public SpriteRenderer TamperRenderer { get; set; }
        public SpriteRenderer WaterBottleRenderer { get; set; }
        public SpriteRenderer HotWaterDispenserRenderer { get; set; }
        public SpriteRenderer PitcherRenderer { get; set; }
        public SpriteRenderer LidRenderer { get; set; }
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
        public HUDView HUDView { get; set; }

        public void SetCupVisual(string assetName, Vector2 size, Color tint)
        {
            if (CupRenderer == null)
            {
                return;
            }

            CupRenderer.sprite = SpriteFactory.Load(assetName, size, tint);
            CupRenderer.color = Color.white;
        }

        public void SetShotGlassVisual(string assetName, Vector2 size, Color tint)
        {
            if (ShotGlassRenderer == null)
            {
                return;
            }

            ShotGlassRenderer.sprite = SpriteFactory.Load(assetName, size, tint);
            ShotGlassRenderer.color = Color.white;
        }

        public void SetMachineVisual(string assetName, Vector2 size, Color tint)
        {
            if (MachineRenderer == null)
            {
                return;
            }

            MachineRenderer.sprite = SpriteFactory.Load(assetName, size, tint);
            MachineRenderer.color = Color.white;
        }

        public void SetInstruction(string message)
        {
            if (InstructionText != null)
            {
                InstructionText.text = message;
            }

            HUDView?.SetInstruction(message);
        }

        public void SetStatus(string message)
        {
            if (StatusText != null)
            {
                StatusText.text = message;
            }

            HUDView?.SetStatus(message);
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

            HUDView?.SetCurrentOrder(message);
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

            HUDView?.SetFeedback(message, color);
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

            CreateSpriteObjectExact("Wall", SpriteAssetNames.BackgroundWall, backgroundRoot, config.WallPosition, config.WallSize, config.WallColor, -30);
            CreateSpriteObjectExact("CounterEdge", SpriteAssetNames.BackgroundCounterEdge, backgroundRoot, config.CounterEdgePosition, config.CounterEdgeSize, config.CounterEdgeColor, -20);
            CreateSpriteObjectExact("Counter", SpriteAssetNames.BackgroundCounter, backgroundRoot, config.CounterPosition, config.CounterSize, config.CounterColor, -10);

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
                2);

            context.GrinderRenderer = CreateSpriteObject(
                "Grinder",
                SpriteAssetNames.Grinder,
                propsRoot,
                config.GrinderPosition,
                config.GrinderSize,
                config.IngredientTrayColor,
                3);

            context.TamperRenderer = CreateSpriteObject(
                "Tamper",
                SpriteAssetNames.Tamper,
                interactiveRoot,
                config.TamperPosition,
                config.TamperSize,
                config.MachineSlotSnapColor,
                12);
            context.TamperRenderer.gameObject.SetActive(false);

            context.WaterBottleRenderer = CreateSpriteObject(
                "WaterBottle",
                SpriteAssetNames.WaterBottle,
                interactiveRoot,
                config.WaterBottlePosition,
                config.WaterBottleSize,
                config.CupLatteColor,
                9);
            context.WaterBottleRenderer.gameObject.SetActive(false);

            context.HotWaterDispenserRenderer = CreateSpriteObject(
                "HotWaterDispenser",
                SpriteAssetNames.HotWaterDispenser,
                propsRoot,
                config.HotWaterDispenserPosition,
                config.HotWaterDispenserSize,
                config.CupLatteColor,
                1);

            context.CupRoot = new GameObject("Cup").transform;
            context.CupRoot.SetParent(interactiveRoot, false);
            context.CupRoot.position = config.CupPosition;
            context.CupRenderer = context.CupRoot.gameObject.AddComponent<SpriteRenderer>();
            context.SetCupVisual(SpriteAssetNames.CupPlasticEmpty, config.CupSize, config.CupEmptyColor);
            context.CupRenderer.sortingOrder = 6;
            context.CupRoot.gameObject.SetActive(false);
            context.CupAnchorPosition = config.CupPosition;

            context.ShotGlassRoot = new GameObject("ShotGlass").transform;
            context.ShotGlassRoot.SetParent(interactiveRoot, false);
            context.ShotGlassRoot.position = config.ShotGlassPosition;
            context.ShotGlassRenderer = context.ShotGlassRoot.gameObject.AddComponent<SpriteRenderer>();
            context.SetShotGlassVisual(SpriteAssetNames.ShotGlassEmpty, config.ShotGlassSize, config.CupEspressoColor);
            context.ShotGlassRenderer.sortingOrder = 14;
            context.ShotGlassRoot.gameObject.SetActive(false);
            context.ShotGlassPosition = config.ShotGlassPosition;

            context.LidRoot = new GameObject("Lid").transform;
            context.LidRoot.SetParent(interactiveRoot, false);
            context.LidRoot.position = config.LidPosition;
            context.LidRenderer = context.LidRoot.gameObject.AddComponent<SpriteRenderer>();
            context.LidRenderer.sprite = SpriteFactory.Load(SpriteAssetNames.DomeLid, config.IcedLidSize, config.CupEmptyColor);
            context.LidRenderer.color = Color.white;
            context.LidRenderer.sortingOrder = 16;
            context.LidRoot.gameObject.SetActive(false);
            context.LidPosition = config.LidPosition;

            context.PitcherRenderer = CreateSpriteObject(
                "Pitcher",
                SpriteAssetNames.Pitcher,
                interactiveRoot,
                config.PitcherPosition,
                config.PitcherSize,
                config.PitcherColor,
                12);
            context.PitcherRenderer.gameObject.SetActive(false);

            context.SyrupBottleRenderer = CreateSpriteObject(
                "SyrupBottle",
                SpriteAssetNames.TraySyrup,
                interactiveRoot,
                config.SyrupBottlePosition,
                config.SyrupBottleSize,
                config.SyrupBottleColor,
                12);
            context.SyrupBottleRenderer.gameObject.SetActive(false);

            context.SteamWandRoot = new GameObject("SteamWand").transform;
            context.SteamWandRoot.SetParent(interactiveRoot, false);
            context.SteamWandRoot.position = config.SteamWandSpawnPosition;
            context.SteamWandRenderer = context.SteamWandRoot.gameObject.AddComponent<SpriteRenderer>();
            context.SteamWandRenderer.sprite = SpriteFactory.Load(SpriteAssetNames.SteamWand, config.SteamWandSize, config.SteamWandColor);
            context.SteamWandRenderer.color = Color.white;
            context.SteamWandRenderer.sortingOrder = 15;
            context.SteamWandRenderer.gameObject.SetActive(false);
            context.SteamWandSpawnPosition = config.SteamWandSpawnPosition;
            context.SteamWandSnapPosition = config.SteamWandSnapPosition;

            context.ServingAreaRenderer = CreateSpriteObject(
                "ServingArea",
                SpriteAssetNames.CupTray,
                propsRoot,
                config.ServingAreaPosition,
                config.ServingAreaSize,
                config.ServingAreaIdleColor,
                1);
            context.ServingAreaPosition = config.ServingAreaPosition;

            var extractionButtonRoot = new GameObject("ExtractionButtonGroup");
            extractionButtonRoot.transform.SetParent(interactiveRoot, false);
            extractionButtonRoot.transform.position = config.ExtractionButtonPosition;

            var ringSize = config.ExtractionButtonSize + new Vector2(0.18f, 0.18f);
            var ringGo = new GameObject("ExtractionRing");
            ringGo.transform.SetParent(extractionButtonRoot.transform, false);
            ringGo.transform.localPosition = Vector3.zero;
            var ringRenderer = ringGo.AddComponent<SpriteRenderer>();
            ringRenderer.sprite = SpriteFactory.CreateEllipse("ExtractionRing", ringSize, ColorPalette.ExtractionButtonRing);
            ringRenderer.color = ColorPalette.ExtractionButtonRing;
            ringRenderer.sortingOrder = 12;
            context.ExtractionButtonRingRenderer = ringRenderer;

            var btnGo = new GameObject("ExtractionButton");
            btnGo.transform.SetParent(extractionButtonRoot.transform, false);
            btnGo.transform.localPosition = Vector3.zero;
            var btnRenderer = btnGo.AddComponent<SpriteRenderer>();
            btnRenderer.sprite = SpriteFactory.CreateEllipse("ExtractionButton", config.ExtractionButtonSize, config.ExtractionButtonIdleColor);
            btnRenderer.color = config.ExtractionButtonIdleColor;
            btnRenderer.sortingOrder = 13;
            context.ExtractionButtonRenderer = btnRenderer;

            var labelGo = new GameObject("ExtractionLabel");
            labelGo.transform.SetParent(extractionButtonRoot.transform, false);
            labelGo.transform.localPosition = Vector3.zero;
            var label = labelGo.AddComponent<TextMesh>();
            label.text = "BREW";
            label.fontSize = 64;
            label.characterSize = 0.07f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.white;
            label.fontStyle = FontStyle.Bold;
            labelGo.GetComponent<MeshRenderer>().sortingOrder = 14;
            context.ExtractionButtonLabel = label;

            extractionButtonRoot.SetActive(false);

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

            SetTextVisible(context.InstructionText, false);
            SetTextVisible(context.StatusText, false);
            SetTextVisible(context.RoundText, false);
            SetTextVisible(context.ScoreText, false);
            SetTextVisible(context.QueueText, false);
            SetTextVisible(context.ActiveOrderText, false);
            SetTextVisible(context.FeedbackText, false);

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
            renderer.color = assetName == null ? color : Color.white;
            renderer.sortingOrder = sortingOrder;

            return renderer;
        }

        private static SpriteRenderer CreateSpriteObjectExact(
            string name,
            string assetName,
            Transform parent,
            Vector3 position,
            Vector2 size,
            Color color,
            int sortingOrder)
        {
            var renderer = CreateSpriteObject(name, assetName, parent, position, size, color, sortingOrder);
            if (renderer.sprite != null)
            {
                var bounds = renderer.sprite.bounds.size;
                if (bounds.x > 0.0001f && bounds.y > 0.0001f)
                {
                    renderer.transform.localScale = new Vector3(size.x / bounds.x, size.y / bounds.y, 1f);
                }
            }

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

        private static void SetTextVisible(TextMesh textMesh, bool isVisible)
        {
            if (textMesh == null)
            {
                return;
            }

            var meshRenderer = textMesh.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = isVisible;
            }
        }

    }
}
