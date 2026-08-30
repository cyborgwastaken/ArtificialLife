using UnityEngine;

namespace ArtificialLife
{
    public sealed class Organism : MonoBehaviour
    {
        // [field: SerializeField] -> visible & live-updating in the Inspector during Play.
        [field: SerializeField] public float Energy { get; private set; }
        [field: SerializeField] public float Age { get; private set; }
        [field: SerializeField] public bool IsAlive { get; private set; }

        [field: SerializeField] public int FoodConsumed { get; private set; }
        [field: SerializeField] public int OffspringCount { get; private set; }
        [field: SerializeField] public int Generation { get; private set; }
        public Genome Genome { get; private set; }

        // Last tick's IO, exposed for the HUD / inspector gizmos.
        public float[] LastInput { get; private set; }
        public float[] LastOutput { get; private set; }

        SimulationSettings _s;
        EnvironmentManager _env;
        Rng _rng;
        SimulationManager _sim;

        NeuralNetwork _brain;
        Sensors _sensors;

        public void Spawn(Genome genome, Vector3 position, float startEnergy, int generation,
                          SimulationSettings s, EnvironmentManager env, Rng rng, SimulationManager sim)
        {
            _s = s; _env = env; _rng = rng; _sim = sim;

            Genome = genome;
            Generation = generation;
            _brain = genome.BuildNetwork();          // Genome is Milestone 4; stub returns a net
            _sensors = new Sensors();

            transform.position = new Vector3(position.x, 0f, position.z);
            transform.rotation = Quaternion.Euler(0f, rng.Range(0f, 360f), 0f);

            Energy = startEnergy;
            Age = 0f;
            FoodConsumed = 0;
            OffspringCount = 0;
            IsAlive = true;
            gameObject.SetActive(true);
        }

        public void Step(float dt)
        {
            if (!IsAlive) return;

            // 1. SENSE
            float[] input = _sensors.Sense(this, _env, _s);

            // 2. THINK
            float[] output = _brain.FeedForward(input);
            LastInput = input;
            LastOutput = output;

            // 3. ACT
            float turn = Mathf.Clamp(output[0], -1f, 1f);              // steer
            float move = (Mathf.Clamp(output[1], -1f, 1f) + 1f) * 0.5f; // throttle 0..1
            bool wantEat = output[2] > 0f;                              // eat gate

            transform.Rotate(0f, turn * _s.TurnSpeedMax * dt, 0f, Space.Self);

            float speed = move * _s.MoveSpeedMax;
            transform.position += transform.forward * (speed * dt);
            ConstrainToWorld();

            // 4. PAY ENERGY
            Energy -= _s.MetabolismRate * dt;
            Energy -= _s.MoveCostRate * (speed / _s.MoveSpeedMax) * dt;

            // 5. EAT (only if the network asked to)
            if (wantEat) TryEat();

            // 6. AGE
            Age += dt;

            // 7. REPRODUCE (Milestone 4; behind a setting)
            if (_s.AllowInLifeReproduction && Energy >= _s.ReproductionThreshold)
                Reproduce();

            // 8. DIE
            if (Energy <= 0f || Age >= _s.MaxLifespan) Die();
        }

        void TryEat()
        {
            Food target = null;
            float bestSqr = _s.EatRadius * _s.EatRadius;
            Vector3 p = transform.position;
            var list = _env.Food;
            for (int i = 0; i < list.Count; i++)
            {
                Food f = list[i];
                if (f == null) continue;
                Vector3 d = f.transform.position - p; d.y = 0f;
                float sqr = d.sqrMagnitude;
                if (sqr <= bestSqr) { bestSqr = sqr; target = f; }
            }
            if (target == null) return;

            Energy = Mathf.Min(_s.MaxEnergy, Energy + target.Energy);
            FoodConsumed++;
            _env.RemoveFood(target);
        }

        void Reproduce()
        {
            Energy -= _s.ReproductionCost;
            OffspringCount++;

            Genome child = Mutation.Mutated(Genome, _rng, _s.MutationRate, _s.MutationStrength,
                                            _sim.NextGenomeId(), Generation + 1);
            _sim.SpawnOrganism(child, transform.position + RandomOffset(),
                               _s.OffspringStartEnergy, Generation + 1);
        }

        Vector3 RandomOffset()
        {
            float a = _rng.Range(0f, Mathf.PI * 2f);
            return new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 1.5f;
        }

        void ConstrainToWorld()
        {
            Vector3 p = transform.position; p.y = 0f;
            float r = new Vector2(p.x, p.z).magnitude;
            if (r > _s.WorldRadius)
            {
                p.x *= _s.WorldRadius / r;
                p.z *= _s.WorldRadius / r;
            }
            transform.position = p;
        }

        void Die()
        {
            IsAlive = false;
            gameObject.SetActive(false);
        }

        void OnDrawGizmosSelected()
        {
            if (_s == null) return;
            Vector3 p = transform.position;
            Gizmos.color = Color.yellow;
            float[] angles = { -50f, -25f, 0f, 25f, 50f };
            foreach (float a in angles)
            {
                Vector3 dir = Quaternion.AngleAxis(a, Vector3.up) * transform.forward;
                Gizmos.DrawLine(p, p + dir * _s.VisionRange);
            }
        }
    }
}
