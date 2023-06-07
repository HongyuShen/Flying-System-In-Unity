using UnityEngine;

public class FlyingVehicleFlyingSystem : MonoBehaviour
{
    [Header("Object References")]
    public Transform rootTransform;

    public Transform cameraTransform;

    public Transform rollRootTransform;
    public Transform meshRootTransform;

    [Header("Camera Attributes")]
    public bool cameraAlwaysLookAtMeshBack = false;

    [Header("General Attributes")]
    public float forwardSpeed = 20.0f;
    public float maximumforwardSpeed = 35.0f;
    public float boostAcceleration = 5.0f;
    public float slowDownAcceleration = 6.0f;
    public float backwardSpeed = 10.0f;
    [Range(0.0f, 10.0f)]
    public float airDrag = 2.5f;
    public bool hoverMode = false;
    public bool remainPitchZeroWhenHovering = true;

    [Header("Turning Attributes")]
    public bool controlYawAndRollSeparately = false;
    public float meshYawTurningSpeed = 30.0f;
    [Range(0.0f, 100.0f)]
    public float meshYawTurningSmoothingFactor = 2.0f;
    [Range(0.0f, 100.0f)]
    public float meshYawTurningRecoverySmoothingFactor = 0.5f;
    public float meshPitchTurningSpeed = 45.0f;
    [Range(0.0f, 100.0f)]
    public float meshPitchTurningSmoothingFactor = 4.0f;
    [Range(0.0f, 100.0f)]
    public float meshPitchTurningRecoverySmoothingFactor = 0.5f;
    public float maximumMeshRollAngle = 30.0f;
    [Range(0.0f, 100.0f)]
    public float meshRollTurningSmoothingFactor = 2.0f;
    [Range(0.0f, 100.0f)]
    public float meshRollTurningRecoverySmoothingFactor = 0.75f;
    public float meshTurningRecoveryDelay = 0.5f;

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
    public bool slowingDown = false;

    private float currentFlyingSpeed;

    // Movement variables
    private bool isMovingForward = false;
    private float forwardDirection = 1.0f;

    // Turning variables
    private bool isTurning = false;
    private bool isLookingUp = false;
    private float targetMeshLocalRotationX, targetMeshLocalRotationY, targetMeshLocalRotationZ;

    void Update()
    {
        if (enabledFlyingLogic)
            Fly();
    }

    public void AddForwardInput(float value)
    {
        isMovingForward = true;

        forwardDirection = value;
    }

    public void StopMovingForward()
    {
        isMovingForward = false;
    }

    public void AddYawInput(float value)
    {
        isTurning = true;

        if (isMovingForward)
        {
            if (forwardDirection > 0.0f)
                targetMeshLocalRotationY += value * meshYawTurningSpeed * Time.deltaTime;
            else
                targetMeshLocalRotationY -= value * meshYawTurningSpeed * Time.deltaTime;
        }
        else
            targetMeshLocalRotationY += value * meshYawTurningSpeed * Time.deltaTime;

        targetMeshLocalRotationZ = -value * maximumMeshRollAngle;
    }

    public void StopTurning()
    {
        if (!isMovingForward && !isLookingUp)
            Invoke("LateStopTurning", meshTurningRecoveryDelay);
        else
            LateStopTurning();
    }

    void LateStopTurning()
    {
        isTurning = false;
        targetMeshLocalRotationZ = 0.0f;
    }

    public void AddPitchInput(float value)
    {
        isLookingUp = true;
        targetMeshLocalRotationX += value * meshPitchTurningSpeed * Time.deltaTime;
    }

    public void StopLookingUp()
    {
        isLookingUp = false;
    }

    void Fly()
    {
        if (enabledFlyingLogic)
        {
            rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, targetMeshLocalRotationY, rollRootTransform.localRotation.eulerAngles.z), meshYawTurningSmoothingFactor * Time.deltaTime);

            if (isTurning)
                rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, rollRootTransform.localRotation.eulerAngles.y, targetMeshLocalRotationZ), meshRollTurningSmoothingFactor * Time.deltaTime);
            else
                rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, rollRootTransform.localRotation.eulerAngles.y, 0.0f), meshRollTurningRecoverySmoothingFactor * Time.deltaTime);

            if (!isMovingForward && !isTurning && !isLookingUp && remainPitchZeroWhenHovering)
                targetMeshLocalRotationX = Mathf.Lerp(targetMeshLocalRotationX, 0.0f, meshPitchTurningRecoverySmoothingFactor * Time.deltaTime);

            meshRootTransform.localRotation = Quaternion.Lerp(meshRootTransform.localRotation, Quaternion.Euler(targetMeshLocalRotationX, 0.0f, 0.0f), meshPitchTurningSmoothingFactor * Time.deltaTime);

            if (isMovingForward)
            {
                if (boosting)
                    currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, maximumforwardSpeed);
                else
                    currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, forwardSpeed);

                flyingDirection = forwardDirection * meshRootTransform.forward;

                flyingSpeed = currentFlyingSpeed;
                flyingVelocity = flyingDirection * flyingSpeed;

                rootTransform.position += flyingVelocity * Time.deltaTime;
            }
        }
    }
}