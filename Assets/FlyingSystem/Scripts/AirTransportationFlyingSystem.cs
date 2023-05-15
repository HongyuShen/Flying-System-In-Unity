using UnityEngine;

public class AirTransportationFlyingSystem : MonoBehaviour
{
    [Header("Object References")]
    public Transform rootTransform;

    // Necessary for helicopter only
    public Transform springArmTransform;
    public Transform springArmLocalReferenceTransform;
    
    public Transform rollRootTransform;
    public Transform meshTransform;

    [Header("Camera Attributes")]
    public float cameraLookAtOffsetY = 0.5f;

    [Header("Air Transportation Attributes")]
    public float normalFlyingSpeed = 20.0f;
    public float maximumFlyingSpeed = 35.0f;
    public float boostAcceleration = 12.0f;
    public float slowDownAcceleration = 10.0f;
    [Range(0.0f, 10.0f)]
    public float airDrag = 2.5f;
    public float turningSpeed = 25.0f;
    public float meshYawTurningSpeed = 10.0f;
    [Range(0.0f, 100.0f)]
    public float meshYawTurningSmoothingFactor = 0.5f;
    public float maximumMeshYawAngle = 5.0f;
    public float meshPitchTurningSpeed = 20.0f;
    [Range(0.0f, 100.0f)]
    public float meshPitchTurningSmoothingFactor = 2.0f;
    public float meshRollTurningSpeed = 75.0f;
    [Range(0.0f, 100.0f)]
    public float meshRollTurningSmoothingFactor = 1.5f;

    [Header("Helicopter Special Case Attributes")]
    public bool helicopterFlightMode = false;
    public float verticalFlyingSpeed = 20.0f;
    public float maximumVerticalFlyingSpeed = 35.0f;
    public float verticalBoostAcceleration = 25.0f;
    public float maximumMeshPitchAngle = 25.0f;
    public float maximumMeshRollAngle = 25.0f;

    [Header("Custom Attributes")]
    public float g = 9.8f;
    public bool calculatePowerConsumption = true;
    public float maximumPower = 800.0f;
    public float powerDecreaseSpeed = 0.25f;
    public float powerDecreaseSpeedWhenBoosting = 1.5f;
    public AnimationCurve speedRemainingPowerRatioAnimationCurve;

    public bool calculateCarryingWeight = true;
    public float maximumCarryingWeight = 1000.0f;
    public AnimationCurve speedCarryingWeightRatioAnimationCurve;

    [HideInInspector]
    public bool enabledFlyingLogic = true;

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
    public bool verticalSlowingDown = false;

    private Vector3 targetCharacterPosition;

    // Turning variables
    private float targetMeshLocalRotationX, targetMeshLocalRotationY, targetMeshLocalRotationZ;
    private float totalTurningDegree;

    // Helicopter variables
    private bool verticalMoving = false;
    private bool ascending = false;
    private bool verticalBoosting = false;
    private bool horizontalMoving = false;
    [HideInInspector]
    public float currentVerticalFlyingSpeed;
    private float nextVerticalFlyingSpeed;

    void Start()
    {

    }

    void Update()
    {
        if (enabledFlyingLogic)
            Fly();
    }

