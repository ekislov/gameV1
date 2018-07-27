using UnityEngine;

public class PlayerControl : MonoBehaviour {

    public Animator animator;
    public Rigidbody2D rigidbody;

    private float jumpDirection;
    private float direction;

    private float playerHeight;
    private float playerWidth;

    private int speed = 20;
    private int jumpForce = 8000;

    private bool isFacingRight = true;
    private bool attacks;
    private bool isGrounded;

    private Vector2 moveVector = new Vector2();

    private enum BaseState {
        IDLE,
        MOVE
    }

    private enum AdditionalState {
        NONE,
        JUMP,
        SIT
    }

    private BaseState baseState;
    private AdditionalState additionalState;

    private void Start() {
        playerHeight = GetComponent<SpriteRenderer>().size.y * transform.localScale.y;
    }

    void FixedUpdate() {
        playerWidth = System.Math.Abs(GetComponent<SpriteRenderer>().bounds.size.x / transform.lossyScale.x);

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

        //moveVector.y = rigidbody.velocity.y;
        rigidbody.velocity = moveVector;
    }

    private void handleAdditionalState() {
        if (additionalState == AdditionalState.JUMP)
            handleJump();
    }

    private void handleJump() {
        jumpDirection = direction;
        if (isGrounded && !attacks)
            rigidbody.AddForce(new Vector2(0, jumpForce));

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

    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("Stay");
        if (other.gameObject.tag == "Ground")
            isGrounded = true;     
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Exit");
        if (other.gameObject.tag == "Ground")
        isGrounded = false;
    }

    void handleMoveInput()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            baseState = BaseState.MOVE;
        else
            baseState = BaseState.IDLE;

        if (Input.GetKeyDown(KeyCode.Space) || !isGrounded)
            additionalState = AdditionalState.JUMP;
        else if (Input.GetKey(KeyCode.LeftControl) && isGrounded)
            additionalState = AdditionalState.SIT;
        else if (isGrounded)
            additionalState = AdditionalState.NONE;
    }
}