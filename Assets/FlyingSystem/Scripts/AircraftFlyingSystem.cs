using UnityEngine;

public class AircraftFlyingSystem : MonoBehaviour
{
    [Header("Object References")]
    public Transform rootTransform;
    public Transform rollRootTransform;
    public Transform meshRootTransform;

    [Header("General Attributes")]
    public float minimumTakeOffSpeed = 79.5f;
    public float normalFlyingSpeed = 80.0f;
    public float maximumFlyingSpeed = 110.0f;
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
    public bool inAir = false;

    [HideInInspector]
    public Vector3 flyingDirection;

    [HideInInspector]
    public float flyingSpeed;

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

    private float currentFlyingSpeed;

    // Turning variables
    private float targetMeshLocalRotationX, targetMeshLocalRotationY, targetMeshLocalRotationZ;
    private float totalTurningDegree;

    private float powerFactor = 1.0f;

    private float carryingWeightFactor = 1.0f;

    void Update()
    {
        if (enabledFlyingLogic)
            Fly();
    }

    public bool TakeOff(float groundMovementSpeed)
    {
        if (groundMovementSpeed > minimumTakeOffSpeed)
        {
            currentFlyingSpeed = normalFlyingSpeed;

            inAir = true;
        }

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
                meshRootTransform.localRotation = Quaternion.Lerp(meshRootTransform.localRotation, Quaternion.Euler(targetMeshLocalRotationX, meshRootTransform.localRotation.eulerAngles.y, meshRootTransform.localRotation.eulerAngles.z), meshPitchTurningSmoothingFactor * Time.deltaTime);
                meshRootTransform.localRotation = Quaternion.Lerp(meshRootTransform.localRotation, Quaternion.Euler(meshRootTransform.localRotation.eulerAngles.x, targetMeshLocalRotationY, meshRootTransform.localRotation.eulerAngles.z), meshYawTurningSmoothingFactor * Time.deltaTime);

                if (calculatePowerConsumption)
                    powerFactor = speedRemainingPowerRatioAnimationCurve.Evaluate(1.0f - currentPower / maximumPower);
                else
                    powerFactor = 1.0f;

                if (calculateCarryingWeight)
                    carryingWeightFactor = speedCarryingWeightRatioAnimationCurve.Evaluate(currentCarryingWeight / maximumCarryingWeight);
                else
                    carryingWeightFactor = 1.0f;

                flyingSpeed = currentFlyingSpeed * powerFactor * carryingWeightFactor;

                flyingVelocity = meshRootTransform.forward * flyingSpeed;

                rootTransform.position += flyingVelocity * Time.deltaTime;

                totalTurningDegree = targetMeshLocalRotationY - targetMeshLocalRotationZ;

                if (Mathf.Abs(totalTurningDegree) > 180.0f)
                    totalTurningDegree = -totalTurningDegree % 180.0f;

                rootTransform.Rotate(Vector3.up * turningSpeed * Mathf.Clamp(totalTurningDegree / 180.0f, -1.0f, 1.0f) * Time.deltaTime);
            }
        }
    }
}