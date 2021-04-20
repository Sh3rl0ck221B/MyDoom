using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrampolineEffect : MonoBehaviour
{
    
    private bool willBounce;
    private GameObject gameObject;
    private float jumpHeight = 10f;
    private float gravity = -9.81f;

    void Update()
    {
        if (willBounce)
        {
            CharacterController controller = gameObject.GetComponent<CharacterController>();
            
            if (controller != null)
            {
                controller.Move(transform.up * Mathf.Sqrt(jumpHeight * -2f * gravity) * Time.deltaTime);
            }
            willBounce = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Colission entered");
        
        if (other.gameObject.CompareTag("Player")) 
        {
            willBounce = true;
            gameObject = other.gameObject;
        }
    }
}
