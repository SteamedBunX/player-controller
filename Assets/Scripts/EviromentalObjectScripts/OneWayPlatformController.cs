using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatformController : MonoBehaviour
{
    public GameObject platform;
    Collider platformCollider;
    Vector3 platformSize;
    float platformLocalGroundLevel;
    public float triggerExtension = 0.4f;
    public float triggerYOffset = -0.05f;
    List<int> colliderInBottomTrigger = new List<int>();
    List<int> colliderIgnored = new List<int>();

    void Awake()
    {
        registerPlatform();
        createChildObjects();
    }

    void registerPlatform()
    {
        platform = transform.Find("Platform").gameObject;
        platformCollider = platform.GetComponent<Collider>();
        platformLocalGroundLevel = platform.transform.localPosition.y + platform.transform.localScale.y / 2;
        platformSize = platform.transform.localScale;
    }

    void createChildObjects()
    {
        createTrigger(
            new Vector3(0, platformLocalGroundLevel - (platformSize.y + triggerExtension + triggerYOffset) / 2 + triggerYOffset, 0),
            new Vector3(platformSize.x + triggerExtension * 2, platformSize.y + triggerExtension + triggerYOffset, platformSize.z + triggerExtension * 2),
            platform.transform.localRotation,
            this
        );
    }

    void createTrigger(Vector3 localPosition, Vector3 localSize, Quaternion localRotation, OneWayPlatformController controller)
    {
        GameObject trigger = new GameObject();
        Transform triggerTransform = trigger.GetComponent<Transform>();
        triggerTransform.parent = transform;
        triggerTransform.localPosition = localPosition;
        triggerTransform.localScale = localSize;
        triggerTransform.localRotation = localRotation;
        BoxCollider triggerBoxCollider = trigger.AddComponent<BoxCollider>();
        triggerBoxCollider.isTrigger = true;
        OneWayPlatformTrigger script = trigger.AddComponent<OneWayPlatformTrigger>();
        script.setController(controller);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void gameObjectEnter(GameObject go)
    {
        Physics.IgnoreCollision(go.GetComponent<Collider>(), platformCollider, true);
    }

    void OnDrawGizmos()
    {
    }

    public void gameObjectExit(GameObject go)
    {
        Physics.IgnoreCollision(go.GetComponent<Collider>(), platformCollider, false);
    }
}
