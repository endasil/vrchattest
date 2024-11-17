using UdonSharp;

using UnityEngine;
using UnityEngine.Serialization;

using VRC.SDK3.Components;
using VRC.SDKBase;

public class CardSpawnerScript : UdonSharpBehaviour
{
    public VRCObjectPool cardPool;

    public Transform objSpawnPoint;

    public override void Interact()
    {


        Debug.Log("Card spawned");
        cardPool.Shuffle();

        var card = cardPool.TryToSpawn();

        if (!Utilities.IsValid(card))
        {
            Debug.LogWarning("Unable to spawn card");
            return;
        }
        Networking.SetOwner(Networking.LocalPlayer, cardPool.gameObject);
        var transform1 = objSpawnPoint.transform;
        cardPool.transform.SetPositionAndRotation(transform1.position, transform1.rotation);
    }
}