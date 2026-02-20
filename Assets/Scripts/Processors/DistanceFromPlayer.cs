using UnityEngine;

namespace Assets.Scripts
{
    public class DistanceFromPlayer : IProcessor<Vector3, float>
    {
        readonly Transform player;

        public DistanceFromPlayer(Transform player)
        {
            this.player = player;
        }

        public float Process(Vector3 point) => Vector3.Distance(player.position, point);
    }
}
