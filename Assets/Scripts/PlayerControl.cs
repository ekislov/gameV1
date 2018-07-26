using UnityEngine;

public class PlayerControl : MonoBehaviour {

    public Animator animator;
    public Rigidbody2D rigidbody;
    public LayerMask groundMask;

    //public Rigidbody2D bullet;
    //public GameObject scoper;

    private float jumpDirection;
    private float direction;

    public float playerHeight;
    public float playerWidth;

    private int speed = 20;
    //private int jumpSpeed = 100;

    private bool isFacingRight = true;
    private bool attacks;

    private int jumpForce = 2000;

    public bool isGrounded;

    public float dist;

    //private float scoperLen = 3;
    //private float scoperAng = 0;
    //private float scopeRotSpeed = 1;
    //private float scopeMoveSpeed = 0.1f;
    //private float scopeMaxLen = 50;
    //private float fireForce = 100000;
    //private const float angleRealtion = 0.0174533f;

    //private Rigidbody2D curSlot, slot1, slot2, slot3;
    //public Rigidbody2D weapon1, weapon2, weapon3;
    //public Rigidbody2D scope;

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

        //scoper = Instantiate(scope.gameObject, new Vector2(0, 0), Quaternion.identity);
        //slot1 = weapon1; slot2 = weapon2; slot3 = weapon3;
        //curSlot = slot1;
    }

    void FixedUpdate() {
        checkGrounded();

        playerWidth =  System.Math.Abs(GetComponent<SpriteRenderer>().bounds.size.x / transform.lossyScale.x);
        //GetComponent<BoxCollider2D>().size = new Vector2(playerWidth, GetComponent<BoxCollider2D>().size.y);

        handleInput();
        handleState();
    }

    private void handleInput() {
        handleMoveInput();
        //handleWeaponInput();
    }

    private void handleState() {
        animator.SetInteger("baseState", (int)baseState);
        animator.SetInteger("additionalState", (int)additionalState);
        animator.SetBool("attacks", attacks);

        handleAdditionalState();
        handleBaseState();

        moveVector.y = rigidbody.velocity.y;
        rigidbody.velocity = moveVector;
        //if (nearTheWall() && !isGrounded())
            //rigidbody.AddForce(Vector2.down * 1000);
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

    void checkGrounded()
    {

        //DEBUG
        Debug.DrawRay(rigidbody.position - new Vector2(1.0f, 2.17f), Vector2.right * 2.0f, Color.green);

        Debug.DrawRay(rigidbody.position + new Vector2(playerWidth, playerHeight) / 2.0f, Vector2.down * playerHeight, Color.green);
        Debug.DrawRay(rigidbody.position + new Vector2(-playerWidth, playerHeight) / 2.0f, Vector2.down * playerHeight, Color.green);

        bool hit_b = Physics2D.Raycast(rigidbody.position - new Vector2(1.0f, 2.17f), Vector2.right * 2.0f, groundMask);
        //RaycastHit2D hit_r = Physics2D.Raycast(rigidbody.position + new Vector2(playerWidth, playerHeight) / 2.0f,
        //                                       Vector2.down * playerHeight, groundMask);
        //RaycastHit2D hit_l = Physics2D.Raycast(rigidbody.position + new Vector2(-playerWidth, playerHeight) / 2.0f,
                                               //Vector2.down * playerHeight, groundMask);

        isGrounded = hit_b;
    }

    //bool nearTheWall()
    //{
    //    RaycastHit2D hit_r = Physics2D.Raycast(rigidbody.position, Vector2.right, playerWidth / 2.0f, groundMask);
    //    RaycastHit2D hit_l = Physics2D.Raycast(rigidbody.position, Vector2.left, playerWidth / 2.0f, groundMask);

    //    if ((hit_l.collider != null) || (hit_r.collider != null))
    //        return true;
        
    //    return false;
    //}

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

    //void handleWeaponInput()
    //{
    //    if (Input.GetKeyDown(KeyCode.F))
    //        attacks = true;

    //    if (isFacingRight)
    //        scoper.transform.position = new Vector2(this.rigidbody.transform.position.x + scoperLen * Mathf.Cos(scoperAng * angleRealtion), this.rigidbody.transform.position.y + scoperLen * Mathf.Sin(scoperAng * angleRealtion));
    //    if (!isFacingRight)
    //        scoper.transform.position = new Vector2(rigidbody.transform.position.x - scoperLen * Mathf.Cos(scoperAng * angleRealtion), rigidbody.transform.position.y + scoperLen * Mathf.Sin(scoperAng * angleRealtion));
    //    if ((Input.GetKey(KeyCode.Q) && (scoperAng < 90)))
    //        scoperAng+=scopeRotSpeed;
    //    if ((Input.GetKey(KeyCode.E) && (scoperAng > -90)))
    //        scoperAng-=scopeRotSpeed;
    //    if ((Input.GetKey(KeyCode.LeftBracket) && (scoperLen < scopeMaxLen)))
    //        scoperLen += scopeMoveSpeed;
    //    if ((Input.GetKey(KeyCode.RightBracket) && (scoperLen > 1)))
    //        scoperLen -= scopeMoveSpeed;


    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //        curSlot = slot1;
    //    if (Input.GetKeyDown(KeyCode.Alpha2))
    //        curSlot = slot2;
    //    if (Input.GetKeyDown(KeyCode.Alpha3))
    //        curSlot = slot3;

    //    int bulletDirection = isFacingRight ? 1 : -1;

    //    if (Input.GetKeyDown(KeyCode.G))
    //        Instantiate(curSlot, new Vector2(this.transform.position.x + bulletDirection * playerHeight/2.0f, this.transform.position.y), Quaternion.identity)
    //            .AddForce((Vector2.up * Mathf.Sin(scoperAng * angleRealtion) + bulletDirection * Vector2.right * Mathf.Cos(scoperAng * angleRealtion)) * fireForce);
    //}
}