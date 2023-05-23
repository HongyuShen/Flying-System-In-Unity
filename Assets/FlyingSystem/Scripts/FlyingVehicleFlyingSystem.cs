using UnityEngine;

public class FlyingVehicleFlyingSystem : MonoBehaviour
{
    [Header("Object References")]
    public Transform rootTransform;

    public Transform springArmTransform;
    public Transform cameraTransform;

    public Transform meshTransform;

    // Flying attributes
    [HideInInspector]
    public bool isTurnedOff = false;

    [HideInInspector]
    public bool enabledFlyingLogic = true;

    [HideInInspector]
    public bool inAir = false;

    [HideInInspector]
    public bool hoverMode = false;

    [HideInInspector]
    public Vector3 flyingDirection;

    [HideInInspector]
    public float currentFlyingSpeed;

    [HideInInspector]
    public Vector3 flyingVelocity;

    [HideInInspector]
    public bool flyingInNormalSpeed = false;
    [HideInInspector]
    public bool boosting = false;
    [HideInInspector]
    public bool slowingDown = false;

    void Start()
    {

    }

    void Update()
    {
        if (enabledFlyingLogic)
            Fly();
    }

    public void AddYawInput(float value)
    {
        flyingDirection.x += value;

        rootTransform.Rotate(rootTransform.up * value * 10.0f * Time.deltaTime);
    }

    public void AddPitchInput(float value)
    {
        rootTransform.Rotate(rootTransform.right * value * 10.0f * Time.deltaTime);
    }

    void Fly()
    {
        if (enabledFlyingLogic)
        {
            
        }
    }
}