using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float ExplosionForce = 100.0f;
    Rigidbody rigi;

    // Start is called before the first frame update
    void Start()
    {
        rigi = GetComponent<Rigidbody>();
        
        rigi.AddForce(transform.forward * ExplosionForce * Time.deltaTime, ForceMode.Impulse);
        rigi.AddForce(-transform.up * 10.0f * Time.deltaTime, ForceMode.Force);

        Destroy(gameObject, 3.0f);

    }


    
    // Update is called once per frame
    void Update()
    {
        
    }
}
