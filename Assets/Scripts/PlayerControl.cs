using UnityEngine;

public class PlayerControl : MonoBehaviour {

    public Animator animator;
    public Rigidbody2D rigidbody;
    public Transform groundCheck;
    public LayerMask groundMask;
    public Rigidbody2D bullet;
    public GameObject scoper;

    private float jumpDirection;
    private float direction;

    private int speed = 20;
    private int jumpSpeed = 100;

    private bool isFacingRight = true;
    private bool attacks;

    private float scoperLen = 3;
    private float scoperAng = 0;
    private float scopeRotSpeed = 1;
    private float scopeMoveSpeed = 0.1f;
    private float scopeMaxLen = 50;
    private float fireForce = 100000;
    private float playerHeight;
    private const float angleRealtion = 0.0174533f;

    private Rigidbody2D curSlot, slot1, slot2, slot3;
    public Rigidbody2D weapon1, weapon2, weapon3;
    public Rigidbody2D scope;

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
        scoper = Instantiate(scope.gameObject, new Vector2(0, 0), Quaternion.identity);
        slot1 = weapon1; slot2 = weapon2; slot3 = weapon3;
        curSlot = slot1;
    }

    void FixedUpdate() {
        handleInput();
        handleState();
        aiming();


    }

    private void handleInput() {
        movement();
        aiming();
    }

    private void handleState() {
        animator.SetInteger("baseState", (int)baseState);
        animator.SetInteger("additionalState", (int)additionalState);
        animator.SetBool("attacks", attacks);

        handleAdditionalState();
        handleBaseState();

        rigidbody.velocity = moveVector;
        if (nearTheWall(isGrounded()) && !isGrounded())
            rigidbody.AddForce(Vector2.down * 1000);
    }

    private void handleAdditionalState() {
        if (additionalState == AdditionalState.JUMP)
            handleJump();
    }

    private void handleJump() {
        jumpDirection = direction;
        if (isGrounded() && !attacks) 
            moveVector.y = jumpSpeed;
        else
            moveVector.y = 0;
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

    bool isGrounded()
    {
        RaycastHit2D hit_c = Physics2D.Raycast(rigidbody.position, Vector2.down, playerHeight / 2.0f, groundMask);
        RaycastHit2D hit_r = Physics2D.Raycast(rigidbody.position + new Vector2(GetComponent<SpriteRenderer>().size.x, 0) / 2.0f, Vector2.down, playerHeight / 2.0f, groundMask);
        RaycastHit2D hit_l = Physics2D.Raycast(rigidbody.position - new Vector2(GetComponent<SpriteRenderer>().size.x, 0) / 2.0f, Vector2.down, playerHeight / 2.0f, groundMask);

        if ((hit_c.collider != null) || (hit_l.collider != null) || (hit_r.collider != null))
            return true;
        return false;
    }

    bool nearTheWall(bool isGr)
    {
        RaycastHit2D hit_r = Physics2D.Raycast(rigidbody.position, Vector2.right, GetComponent<SpriteRenderer>().size.x* transform.localScale.x / 2f, groundMask);
        RaycastHit2D hit_l = Physics2D.Raycast(rigidbody.position, Vector2.left, GetComponent<SpriteRenderer>().size.x* transform.localScale.x / 2f, groundMask);
        if ((hit_l.collider != null) || (hit_r.collider != null))
        {
            return true;
        }
        return false;
    }

    void aiming()
    {

        if (Input.GetKeyDown(KeyCode.F))
            attacks = true;

        if (isFacingRight)
            scoper.transform.position = new Vector2(this.rigidbody.transform.position.x + scoperLen * Mathf.Cos(scoperAng * angleRealtion), this.rigidbody.transform.position.y + scoperLen * Mathf.Sin(scoperAng * angleRealtion));
        if (!isFacingRight)
            scoper.transform.position = new Vector2(rigidbody.transform.position.x - scoperLen * Mathf.Cos(scoperAng * angleRealtion), rigidbody.transform.position.y + scoperLen * Mathf.Sin(scoperAng * angleRealtion));
        if ((Input.GetKey(KeyCode.Q) && (scoperAng < 90)))
            scoperAng+=scopeRotSpeed;
        if ((Input.GetKey(KeyCode.E) && (scoperAng > -90)))
            scoperAng-=scopeRotSpeed;
        if ((Input.GetKey(KeyCode.LeftBracket) && (scoperLen < scopeMaxLen)))
            scoperLen += scopeMoveSpeed;
        if ((Input.GetKey(KeyCode.RightBracket) && (scoperLen > 1)))
            scoperLen -= scopeMoveSpeed;


        if (Input.GetKeyDown(KeyCode.Alpha1))
            curSlot = slot1;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            curSlot = slot2;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            curSlot = slot3;

        int bulletDirection = isFacingRight ? 1 : -1;

        if (Input.GetKeyDown(KeyCode.G))
            Instantiate(curSlot, new Vector2(this.transform.position.x + bulletDirection * playerHeight/2.0f, this.transform.position.y), Quaternion.identity)
                .AddForce((Vector2.up * Mathf.Sin(scoperAng * angleRealtion) + bulletDirection * Vector2.right * Mathf.Cos(scoperAng * angleRealtion)) * fireForce);
      
    }

    void movement()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            baseState = BaseState.MOVE;
        else
            baseState = BaseState.IDLE;

        if (Input.GetKeyDown(KeyCode.Space) || !isGrounded())
            additionalState = AdditionalState.JUMP;
        else if (Input.GetKey(KeyCode.LeftControl) && isGrounded())
            additionalState = AdditionalState.SIT;
        else if (isGrounded())
            additionalState = AdditionalState.NONE;

    }
}