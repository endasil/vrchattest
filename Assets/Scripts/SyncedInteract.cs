
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedInteract : UdonSharpBehaviour
{
    [UdonSynced] bool ShouldBeActive = true;
    [SerializeField] private GameObject toggleObject;
    [SerializeField] private TextMeshPro infoBox;

    int numberOfDeserializations = 0;
    int numberOfSuccesses = 0;
    private int numberOfFailures = 0;
    private int lastByteCount = 0;
    private int byteCount = 0;

    private string newLine = "\n";

    public override void Interact()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        ShouldBeActive = !ShouldBeActive;

        toggleObject.SetActive(ShouldBeActive);
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        toggleObject.SetActive(ShouldBeActive);
        numberOfDeserializations++;
    }

    public override void OnPostSerialization(SerializationResult result)
    {
        if (result.success) 
            numberOfSuccesses++;
        else
        {
            numberOfFailures++;
        }

        lastByteCount = result.byteCount;
        byteCount += result.byteCount;
    }

    private void Update()
    {
        string outputText = "";
        VRCPlayerApi owner = Networking.GetOwner(gameObject);
        outputText += "Owner:" + owner.playerId + ":" + owner.displayName;
        if (owner == Networking.LocalPlayer)
        {
            outputText += " (You)";
        }

        outputText += newLine;
        outputText += "Number of deserializations (local) = " + numberOfDeserializations + newLine;
        outputText += "Number of successes = " + numberOfSuccesses + newLine;
        outputText += "Number of failures = " + numberOfFailures + newLine;
        outputText += "Last byte count =" + numberOfFailures + newLine;
        outputText += "Byte count = " + byteCount + newLine;

        infoBox.text = outputText;
    }
}
