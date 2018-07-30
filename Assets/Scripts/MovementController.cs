using UnityEngine;

public class MovementController : MonoBehaviour {

    public Animator animator;
    public Rigidbody2D rigidbody;
    public BoxCollider2D hitbox; // on triger check if attacked
    public BoxCollider2D groundCollider; // ground collider
    public BoxCollider2D runCollider; // controls wall collisions while running
    public LayerMask groundMask;

    private float jumpDirection;
    private float direction;

    private float playerHeight;
    private float idleHeight;
    private float playerWidth;

    private int speed;
    private int crouchSpeed = 5;
    private int moveSpeed = 20;
    private int jumpForce = 2000;

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
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            baseState = BaseState.MOVE;
        else
            baseState = BaseState.IDLE;

        if ((Input.GetKeyDown(KeyCode.Space) || !isGrounded) && additionalState != AdditionalState.CROUCH)
            additionalState = AdditionalState.JUMP;
        else if (Input.GetKey(KeyCode.C) && isGrounded)
            additionalState = AdditionalState.CROUCH;
        else if (isGrounded && isPossibleStand())
            additionalState = AdditionalState.NONE;
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

        if (additionalState == AdditionalState.CROUCH)
            speed = crouchSpeed;
        else
            speed = moveSpeed;

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

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Ground" || other.gameObject.tag == "Enemy")
            isGrounded = true;     
    }

    void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.tag == "Ground" || other.gameObject.tag == "Enemy")
            isGrounded = true;
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.tag == "Ground" || other.gameObject.tag == "Enemy")
        isGrounded = false;
    }

    private void changeColliderSize() {
        playerWidth = System.Math.Abs(GetComponent<SpriteRenderer>().bounds.size.x / transform.lossyScale.x);

        hitbox.size = new Vector2(playerWidth, playerHeight);

        groundCollider.size = new Vector2(Mathf.Max(playerWidth, runCollider.size.x) * 0.99f, groundCollider.size.y);
        groundCollider.offset = new Vector2(0, -hitbox.size.y / 2);

        runCollider.size = new Vector2(runCollider.size.x, playerHeight);
    }

    private bool isPossibleStand() {
        float distance = (idleHeight / 2 + (idleHeight - playerHeight) / 2);

        RaycastHit2D hit_c = Physics2D.Raycast(rigidbody.position, Vector2.up, distance, groundMask);
        RaycastHit2D hit_r = Physics2D.Raycast(rigidbody.position + new Vector2(-playerWidth, 0) / 2.0f, Vector2.up, distance, groundMask);
        RaycastHit2D hit_l = Physics2D.Raycast(rigidbody.position + new Vector2(+playerWidth, 0) / 2.0f, Vector2.up, distance, groundMask);

        if (hit_c.collider != null || hit_r.collider != null || hit_l.collider != null)
            return false;
        return true;
    }
}