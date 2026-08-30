using System.Collections.Generic;
using UnityEngine;

namespace ArtificialLife
{
    /// Builds the network's 8-element input vector from the world around one organism.
    public sealed class Sensors
    {
        public const int Count = 8;

        static readonly float[] RayAnglesDeg = { -50f, -25f, 0f, 25f, 50f };

        readonly float[] _input = new float[Count];

        public float[] Sense(Organism self, EnvironmentManager env, SimulationSettings s)
        {
            Vector3 pos = self.transform.position;
            Vector3 fwd = self.transform.forward; fwd.y = 0f; fwd.Normalize();

            for (int r = 0; r < RayAnglesDeg.Length; r++)
            {
                Vector3 dir = Quaternion.AngleAxis(RayAnglesDeg[r], Vector3.up) * fwd;
                _input[r] = ClosestFoodProximity(pos, dir, env.Food,
                                                 s.VisionRange, s.SensorHalfConeDegrees);
            }

            _input[5] = Mathf.Clamp01(self.Energy / s.MaxEnergy);
            _input[6] = Mathf.Clamp01(self.Age / s.MaxLifespan);
            _input[7] = ClosestOrganismProximity(self, env.Organisms, fwd,
                                                 s.VisionRange, s.SensorHalfConeDegrees * 2f);
            return _input;
        }

        static float ClosestFoodProximity(Vector3 origin, Vector3 dir, List<Food> food,
                                          float range, float halfConeDeg)
        {
            float cosHalf = Mathf.Cos(halfConeDeg * Mathf.Deg2Rad);
            float best = 0f;
            for (int i = 0; i < food.Count; i++)
            {
                Food f = food[i];
                if (f == null) continue;
                Vector3 to = f.transform.position - origin; to.y = 0f;
                float d = to.magnitude;
                if (d > range || d < 1e-4f) continue;
                if (Vector3.Dot(dir, to / d) < cosHalf) continue;   // outside cone
                float proximity = 1f - d / range;
                if (proximity > best) best = proximity;
            }
            return best;
        }

        static float ClosestOrganismProximity(Organism self, List<Organism> all, Vector3 fwd,
                                              float range, float coneDeg)
        {
            float cosHalf = Mathf.Cos(coneDeg * 0.5f * Mathf.Deg2Rad);
            Vector3 pos = self.transform.position;
            float best = 0f;
            for (int i = 0; i < all.Count; i++)
            {
                Organism o = all[i];
                if (o == null || o == self || !o.IsAlive) continue;
                Vector3 to = o.transform.position - pos; to.y = 0f;
                float d = to.magnitude;
                if (d > range || d < 1e-4f) continue;
                if (Vector3.Dot(fwd, to / d) < cosHalf) continue;
                float proximity = 1f - d / range;
                if (proximity > best) best = proximity;
            }
            return best;
        }
    }
}
