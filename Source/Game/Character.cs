using FlaxEngine;
using FlaxEngine.GUI;

namespace Game
{
    public class Character : Script
    {

        // Movement
        [Tooltip("The character model"), EditorDisplay(null,"Character"), EditorOrder(1)]
        public Actor CharacterObj { get; set; } = null;

        [Range(0f, 2000f), Tooltip("Forward speed factor"), EditorDisplay(null,"Speed"), EditorOrder(2)]
        public float Speed { get; set; } = 930;

        [Range(0f, 1f), Tooltip("Laneshift speed factor"), EditorDisplay(null, "Lane Shift Speed"), EditorOrder(3)]
        public float LaneSpeed { get; set; } = 0.5f;
        public float LaneWidth = 110f;
        public UIControl LeftRightKeyGroup;
        public UIControl ColLabelQty;
        public UIControl RestartGroup;
        public SceneReference MainScene;

        private int LaneID;
        private bool LaneChanging = false;
        private Vector3 LaneMovement = Vector3.Zero;
        private int LaneTargetID = 0;
        private float LaneTargetXpos = 0;
        private Camera cam;
        private int ColQty = 0;
        private AudioSource Asou1;
        private bool Pausing = false;
        private AnimGraphParameter AGparameter;

        public override void OnStart()
        {
            cam = Actor.GetChild<Camera>();
            Asou1 = Actor.GetChild<AudioSource>();
            AGparameter = Actor.GetChild<AnimatedModel>().GetParameter("AniSpeed");
        }

        public override void OnUpdate()
        {
            Screen.CursorLock = CursorLockMode.Locked;
            Screen.CursorVisible = false;
        }

        public override void OnFixedUpdate()
        {

            if (Pausing)
            {
                WaitForPauseToEnd();
                return;
            }
            // Raycast
            if (Physics.RayCast(Actor.Position+new Vector3(0,60,0), Actor.Direction,  out RayCastHit hit, 50f,layerMask: (1U << 2),true))
            {
                //DebugDraw.DrawSphere(new BoundingSphere(hit.Point, 50), Color.Red);
                if (!Asou1.IsActuallyPlayingSth)
                    Asou1.Stop();
                Asou1.Play();
                ColQty++;
                ColLabelQty.Get<Label>().Text = ColQty.ToString();
                hit.Collider.IsActive = false;
            }

            // Get input axes
            if (!LaneChanging)
            {
                float inputH = Input.GetAxis("Horizontal");
                if (inputH < -0.1 && LaneID != -1)
                    SetLaneTarget(-1);
                if (inputH > 0.1 && LaneID != 1)
                    SetLaneTarget(1);
             }
            else
                UpdateLaneMovement();
            // Apply movement towards the target direction
            Vector3 movementDirection = Actor.Transform.TransformDirection(Vector3.Forward + (LaneMovement*LaneSpeed));
            // Apply controller movement, evaluate whether we are sprinting or not
            Actor.As<CharacterController>().Move(movementDirection * Time.DeltaTime *  Speed);
            
        }

        private void SetLaneTarget(int LaneDelta)
        {
            LaneChanging = true;
            LaneTargetID = LaneID+LaneDelta;
            LaneTargetXpos = LaneTargetID * LaneWidth;
        }

        private void UpdateLaneMovement()
        {
            LaneMovement.X = (LaneTargetID - LaneID);
            if ((LaneTargetXpos - Actor.Position.X) * (LaneTargetID - LaneID) < 0)
            {
                LaneID = LaneTargetID;
                LaneMovement.X = 0;
                Actor.Position = new Vector3(LaneTargetXpos, Actor.Position.Y, Actor.Position.Z);
                LaneChanging = false;
            }
            cam.Position = new Vector3(0, cam.Position.Y, cam.Position.Z);
        }

        public void OnPause()
        {
            Pausing = true;
            AGparameter.Value = 0;
            LeftRightKeyGroup.IsActive = false;
            RestartGroup.IsActive = true;
        }

        private void WaitForPauseToEnd()
        {
            if (Input.GetKey(KeyboardKeys.Return))
                Level.ChangeSceneAsync(MainScene);
         }
    }
}