using UnityEngine;

namespace ArtificialLife
{
    [CreateAssetMenu(menuName = "ArtificialLife/Simulation Settings", fileName = "SimSettings")]
    public sealed class SimulationSettings : ScriptableObject
    {
        [Header("Determinism")]
        public int Seed = 12345;
        [Tooltip("Ignore wall-clock time; run a fixed number of ticks every frame. " +
                 "On = perfectly reproducible but frame-rate affects how fast sim-time passes. " +
                 "Off = real-time-ish with a safety cap.")]
        public bool DeterministicLockstep = true;
        [Range(1, 512)] public int LockstepStepsPerFrame = 4;
        [Range(1, 4000)] public int MaxStepsPerFrame = 400;
        public float SimStepSeconds = 1f / 60f;

        [Header("World")]
        [Tooltip("Organisms and food stay inside this radius on the XZ plane.")]
        public float WorldRadius = 40f;

        [Header("Population")]
        public int PopulationSize = 40;
        [Tooltip("A generation ends after this many sim-seconds, or earlier if everyone dies.")]
        public float GenerationDuration = 45f;
        public bool AllowInLifeReproduction = false;

        [Header("Food")]
        public int FoodTarget = 120;
        public float FoodSpawnPerSecond = 8f;
        public float FoodEnergy = 25f;

        [Header("Organism body (not yet heritable - see doc section 25)")]
        public float StartEnergy = 50f;
        public float MaxEnergy = 100f;
        public float MoveSpeedMax = 6f;      // world units / sim-second
        public float TurnSpeedMax = 180f;    // degrees / sim-second
        public float MetabolismRate = 1.5f;  // energy / sim-second, always paid
        public float MoveCostRate = 3.0f;    // extra energy / sim-second at full speed
        public float MaxLifespan = 60f;      // sim-seconds
        public float VisionRange = 14f;
        public float SensorHalfConeDegrees = 18f;
        public float EatRadius = 1.2f;

        [Header("Reproduction")]
        public float ReproductionThreshold = 85f;
        public float ReproductionCost = 45f;
        public float OffspringStartEnergy = 35f;

        [Header("Brain topology (fixed for the prototype)")]
        public int InputCount = 8;
        public int HiddenCount = 6;
        public int OutputCount = 3;

        [Header("Mutation")]
        [Range(0f, 1f)] public float MutationRate = 0.06f;   // chance each gene is touched
        public float MutationStrength = 0.12f;               // stddev of the nudge

        [Header("Selection")]
        [Range(1, 8)] public int TournamentK = 3;
        [Range(0, 10)] public int EliteCount = 2;

        [Header("Fitness weights (kept deliberately simple - doc section 17)")]
        public float FitnessPerSecondAlive = 1f;
        public float FitnessPerFood = 5f;
        public float FitnessPerOffspring = 25f;

        [Header("Save / resume")]
        public bool LoadOnStart = true;
        public bool SaveOnQuit = true;


        public int[] LayerSizes => new[] { InputCount, HiddenCount, OutputCount };
    }
}
