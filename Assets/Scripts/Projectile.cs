using System;
using System.Collections;
using System.Collections.Generic;

using UdonSharp;

using UnityEngine;

using VRC.Core.Pool;
using VRC.SDK3.Components;
using VRC.SDKBase;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public abstract class Projectile : UdonSharpBehaviour
{
    public float speed = 10f;
 [SerializeField] private Rigidbody _rb;
    private VRCObjectPool _projectilePool;
    private float lifeTime = 5f;

    // Synced variables to ensure direction is shared across the network
    [UdonSynced] private Vector3 syncedDirection;

    public void Awake()
    {
        if (!_rb)
        {
            Debug.Log($"!_rb calling GetComponent<Rigidbody>();");

            _rb = GetComponent<Rigidbody>();
        }
    }

    [System.NonSerialized]
    public int Id = -999;

    
    public void Setup()
    {
        Debug.Log($"Setup called");

        if (Id < 0)
        {
            Debug.Log($"Id < 0 CallingGetSiblingIndex");

            Id = transform.GetSiblingIndex();
        }


    }
    public void Initialize(Vector3 direction, VRCObjectPool pool)
    {
        Debug.Log($"Initialize on projectile {Id} called");
        SendCustomEventDelayedSeconds(nameof(ReturnToPool), 5f);

        if (Id < 0)
            Setup();

        Debug.Log($"Setup done on projectile. Setting velocity and pool");

        // Set the synced direction to synchronize across the network
        syncedDirection = direction;
        _projectilePool = pool;
        ApplyVelocity();

        Debug.Log($"SendCustomEventDelayedSeconds(nameof(ReturnToPool), 5f);");
        // Automatically deactivate and return to the pool after a duration or upon collision

        Debug.Log($"Calling RequestSerialization from projectile initialize.");

        RequestSerialization();
    }

    public void NetworkInitalize()
    {
         // Ensure that _rb is assigned, even on other clients
        if (_rb == null)
        {
            Debug.LogWarning("_rb was null in OnDeserialization, assigning it now.");
            _rb = GetComponent<Rigidbody>();
        }
        ApplyVelocity();

    }
    public override void OnDeserialization()
    {
        Debug.Log($"On OnDeserialization");
        NetworkInitalize();
        // Apply the velocity when the object is deserialized on other clients
    }

    private void ApplyVelocity()
    {
        if (_rb != null)
        {
            _rb.velocity = syncedDirection * speed;
        }
        else
        {
            Debug.LogError("No rigidbody found on projectile");
        }
    }
    public void ReturnToPool()
    {
        _rb.velocity = Vector3.zero;
        _projectilePool.Return(this.gameObject);
    }

}
