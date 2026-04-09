using CoffeeKing.Util;
using UnityEngine;

namespace CoffeeKing.Core
{
    public sealed class GameConfig : ScriptableObject
    {
        public float CameraOrthographicSize;
        public Vector3 CameraPosition;

        public Vector2 WallSize;
        public Vector2 CounterSize;
        public Vector2 MachineSize;
        public Vector2 MachineSlotSize;
        public Vector2 IngredientTraySize;
        public Vector2 PortafilterBodySize;
        public Vector2 CupSize;
        public Vector2 ServingAreaSize;
        public Vector2 ExtractionButtonSize;
        public Vector2 GaugeSize;
        public Vector2 SteamWandSize;
        public Vector2 PitcherSize;
        public Vector2 SyrupBottleSize;
        public Vector2 CustomerSize;
        public Vector2 SpeechBubbleSize;
        public Vector2 GrinderSize;
        public Vector2 TamperSize;
        public Vector2 ShotGlassSize;
        public Vector2 WaterBottleSize;

        public Vector3 WallPosition;
        public Vector3 CounterPosition;
        public Vector3 MachinePosition;
        public Vector3 MachineSlotPosition;
        public Vector3 IngredientTrayPosition;
        public Vector3 PortafilterSpawnPosition;
        public Vector3 CupPosition;
        public Vector3 ServingAreaPosition;
        public Vector3 ExtractionButtonPosition;
        public Vector3 GaugePosition;
        public Vector3 SteamWandSpawnPosition;
        public Vector3 SteamWandSnapPosition;
        public Vector3 PitcherPosition;
        public Vector3 SyrupBottlePosition;
        public Vector3 CustomerPosition;
        public Vector3 SpeechBubblePosition;
        public Vector3 GrinderPosition;
        public Vector3 PortafilterWorkbenchPosition;
        public Vector3 TamperPosition;
        public Vector3 ShotGlassPosition;
        public Vector3 WaterBottlePosition;

        public float PortafilterSnapDistance;
        public float PortafilterLockAngle;
        public float PortafilterRotateRadius;
        public float ServingSnapDistance;
        public float SteamWandSnapDistance;
        public float SteamWandMinY;
        public float SteamWandMaxY;

        public float ExtractionGaugeMin;
        public float ExtractionGaugeMax;
        public float ExtractionGaugeSpeed;
        public float ExtractionPerfectMin;
        public float ExtractionPerfectMax;
        public float ExtractionGoodMin;
        public float ExtractionGoodMax;

        public float SteamStartTemperature;
        public float SteamBurnTemperature;
        public float SteamPerfectMin;
        public float SteamPerfectMax;
        public float SteamGoodMin;
        public float SteamGoodMax;
        public float SteamHeatMinPerSecond;
        public float SteamHeatMaxPerSecond;
        public float GrinderSnapDistance;
        public float ShotPourSnapDistance;
        public float GrindingGaugeMin;
        public float GrindingGaugeMax;
        public float GrindingGaugeSpeed;
        public float GrindingPerfectMin;
        public float GrindingPerfectMax;
        public float GrindingGoodMin;
        public float GrindingGoodMax;
        public float TampingGaugeMin;
        public float TampingGaugeMax;
        public float TampingGaugeSpeed;
        public float TampingPerfectMin;
        public float TampingPerfectMax;
        public float TampingGoodMin;
        public float TampingGoodMax;

