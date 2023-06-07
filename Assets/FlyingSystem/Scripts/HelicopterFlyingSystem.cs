using UnityEngine;

public class HelicopterFlyingSystem : MonoBehaviour
{
    [Header("Object References")]
    public Transform rootTransform;

    public Transform springArmTransform;
    public Transform springArmLocalReferenceTransform;

    public Transform rollRootTransform;
    public Transform meshRootTransform;

    [Header("General Attributes")]
    public float normalFlyingSpeed = 60.0f;
    public float maximumFlyingSpeed = 100.0f;
    public float boostAcceleration = 12.0f;
    public float slowDownAcceleration = 10.0f;
    [Range(0.0f, 10.0f)]
    public float airDrag = 2.5f;

    [Header("Turning Attributes")]
    public float meshYawTurningSpeed = 60.0f;
    [Range(0.0f, 100.0f)]
    public float meshYawTurningSmoothingFactor = 2.0f;

    [Header("Horizontal Movement Attributes")]
    public float maximumMeshPitchAngle = 25.0f;
    [Range(0.0f, 100.0f)]
    public float meshPitchTurningSmoothingFactor = 2.0f;
    [Range(0.0f, 100.0f)]
    public float meshPitchTurningRecoverySmoothingFactor = 0.25f;

    public float maximumMeshRollAngle = 25.0f;
    [Range(0.0f, 100.0f)]
    public float meshRollTurningSmoothingFactor = 2.0f;
    [Range(0.0f, 100.0f)]
    public float meshRollTurningRecoverySmoothingFactor = 0.25f;

    [Header("Vertical Movement Attributes")]
    public float verticalFlyingSpeed = 20.0f;
    public float maximumVerticalFlyingSpeed = 35.0f;
    public float verticalBoostAcceleration = 25.0f;

    // The angle between the helicopter speed direction and tail rotor pushing direction
    public AnimationCurve speedTailRotorAngleRatioAnimationCurve;

    [Header("Custom Attributes")]
    public float g = 9.8f;
    public bool calculatePowerConsumption = true;
    public float currentPower = 100.0f;
    public float maximumPower = 800.0f;
    public float powerDecreaseSpeed = 0.25f;
    public float powerDecreaseSpeedWhenBoosting = 1.5f;
    public AnimationCurve speedRemainingPowerRatioAnimationCurve;

    public bool calculateCarryingWeight = true;
    public float currentCarryingWeight = 0.5f;
    public float maximumCarryingWeight = 600.0f;
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
    public float currentFlyingSpeed;

    [HideInInspector]
    public bool flyingInNormalSpeed = false;
    [HideInInspector]
    public bool boosting = false;
    [HideInInspector]
    public bool slowingDown = false, fullStop = false;
    [HideInInspector]
    public bool verticalSlowingDown = false;

    // Turning variables
    private float targetMeshLocalRotationX, targetMeshLocalRotationY, targetMeshLocalRotationZ;

    // Movement variables
    private bool verticalMoving = false;
    private bool ascending = false;
    private bool horizontalMoving = false;
    [HideInInspector]
    public float currentVerticalFlyingSpeed;

    void Update()
    {
        if (enabledFlyingLogic)
            Fly();
    }