    public Quaternion GetRotation()
    {
        return meshTransform.rotation;
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

    public void StopFlying()
    {
        flyingInNormalSpeed = false;
        slowingDown = false;
        fullStop = false;
    }

    public void AddYawInput(float value)
    {
        if (!helicopterFlightMode)
            targetMeshLocalRotationY = Mathf.Clamp(targetMeshLocalRotationY + value * meshYawTurningSpeed * Time.deltaTime, -maximumMeshYawAngle, maximumMeshYawAngle);
        else
            targetMeshLocalRotationY += value * meshYawTurningSpeed * Time.deltaTime;
    }

    public void AddHelicopterYawInput(Vector2 direction)
    {
        horizontalMoving = true;

        springArmLocalReferenceTransform.localPosition = new Vector3(direction.x, 0.0f, direction.y).normalized * 0.1f;

        if (springArmTransform.localRotation.eulerAngles.y < 90.0f || springArmTransform.localRotation.eulerAngles.y > 270.0f)
            springArmLocalReferenceTransform.localRotation = Quaternion.Euler(maximumMeshPitchAngle * springArmLocalReferenceTransform.localPosition.z / 0.1f, 0.0f, maximumMeshRollAngle * -springArmLocalReferenceTransform.localPosition.x / 0.1f);
        else
            springArmLocalReferenceTransform.localRotation = Quaternion.Euler(maximumMeshPitchAngle * -springArmLocalReferenceTransform.localPosition.z / 0.1f, 0.0f, maximumMeshRollAngle * springArmLocalReferenceTransform.localPosition.x / 0.1f);

        targetMeshLocalRotationZ = springArmLocalReferenceTransform.rotation.eulerAngles.z;
        targetMeshLocalRotationX = springArmLocalReferenceTransform.rotation.eulerAngles.x;

        flyingDirection = (springArmLocalReferenceTransform.position - rootTransform.position);
        flyingDirection.y = 0.0f;
        flyingDirection = flyingDirection.normalized;
    }

    public void StopYawInput()
    {
        if (!helicopterFlightMode)
            targetMeshLocalRotationY = 0.0f;
        else
            horizontalMoving = false;
    }

    public void AddPitchInput(float value)
    {
        if (!helicopterFlightMode)
            targetMeshLocalRotationX -= value * meshPitchTurningSpeed * Time.deltaTime;
        else
        {
            verticalMoving = true;
            verticalSlowingDown = false;

            if (value > 0.01f)
                ascending = true;
            else
                ascending = false;
        }
    }

    public void AddRollInput(float value)
    {
        targetMeshLocalRotationZ -= value * meshRollTurningSpeed * Time.deltaTime;
    }

    public void Fly()
    {
        if (enabledFlyingLogic)
        {
            if (!helicopterFlightMode)
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
            else
            {
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

                    targetCharacterPosition = rootTransform.position + new Vector3(0.0f, currentVerticalFlyingSpeed * Time.deltaTime, 0.0f);
                }
                else if (verticalSlowingDown)
                {
                    if (currentVerticalFlyingSpeed < 0.0f)
                        currentVerticalFlyingSpeed = Mathf.Clamp(currentVerticalFlyingSpeed + verticalBoostAcceleration * Time.deltaTime, -maximumVerticalFlyingSpeed, 0.0f);
                    else
                        currentVerticalFlyingSpeed = Mathf.Clamp(currentVerticalFlyingSpeed - verticalBoostAcceleration * Time.deltaTime, 0.0f, maximumVerticalFlyingSpeed);

                    targetCharacterPosition = rootTransform.position + new Vector3(0.0f, currentVerticalFlyingSpeed * Time.deltaTime, 0.0f);
                }

                meshTransform.localRotation = Quaternion.Lerp(meshTransform.localRotation, Quaternion.Euler(meshTransform.localRotation.eulerAngles.x, targetMeshLocalRotationY, meshTransform.localRotation.eulerAngles.z), meshYawTurningSmoothingFactor * Time.deltaTime);

                if (horizontalMoving)
                {
                    rollRootTransform.rotation = Quaternion.Lerp(rollRootTransform.rotation, Quaternion.Euler(0.0f, 0.0f, targetMeshLocalRotationZ), meshRollTurningSmoothingFactor * Time.deltaTime);
                    meshTransform.rotation = Quaternion.Lerp(meshTransform.rotation, Quaternion.Euler(targetMeshLocalRotationX, meshTransform.rotation.eulerAngles.y, meshTransform.rotation.eulerAngles.z), meshPitchTurningSmoothingFactor * Time.deltaTime);

                    targetCharacterPosition += flyingDirection * normalFlyingSpeed * Time.deltaTime;
                }
                else
                {
                    rollRootTransform.rotation = Quaternion.Lerp(rollRootTransform.rotation, Quaternion.Euler(0.0f, 0.0f, 0.0f), meshRollTurningSmoothingFactor * Time.deltaTime);
                    meshTransform.rotation = Quaternion.Lerp(meshTransform.rotation, Quaternion.Euler(0.0f, meshTransform.rotation.eulerAngles.y, 0.0f), meshPitchTurningSmoothingFactor * Time.deltaTime);
                }

                rootTransform.position = targetCharacterPosition;
            }
        }
    }
}