        public Color WallColor;
        public Color CounterColor;
        public Color CounterEdgeColor;
        public Color MachineColor;
        public Color MachineSlotIdleColor;
        public Color MachineSlotSnapColor;
        public Color MachineSlotLockedColor;
        public Color IngredientTrayColor;
        public Color PortafilterIdleColor;
        public Color PortafilterSnapColor;
        public Color PortafilterLockedColor;
        public Color CupEmptyColor;
        public Color CupEspressoColor;
        public Color CupLatteColor;
        public Color CupVanillaColor;
        public Color ServingAreaIdleColor;
        public Color ServingAreaActiveColor;
        public Color ExtractionButtonIdleColor;
        public Color ExtractionButtonActiveColor;
        public Color ExtractionButtonRunningColor;
        public Color GaugeBackgroundColor;
        public Color GaugeFillColor;
        public Color GaugeTargetColor;
        public Color GaugeFrameColor;
        public Color SteamWandColor;
        public Color PitcherColor;
        public Color SyrupBottleColor;
        public Color CustomerColor;
        public Color SpeechBubbleColor;
        public Color InstructionTextColor;
        public Color StatusTextColor;
        public Color SecondaryTextColor;

        public static GameConfig CreateRuntimeDefault()
        {
            var config = CreateInstance<GameConfig>();
            config.hideFlags = HideFlags.DontSave;
            config.ResetToDefaults();
            return config;
        }