    public Quaternion GetMeshRotation()
    {
        return meshRootTransform.rotation;
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

    public void VerticalSlowDown()
    {
        verticalMoving = false;
        verticalSlowingDown = true;
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
        targetMeshLocalRotationY += value * meshYawTurningSpeed * Time.deltaTime;
    }

    public void AddHorizontalInput(Vector2 direction)
    {
        horizontalMoving = true;

        direction = direction.normalized;

        if (DeltaAngle(springArmTransform.rotation.eulerAngles.y, meshRootTransform.rotation.eulerAngles.y) < 90.0f)
        {
            if (direction.x > 0.0f)
                targetMeshLocalRotationZ = -maximumMeshRollAngle * direction.x;
            else
                targetMeshLocalRotationZ = maximumMeshRollAngle * -direction.x;

            targetMeshLocalRotationX = maximumMeshPitchAngle * direction.y;
        }
        else
        {
            if (direction.x > 0.0f)
                targetMeshLocalRotationZ = -maximumMeshRollAngle * -direction.x;
            else
                targetMeshLocalRotationZ = maximumMeshRollAngle * direction.x;

            targetMeshLocalRotationX = maximumMeshPitchAngle * -direction.y;
        }

        springArmLocalReferenceTransform.localPosition = new Vector3(direction.x, 0.0f, direction.y) * 10000.0f;

        flyingDirection = (springArmLocalReferenceTransform.position - rootTransform.position);
        flyingDirection.y = 0.0f;
        flyingDirection = flyingDirection.normalized;
    }

    public void StopYawInput()
    {
        horizontalMoving = false;
    }

    public void AddVerticalInput(float value)
    {
        verticalMoving = true;
        verticalSlowingDown = false;

        if (value > 0.01f)
            ascending = true;
        else
            ascending = false;
    }

    public void AddWeight(float increaseValue)
    {
        currentCarryingWeight += Mathf.Clamp(currentCarryingWeight + increaseValue, 0.0f, maximumCarryingWeight);
    }

    void Fly()
    {
        if (enabledFlyingLogic)
        {
            rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, targetMeshLocalRotationY, rollRootTransform.localRotation.eulerAngles.z), meshYawTurningSmoothingFactor * Time.deltaTime);

            if (horizontalMoving)
            {
                rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, rollRootTransform.localRotation.eulerAngles.y, targetMeshLocalRotationZ), meshRollTurningSmoothingFactor * Time.deltaTime);
                meshRootTransform.localRotation = Quaternion.Lerp(meshRootTransform.localRotation, Quaternion.Euler(targetMeshLocalRotationX, 0.0f, 0.0f), meshPitchTurningSmoothingFactor * Time.deltaTime);

                currentFlyingSpeed = normalFlyingSpeed * speedTailRotorAngleRatioAnimationCurve.Evaluate(Vector3.Angle(new Vector3(flyingDirection.x, 0.0f, flyingDirection.z), new Vector3(meshRootTransform.forward.x, 0.0f, meshRootTransform.forward.z)) / 180.0f);

                rootTransform.position += flyingDirection * currentFlyingSpeed * Time.deltaTime;
            }
            else
            {
                rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, rollRootTransform.localRotation.eulerAngles.y, 0.0f), meshRollTurningRecoverySmoothingFactor * Time.deltaTime);
                meshRootTransform.localRotation = Quaternion.Lerp(meshRootTransform.localRotation, Quaternion.Euler(0.0f, 0.0f, 0.0f), meshPitchTurningRecoverySmoothingFactor * Time.deltaTime);
            }

            if (verticalMoving)
            {
                if (ascending)
                {
                    if (boosting)
                        currentVerticalFlyingSpeed = Mathf.Clamp(currentVerticalFlyingSpeed + verticalBoostAcceleration * Time.deltaTime, 0.0f, maximumVerticalFlyingSpeed);
                    else
                        currentVerticalFlyingSpeed = Mathf.Clamp(currentVerticalFlyingSpeed + verticalBoostAcceleration * Time.deltaTime, 0.0f, verticalFlyingSpeed);
                }
                else
                {
                    if (boosting)
                        currentVerticalFlyingSpeed = Mathf.Clamp(currentVerticalFlyingSpeed - verticalBoostAcceleration * Time.deltaTime, -maximumVerticalFlyingSpeed, maximumVerticalFlyingSpeed);
                    else
                        currentVerticalFlyingSpeed = Mathf.Clamp(currentVerticalFlyingSpeed - verticalBoostAcceleration * Time.deltaTime, -verticalFlyingSpeed, verticalFlyingSpeed);
                }

                rootTransform.position = rootTransform.position + new Vector3(0.0f, currentVerticalFlyingSpeed * Time.deltaTime, 0.0f);
            }
            else if (verticalSlowingDown)
            {
                if (currentVerticalFlyingSpeed < 0.0f)
                    currentVerticalFlyingSpeed = Mathf.Clamp(currentVerticalFlyingSpeed + verticalBoostAcceleration * Time.deltaTime, -maximumVerticalFlyingSpeed, 0.0f);
                else
                    currentVerticalFlyingSpeed = Mathf.Clamp(currentVerticalFlyingSpeed - verticalBoostAcceleration * Time.deltaTime, 0.0f, maximumVerticalFlyingSpeed);

                rootTransform.position = rootTransform.position + new Vector3(0.0f, currentVerticalFlyingSpeed * Time.deltaTime, 0.0f);
            }
        }
    }

    float DeltaAngle(float angle1, float angle2)
    {
        if (angle1 > 270.0f && angle2 < 90.0f)
            return angle2 + 360.0f - angle1;
        else if (angle1 < 90.0f && angle2 > 270.0f)
            return angle1 + 360.0f - angle2;
        else
            return Mathf.Abs(angle2 - angle1);
    }
}