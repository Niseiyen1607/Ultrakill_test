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

    private bool playedPullSound = false;
    private bool hasPreAttacked = false;
    private bool wasGrapplingBeforeStop = false;

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
            Debug.Log("Input: Souris clic droit enfoncé.");
            StartGrapple();
        }
        if (Input.GetMouseButtonUp(1))
        {
            Debug.Log("Input: Souris clic droit relâché.");
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
                Debug.Log("Son joué: GRAPPLE_HOOK_PULL (grappin ennemi, première fois)");
                playedPullSound = true;
            }

            grapplePoint = grappledTargetTransform.position;

            Vector3 directionToGrapplePoint = (grapplePoint - player.position).normalized;
            playerRb.AddForce(directionToGrapplePoint * enemyGrappleSpeed, ForceMode.Acceleration);

            float distanceToEnemy = Vector3.Distance(player.position, grapplePoint);

            if (distanceToEnemy < enemyGrappleMinDistance + 1f && !hasPreAttacked)
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
        Debug.Log("StartGrapple() appelé.");
        wasGrapplingBeforeStop = IsGrappling();
        StopGrapple();

        RaycastHit hit;
        LayerMask combinedEnemyAndObstacleLayer = whatIsEnemy | whatIsObstacle;

        if (Physics.Raycast(camera.position, camera.forward, out hit, enemyAutoAimMaxDistance, combinedEnemyAndObstacleLayer))
        {
            Debug.Log($"Raycast ennemi/obstacle touché à: {hit.point} sur objet: {hit.collider.name}");
            _lastAutoAimHitPoint = hit.point;
            _drawAutoAimGizmo = true;

            Transform bestTarget = null;

            if (((1 << hit.collider.gameObject.layer) & whatIsEnemy) != 0)
            {
                Debug.Log($"Raycast direct sur ennemi: {hit.transform.name}");
                Transform enemyGrapplePoint = hit.transform.Find("EnemyGrapplePoint");

                if (enemyGrapplePoint != null)
                {
                    bestTarget = enemyGrapplePoint;
                }
                else
                {
                    bestTarget = hit.transform; 
                }
            }
            else
            {
                Debug.Log($"Raycast sur obstacle ({hit.collider.name}), recherche d'ennemi proche.");
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
                Debug.Log($"Grappin accroché à un ennemi: {bestTarget.name}.");
                SoundManager.PlaySound(SoundType.GRAPPLE_HOOK);
                Debug.Log("Son joué: GRAPPLE_HOOK (lancement, cible trouvée)");
                SoundManager.PlaySound(SoundType.GRAPPLE_HOOK_ATTACH);
                Debug.Log("Son joué: GRAPPLE_HOOK_ATTACH (ennemi)");

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
        else
        {
            Debug.Log("Raycast ennemi/obstacle n'a rien touché.");
        }

        if (Physics.Raycast(camera.position, camera.forward, out hit, autoAimMaxDistance, whatIsObstacle))
        {
            Debug.Log($"Raycast obstacle/grapple point touché à: {hit.point} sur objet: {hit.collider.name}");
            _lastAutoAimHitPoint = hit.point;
            _drawAutoAimGizmo = true;

            Transform bestGrapplePoint = FindBestGrapplePointAroundHit(hit.point);
            if (bestGrapplePoint != null)
            {
                Debug.Log($"Grappin accroché à un point de grappin: {bestGrapplePoint.name}.");
                SoundManager.PlaySound(SoundType.GRAPPLE_HOOK);
                Debug.Log("Son joué: GRAPPLE_HOOK (lancement, cible trouvée)");
                SoundManager.PlaySound(SoundType.GRAPPLE_HOOK_ATTACH);
                Debug.Log("Son joué: GRAPPLE_HOOK_ATTACH (point de grappin)");

                grapplePoint = bestGrapplePoint.position;
                grappledTargetTransform = bestGrapplePoint;
                isGrapplingStandard = true;
                isGrapplingEnemy = false;
                SetupSpringJoint();
                Debug.Log("Grappling to Auto-Aimed Grapple Point near general hit!");
                return;
            }
            else
            {
                Debug.Log("Raycast obstacle touché, mais aucun point de grappin trouvé via auto-aim.");
            }
        }
        else
        {
            Debug.Log("Raycast point de grappin n'a rien touché.");
        }

        Debug.Log("Aucune cible de grappin valide trouvée. Grappin échoué.");

        playedPullSound = false;
    }

    private void SetupSpringJoint()
    {
        Debug.Log("SpringJoint configuré.");
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

    private void StopGrapple()
    {
        hasPreAttacked = false;

        if (IsGrappling() || wasGrapplingBeforeStop)
        {
            SoundManager.PlaySound(SoundType.GRAPPLE_HOOK_RELEASE);
            Debug.Log("Son joué: GRAPPLE_HOOK_RELEASE");
        }
        else
        {
            Debug.Log("StopGrapple() appelé, mais grappin inactif. Pas de son de relâchement.");
        }


        if (springJoint != null)
        {
            Destroy(springJoint);
            Debug.Log("SpringJoint détruit.");
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
        Debug.Log("Grappin arrêté et réinitialisé.");
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