using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{

    public Transform cameraTransform;
    private Transform _target;

    // The distance in the x-z plane to the target

    public float distance = 7.0f;

    // the height we want the camera to be above the target
    public float height = 5.0f;

    public float angularSmoothLag = 0.3f;
    public float angularMaxSpeed = 15.0f;

    public float heightSmoothLag = 0.3f;

    public float snapSmoothLag = 0.2f;
    public float snapMaxSpeed = 720.0f;

    public float clampHeadPositionScreenSpace = 0.75f;

    public float lockCameraTimeout = 0.2f;

    private Vector3 headOffset = Vector3.zero;
    private Vector3 centerOffset = Vector3.zero;

    private float heightVelocity = 0.0f;
    private float angleVelocity = 0.0f;
    private bool snap = false;
    private ThirdPersonController controller;
    private float targetHeight = 100000.0f;

    void OnEnable()
    {
        if( !cameraTransform && Camera.main )
            cameraTransform = Camera.main.transform;
        if( !cameraTransform )
        {
            Debug.Log( "Please assign a camera to the ThirdPersonCamera script." );
            enabled = false;
        }

        _target = transform;
        if( _target )
        {
            controller = _target.GetComponent<ThirdPersonController>();
        }

        if( controller )
        {
            CharacterController characterController = (CharacterController)_target.GetComponent<Collider>();
            centerOffset = characterController.bounds.center - _target.position;
            headOffset = centerOffset;
            headOffset.y = characterController.bounds.max.y - _target.position.y;
        }
        else
            Debug.Log( "Please assign a target to the camera that has a ThirdPersonController script attached." );


        //Cut( _target, centerOffset );
    }

    void DebugDrawStuff()
    {
        Debug.DrawLine( _target.position, _target.position + headOffset );

    }

    float AngleDistance( float a, float b )
    {
        a = Mathf.Repeat( a, 360 );
        b = Mathf.Repeat( b, 360 );

        return Mathf.Abs( b - a );
    }

    void Apply( Transform dummyTarget, Vector3 dummyCenter )
    {
        // Early out if we don't have a target
        if( !controller )
            return;

        Vector3 targetCenter = _target.position + centerOffset;

        //	DebugDrawStuff();

        // Calculate the current & target rotation angles
        float originalTargetAngle = _target.eulerAngles.y;
        float currentAngle = cameraTransform.eulerAngles.y;

        // Adjust real target angle when camera is locked
        float targetAngle = originalTargetAngle;

        // When pressing Fire2 (alt) the camera will snap to the target direction real quick.
        // It will stop snapping when it reaches the target
        if( Input.GetButton( "Fire2" ) )
            snap = true;

        if( snap )
        {
            // We are close to the target, so we can stop snapping now!
            if( AngleDistance( currentAngle, originalTargetAngle ) < 3.0f )
                snap = false;

            currentAngle = Mathf.SmoothDampAngle( currentAngle, targetAngle, ref angleVelocity, snapSmoothLag, snapMaxSpeed );
        }
        // Normal camera motion
        else
        {
            if( controller.GetLockCameraTimer() < lockCameraTimeout )
            {
                targetAngle = currentAngle;
            }

            // Lock the camera when moving backwards!
            // * It is really confusing to do 180 degree spins when turning around.
            if( AngleDistance( currentAngle, targetAngle ) > 160 && controller.IsMovingBackwards() )
                targetAngle += 180;

            currentAngle = Mathf.SmoothDampAngle( currentAngle, targetAngle, ref angleVelocity, angularSmoothLag, angularMaxSpeed );
        }


        // When jumping don't move camera upwards but only down!
        if( controller.IsJumping() )
        {
            // We'd be moving the camera upwards, do that only if it's really high
            float newTargetHeight = targetCenter.y + height;
            if( newTargetHeight < targetHeight || newTargetHeight - targetHeight > 5 )
                targetHeight = targetCenter.y + height;
        }
        // When walking always update the target height
        else
        {
            targetHeight = targetCenter.y + height;
        }

        // Damp the height
        float currentHeight = cameraTransform.position.y;
        currentHeight = Mathf.SmoothDamp( currentHeight, targetHeight, ref heightVelocity, heightSmoothLag );

        // Convert the angle into a rotation, by which we then reposition the camera
        //Quaternion currentRotation = Quaternion.Euler( 0, currentAngle, 0 );

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
		cameraTransform.position = targetCenter;

		//cameraTransform.position += currentRotation * Vector3.back * distance;
		cameraTransform.position += Vector3.back * distance;
		
		// Set the height of the camera
        cameraTransform.position = new Vector3( cameraTransform.position.x, currentHeight, cameraTransform.position.z );
    }

    void LateUpdate()
    {
        Apply( transform, Vector3.zero );
    }
}