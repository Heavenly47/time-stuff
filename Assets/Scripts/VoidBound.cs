﻿using UnityEngine;

public class VoidBound : MonoBehaviour
{
    public Vector3 shiftValue;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Platform") || col.CompareTag("Hazard"))
        {
            Rigidbody2D cRb = col.GetComponent<Rigidbody2D>();
            if (cRb != null)
            {
                if (cRb.velocity.x > 0)
                    col.transform.position -= shiftValue;
                else
                    col.transform.position += shiftValue;
            }
        }
    }
}