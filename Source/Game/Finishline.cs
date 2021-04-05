using FlaxEngine;

namespace Game
{
    public class Finishline : Script
    {
        public CharacterController player;

        public override void OnEnable()
        {
            Actor.As<Collider>().TriggerEnter += OnTriggerEnter;
        }

        public override void OnDisable()
        {
            Actor.As<Collider>().TriggerEnter -= OnTriggerEnter;
        }

        void OnTriggerEnter(PhysicsColliderActor coll)
        {
            if (coll is CharacterController)
                player.GetScript<Character>().OnPause();
                    //Debug.Log("triggered");
        }

    }
}
