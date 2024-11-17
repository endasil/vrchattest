
using UdonSharp;
using UnityEngine;

public class LocalInteract : UdonSharpBehaviour
{
    [SerializeField] private GameObject objectToToggle;
    public override void Interact()
    {
        Debug.Log("Interact");
        objectToToggle.SetActive(!objectToToggle.activeSelf);
    }

}
