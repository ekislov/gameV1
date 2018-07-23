using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;

public class PlayerControl : MonoBehaviour {

    public float speed = 20f;
    public int hp = 100;
    public static Rigidbody2D rb;
    public Rigidbody2D Fire;
    public Rigidbody2D Water;
    public Rigidbody2D Wind;
    private Rigidbody2D Bullet;
    private bool faceRight = true;


	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        Thread regen = new Thread(regener);
        Thread gravity = new Thread(graver);
        regen.Start();
        gravity.Start();
        Bullet = Fire;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(hp<=0)
        {
            SceneManager.LoadScene(0);
        }

        //selector of weapon
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            Bullet = Fire;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Bullet = Water;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Bullet = Wind;
        }

        float moveX = Input.GetAxis("Horizontal");
        rb.MovePosition(rb.position + Vector2.right * moveX * speed * Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            rb.MovePosition(rb.position + Vector2.left * 5);
            if (!faceRight) { flip(); }
        }       

        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            rb.MovePosition(rb.position + Vector2.right * 5);
            if (faceRight) { flip(); }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector2.up * 10000);
        }

        if (moveX < 0 && faceRight)
        {
            flip();
        }
        if (moveX > 0 && !faceRight)
        {
            flip();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            transform.localScale = new Vector3(transform.localScale.x, (float)(transform.localScale.y / 1.5), transform.localScale.z);
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            transform.localScale = new Vector3(transform.localScale.x, (float)(transform.localScale.y * 1.5), transform.localScale.z);
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (faceRight)
            {
                Instantiate(Bullet, new Vector2(rb.transform.position.x+(float)1, rb.transform.position.y+(float)0.5), Quaternion.identity).AddForce(Vector2.right * 800);
            }
            if (!faceRight)
            {
                Instantiate(Bullet, new Vector2(rb.transform.position.x-(float)1, rb.transform.position.y+(float)0.5), Quaternion.identity).AddForce(Vector2.left * 800);
            }

        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            Destroy(other);
            Destroy(Fire.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        hp -= 1;
    }

    private void flip()
    {
        faceRight = !faceRight;
        rb.transform.localScale = new Vector2(rb.transform.localScale.x * -1, rb.transform.localScale.y);
    }
    public void regener()
    {
        while (true)
        {
            hp++;
            Thread.Sleep(750);
        }
    }

    public static void graver()
    {
        int acsel = 9;
        int speed = 0;
        if (isGrounded(rb))
        {
            speed = 0;
        }
        else
        {
            rb.AddForce(Vector2.down * speed);
            speed += acsel;
            Thread.Sleep(100);
        }
    }

    public static bool isGrounded(Rigidbody2D rigidbody2D)
    {
        return false;
    }
}
