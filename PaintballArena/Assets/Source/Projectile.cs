using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : iGameStateListener
{
    public float ExplosionForce = 100.0f;
    public GameObject ExplosionEffect = null;

    Rigidbody rigi;
    Vector3 lastPosition = Vector3.zero;

    public LayerMask contactLayers;

    bool hasFired = false;

    bool isGameOver = false;
    bool isGamePaused = false;
   
    protected override void Start()
    {
        lastPosition = transform.position;
    }
   
    void DestroyIfAlive()
    {
        Destroy(gameObject);
    }
    public override void HandleGamePaused()
    {
        
    }

    void FixedUpdate()
    {
        if(!hasFired )
            return;
        
        if (isGameOver)
            return;

        if (isGamePaused)
            return;

        var direction = transform.position - lastPosition;
        Ray ray = new Ray(lastPosition, direction);

        Debug.DrawRay(ray.origin, ray.direction, Color.red, 1.0f);

        if (Physics.Raycast(ray, out RaycastHit hit, direction.magnitude, contactLayers))
        {
            if (isGameOver)
                return;

            if (isGamePaused)
                return;

            StartTriggerImpact(hit);
        }

        lastPosition = transform.position;
    }

    private void StartTriggerImpact(RaycastHit hit)
    {
        CancelInvoke(nameof(DestroyIfAlive));

        if (hit.collider.CompareTag("Player"))
        {
            var player = hit.collider.GetComponent<Player>();
            player.Die(); // the player cleans themselves up.

            GameManager.SignalGameOver();
            
        }
        else if (hit.collider.CompareTag("Enemy"))
        {
            var enemy = hit.collider.GetComponent<Enemy>();
            enemy.Die();
        }

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

    public override void HandleGameOver()
    {
        isGameOver = true;
    }

    public override void HandleGameUnpaused()
    {
        isGamePaused = false;
    }

    public override void HangleGameStarted()
    {
      
    }
}
