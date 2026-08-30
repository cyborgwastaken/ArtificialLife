using System.Collections.Generic;
using UnityEngine;

namespace ArtificialLife
{
    public sealed class SimulationManager : MonoBehaviour
    {
        [SerializeField] SimulationSettings _settings;
        [SerializeField] Organism _organismPrefab;

        public SimulationSettings Settings => _settings;
        public Rng Rng { get; private set; }

        [SerializeField] EnvironmentManager _environment;
        public EnvironmentManager Environment => _environment;


        public bool Paused;
        public float SpeedMultiplier = 1f;

        readonly List<Organism> _pool = new List<Organism>();
        double _accumulator;

        void Awake()
        {
            Rng = new Rng(_settings.Seed);

            _environment.Setup(_settings, Rng);
            _environment.ResetWorld();


            // MILESTONE 1: just drop 10 organisms in and watch them wander and die.
            for (int i = 0; i < _settings.PopulationSize; i++)
            {
                Vector3 pos = RandomPointInWorld();
                SpawnOrganism(pos, _settings.StartEnergy, 0);
            }
        }

        void Update()
        {
            if (Paused) return;

            if (_settings.DeterministicLockstep)
            {
                int steps = Mathf.Max(1,
                    Mathf.RoundToInt(_settings.LockstepStepsPerFrame * SpeedMultiplier));
                for (int i = 0; i < steps; i++) Step(_settings.SimStepSeconds);
            }
            else
            {
                _accumulator += Time.deltaTime * SpeedMultiplier;
                int steps = 0;
                while (_accumulator >= _settings.SimStepSeconds && steps < _settings.MaxStepsPerFrame)
                {
                    Step(_settings.SimStepSeconds);
                    _accumulator -= _settings.SimStepSeconds;
                    steps++;
                }
                if (steps >= _settings.MaxStepsPerFrame) _accumulator = 0.0;
            }
        }

        void Step(float dt)
        {
            _environment.Step(dt);

            var organisms = _environment.Organisms;
            for (int i = 0; i < organisms.Count; i++)
                if (organisms[i] != null && organisms[i].IsAlive)
                    organisms[i].Step(dt);
        }


        public Organism SpawnOrganism(Vector3 position, float startEnergy, int generation)
        {
            Organism o;
            int last = _pool.Count - 1;
            if (last >= 0) { o = _pool[last]; _pool.RemoveAt(last); }
            else           { o = Instantiate(_organismPrefab, transform); }

            o.Spawn(position, startEnergy, generation, _settings, Rng, _environment);
            _environment.Organisms.Add(o);     // replaces the old _organisms list

            return o;
        }

        Vector3 RandomPointInWorld()
        {
            // Uniform inside a disc: angle uniform, radius ∝ sqrt(uniform) so points don't clump
            // toward the centre.
            float a = Rng.NextFloat() * Mathf.PI * 2f;
            float r = Mathf.Sqrt(Rng.NextFloat()) * _settings.WorldRadius;
            return new Vector3(Mathf.Cos(a) * r, 0.6f, Mathf.Sin(a) * r);
        }
    }
}
