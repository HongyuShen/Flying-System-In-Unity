using UnityEngine;

public class AircraftFlyingSystem : MonoBehaviour
{
    [Header("Object References")]
    public Transform rootTransform;
    public Transform springArmTransform;
    public Transform rollRootTransform;
    public Transform meshTransform;

    [Header("General Attributes")]
    public bool canMoveOnGround = true;
    public float maximumGroundMovementSpeed = 100.0f;
    public float groundAcceleration = 10.0f;
    public float groundTurningSpeed = 10.0f;
    public float minimumTakeOffSpeed = 70.0f;
    public float normalFlyingSpeed = 20.0f;
    public float maximumFlyingSpeed = 35.0f;
    public float boostAcceleration = 12.0f;
    public float slowDownAcceleration = 10.0f;
    [Range(0.0f, 10.0f)]
    public float airDrag = 2.5f;

    [Header("Turning Attributes")]
    public float turningSpeed = 25.0f;
    public float meshYawTurningSpeed = 10.0f;
    [Range(0.0f, 100.0f)]
    public float meshYawTurningSmoothingFactor = 0.5f;
    public float maximumMeshYawAngle = 5.0f;
    public float meshPitchTurningSpeed = 20.0f;
    [Range(0.0f, 100.0f)]
    public float meshPitchTurningSmoothingFactor = 2.0f;
    [Range(0.0f, 100.0f)]
    public float meshPitchTurningRecoverySmoothingFactor = 0.25f;
    public float meshRollTurningSpeed = 75.0f;
    [Range(0.0f, 100.0f)]
    public float meshRollTurningSmoothingFactor = 1.5f;
    [Range(0.0f, 100.0f)]
    public float meshRollTurningRecoverySmoothingFactor = 0.25f;

    [Header("Custom Attributes")]
    public float g = 9.8f;
    public bool calculatePowerConsumption = true;
    public float currentPower = 1000.0f;
    public float maximumPower = 1000.0f;
    public float powerDecreaseSpeed = 0.25f;
    public float powerDecreaseSpeedWhenBoosting = 1.5f;
    public AnimationCurve speedRemainingPowerRatioAnimationCurve;

    public bool calculateCarryingWeight = true;
    public float currentCarryingWeight = 0.5f;
    public float maximumCarryingWeight = 2000.0f;
    public AnimationCurve speedCarryingWeightRatioAnimationCurve;

    // Flying attributes
    [HideInInspector]
    public bool isTurnedOff = false;

    [HideInInspector]
    public bool enabledFlyingLogic = true;

    [HideInInspector]
    public float currentGroundMovementSpeed;

    [HideInInspector]
    public bool inAir = false;

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
    public bool slowingDown = false, fullStop = false;
    [HideInInspector]
    public bool verticalSlowingDown = false;

    private Vector3 targetCharacterPosition;

    // Turning variables
    private float targetMeshLocalRotationX, targetMeshLocalRotationY, targetMeshLocalRotationZ;
    private float totalTurningDegree;

    void Start()
    {

    }

    void Update()
    {
        if (enabledFlyingLogic)
            Fly();
    }

    public Quaternion GetMeshRotation()
    {
        return meshTransform.rotation;
    }

    public bool TakeOff()
    {
        inAir = true;

        return inAir;
    }

    public void Land()
    {
        inAir = false;
    }

    public void TurnOn()
    {
        isTurnedOff = false;
    }

    public void TurnOff()
    {
        isTurnedOff = true;

        flyingInNormalSpeed = false;
        slowingDown = false;
        fullStop = false;
    }

    public void GoundMoveForward(float value)
    {
        currentGroundMovementSpeed += Mathf.Clamp(groundAcceleration * Time.deltaTime, 0.0f, maximumGroundMovementSpeed);
        rootTransform.position += meshTransform.forward * value * currentGroundMovementSpeed * Time.deltaTime;
    }

    public void GoundTurnRight(float value)
    {
        rootTransform.Rotate(rootTransform.up * value * groundTurningSpeed * Time.deltaTime);
    }

    public void SlowDown()
    {

    }

    public void StopSlowingDown()
    {
        slowingDown = false;
    }

    public void FullStopInAir()
    {
        boosting = false;
        flyingInNormalSpeed = false;
        slowingDown = false;
        fullStop = true;
    }

    public void AddYawInput(float value)
    {
        targetMeshLocalRotationY = Mathf.Clamp(targetMeshLocalRotationY + value * meshYawTurningSpeed * Time.deltaTime, -maximumMeshYawAngle, maximumMeshYawAngle);
    }

    public void StopYawInput()
    {
        targetMeshLocalRotationY = 0.0f;
    }

    public void AddPitchInput(float value)
    {
        targetMeshLocalRotationX -= value * meshPitchTurningSpeed * Time.deltaTime;

    }

    public void AddRollInput(float value)
    {
        targetMeshLocalRotationZ -= value * meshRollTurningSpeed * Time.deltaTime;
    }

    public void AddWeight(float increaseValue)
    {
        currentCarryingWeight += Mathf.Clamp(currentCarryingWeight + increaseValue, 0.0f, maximumCarryingWeight);
    }

    void Fly()
    {
        if (enabledFlyingLogic)
        {
            if (inAir)
            {
                rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, 0.0f, targetMeshLocalRotationZ), meshRollTurningSmoothingFactor * Time.deltaTime);
                meshTransform.localRotation = Quaternion.Lerp(meshTransform.localRotation, Quaternion.Euler(targetMeshLocalRotationX, meshTransform.localRotation.eulerAngles.y, meshTransform.localRotation.eulerAngles.z), meshPitchTurningSmoothingFactor * Time.deltaTime);
                meshTransform.localRotation = Quaternion.Lerp(meshTransform.localRotation, Quaternion.Euler(meshTransform.localRotation.eulerAngles.x, targetMeshLocalRotationY, meshTransform.localRotation.eulerAngles.z), meshYawTurningSmoothingFactor * Time.deltaTime);

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