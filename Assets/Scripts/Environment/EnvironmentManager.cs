using System.Collections.Generic;
using UnityEngine;

namespace ArtificialLife
{
    public sealed class EnvironmentManager : MonoBehaviour
    {
        [SerializeField] Food _foodPrefab;

        public readonly List<Food> Food = new List<Food>();
        public readonly List<Organism> Organisms = new List<Organism>();

        readonly List<Food> _foodPool = new List<Food>();
        SimulationSettings _s;
        Rng _rng;
        float _spawnCarry;

        public void Setup(SimulationSettings settings, Rng rng)
        {
            _s = settings;
            _rng = rng;
        }

        /// Wipe all food and reseed to the target count. Called at the start of every generation.
        public void ResetWorld()
        {
            for (int i = 0; i < Food.Count; i++)
                if (Food[i] != null) { Food[i].Consume(); _foodPool.Add(Food[i]); }
            Food.Clear();

            for (int i = 0; i < _s.FoodTarget; i++) SpawnFood();
        }

        public void Step(float dt)
        {
            _spawnCarry += _s.FoodSpawnPerSecond * dt;
            while (_spawnCarry >= 1f)
            {
                _spawnCarry -= 1f;
                if (Food.Count < _s.FoodTarget) SpawnFood();
            }
        }

        void SpawnFood()
        {
            Food f = GetPooled();
            f.transform.position = RandomPointInWorld();
            f.Init(_s.FoodEnergy);
            Food.Add(f);
        }

        Food GetPooled()
        {
            int last = _foodPool.Count - 1;
            if (last >= 0) { Food f = _foodPool[last]; _foodPool.RemoveAt(last); return f; }
            return Instantiate(_foodPrefab, transform);
        }

        /// Remove one food item (it was eaten). Safe to call from Organism.Step.
        public void RemoveFood(Food f)
        {
            f.Consume();
            Food.Remove(f);          // O(n) - fine at prototype scale
            _foodPool.Add(f);
        }

        public Vector3 RandomPointInWorld()
        {
            float a = _rng.NextFloat() * Mathf.PI * 2f;
            float r = Mathf.Sqrt(_rng.NextFloat()) * _s.WorldRadius;
            return new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
        }
    }
}
