using UnityEngine;

public class PlayerControl : MonoBehaviour {

    public Animator animator;
    public Rigidbody2D rigidbody;
    public BoxCollider2D hitbox; // on triger check if attacked
    public BoxCollider2D groundCollider; // ground collider
    public BoxCollider2D runCollider; // controls wall collisions while running

    private float jumpDirection;
    private float direction;

    private float playerHeight;
    private float idleHeight;
    private float playerWidth;

    private int speed = 20;
    private int jumpForce = 2000;
    private float crouchSlowDown = 0.75f;

    private bool isFacingRight = true;
    private bool attacks;

    public bool isGrounded;

    private Vector2 moveVector = new Vector2();

    public enum BaseState {
        IDLE,
        MOVE
    }

    public enum AdditionalState {
        NONE,
        JUMP,
        CROUCH
    }

    public BaseState baseState;
    public AdditionalState additionalState;

    private void Start() {
        playerHeight = idleHeight = hitbox.size.y;
    }

    void FixedUpdate() {
        changeColliderSize();
        handleInput();
        handleState();
    }

    private void handleInput() {
        handleMoveInput();
    }

    private void handleState() {
        animator.SetInteger("baseState", (int)baseState);
        animator.SetInteger("additionalState", (int)additionalState);
        animator.SetBool("attacks", attacks);

        handleAdditionalState();
        handleBaseState();

        moveVector.y = rigidbody.velocity.y;
        rigidbody.velocity = moveVector;
    }

    private void handleAdditionalState() {
        if (additionalState == AdditionalState.JUMP)
            handleJump();
        if (additionalState == AdditionalState.CROUCH)
            handleCrouch();
        else
            playerHeight = idleHeight;
    }

    private void handleJump() {
        jumpDirection = direction;
        if (isGrounded && !attacks)
            rigidbody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Force);
    }

    private void handleCrouch() {
        playerHeight = GetComponent<SpriteRenderer>().bounds.size.y / transform.lossyScale.y;
    }

    private void handleBaseState() {
        direction = Input.GetAxis("Horizontal");
        if (additionalState == AdditionalState.JUMP && direction * jumpDirection <= 0)
            direction = 0;
        moveVector.x = direction * speed;
        if (direction > 0 && !isFacingRight || direction < 0 && isFacingRight)
            Flip();
    }

    private void Flip() {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void observeAttackEnded() {
        attacks = false;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Ground" || other.gameObject.tag == "Enemy")
            isGrounded = true;     
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Ground" || other.gameObject.tag == "Enemy")
            isGrounded = true;
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.tag == "Ground" || other.gameObject.tag == "Enemy")
        isGrounded = false;
    }

    private void handleMoveInput()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            baseState = BaseState.MOVE;
        else
            baseState = BaseState.IDLE;

        if ((Input.GetKeyDown(KeyCode.Space) || !isGrounded) && additionalState != AdditionalState.CROUCH)
            additionalState = AdditionalState.JUMP;
        else if (Input.GetKey(KeyCode.C) && isGrounded)
            additionalState = AdditionalState.CROUCH;
        else if (isGrounded)
            additionalState = AdditionalState.NONE;
    }

    private void changeColliderSize() {
        playerWidth = System.Math.Abs(GetComponent<SpriteRenderer>().bounds.size.x / transform.lossyScale.x);
        hitbox.size = new Vector2(playerWidth, playerHeight);
        groundCollider.size = new Vector2(Mathf.Max(playerWidth, runCollider.size.x) * 0.98f, groundCollider.size.y);
        groundCollider.offset = new Vector2(0, -hitbox.size.y / 2);

        runCollider.size = new Vector2(runCollider.size.x, playerHeight);
    }
}