using System.Collections.Generic;
using UnityEngine;

namespace ArtificialLife
{
    public sealed class EvolutionManager
    {
        readonly SimulationManager _sim;
        readonly SimulationSettings _s;
        readonly EnvironmentManager _env;
        readonly Rng _rng;

        public int Generation { get; private set; }
        public float GenerationElapsed { get; private set; }
        public GenerationStats LastStats { get; private set; }
        public readonly List<GenerationStats> History = new List<GenerationStats>();

        List<Genome> _seedGenomes = new List<Genome>();

        public EvolutionManager(SimulationManager sim, SimulationSettings s,
                                EnvironmentManager env, Rng rng)
        {
            _sim = sim; _s = s; _env = env; _rng = rng;
        }

        public void Begin()
        {
            PopulationSave save = _s.LoadOnStart ? SaveSystem.Load() : null;

            int genes = new NeuralNetwork(_s.LayerSizes).ParameterCount;
            bool usable = save != null
                       && save.seedGenomes.Count == _s.PopulationSize
                       && save.seedGenomes[0] != null
                       && save.seedGenomes[0].Genes != null
                       && save.seedGenomes[0].Genes.Length == genes;

            if (usable)
            {
                Generation = save.generation;
                _sim.GenomeIdCounter = save.nextGenomeId;
                _seedGenomes = new List<Genome>(save.seedGenomes);
                History.Clear();
                History.AddRange(save.history);
            }
            else
            {
                Generation = 0;
                _seedGenomes = new List<Genome>(_s.PopulationSize);
                for (int i = 0; i < _s.PopulationSize; i++)
                    _seedGenomes.Add(Genome.Random(_s.LayerSizes, _rng, _sim.NextGenomeId(), 0));
            }

            BeginGeneration();
        }

        public PopulationSave CaptureState() => new PopulationSave
        {
            seed         = _s.Seed,
            generation   = Generation,
            nextGenomeId = _sim.GenomeIdCounter,
            seedGenomes  = new List<Genome>(_seedGenomes),
            history      = new List<GenerationStats>(History),
        };


        void BeginGeneration()
        {
            GenerationElapsed = 0f;

            _sim.DestroyAllOrganisms();   // clear the previous generation (dead + survivors)
            _env.ResetWorld();

            for (int i = 0; i < _seedGenomes.Count; i++)
                _sim.SpawnOrganism(_seedGenomes[i], _env.RandomPointInWorld(),
                                   _s.StartEnergy, Generation);
        }

        public void Step(float dt)
        {
            GenerationElapsed += dt;
            if (GenerationElapsed >= _s.GenerationDuration || CountAlive() == 0)
                EndGeneration();
        }

        int CountAlive()
        {
            int n = 0;
            var all = _env.Organisms;
            for (int i = 0; i < all.Count; i++)
                if (all[i] != null && all[i].IsAlive) n++;
            return n;
        }

        float Fitness(Organism o) =>
            o.Age            * _s.FitnessPerSecondAlive +
            o.FoodConsumed   * _s.FitnessPerFood +
            o.OffspringCount * _s.FitnessPerOffspring;

        void EndGeneration()
        {
            // Every organism that lived this generation - seeded or born mid-generation, dead or
            // alive - is still in _env.Organisms (dead ones deactivated). Score them all now,
            // before BeginGeneration destroys the lot.
            var all = _env.Organisms;

            var candidates = new List<Candidate>(all.Count);
            float sumLife = 0f, maxLife = 0f, sumEnergy = 0f, sumFit = 0f, maxFit = 0f;
            int food = 0, offspring = 0;

            for (int i = 0; i < all.Count; i++)
            {
                Organism o = all[i];
                if (o == null || o.Genome == null) continue;

                float fit = Fitness(o);
                candidates.Add(new Candidate(o.Genome, fit));

                sumLife += o.Age;
                if (o.Age > maxLife) maxLife = o.Age;
                sumEnergy += Mathf.Max(0f, o.Energy);
                food += o.FoodConsumed;
                offspring += o.OffspringCount;
                sumFit += fit;
                if (fit > maxFit) maxFit = fit;
            }

            int n = Mathf.Max(1, candidates.Count);
            var stats = new GenerationStats
            {
                Generation        = Generation,
                Cohort            = candidates.Count,
                AvgLifespan       = sumLife / n,
                MaxLifespan       = maxLife,
                AvgEnergyAtEnd    = sumEnergy / n,
                TotalFoodConsumed = food,
                TotalOffspring    = offspring,
                AvgFitness        = sumFit / n,
                MaxFitness        = maxFit,
            };
            LastStats = stats;
            History.Add(stats);
            Debug.Log($"[Gen {Generation}] cohort={stats.Cohort} " +
                      $"avgLife={stats.AvgLifespan:F1}s maxLife={stats.MaxLifespan:F1}s " +
                      $"food={stats.TotalFoodConsumed} avgFit={stats.AvgFitness:F1} " +
                      $"maxFit={stats.MaxFitness:F1}");

            // --- build the next generation ------------------------------------
            var next = new List<Genome>(_s.PopulationSize);

            if (candidates.Count == 0)
            {
                for (int i = 0; i < _s.PopulationSize; i++)
                    next.Add(Genome.Random(_s.LayerSizes, _rng, _sim.NextGenomeId(), Generation + 1));
            }
            else
            {
                foreach (var elite in Selection.TopElites(candidates, _s.EliteCount))
                {
                    var copy = new Genome(elite.LayerSizes, elite.Genes) // deep copy
                    {
                        Id = _sim.NextGenomeId(),
                        ParentId = elite.Id,
                        GenerationBorn = Generation + 1,
                    };
                    next.Add(copy);
                }
                while (next.Count < _s.PopulationSize)
                {
                    Genome parent = Selection.Tournament(candidates, _rng, _s.TournamentK);
                    next.Add(Mutation.Mutated(parent, _rng, _s.MutationRate, _s.MutationStrength,
                                              _sim.NextGenomeId(), Generation + 1));
                }
            }

            _seedGenomes = next;
            Generation++;
            BeginGeneration();
        }
    }
}
