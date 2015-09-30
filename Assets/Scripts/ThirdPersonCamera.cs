using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{

    public Transform cameraTransform;
    private Transform _target;

    public float distance = 7.0f;

    public float height = 5.0f;

    public float heightSmoothLag = 0.3f;

    private Vector3 headOffset = Vector3.zero;
    private Vector3 centerOffset = Vector3.zero;

    private float heightVelocity = 0.0f;
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


    }

    void Apply( Transform dummyTarget, Vector3 dummyCenter )
    {
        if( !controller )
            return;

        Vector3 targetCenter = _target.position + centerOffset;

        if( controller.IsJumping() )
        {
            float newTargetHeight = targetCenter.y + height;
            if( newTargetHeight < targetHeight || newTargetHeight - targetHeight > 5 )
                targetHeight = targetCenter.y + height;
        }
        else
        {
            targetHeight = targetCenter.y + height;
        }

        float currentHeight = cameraTransform.position.y;
        currentHeight = Mathf.SmoothDamp( currentHeight, targetHeight, ref heightVelocity, heightSmoothLag );


		cameraTransform.position = targetCenter;
		cameraTransform.position += Vector3.back * distance;
        cameraTransform.position = new Vector3( cameraTransform.position.x, currentHeight, cameraTransform.position.z );

    }

    void LateUpdate()
    {
        Apply( transform, Vector3.zero );
    }
}