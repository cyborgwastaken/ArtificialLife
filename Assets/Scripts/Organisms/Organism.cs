using UnityEngine;

namespace ArtificialLife
{
    public sealed class Organism : MonoBehaviour
    {
        // [field: SerializeField] serialises the hidden backing field so you can WATCH these
        // in the Inspector during Play. They stay private-set - nothing external can write them.
        [field: SerializeField] public float Energy { get; private set; }
        [field: SerializeField] public float Age { get; private set; }
        [field: SerializeField] public bool IsAlive { get; private set; }

        [field: SerializeField] public int FoodConsumed { get; private set; }
        public int OffspringCount { get; private set; }
        public int Generation { get; private set; }

        EnvironmentManager _env;
        SimulationSettings _s;
        Rng _rng;

        // MILESTONE 1: a slowly drifting target heading so the walk looks purposeful, not jittery.
        float _wanderAngle;

        /// Called once when the organism is (re)used from the pool.
        public void Spawn(Vector3 position, float startEnergy, int generation, SimulationSettings s, Rng rng, EnvironmentManager env)
        {
            _s = s;
            _rng = rng;
            _env = env;
            Generation = generation;

            transform.position = new Vector3(position.x, 0f, position.z);
            transform.rotation = Quaternion.Euler(0f, rng.Range(0f, 360f), 0f);

            Energy = startEnergy;
            Age = 0f;
            FoodConsumed = 0;
            OffspringCount = 0;
            _wanderAngle = 0f;
            IsAlive = true;
            gameObject.SetActive(true);
        }

        /// One simulation tick. dt is always SimulationSettings.SimStepSeconds.
        public void Step(float dt)
        {
            if (!IsAlive) return;

            // --- decide (random walk for now) -------------------------------------
            _wanderAngle += _rng.Range(-1f, 1f) * 90f * dt;          // degrees
            float turn = Mathf.Clamp(_wanderAngle * 0.05f, -1f, 1f); // -1..1
            float move = 0.7f;                                        // 0..1, constant for now

            // --- act -------------------------------------------------------------
            transform.Rotate(0f, turn * _s.TurnSpeedMax * dt, 0f, Space.Self);
            float speed = move * _s.MoveSpeedMax;
            transform.position += transform.forward * (speed * dt);
            ConstrainToWorld();

            TryEat();

            // --- pay energy -----------------------------------------------------
            Energy -= _s.MetabolismRate * dt;
            Energy -= _s.MoveCostRate * (speed / _s.MoveSpeedMax) * dt;

            // --- age & die ----------------------------------------------------
            Age += dt;
            if (Energy <= 0f || Age >= _s.MaxLifespan) Die();
        }

        void ConstrainToWorld()
        {
            Vector3 p = transform.position;
            p.y = 0.6f;
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
                Vector3 d = f.transform.position - p;
                d.y = 0f;
                float sqr = d.sqrMagnitude;
                if (sqr <= bestSqr) { bestSqr = sqr; target = f; }
            }
            if (target == null) return;

            Energy = Mathf.Min(_s.MaxEnergy, Energy + target.Energy);
            FoodConsumed++;
            _env.RemoveFood(target);
        }

    }
}
