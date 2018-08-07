using UnityEngine;


public class PlayerController : MonoBehaviour {
    
    private float gravity;
    private float jumpSpeed;
    private const float jumpHeight = 6;
    private const float jumpTime = 0.3f;
    private const float runSpeed = 10f;
    private const float groundDamping = 20f; // how fast do we change direction? higher means faster
    private const float airDamping = 5f;

    private Vector3 velocity;
    private bool isJumping;
    private float direction;

    private BaseController controller;
    private Animator animator;

    public enum BaseState {
        IDLE,
        MOVE
    }

    public enum AdditionalState {
        NONE,
        JUMP
    }

    public BaseState baseState;
    public AdditionalState additionalState;

    void Awake() {
        // init components
        animator = GetComponent<Animator>();
        controller = GetComponent<BaseController>();
        //init constants
        gravity = -2 * jumpHeight / Mathf.Pow(jumpTime, 2);
        jumpSpeed = Mathf.Abs(gravity * jumpTime);
    }

    void Update() {
        handleMovement();
    }

    private void handleMovement() {
        // no y axis velocity while stands
        if (controller.isGrounded)
            velocity.y = 0;
        // handles basic state 
        if (Input.GetKey(KeyCode.RightArrow)) {
            direction = 1;
            if (transform.localScale.x < 0f)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

            baseState = BaseState.MOVE;
        }
        else if (Input.GetKey(KeyCode.LeftArrow)) {
            direction = -1;
            if (transform.localScale.x > 0f)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

            baseState = BaseState.MOVE;
        }
        else {
            direction = 0;
            baseState = BaseState.IDLE;
        }
        // handles aditional state
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.UpArrow)) {
            isJumping = true;
            velocity.y = jumpSpeed;
            additionalState = AdditionalState.JUMP;
        }
        else if (isJumping && controller.isGrounded) {
            isJumping = false;
            additionalState = AdditionalState.NONE;
        }
        // sets state to to animator
        animator.SetInteger("baseState", (int)baseState);
        animator.SetInteger("additionalState", (int)additionalState);
        // calculates and apllies velocity
        var smoothedMovementFactor = controller.isGrounded ? groundDamping : airDamping;
        velocity.x = Mathf.Lerp(velocity.x, direction * runSpeed, Time.deltaTime * smoothedMovementFactor);
        velocity.y += gravity * Time.deltaTime;
        controller.move(velocity * Time.deltaTime);
        velocity = controller.velocity;
    }
}