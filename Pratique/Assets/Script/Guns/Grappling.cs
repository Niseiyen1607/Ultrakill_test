using SmallHedge.SoundManager;
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
    [SerializeField] private Rigidbody playerRb;

    [SerializeField] private float maxDistance = 100f;

    [Header("Enemy Grapple Settings")]
    [SerializeField] private float enemyGrappleSpeed = 30f;
    [SerializeField] private float enemyGrappleMinDistance = 5f;
    [SerializeField] private float enemyGrappleBoostForce = 15f;
    [SerializeField] private float enemyGrappleVerticalBoost = 5f;
    [SerializeField] private float enemyAutoAimSearchRadius = 5f;
    [SerializeField] private float enemyAutoAimMaxDistance = 70f;
    [SerializeField] private float enemyGrappleDistanceAttack = 1f;

    [Header("Standard Spring Settings")]
    [SerializeField] private float springJointSpring = 4.5f;
    [SerializeField] private float springJointDamper = 7f;
    [SerializeField] private float springJointMassScale = 4.5f;
    [SerializeField] private float springJointMinDistanceMultiplier = 0.25f;
    [SerializeField] private float springJointMaxDistanceMultiplier = 0.8f;

    [Header("Enemy Spring Settings")]
    [SerializeField] private float enemySpringJointSpring = 100f;
    [SerializeField] private float enemySpringJointDamper = 10f;
    [SerializeField] private float enemySpringJointMassScale = 1f;
    [SerializeField] private float enemySpringJointMinDistanceMultiplier = 0f;
    [SerializeField] private float enemySpringJointMaxDistanceMultiplier = 0.2f;

    [Header("Grapple Point Auto-Aim")]
    [SerializeField] private float autoAimSearchRadius = 3f;
    [SerializeField] private float autoAimMaxDistance = 50f;

    private SpringJoint springJoint;
    private bool isGrapplingEnemy = false;
    private bool isGrapplingStandard = false;
    private Transform grappledTargetTransform;

    private Vector3 _lastAutoAimHitPoint = Vector3.zero;
    private bool _drawAutoAimGizmo = false;

    private bool playedPullSound = false;
    private bool hasPreAttacked = false;
    private bool wasGrapplingBeforeStop = false;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
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

        if (meleeWeapon != null && meleeWeapon.isAttacking)
        {
            lr.enabled = false;
            animator.SetBool("IsGrappling", false);
        }
    }

    private void FixedUpdate()
    {
        if (isGrapplingEnemy && grappledTargetTransform != null)
        {
            if (!playedPullSound)
            {
                SoundManager.PlaySound(SoundType.GRAPPLE_HOOK_PULL);
                playedPullSound = true;
            }

            grapplePoint = grappledTargetTransform.position;

            Vector3 directionToGrapplePoint = (grapplePoint - player.position).normalized;
            playerRb.AddForce(directionToGrapplePoint * enemyGrappleSpeed, ForceMode.Acceleration);

            float distanceToEnemy = Vector3.Distance(player.position, grapplePoint);

            if (distanceToEnemy < enemyGrappleMinDistance + enemyGrappleDistanceAttack && !hasPreAttacked)
            {
                meleeWeapon.PerformMeleeAttack();
                meleeWeapon.GrappingAnimation();
                hasPreAttacked = true;
            }

            if (distanceToEnemy < enemyGrappleMinDistance)
            {
                Vector3 horizontalBounceDirection = camera.forward;
                horizontalBounceDirection.y = 0;
                horizontalBounceDirection.Normalize();

                Vector3 totalBounceForce = horizontalBounceDirection * enemyGrappleBoostForce;
                totalBounceForce += Vector3.up * enemyGrappleVerticalBoost;

                playerRb.AddForce(totalBounceForce, ForceMode.Impulse);

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
        wasGrapplingBeforeStop = IsGrappling();
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
                Transform enemyGrapplePoint = hit.transform.Find("EnemyGrapplePoint");

                if (enemyGrapplePoint != null)
                {
                    SetupEnemySpringJoint();
                    bestTarget = enemyGrapplePoint;
                }
                else
                {
                    bestTarget = hit.transform; 
                }

                playerRb.useGravity = false;
            }
            else
            {
                bestTarget = FindBestEnemyForGrapple(hit.point);
                if (bestTarget != null)
                {
                    Debug.Log($"Meilleur ennemi trouvé via auto-aim: {bestTarget.name}");
                }
                else
                {
                    Debug.Log("Aucun ennemi trouvé via auto-aim après Raycast sur obstacle.");
                }
            }

            if (bestTarget != null)
            {
                SoundManager.PlaySound(SoundType.GRAPPLE_HOOK);
                SoundManager.PlaySound(SoundType.GRAPPLE_HOOK_ATTACH);

                grapplePoint = bestTarget.position;
                grappledTargetTransform = bestTarget;
                isGrapplingEnemy = true;
                isGrapplingStandard = false;

                lr.positionCount = 2;
                lr.enabled = true;

                playerMovement.overrideGravity = true;
                playerRb.useGravity = false;

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
                SoundManager.PlaySound(SoundType.GRAPPLE_HOOK);
                SoundManager.PlaySound(SoundType.GRAPPLE_HOOK_ATTACH);

                grapplePoint = bestGrapplePoint.position;
                grappledTargetTransform = bestGrapplePoint;
                isGrapplingStandard = true;
                isGrapplingEnemy = false;
                SetupSpringJoint();
                return;
            }

        }

        playedPullSound = false;
    }

    private void StopGrapple()
    {
        hasPreAttacked = false;

        if (IsGrappling() || wasGrapplingBeforeStop)
        {
            SoundManager.PlaySound(SoundType.GRAPPLE_HOOK_RELEASE);
        }

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
        playedPullSound = false;
        _lastAutoAimHitPoint = Vector3.zero;
        wasGrapplingBeforeStop = false;

        playerMovement.overrideGravity = false;
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

    private void SetupEnemySpringJoint()
    {
        springJoint = player.gameObject.AddComponent<SpringJoint>();
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.connectedAnchor = grapplePoint;

        float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
        springJoint.maxDistance = distanceFromPoint * enemySpringJointMaxDistanceMultiplier;
        springJoint.minDistance = distanceFromPoint * enemySpringJointMinDistanceMultiplier;

        springJoint.spring = enemySpringJointSpring;
        springJoint.damper = enemySpringJointDamper;
        springJoint.massScale = enemySpringJointMassScale;

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

                    Transform enemyCenterPoint = col.transform.Find("EnemyGrapplePoint");

                    if (enemyCenterPoint != null)
                        bestTarget = enemyCenterPoint;
                    else
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
        if (IsGrappling())
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(grapplePoint, 0.5f);
            Gizmos.DrawLine(gunTip.position, grapplePoint);
        }

        if (_drawAutoAimGizmo && _lastAutoAimHitPoint != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_lastAutoAimHitPoint, autoAimSearchRadius);
            Gizmos.DrawWireSphere(_lastAutoAimHitPoint, enemyAutoAimSearchRadius);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(camera.position, _lastAutoAimHitPoint);
        }
    }
}