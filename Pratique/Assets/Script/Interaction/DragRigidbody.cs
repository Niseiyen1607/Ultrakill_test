using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DragRigidbody : MonoBehaviour
{
    public float force = 600;
    public float damping = 6;
    public float distance = 15;

    public LineRenderer lr;
    public Transform lineRenderLocation;

    private Transform jointTrans;
    private float dragDepth;

    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            HandleInputBegin(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(2))
        {
            HandleInputEnd(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            HandleInput(Input.mousePosition);
        }
    }

    public void HandleInputBegin(Vector3 screenPosition)
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("No main camera found.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Interactive") && hit.rigidbody != null)
            {
                dragDepth = CameraPlane.CameraToPointDepth(Camera.main, hit.point);
                jointTrans = AttachJoint(hit.rigidbody, hit.point);

                if (lr != null)
                    lr.positionCount = 2;
            }
        }
    }

    public void HandleInput(Vector3 screenPosition)
    {
        if (jointTrans == null || Camera.main == null)
            return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        jointTrans.position = CameraPlane.ScreenToWorldPlanePoint(Camera.main, dragDepth, screenPosition);

        DrawRope();
    }

    public void HandleInputEnd(Vector3 screenPosition)
    {
        DestroyRope();

        if (jointTrans != null)
        {
            Destroy(jointTrans.gameObject);
            jointTrans = null;
        }
    }

    Transform AttachJoint(Rigidbody rb, Vector3 attachmentPosition)
    {
        if (rb == null)
            return null;

        GameObject go = new GameObject("Attachment Point");
        go.hideFlags = HideFlags.HideInHierarchy;
        go.transform.position = attachmentPosition;

        Rigidbody newRb = go.AddComponent<Rigidbody>();
        newRb.isKinematic = true;

        ConfigurableJoint joint = go.AddComponent<ConfigurableJoint>();
        joint.connectedBody = rb;
        joint.configuredInWorldSpace = true;
        joint.xDrive = NewJointDrive(force, damping);
        joint.yDrive = NewJointDrive(force, damping);
        joint.zDrive = NewJointDrive(force, damping);
        joint.slerpDrive = NewJointDrive(force, damping);
        joint.rotationDriveMode = RotationDriveMode.Slerp;

        return go.transform;
    }

    private JointDrive NewJointDrive(float force, float damping)
    {
        JointDrive drive = new JointDrive();
        drive.mode = JointDriveMode.Position;
        drive.positionSpring = force;
        drive.positionDamper = damping;
        drive.maximumForce = Mathf.Infinity;
        return drive;
    }

    private void DrawRope()
    {
        if (jointTrans == null || lr == null || lineRenderLocation == null)
            return;

        lr.SetPosition(0, lineRenderLocation.position);
        lr.SetPosition(1, jointTrans.position);
    }

    private void DestroyRope()
    {
        if (lr != null)
            lr.positionCount = 0;
    }
}
