using UnityEngine;

namespace ArtificialLife
{
    public sealed class Food : MonoBehaviour
    {
        [field: SerializeField] public float Energy { get; private set; }
        [field: SerializeField] public bool Eaten { get; private set; }

        public void Init(float energy)
        {
            Energy = energy;
            Eaten = false;
            gameObject.SetActive(true);
        }

        public void Consume()
        {
            Eaten = true;
            gameObject.SetActive(false);
        }
    }
}
