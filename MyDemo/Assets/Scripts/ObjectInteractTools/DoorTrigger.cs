
using System;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public DoorControl doorControl;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Enter!");
            doorControl.playerInRange = true;
            var collector = other.gameObject.GetComponent<Collector>();
            if (collector.CanExit())
            {
                doorControl.hasEnoughKey = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Exit!");
            doorControl.playerInRange = false;
        }
    }
}