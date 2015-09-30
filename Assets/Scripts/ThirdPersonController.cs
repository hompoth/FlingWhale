using UnityEngine;
using System.Collections;

public enum CharacterState
{
    Idle = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4,
}

public class ThirdPersonController : MonoBehaviour
{
	
	public AnimationClip upAnimation;
	public AnimationClip downAnimation;
    public AnimationClip leftAnimation;
    public AnimationClip rightAnimation;

    private Animator _animation;

    public CharacterState _characterState;

	public GameObject[] lights;

    public float walkSpeed = 2.0f;
    public float trotSpeed = 4.0f;
    public float runSpeed = 6.0f;

    public float inAirControlAcceleration = 3.0f;

    public float jumpHeight = 0.5f;

    public float gravity = 20.0f;
    public float speedSmoothing = 10.0f;
    public float trotAfterSeconds = 3.0f;

    public bool canJump = false;

    private float jumpRepeatTime = 0.05f;
    private float jumpTimeout = 0.15f;

    private Vector3 moveDirection = Vector3.zero;
    private float verticalSpeed = 0.0f;
    private float moveSpeed = 0.0f;

    private CollisionFlags collisionFlags;

    private bool jumping = false;
    private bool jumpingReachedApex = false;

    private bool isMoving = false;
    private float walkTimeStart = 0.0f;
    private float lastJumpButtonTime = -10.0f;
    private float lastJumpTime = -1.0f;
    private Vector3 inAirVelocity = Vector3.zero;

	public bool controlLight = true;
    public bool isControllable = true;

    void Awake()
	{
		lights = GameObject.FindGameObjectsWithTag ("Light");

        moveDirection = transform.TransformDirection(Vector3.forward);

        _animation = GetComponent<Animator>();
        if (!_animation)
            Debug.Log("The character you would like to control doesn't have animations.");

        if (!upAnimation)
        {
            _animation = null;
			Debug.Log("No up animation found. Turning off animations.");
        }
        if (!downAnimation)
        {
            _animation = null;
			Debug.Log("No down animation found. Turning off animations.");
        }
        if (!leftAnimation)
        {
            _animation = null;
			Debug.Log("No left animation found. Turning off animations.");
        }
        if (!rightAnimation && canJump)
        {
            _animation = null;
			Debug.Log("No right animation found and the character has canJump enabled. Turning off animations.");
        }
		transform.eulerAngles = new Vector3 (80f, 0f, 0f);
	}

    private Vector3 lastPos;

