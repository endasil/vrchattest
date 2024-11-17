using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;
using static UnityEngine.Random;


// About continues synchronization
// It makes sense to synchronize some  variables continuously.
// However, the data is usually only received a few times per second.
// So far less often than the framerate. There are different ways to
// handle the time between.
// - The default behavior is using the UdonSyncMode None. This only
// updates the variable to the latest know state. This means that 
// the variable will jump once data is received.
//
// - The linear behavior increases the value linearly. This makes the 
// change more smoothly but still very predictable.
// 
// The smooth behavior increases the value smoothly. This makes the
// change smooth but can sometimes make the value overshoot.
// 
// About this implementation
// The owner makes the sphere leap between random points. The position
// of the sphere is sent out to the other players as different variables,
// each with their own sync behavior. The difference between behavior is
// visible for everyone who isn't the owner.
 
[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class SyncedVariables : UdonSharpBehaviour
{
    [SerializeField] private Transform movingObjectNone;
    [SerializeField] private Transform movingObjectLinear;
    [SerializeField] private Transform movingObjectSmooth;
    [SerializeField] private GameObject playerIsOwnerWarning;
    [SerializeField] private TextMeshPro debugField;
    
    [UdonSynced(UdonSyncMode.None)] Vector3 currentPositionNone;
    [UdonSynced(UdonSyncMode.Linear)] private Vector3 currentPositionLinear;
    [UdonSynced(UdonSyncMode.Smooth)] private Vector3 currentPositionSmooth;
    
    private Vector3 StartPosition;
    private Vector3 TargetPosition;
    private float transitionTime = 2;
    private float startTime = 0;

    private int deserializationCount = 0;
    private float previousDeserializationTime = 0;
    float deserializationDeltaTime = 0;
    private int byteCount = 0;

    private string newline = "\n";

    private void Start()
    {
        SetNewPosition();
    }

    private void Update()
    {
        playerIsOwnerWarning.SetActive(Networking.IsOwner(gameObject));
        if (Networking.IsOwner(gameObject))
        {
            float lerpValue = (Time.time - startTime) / transitionTime;
            if (lerpValue > 1)
            {
                SetNewPosition();
            }
            else
            {
                Vector3 position = Vector3.Lerp(a: StartPosition, b: TargetPosition, t: lerpValue);

                currentPositionNone = position;
                currentPositionLinear = position;
                currentPositionSmooth = position;
            }
        }

        SetObjectToCurrentPosition();

        string outputText = "";
        VRCPlayerApi owner = Networking.GetOwner(gameObject);
        outputText += "Owner:" + owner.playerId + ":" + owner.displayName;
        if (owner == Networking.LocalPlayer)
            outputText += "(You)";
        outputText += newline;

        outputText += "Deserialization:" + deserializationCount + newline;
        outputText += "Deserialization delta time =:" + deserializationDeltaTime + newline;
        outputText += "Position magnitude (Sync ";

    }

    private void SetNewPosition()
    {
        StartPosition = movingObjectNone.transform.localPosition;
        TargetPosition = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.5f, 0.5f), Random.Range(0.5f, 0.5f));
        startTime = Time.time;
    }

    void SetObjectToCurrentPosition()
    {
        movingObjectNone.transform.localPosition = currentPositionNone;
        movingObjectLinear.transform.localPosition = currentPositionLinear;
        movingObjectSmooth.transform.localPosition = currentPositionSmooth;
    }

    public override void Interact()
    {
        Networking.SetOwner(Networking.LocalPlayer, obj:gameObject);
    }
}
