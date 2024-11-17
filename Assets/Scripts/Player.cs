using System;
using Assets.Scripts;
using Cyan.PlayerObjectPool;
using UdonSharp;

using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
#pragma warning disable IDE1006


[RequireComponent(typeof(CapsuleCollider)), UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Player : CyanPlayerObjectPoolObject
{
    public bool printDebugMessages = true;

    [Tooltip("Will automatically set event parameters on this animator for the following events:" +
             " \"OnIncreaseHealth\", \"OnDecreaseHealth\", \"OnMaxHealth\", \"OnMinHealth\", " +
             "Similar parameters will also be set for your resources, but with the resource name " +
             "in place of \"Health\"")]
    public Animator eventAnimator = null;

    public PlayerHandler playerHandler;

    // [System.NonSerialized]: This attribute is used to prevent a field from being serialized by the
    // standard .NET serialization process. In Unity and UdonSharp, this would typically mean the
    // field is not saved with the scene or prefab and is not intended to be part of the serialization
    // process for saving or network replication.

    // [UdonSynced(UdonSyncMode.None)]: In Udon and UdonSharp, the UdonSynced attribute marks a field
    // for synchronization across the network in a VRChat world, ensuring all users share the same value.
    // The parameter UdonSyncMode.None indicates that this field is not automatically synchronized by the
    // Udon networking system. This can be used for manual synchronization control or for fields that are
    // related to networked behavior but do not utilize Udon's automatic synchronization.

    [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(Health))]
    public int _health = 100;

    [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(MaxHealth))]
    public int _maxHealth = 100;

    [System.NonSerialized] public Player lastHealer = null;
    [System.NonSerialized] public Player lastAttacker = null;

    [System.NonSerialized]
    public int Id = -1001;
     
    [System.NonSerialized] 
    private VRCPlayerApi _localPlayer = null;
    
    [System.NonSerialized]
    public Player LocalPlayerObject = null;


    [HideInInspector]
    public CapsuleCollider capsuleCollider = null;
    public int Health
    { 
        get => _health;
        set
        {
            _print("set health from " + _health + " to " + value);
            if (value > _health)
            {
                if (_health >= MaxHealth)
                {
                    //prevent repeat events
                    return;
                }
                _health = value;
                if (value >= MaxHealth)
                {
                    _health = MaxHealth;
                    _OnIncreaseHealth();
                    _OnMaxHealth();
                }
                else
                {
                    _OnIncreaseHealth();
                }
            }
            else if (value < _health)
            {
                if (_health <= 0)
                {
                    //prevent repeat events
                    return;
                }
                _health = value;
                if (value <= 0)
                {
                    _health = 0;
                    _OnDecreaseHealth();
                    _OnMinHealth();
                }
                else
                {
                    _OnDecreaseHealth();
                }
            }
            if (IsOwnerLocal())
            {
                RequestSerialization();
            }
        }
    }


    [Tooltip("How tall do we assume a player is if they don't have a head bone on their avatar")]
    public float defaultHeight = 2f;

    public virtual void Start()
    {
        lastHealer = this;
        lastAttacker = this;
        if (Id < 0)
        {
            Id = transform.GetSiblingIndex();
        }

        parent = transform.parent;
        _localPlayer = Networking.LocalPlayer;
    }

    public virtual void _OnMinHealth()
    {
        if (Utilities.IsValid(eventAnimator))
        {
            eventAnimator.SetTrigger("OnMinHealth");
        }
        foreach (PlayerListenerBase listener in playerHandler.PlayerListeners)
        {
            listener.OnMinHealth(lastAttacker, this, Health);
        }
    }
    public virtual void _OnDecreaseHealth()
    {
        if (Utilities.IsValid(eventAnimator))
        {
            eventAnimator.SetTrigger("OnDecreaseHealth");
        }
        foreach (PlayerListenerBase listener in playerHandler.PlayerListeners)
        {
            listener.OnDecreaseHealth(lastAttacker, this, Health);
        }
    }


    public void OnCollisionEnter(Collision other)
    {
        Debug.Log("On collision enter"+ other.gameObject.name);
    }

    private void _OnMaxHealth()

    {
        if (Utilities.IsValid(eventAnimator))
        {
            eventAnimator.SetTrigger("OnMaxHealth");
        }
        foreach (PlayerListenerBase listener in playerHandler.PlayerListeners)
        {
            listener.OnMaxHealth(lastHealer, this, Health);
        }
    }


    private void _OnIncreaseHealth()
    {
        if (Utilities.IsValid(eventAnimator))
        {
            eventAnimator.SetTrigger("OnIncreaseHealth");
        }
        foreach (PlayerListenerBase listener in playerHandler.PlayerListeners)
        {
            listener.OnIncreaseHealth(lastHealer, this, Health);
        }
    }

    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = value;
            if (IsOwnerLocal())
            {
                RequestSerialization();
            }
        }
    }


    private const float HeadPositionRatio = 2.75f / 5f;
    private const float CapsuleColliderHeightRatio = (6f / 5f);

    public void Update()
    {
        if (!Utilities.IsValid(Owner))
        {
            return;
        }

        MatchPlayerPosition();
    }

    public void MatchPlayerPosition()
    {
        Vector3 leftFootPos = Owner.GetBonePosition(HumanBodyBones.LeftFoot);
        Vector3 rightFootPos = Owner.GetBonePosition(HumanBodyBones.RightFoot);
        Vector3 feetPos;
        
        feetPos = (leftFootPos != Vector3.zero && rightFootPos != Vector3.zero) ? 
            Vector3.Lerp(leftFootPos, rightFootPos, 0.5f) : 
            Owner.GetPosition();

        var headPos = Owner.GetBonePosition(HumanBodyBones.Head);
        headPos = headPos != Vector3.zero ? headPos : feetPos + Vector3.up * defaultHeight;

        //assuming the model is 6 heads tall, headPos is at the 5th head, so we need to add 10% (1/5th) to the height
        transform.position = Vector3.Lerp(feetPos, headPos, HeadPositionRatio);
        capsuleCollider.height = Vector3.Distance(feetPos, headPos) * CapsuleColliderHeightRatio;

        // Calculate the posture adjustment rotation, handle cases
        // where the player is lying down etc instead of standing straight
        Quaternion postureAdjustmentRotation = Quaternion.FromToRotation(Vector3.up, headPos - feetPos);

        // Apply the posture adjustment rotation to the owner's rotation
        transform.rotation = Owner.GetRotation() * postureAdjustmentRotation;
    }

    public void OnParticleCollision(GameObject other)
    {
        if (!Utilities.IsValid(other))
        {
            return;
        }
    }

    [System.NonSerialized]
    Transform parent;

    public override void _OnOwnerSet()
    {
        ResetPlayer();
        if (Owner.isLocal)
        {
            //make it skinny so it's hard to shoot yourself 
            capsuleCollider.radius = 0.1f;
            if (!Utilities.IsValid(parent))
            {
                Debug.LogError("Parent not found");
            }

            var children = parent.GetComponentsInChildren<Player>(true);
            if (!Utilities.IsValid(children))
            {
                Debug.LogError("children not found");
            }



            foreach (Player p in parent.GetComponentsInChildren<Player>(true))
            {
                p.LocalPlayerObject = this;
            }
            if (Utilities.IsValid(playerHandler))
            {
                playerHandler.LocalPlayer = this;
            }
        }
    }

    public override void _OnCleanup()
    {
        //if (Utilities.IsValid(statsAnimator))
        //{
        //    statsAnimator.gameObject.SetActive(false);
        //}
    }

    public void ResetPlayer()
    {
        ResetPlayerResources();

    }

    private void ResetPlayerResources()
    {
        _print("ResetPlayerResources");

        if (IsOwnerLocal())
        {
            Health = playerHandler.startingHealth;
        }

    }
    public bool IsOwnerLocal()
    {
        return Utilities.IsValid(Owner) && Owner.isLocal;
    }

    public void _print(string message)
    {
        if (printDebugMessages)
        {
            Debug.Log("<color=yellow>[P-Shooters Player.cs] " + name + ": </color>" + message);
        }
    }

}
