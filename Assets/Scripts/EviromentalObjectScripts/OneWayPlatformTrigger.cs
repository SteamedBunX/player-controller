using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatformTrigger : MonoBehaviour
{
    OneWayPlatformController controller;

    bool isSameObject(int instanceId, GameObject other)
    {
        return instanceId == other.GetInstanceID();
    }

    void OnTriggerEnter(Collider other)
    {
        controller.gameObjectEnter(other.gameObject);
    }


    void OnTriggerExit(Collider other)
    {
        controller.gameObjectExit(other.gameObject);
    }

    public void setController(OneWayPlatformController controller)
    {
        this.controller = controller;
    }
}