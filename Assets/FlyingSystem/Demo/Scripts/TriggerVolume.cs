using UnityEngine;

public class TriggerVolume : MonoBehaviour
{
    public Manager manager;

    public int flyerId;

    void OnTriggerEnter(Collider other)
    {
        manager.possessedControllerId = flyerId;
    }
}