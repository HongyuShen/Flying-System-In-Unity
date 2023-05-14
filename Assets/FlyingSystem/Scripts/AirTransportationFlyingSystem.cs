using UnityEngine;

public class AirTransportationFlyingSystem : MonoBehaviour
{
    public enum AirTransportationType { Aircraft, Helicopter, Drone };

    [Header("Object References")]
    public Transform rootTransform;
    public Transform rollRootTransform;
    public Transform meshTransform;
    public Transform cameraTransform;

    [Header("Camera Attributes")]
    public float cameraLookAtOffsetY = 0.5f;

    [Header("Air Transportation Attributes")]
    public AirTransportationType airTransportationType;
    public float normalFlyingSpeed = 20.0f;
    public float maximumFlyingSpeed = 35.0f;
    public float boostAcceleration = 12.0f;
    public float slowDownAcceleration = 10.0f;
    [Range(0.0f, 10.0f)]
    public float airDrag = 2.5f;
    public float turningSpeed = 25.0f;
    public float meshHorizontalTurningSpeed = 10.0f;
    [Range(0.0f, 100.0f)]
    public float meshHorizontalTurningSmoothingFactor = 0.5f;
    public float maximumMeshHorizontalTurningAngle = 5.0f;
    public float meshVerticalTurningSpeed = 20.0f;
    [Range(0.0f, 100.0f)]
    public float meshVerticalTurningSmoothingFactor = 2.0f;
    public float meshRollTurningSpeed = 75.0f;
    [Range(0.0f, 100.0f)]
    public float meshRollTurningSmoothingFactor = 1.5f;

    [Header("Custom Attributes")]
    public float g = 9.8f;
    public bool calculatePowerConsumption = true;
    public float maximumPower = 200.0f;
    public float powerDecreaseSpeed = 0.25f;
    public float powerDecreaseSpeedWhenBoosting = 1.5f;
    public AnimationCurve speedTirednessRatioAnimationCurve;

    public bool calculateCarryingWeight = true;
    public float maximumCarryingWeight = 1000.0f;
    public AnimationCurve speedCarryingWeightRatioAnimationCurve;

    [HideInInspector]
    public bool enabledFlyingLogic = true;

    [HideInInspector]
    public bool inAir = false;

    [HideInInspector]
    private float currentFlyingSpeed;

    [HideInInspector]
    public Vector3 flyingVelocity;

    [HideInInspector]
    public bool flyingInNormalSpeed = false;
    [HideInInspector]
    public bool boosting = false;
    [HideInInspector]
    public bool slowingDown = false;

    private Vector3 targetCharacterPosition;

    // Turning variables
    private float targetMeshLocalRotationX, targetMeshLocalRotationY;
    [HideInInspector]
    public float targetMeshLocalRotationZ;
    private float totalTurningDegree;

    void Start()
    {

    }

    void Update()
    {
        if (enabledFlyingLogic)
            Fly();
    }

    public void TakeOff()
    {
        inAir = true;
    }

    public void MoveForward()
    {

    }

    public void SlowDown()
    {

    }

    public void AddHorizontalMovement(float value)
    {
        targetMeshLocalRotationY = Mathf.Clamp(targetMeshLocalRotationY + value * meshHorizontalTurningSpeed * Time.deltaTime, -maximumMeshHorizontalTurningAngle, maximumMeshHorizontalTurningAngle);
    }

    public void StopHorizontalMovement()
    {
        targetMeshLocalRotationY = 0.0f;
    }

    public void AddVerticalMovement(float value)
    {
        targetMeshLocalRotationX -= value * meshVerticalTurningSpeed * Time.deltaTime;
    }

    public void AddRollMovement(float value)
    {
        targetMeshLocalRotationZ -= value * meshRollTurningSpeed * Time.deltaTime;
    }

    public void Fly()
    {
        if (enabledFlyingLogic)
        {
            if (inAir)
            {
                rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, 0.0f, targetMeshLocalRotationZ), meshRollTurningSmoothingFactor * Time.deltaTime);
                meshTransform.localRotation = Quaternion.Lerp(meshTransform.localRotation, Quaternion.Euler(targetMeshLocalRotationX, meshTransform.localRotation.eulerAngles.y, meshTransform.localRotation.eulerAngles.z), meshVerticalTurningSmoothingFactor * Time.deltaTime);
                meshTransform.localRotation = Quaternion.Lerp(meshTransform.localRotation, Quaternion.Euler(meshTransform.localRotation.eulerAngles.x, targetMeshLocalRotationY, meshTransform.localRotation.eulerAngles.z), meshHorizontalTurningSmoothingFactor * Time.deltaTime);

                flyingVelocity = meshTransform.forward * normalFlyingSpeed;
                targetCharacterPosition = rootTransform.position + flyingVelocity * Time.deltaTime;

                rootTransform.position = targetCharacterPosition;

                totalTurningDegree = targetMeshLocalRotationY - targetMeshLocalRotationZ;
                
                if (Mathf.Abs(totalTurningDegree) > 180.0f)
                    totalTurningDegree = -totalTurningDegree % 180.0f;

                rootTransform.Rotate(Vector3.up * turningSpeed * Mathf.Clamp(totalTurningDegree / 180.0f, -1.0f, 1.0f) * Time.deltaTime);
            }
        }
    }
}