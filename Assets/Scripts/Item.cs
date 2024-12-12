using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
        {
            Destroy(gameObject); 
        }
        else if (other.CompareTag("Player2"))
        {
            Destroy(gameObject); 
        }
        
    }
}