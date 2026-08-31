using System.Collections.Generic;
using UnityEngine;

namespace ArtificialLife
{
    public sealed class SimulationManager : MonoBehaviour
    {
        [SerializeField] SimulationSettings _settings;
        [SerializeField] EnvironmentManager _environment;
        [SerializeField] Organism _organismPrefab;

        public SimulationSettings Settings => _settings;
        public EnvironmentManager Environment => _environment;
        public Rng Rng { get; private set; }
        public EvolutionManager Evolution { get; private set; }

        public bool Paused;
        public float SpeedMultiplier = 1f;

        int _nextGenomeId;
        double _accumulator;
        public int GenomeIdCounter { get => _nextGenomeId; set => _nextGenomeId = value; }

        void Awake()
        {
            Rng = new Rng(_settings.Seed);
            _environment.Setup(_settings, Rng);
            Evolution = new EvolutionManager(this, _settings, _environment, Rng);
            Evolution.Begin();
        }

        void OnApplicationQuit()
        {
            if (_settings.SaveOnQuit && Evolution != null)
                SaveSystem.Save(Evolution.CaptureState());
        }

        public void SaveNow()       { if (Evolution != null) SaveSystem.Save(Evolution.CaptureState()); }
        public void NewPopulation() { SaveSystem.Delete(); Evolution.Begin(); }


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

            Evolution.Step(dt);
        }

        public int NextGenomeId() => _nextGenomeId++;

        public Organism SpawnOrganism(Genome genome, Vector3 position, float startEnergy, int generation)
        {
            Organism o = Instantiate(_organismPrefab, transform);
            o.Spawn(genome, position, startEnergy, generation, _settings, _environment, Rng, this);
            _environment.Organisms.Add(o);
            return o;
        }

        public void RestartRun()
        {
            Rng = new Rng(_settings.Seed);
            _environment.Setup(_settings, Rng);
            Evolution = new EvolutionManager(this, _settings, _environment, Rng);
            Evolution.Begin();   // reloads from save if LoadOnStart, else fresh gen 0
        }


        /// Destroy every organism (dead and alive) and clear the registry. Called at each
        /// generation boundary, after EvolutionManager has scored them.
        public void DestroyAllOrganisms()
        {
            var list = _environment.Organisms;
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null) Destroy(list[i].gameObject);
            list.Clear();
        }
    }
}
