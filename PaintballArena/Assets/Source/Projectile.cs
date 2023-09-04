using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float ExplosionForce = 100.0f;
    public GameObject ExplosionEffect = null;

    Rigidbody rigi;
    Vector3 lastPosition = Vector3.zero;

    public List<string> IngoreTags = new List<string>();
    public LayerMask contactLayers;

    bool hasFired = false;

    void Start()
    {
        lastPosition = transform.position;
    }
   
    void DestroyIfAlive()
    {
        Destroy(gameObject);
    }
    void FixedUpdate()
    {
        if(!hasFired)
        {
            return;
        }

        var direction = transform.position - lastPosition;
        Ray ray = new Ray(lastPosition, direction);

        Debug.DrawRay(ray.origin, ray.direction, Color.red, 1.0f);

        if (Physics.Raycast(ray, out RaycastHit hit, direction.magnitude, contactLayers, QueryTriggerInteraction.Collide))
        {
            Debug.Log($"Hitting: {hit.collider.name}");
            StartTriggerImpact(hit);
        }

        lastPosition = transform.position;
    }

    private void StartTriggerImpact(RaycastHit hit)
    {
        CancelInvoke(nameof(DestroyIfAlive));

        var vfxGO = Instantiate(ExplosionEffect);
        vfxGO.transform.position = hit.point;
        vfxGO.transform.rotation = Quaternion.Euler(hit.normal);


        Destroy(vfxGO, 2.0f);

        Destroy(gameObject);
    }

    public void Fire()
    {
        rigi = GetComponent<Rigidbody>();

        rigi.AddForce(transform.forward * ExplosionForce * 10 * Time.deltaTime, ForceMode.Impulse);
        rigi.AddForce(-transform.up * 10.0f * Time.deltaTime, ForceMode.Force);

        hasFired = true;

        Invoke(nameof(DestroyIfAlive), 3.0f);
    }
}
