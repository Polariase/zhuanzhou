using UnityEngine;

namespace FXLavaShader
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemRandomStart : MonoBehaviour
    {
        // a simple script to scale the size, speed and lifetime of a particle system

        public Vector2 Delay = new Vector2(0,30);
        public Vector2 Scale = new Vector2(1,1.2f);

        private void Start()
        {
            var systems = GetComponentsInChildren<ParticleSystem>();
            float delay = Random.Range(Delay.x,Delay.y);
            transform.localScale = Vector3.one * Random.Range(Scale.x, Scale.y);
            foreach (ParticleSystem system in systems)
            {
                system.startDelay = delay;
                system.Clear();
                system.Play();
            }
        }
    }
}
