using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 grapplePoint;

    [Header("Grapple Targets")]
    [SerializeField] private LayerMask whatIsGrapplePoint;
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private LayerMask whatIsObstacle;

    [SerializeField] private string grapplePointTag = "GrapplePoint";

    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;
    private MeleeWeapon meleeWeapon;

    [SerializeField] private Transform gunTip, camera, player;
    private Rigidbody playerRb;

    [SerializeField] private float maxDistance = 100f;

    [Header("Enemy Grapple Settings")]
    public float enemyGrappleSpeed = 30f;
    public float enemyGrappleMinDistance = 5f;
    public float enemyGrappleBoostForce = 15f;
    public float enemyGrappleVerticalBoost = 5f;
    public float enemyAutoAimSearchRadius = 5f; 
    public float enemyAutoAimMaxDistance = 70f;

    [Header("Standard Grapple Settings")]
    public float springJointSpring = 4.5f;
    public float springJointDamper = 7f;
    public float springJointMassScale = 4.5f;
    public float springJointMinDistanceMultiplier = 0.25f;
    public float springJointMaxDistanceMultiplier = 0.8f;

    [Header("Grapple Point Auto-Aim")]
    public float autoAimSearchRadius = 3f;
    public float autoAimMaxDistance = 50f;

    private SpringJoint springJoint;
    private bool isGrapplingEnemy = false;
    private bool isGrapplingStandard = false;
    private Transform grappledTargetTransform;

    private Vector3 _lastAutoAimHitPoint = Vector3.zero;
    private bool _drawAutoAimGizmo = false;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        playerRb = player.GetComponent<Rigidbody>();
        meleeWeapon = GetComponent<MeleeWeapon>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StartGrapple();
        }
        if (Input.GetMouseButtonUp(1))
        {
            StopGrapple();
        }

        animator.SetBool("IsGrappling", IsGrappling());
    }

    private void FixedUpdate()
    {
        if (isGrapplingEnemy && grappledTargetTransform != null)
        {
            grapplePoint = grappledTargetTransform.position;

            Vector3 directionToGrapplePoint = (grapplePoint - player.position).normalized;
            playerRb.AddForce(directionToGrapplePoint * enemyGrappleSpeed, ForceMode.Acceleration);

            if (Vector3.Distance(player.position, grapplePoint) < enemyGrappleMinDistance)
            {
                Vector3 horizontalBounceDirection = camera.forward;
                horizontalBounceDirection.y = 0;
                horizontalBounceDirection.Normalize();

                Vector3 totalBounceForce = horizontalBounceDirection * enemyGrappleBoostForce;
                totalBounceForce += Vector3.up * enemyGrappleVerticalBoost;

                playerRb.AddForce(totalBounceForce, ForceMode.Impulse);

                meleeWeapon.PerformMeleeAttack();
                meleeWeapon.GrappingAnimation();

                StopGrapple();
            }
        }
    }

    private void LateUpdate()
    {
        if (IsGrappling())
        {
            DrawRope();
        }
    }

    private void DrawRope()
    {
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
    }

    private void StartGrapple()
    {
        StopGrapple();
        RaycastHit hit;

        LayerMask combinedEnemyAndObstacleLayer = whatIsEnemy | whatIsObstacle;

        if (Physics.Raycast(camera.position, camera.forward, out hit, enemyAutoAimMaxDistance, combinedEnemyAndObstacleLayer))
        {
            _lastAutoAimHitPoint = hit.point;
            _drawAutoAimGizmo = true;

            Transform bestTarget = null;

            if (((1 << hit.collider.gameObject.layer) & whatIsEnemy) != 0)
            {
                bestTarget = hit.transform;
            }
            else 
            {
                bestTarget = FindBestEnemyForGrapple(hit.point);
            }

            if (bestTarget != null)
            {
                grapplePoint = bestTarget.position;
                grappledTargetTransform = bestTarget;
                isGrapplingEnemy = true;
                isGrapplingStandard = false;

                lr.positionCount = 2;
                lr.enabled = true;
                Debug.Log($"Grappling Enemy: {bestTarget.name}!");
                return;
            }
        }

        if (Physics.Raycast(camera.position, camera.forward, out hit, autoAimMaxDistance, whatIsObstacle))
        {
            _lastAutoAimHitPoint = hit.point;
            _drawAutoAimGizmo = true;

            Transform bestGrapplePoint = FindBestGrapplePointAroundHit(hit.point);
            if (bestGrapplePoint != null)
            {
                grapplePoint = bestGrapplePoint.position;
                grappledTargetTransform = bestGrapplePoint;
                isGrapplingStandard = true;
                isGrapplingEnemy = false;
                SetupSpringJoint();
                Debug.Log("Grappling to Auto-Aimed Grapple Point near general hit!");
                return;
            }
        }

        StopGrapple();
    }

    private void SetupSpringJoint()
    {
        springJoint = player.gameObject.AddComponent<SpringJoint>();
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.connectedAnchor = grapplePoint;

        float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
        springJoint.maxDistance = distanceFromPoint * springJointMaxDistanceMultiplier;
        springJoint.minDistance = distanceFromPoint * springJointMinDistanceMultiplier;

        springJoint.spring = springJointSpring;
        springJoint.damper = springJointDamper;
        springJoint.massScale = springJointMassScale;

        lr.positionCount = 2;
        lr.enabled = true;
    }

    private Transform FindBestEnemyForGrapple(Vector3 searchCenter)
    {
        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        Collider[] collidersInRadius = Physics.OverlapSphere(searchCenter, enemyAutoAimSearchRadius, whatIsEnemy);

        foreach (Collider col in collidersInRadius)
        {
            if (!Physics.Linecast(camera.position, col.transform.position, ~whatIsEnemy & ~whatIsGrapplePoint & ~whatIsObstacle))
            {
                float distToCamera = Vector3.Distance(camera.position, col.transform.position);
                if (distToCamera < closestDistance)
                {
                    closestDistance = distToCamera;
                    bestTarget = col.transform;
                }
            }
        }
        return bestTarget;
    }

    private Transform FindBestGrapplePointAroundHit(Vector3 searchCenter)
    {
        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        Collider[] collidersInRadius = Physics.OverlapSphere(searchCenter, autoAimSearchRadius, whatIsGrapplePoint);

        foreach (Collider col in collidersInRadius)
        {
            if (col.CompareTag(grapplePointTag))
            {
                if (!Physics.Linecast(camera.position, col.transform.position, ~whatIsEnemy & ~whatIsGrapplePoint & ~whatIsObstacle))
                {
                    float distToCamera = Vector3.Distance(camera.position, col.transform.position);
                    if (distToCamera < closestDistance)
                    {
                        closestDistance = distToCamera;
                        bestTarget = col.transform;
                    }
                }
            }
        }
        return bestTarget;
    }

    private void StopGrapple()
    {
        if (springJoint != null)
        {
            Destroy(springJoint);
        }
        isGrapplingEnemy = false;
        isGrapplingStandard = false;
        grappledTargetTransform = null;
        lr.positionCount = 0;
        lr.enabled = false;

        _drawAutoAimGizmo = false;
        _lastAutoAimHitPoint = Vector3.zero;
    }

    public bool IsGrappling()
    {
        return isGrapplingEnemy || isGrapplingStandard;
    }

    public bool IsGrapplingEnemy()
    {
        return isGrapplingEnemy;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }

    private void OnDrawGizmos()
    {
        // Dessine la corde du grappin si active
        if (IsGrappling())
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(grapplePoint, 0.5f);
            Gizmos.DrawLine(gunTip.position, grapplePoint);
        }

        // Dessine la sphère d'auto-aim au dernier point d'impact si active
        if (_drawAutoAimGizmo && _lastAutoAimHitPoint != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_lastAutoAimHitPoint, autoAimSearchRadius); // Pour grapple points
            Gizmos.DrawWireSphere(_lastAutoAimHitPoint, enemyAutoAimSearchRadius); // Pour ennemis (peut se superposer)

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(camera.position, _lastAutoAimHitPoint);
        }
    }
}