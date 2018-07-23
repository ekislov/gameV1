using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class enemy : MonoBehaviour
{
    
    GameObject player = GameObject.Find("player");
    public int hp = 100;
    public int count = 0;
    // Use this for initialization
    void Start()
    {
       
    }


    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.tag == "Bullet")
        {
            
            transform.localScale = new Vector3((float)(transform.localScale.x/1.1), (float)(transform.localScale.y/1.1 ), transform.localScale.z);
            Destroy(other.gameObject);
            count++;
            if (count>5)
            {
                transform.localScale = new Vector3((float)(transform.localScale.x / 150), (float)(transform.localScale.y / 150), transform.localScale.z);
                Destroy(this.gameObject);
            }

           
        }
        if (other.tag == "Player")
        {

            transform.localScale = new Vector3((float)(transform.localScale.x / 1.1), (float)(transform.localScale.y / 1.1), transform.localScale.z);
            
            count=10;
            hp -= 25;
            if (count > 5)
            {
                transform.localScale = new Vector3((float)(transform.localScale.x / 150), (float)(transform.localScale.y / 150), transform.localScale.z);
            }
        }


    }
    private void OnTriggerStay2D(Collider2D other)
    {
        other.transform.rotation *= Quaternion.Euler(0f, 50f * Time.deltaTime, 0f);
    }


private void OnTriggerExit2D(Collider2D other)
    {
        

    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {

        }
    }
}

