using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy : MonoBehaviour {
    public Rigidbody2D enemyO;
     public int hp;

    // Use this for initialization
    void Start()
    {
        hp = 100;
        enemyO = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        if (hp <= 0)
        Destroy(enemyO.gameObject);

    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Bullet")
        {
            if (other.gameObject.name == "water(Clone)")
                hp -= 40;
            if (other.gameObject.name == "fire(Clone)")
                hp -= 20;
            Destroy(other.gameObject);
        }
    }


}