    void UpdateSmoothedMovementDirection()
    {
        Transform cameraTransform = Camera.main.transform;
        bool grounded = IsGrounded();
	
        Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;
        Vector3 right = new Vector3(forward.z, 0, -forward.x);

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");
		CharacterController controller = GetComponent<CharacterController>();
	
		float h2 = controller.velocity.x;
		float v2 = controller.velocity.z;

		if(h2>0) _characterState = CharacterState.Right;
		else if(h2<0) _characterState = CharacterState.Left;
		if(v2>0.1) _characterState = CharacterState.Up;
		else if(v2<-0.1) _characterState = CharacterState.Down;
		if(v2==0 && h2==0) _characterState = CharacterState.Idle; 

        isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        // Target direction relative to the camera
        Vector3 targetDirection = h * right + v * forward;

        // Grounded controls
        if (grounded)
        {

            if (targetDirection != Vector3.zero)
            {
                    moveDirection = targetDirection.normalized;
            }

            float curSmooth = speedSmoothing * Time.deltaTime;

            float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);

            if (Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift))
            {
                targetSpeed *= runSpeed;
            }
            else if (Time.time - trotAfterSeconds > walkTimeStart)
            {
                targetSpeed *= trotSpeed;
            }
            else
            {
                targetSpeed *= walkSpeed;
            }
            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);

            if (moveSpeed < walkSpeed * 0.3f)
                walkTimeStart = Time.time;
        }
        else
        {

            if (isMoving)
                inAirVelocity += targetDirection.normalized * Time.deltaTime * inAirControlAcceleration;
        }
    }
	public void setSpeed(){
		moveSpeed = trotSpeed;
	}
	public void setDirection(Vector3 dir){
		moveDirection = dir;
		moveDirection = moveDirection.normalized;
	}
    void ApplyJumping()
    {
        // Prevent jumping too fast after each other
        if (lastJumpTime + jumpRepeatTime > Time.time)
            return;

        if (IsGrounded())
        {	
            if (canJump && Time.time < lastJumpButtonTime + jumpTimeout)
            {
                verticalSpeed = CalculateJumpVerticalSpeed(jumpHeight);
                SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    void ApplyGravity()
    {
        if (isControllable)	// don't move player at all if not controllable.
        {
            if (jumping && !jumpingReachedApex && verticalSpeed <= 0.0f)
            {
                jumpingReachedApex = true;
                SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
            }

            if (IsGrounded())
                verticalSpeed = 0.0f;
            else
                verticalSpeed -= gravity * Time.deltaTime;
        }
    }

    float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * targetJumpHeight * gravity);
    }

    void DidJump()
    {
        jumping = true;
        jumpingReachedApex = false;
        lastJumpTime = Time.time;
        //lastJumpStartHeight = transform.position.y;
        lastJumpButtonTime = -10;
    }

    Vector3 velocity = Vector3.zero;

    void Update()
    {   
		if(controlLight){
			foreach (GameObject light in lights) {
				Vector3 pos = light.transform.position;
				pos.x = transform.position.x;
				pos.z = transform.position.z;
				light.transform.position = pos;
			}
		}

        if (isControllable)
        {
            if (Input.GetButtonDown("Jump"))
            {
                lastJumpButtonTime = Time.time;
            }

            UpdateSmoothedMovementDirection();

            ApplyGravity();

            ApplyJumping();


            Vector3 movement = moveDirection * moveSpeed + new Vector3(0, verticalSpeed, 0) + inAirVelocity;
            movement *= Time.deltaTime;

            // Move the controller
            CharacterController controller = GetComponent<CharacterController>();
            collisionFlags = controller.Move(movement);
        }
        velocity = (transform.position - lastPos)*25;

        if (_animation)
        {
            if (_characterState == CharacterState.Idle)
            {
				_animation.speed = 0f;
			}
			else
            {
				_animation.speed = 1f;
				if (this.isControllable && velocity.sqrMagnitude < 0.001f)
                {
                    _characterState = CharacterState.Idle;
					_animation.speed = 0f;
				}
				else
                {
                    if (_characterState == CharacterState.Up)
                    {
						_animation.CrossFade(upAnimation.name, 0f);
					}
					else if (_characterState == CharacterState.Down)
                    {
						_animation.CrossFade(downAnimation.name, 0f);
					}
					else if (_characterState == CharacterState.Left)
					{
						_animation.CrossFade(leftAnimation.name, 0f);
					}
					else if (_characterState == CharacterState.Right)
					{
						_animation.CrossFade(rightAnimation.name, 0f);
					}
					
				}
			}
		}

        if (IsGrounded())
        {
            //lastGroundedTime = Time.time;
            inAirVelocity = Vector3.zero;
            if (jumping)
            {
                jumping = false;
                SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
            }
        }

        lastPos = transform.position;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.moveDirection.y > 0.01f)
            return;
    }

    public float GetSpeed()
    {
        return moveSpeed;
    }

    public bool IsJumping()
    {
        return jumping;
    }

    public bool IsGrounded()
    {
        return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
    }

    public Vector3 GetDirection()
    {
        return moveDirection;
    }
	

    public bool IsMoving()
    {
        return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f;
    }

    public bool HasJumpReachedApex()
    {
        return jumpingReachedApex;
    }
	
}