using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DieSpace : MonoBehaviour {
   

    private void OnTriggerEnter2D (Collider2D other)
    {

        SceneManager.LoadScene(0);
    }
}