        private void ResetToDefaults()
        {
            CameraOrthographicSize = 5f;
            CameraPosition = new Vector3(0f, 0f, -10f);

            WallSize = new Vector2(18f, 6.2f);
            CounterSize = new Vector2(18f, 4.1f);
            MachineSize = new Vector2(4.4f, 3.1f);
            MachineSlotSize = new Vector2(1.4f, 0.38f);
            IngredientTraySize = new Vector2(2.4f, 4.6f);
            PortafilterBodySize = new Vector2(2.1f, 0.72f);
            CupSize = new Vector2(0.95f, 1.15f);
            ServingAreaSize = new Vector2(2.4f, 1.8f);
            ExtractionButtonSize = new Vector2(1.2f, 0.6f);
            GaugeSize = new Vector2(4.8f, 0.5f);
            SteamWandSize = new Vector2(0.28f, 2.2f);
            PitcherSize = new Vector2(1.0f, 1.25f);
            SyrupBottleSize = new Vector2(0.72f, 1.5f);
            CustomerSize = new Vector2(1.5f, 2.1f);
            SpeechBubbleSize = new Vector2(2.6f, 1.0f);
            GrinderSize = new Vector2(1.75f, 2.3f);
            TamperSize = new Vector2(0.9f, 1.15f);
            ShotGlassSize = new Vector2(0.55f, 0.85f);
            WaterBottleSize = new Vector2(0.8f, 1.8f);

            WallPosition = new Vector3(0f, 1.95f, 0f);
            CounterPosition = new Vector3(0f, -3f, 0f);
            MachinePosition = new Vector3(0.3f, -0.15f, 0f);
            MachineSlotPosition = new Vector3(0.3f, -0.55f, 0f);
            IngredientTrayPosition = new Vector3(-6.35f, -1.1f, 0f);
            PortafilterSpawnPosition = new Vector3(-6.45f, -2.05f, 0f);
            CupPosition = new Vector3(0.25f, -2.05f, 0f);
            ServingAreaPosition = new Vector3(6.05f, -2.1f, 0f);
            ExtractionButtonPosition = new Vector3(0.25f, -1.95f, 0f);
            GaugePosition = new Vector3(0f, 2.6f, 0f);
            SteamWandSpawnPosition = new Vector3(4.6f, 0.05f, 0f);
            SteamWandSnapPosition = new Vector3(2.75f, -0.6f, 0f);
            PitcherPosition = new Vector3(2.8f, -2.05f, 0f);
            SyrupBottlePosition = new Vector3(-5.75f, -2.0f, 0f);
            CustomerPosition = new Vector3(5.45f, 1.15f, 0f);
            SpeechBubblePosition = new Vector3(5.4f, 2.75f, 0f);
            GrinderPosition = new Vector3(-3.25f, -0.9f, 0f);
            PortafilterWorkbenchPosition = new Vector3(-5.45f, -2.0f, 0f);
            TamperPosition = new Vector3(-4.15f, -0.85f, 0f);
            ShotGlassPosition = new Vector3(0.25f, -1.55f, 0f);
            WaterBottlePosition = new Vector3(-6.15f, -1.95f, 0f);

            PortafilterSnapDistance = 1.05f;
            PortafilterLockAngle = 95f;
            PortafilterRotateRadius = 0.45f;
            ServingSnapDistance = 1.15f;
            SteamWandSnapDistance = 0.9f;
            SteamWandMaxY = -0.85f;
            SteamWandMinY = -1.95f;

            ExtractionGaugeMin = 0f;
            ExtractionGaugeMax = 100f;
            ExtractionGaugeSpeed = 100f / 35f;
            ExtractionPerfectMin = 65f;
            ExtractionPerfectMax = 72f;
            ExtractionGoodMin = 55f;
            ExtractionGoodMax = 80f;

            SteamStartTemperature = 24f;
            SteamBurnTemperature = 75f;
            SteamPerfectMin = 60f;
            SteamPerfectMax = 70f;
            SteamGoodMin = 55f;
            SteamGoodMax = 75f;
            SteamHeatMinPerSecond = 0.5f;
            SteamHeatMaxPerSecond = 3f;
            GrinderSnapDistance = 1.0f;
            ShotPourSnapDistance = 1.0f;
            GrindingGaugeMin = 0f;
            GrindingGaugeMax = 100f;
            GrindingGaugeSpeed = 65f;
            GrindingPerfectMin = 70f;
            GrindingPerfectMax = 78f;
            GrindingGoodMin = 60f;
            GrindingGoodMax = 85f;
            TampingGaugeMin = 0f;
            TampingGaugeMax = 100f;
            TampingGaugeSpeed = 70f;
            TampingPerfectMin = 68f;
            TampingPerfectMax = 76f;
            TampingGoodMin = 55f;
            TampingGoodMax = 85f;

            WallColor = ColorPalette.Wall;
            CounterColor = ColorPalette.Counter;
            CounterEdgeColor = ColorPalette.CounterEdge;
            MachineColor = ColorPalette.MachineBody;
            MachineSlotIdleColor = ColorPalette.MachineSlotIdle;
            MachineSlotSnapColor = ColorPalette.MachineSlotSnap;
            MachineSlotLockedColor = ColorPalette.MachineSlotLocked;
            IngredientTrayColor = ColorPalette.IngredientTray;
            PortafilterIdleColor = ColorPalette.PortafilterIdle;
            PortafilterSnapColor = ColorPalette.PortafilterSnap;
            PortafilterLockedColor = ColorPalette.PortafilterLocked;
            CupEmptyColor = ColorPalette.CupEmpty;
            CupEspressoColor = ColorPalette.CupEspresso;
            CupLatteColor = ColorPalette.CupLatte;
            CupVanillaColor = ColorPalette.CupVanilla;
            ServingAreaIdleColor = ColorPalette.ServingAreaIdle;
            ServingAreaActiveColor = ColorPalette.ServingAreaActive;
            ExtractionButtonIdleColor = ColorPalette.ExtractionButtonIdle;
            ExtractionButtonActiveColor = ColorPalette.ExtractionButtonActive;
            ExtractionButtonRunningColor = ColorPalette.ExtractionButtonRunning;
            GaugeBackgroundColor = ColorPalette.GaugeBackground;
            GaugeFillColor = ColorPalette.GaugeFill;
            GaugeTargetColor = ColorPalette.GaugeTarget;
            GaugeFrameColor = ColorPalette.GaugeFrame;
            SteamWandColor = ColorPalette.SteamWand;
            PitcherColor = ColorPalette.Pitcher;
            SyrupBottleColor = ColorPalette.SyrupBottle;
            CustomerColor = ColorPalette.Customer;
            SpeechBubbleColor = ColorPalette.SpeechBubble;
            InstructionTextColor = ColorPalette.InstructionText;
            StatusTextColor = ColorPalette.StatusText;
            SecondaryTextColor = ColorPalette.SecondaryText;
        }
    }
}
