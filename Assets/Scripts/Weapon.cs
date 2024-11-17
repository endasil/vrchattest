
using Assets.Scripts.Enums;
using MMMaellon;

using UdonSharp;

using UnityEngine;

using VRC.SDK3.Components;
using VRC.SDKBase;

using static VRC.Core.ApiAvatar;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(SmartObjectSync)), RequireComponent(typeof(Animator))]

public class Weapon : UdonSharpBehaviour
{
    public bool printDebugMessages = true;
    public int projectileDamage = 10;
    public int meleeDamage = 5;
    public bool damageOnTriggerEnter = false;
    public bool damageOnParticleCollision = true;
    public bool damageWhenNotHeld = false;
    public bool selfDamage = false;
    public bool toggleableDamage = false;
    public bool toggleOffOnDrop = true;
    public Transform projectileSpawnPoint;

    [System.NonSerialized]
    private VRCPlayerApi _localPlayer;

    [System.NonSerialized] private string _localPlayerName;

    [Header("Required Components")]
    public SmartObjectSync smartObjectSync;
    public Animator _animator;

    [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(State))]
    public WeaponState _state = WeaponState.STATE_IDLE;

    public ParticleSystem shootParticles;
    public ParticleSystem onHitPlayerParticles;
    public VRCObjectPool projectilePool;

    #region properties

    

    public WeaponState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {

                _state = value;

                if (smartObjectSync.IsLocalOwner())
                {
                    RequestSerialization();
                }
            }
            Animator.SetInteger("state", (int)_state);
            PrintDebug("STATE: " + _state);
        }
    }

    [FieldChangeCallback(nameof(ShootSpeed))]
    public float _shootSpeed = 1.0f;
    public float ShootSpeed
    {
        get => _shootSpeed;
        set
        {
            Debug.Log("Field changed shootingspeed");
            _shootSpeed = value;
            Animator.SetFloat("shoot_speed", value);
        }
    }

    public Animator Animator
    {
        get
        {
#if COMPILER_UDONSHARP || !UNITY_EDITOR
                _animator.enabled = true;
#endif
            return _animator;
        }
        set
        {
            _animator = value;
        }
    }

    #endregion // properties

    public virtual void Start()
    {
        _localPlayer = Networking.LocalPlayer;
        _localPlayerName = _localPlayer.displayName + $" ({_localPlayer.playerId})";
    }


    // Called when a player interacts with a pickup object and uses it by pressing the use button.
    // This event typically triggers when the player has grabbed the object and presses the secondary action button (often the trigger button on VR controllers).
    public override void OnPickupUseDown()
    {
        Debug.Log($"OnPickupUseDown {_localPlayerName}");
        if (State == WeaponState.STATE_IDLE) 
        {
            State = WeaponState.STATE_SHOOT;
// FireProjectile();

        }

    }

    public override void OnPickupUseUp()
    {
        State = WeaponState.STATE_IDLE;

    }

    // Only the owner of the pool can spawn an object
    // Hold right shift + § then press 3 to show logs.
     // Fireprojectile is called from an animator event
    // On the shoot animation
     public void FireProjectile()
    {
        if (!Networking.IsOwner(gameObject))
        {        
            Debug.Log("not owner of weapon");
            return;
        }
        else
        {
            Debug.Log("is Owner of weapon");
        }

        Debug.Log("Trying to take ownership of projectilepool");

        Networking.SetOwner(Networking.LocalPlayer, projectilePool.gameObject);
        Debug.Log("Done taking ownership of projectil epool");

        if (!Networking.IsOwner(projectilePool.gameObject))
        {
            Debug.LogError("Not owner of projectilePool after requesting ownership");
            return;
        }

        Debug.Log($"FireProjectile {_localPlayerName}");
        var projectile = projectilePool.TryToSpawn();
        if (!Utilities.IsValid(projectile))
        {
            Debug.LogWarning($"Unable to spawn projectile from pool {_localPlayerName}");
            return;
        }

        Debug.Log($"Projectile spawned");

        if (!Networking.IsOwner(projectile))
        {
            Networking.SetOwner(Networking.LocalPlayer, projectile);
        }

        var spawnPoint = projectileSpawnPoint.transform;
        Debug.Log($"Set position and rotation");

        projectile.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        var projectileScript = projectile.GetComponent<Projectile>();
        if(!projectileScript)
            Debug.LogError("Failed to get projectile script from projectile!");
        Debug.Log($"Initialize projectile");


        projectileScript.Initialize(spawnPoint.forward, projectilePool);
        Debug.Log($"Done initializing projectile");

    }


    public void PrintDebug(string message)
    {
        if (printDebugMessages)
        {
            Debug.Log($"<color=yellow>[{nameof(Weapon)}] " + name + ": </color>" + message);
        }
    }

}
