using UnityEngine;

    public class GrapplingGun : MonoBehaviour
    {
        [Header("References")]
        public Transform gunTip;
        public Transform player;
        public Transform cameraTransform;

        [Header("Grappling Settings")]
        public LayerMask grappleLayer;
        public float maxGrappleDistance = 100f;
        public float spring = 4.5f;
        public float damper = 7f;
        public float massScale = 4.5f;

        [Header("Pull Settings")]
        public float pullForce = 20f; // How strongly it pulls the player toward the grapple point

        private LineRenderer lr;
        private Vector3 grapplePoint;
        private SpringJoint joint;
        private Vector3 currentGrapplePosition;
        private Rigidbody playerRb;

        private void Awake()
        {
            lr = GetComponent<LineRenderer>();
            playerRb = player.GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                StartGrapple();
            else if (Input.GetMouseButtonUp(0))
                StopGrapple();
        }

        private void FixedUpdate()
        {
            if (joint != null)
            {
                // Apply gentle pulling force toward the grapple point
                Vector3 pullDirection = (grapplePoint - player.position).normalized;
                playerRb.AddForce(pullDirection * pullForce, ForceMode.Acceleration);
            }
        }

        private void LateUpdate()
        {
            DrawRope();
        }

        private void StartGrapple()
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, maxGrappleDistance, grappleLayer))
            {
                grapplePoint = hit.point;

                joint = player.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grapplePoint;

                float distance = Vector3.Distance(player.position, grapplePoint);
                joint.maxDistance = distance * 0.8f;
                joint.minDistance = distance * 0.25f;

                joint.spring = spring;
                joint.damper = damper;
                joint.massScale = massScale;

                lr.positionCount = 2;
                currentGrapplePosition = gunTip.position;
            }
        }

        private void StopGrapple()
        {
            lr.positionCount = 0;
            if (joint != null)
                Destroy(joint);
        }

        private void DrawRope()
        {
            if (joint == null) return;

            currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, currentGrapplePosition);
        }

        public bool IsGrappling() => joint != null;
        public Vector3 GetGrapplePoint() => grapplePoint;
    }