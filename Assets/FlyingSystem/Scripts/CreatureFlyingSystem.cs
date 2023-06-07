using UnityEngine;

public class CreatureFlyingSystem : MonoBehaviour
{
    [Header("Object References")]
    public Transform rootTransform;
    public Transform meshRootTransform;
    public Transform cameraTransform;

    [Header("Camera Attributes")]
    public float cameraLookAtMeshOffsetY = 0.5f;

    [Header("Creature Attributes")]
    [Range(0.0f, 90.0f)]
    public float takeOffAngle = 30.0f;
    public float normalFlyingSpeed = 10.0f;
    public float maximumFlyingSpeed = 15.0f;
    public float boostAcceleration = 12.0f;
    public float slowDownAcceleration = 10.0f;
    [Range(0.0f, 10.0f)]
    public float airDrag = 2.5f;
    public float meshHorizontalTurningSpeed = 2.5f;
    public float meshVerticalTurningSpeed = 10.0f;
    public float meshMaximumTurningRotationZ = 25.0f;
    [Range(0.0f, 1.0f)]
    public float meshRotationZSmoothingFactor = 0.125f;
    public bool resetMeshRotationAfterLanding = true;

    [Header("Diving Attributes")]
    public bool canDive = true;
    public float maximumDivingSpeed = 500.0f;
    [Range(0.0f, 90.0f)]
    public float divingStartAngle = 30.0f;
    public float decelerationAfterDiving = 5.0f;

    [Header("Custom Attributes")]
    public float g = 9.8f;
    public bool calculateStaminaConsumption = true;
    public float currentStamina = 200.0f;
    public float maximumStamina = 200.0f;
    public float staminaDecreaseSpeed = 0.25f;
    public float staminaDecreaseSpeedWhenBoosting = 1.5f;
    public float staminaRecoverySpeed = 4.0f;
    public AnimationCurve speedTirednessRatioAnimationCurve;

    public bool calculateCarryingWeight = true;
    public float currentCarryingWeight = 10.0f;
    public float maximumCarryingWeight = 10.0f;
    public AnimationCurve speedCarryingWeightRatioAnimationCurve;

    // Flying attributes
    [HideInInspector]
    public bool enabledFlyingLogic = true;

    [HideInInspector]
    public bool inAir = false;

    [HideInInspector]
    public bool inAirflow = false;

    [HideInInspector]
    public bool stopFlying = false;

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

    private float currentFlyingSpeed;

    private Vector3 targetCharacterPosition;

    // Turning variables
    private bool alignedToTargetDirection = true;
    private Quaternion targetMeshLocalRotation;
    private float relativeRotationY, deltaRotationY, rotationYAlignmentPercentage;
    private float currentMeshLocalRotationY;
    private float turningDirection;
    private float rotationYComponent;
    private float targetMeshLocalRotationZ;

    // Diving variables
    [HideInInspector]
    public bool diving = false;
    private float verticalAcceleration;
    private float verticalSpeed;

    // Airflow variables
    private float airflowSpeed;
    private float airflowIntensity;
    private float airflowAcceleration;
    private float airflowFadeOutAcceleration;

    // Rotation Lerp function variables
    private float increaseAngle;

    private float staminaFactor = 1.0f;

    private float carryingWeightFactor = 1.0f;

    void Start()
    {
        currentStamina = maximumStamina;
    }

    void Update()
    {
        if (enabledFlyingLogic)
            Fly();
    }

    public Quaternion GetRotation()
    {
        return meshRootTransform.rotation;
    }

    public void TakeOff()
    {
        targetMeshLocalRotation = meshRootTransform.localRotation;
        meshRootTransform.localRotation = Quaternion.Euler(-takeOffAngle, meshRootTransform.localRotation.eulerAngles.y, meshRootTransform.localRotation.eulerAngles.z);
        flyingDirection = meshRootTransform.forward;
        meshRootTransform.localRotation = targetMeshLocalRotation;

        currentFlyingSpeed = normalFlyingSpeed;

        inAir = true;
    }

    public void Land()
    {
        inAir = false;

        if (resetMeshRotationAfterLanding)
            meshRootTransform.localRotation = Quaternion.Euler(0.0f, meshRootTransform.localRotation.eulerAngles.y, 0.0f);
    }

    public void FlyForward()
    {
        flyingInNormalSpeed = true;
        slowingDown = false;
        fullStop = false;
    }

    public void SlowDown()
    {
        boosting = false;
        flyingInNormalSpeed = false;
        slowingDown = true;
        fullStop = false;
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
        if (Mathf.Abs(value) > 0.025f)
        {
            flyingDirection = (rootTransform.position + new Vector3(0.0f, cameraLookAtMeshOffsetY, 0.0f) - cameraTransform.position).normalized;

            targetMeshLocalRotation = Quaternion.LookRotation(flyingDirection, meshRootTransform.up);

            relativeRotationY = 0.0f;
            rotationYAlignmentPercentage = 0.0f;

            if (meshRootTransform.localRotation.eulerAngles.y > 270.0f && targetMeshLocalRotation.eulerAngles.y < 90.0f)
                deltaRotationY = 360.0f - meshRootTransform.localRotation.eulerAngles.y + targetMeshLocalRotation.eulerAngles.y;
            else if (meshRootTransform.localRotation.eulerAngles.y < 90.0f && targetMeshLocalRotation.eulerAngles.y > 270.0f)
                deltaRotationY = 360.0f - targetMeshLocalRotation.eulerAngles.y + meshRootTransform.localRotation.eulerAngles.y;
            else
                deltaRotationY = Mathf.Abs(targetMeshLocalRotation.eulerAngles.y - meshRootTransform.localRotation.eulerAngles.y);

            if (Mathf.Abs(deltaRotationY) > 10.0f)
            {
                alignedToTargetDirection = false;

                if (value < 0.0f)
                    turningDirection = 1.0f;
                else
                    turningDirection = -1.0f;
            }
        }
    }

    public void AddAirflowForce(float intensity, float acceleration, float fadeOutAcceleration)
    {
        airflowIntensity = intensity;
        airflowAcceleration = acceleration;
        airflowFadeOutAcceleration = fadeOutAcceleration;

        inAirflow = true;
    }

    public void EndAirflowForce()
    {
        inAirflow = false;
    }

    void Fly()
    {
        if (enabledFlyingLogic)
        {
            if (inAir)
            {
                if (!slowingDown)
                {
                    rotationYComponent = meshHorizontalTurningSpeed * Time.deltaTime;

                    currentMeshLocalRotationY = RotationLerp(meshRootTransform.localRotation.eulerAngles.y, targetMeshLocalRotation.eulerAngles.y, rotationYComponent);

                    if (!alignedToTargetDirection)
                    {
                        relativeRotationY += increaseAngle;

                        rotationYAlignmentPercentage = Mathf.Clamp(relativeRotationY / deltaRotationY, 0.0f, 1.0f);

                        if (rotationYAlignmentPercentage > 0.985f)
                            alignedToTargetDirection = true;

                        targetMeshLocalRotationZ = RotationLerp(meshRootTransform.localRotation.eulerAngles.z, turningDirection * meshMaximumTurningRotationZ, rotationYAlignmentPercentage * meshRotationZSmoothingFactor);
                    }
                    else
                    {
                        targetMeshLocalRotationZ = RotationLerp(meshRootTransform.localRotation.eulerAngles.z, 0.0f, rotationYComponent);
                    }

                    meshRootTransform.localRotation = Quaternion.Euler(RotationLerp(meshRootTransform.localRotation.eulerAngles.x, targetMeshLocalRotation.eulerAngles.x, meshVerticalTurningSpeed * Time.deltaTime), currentMeshLocalRotationY, targetMeshLocalRotationZ);
                }

                if (calculateStaminaConsumption)
                    staminaFactor = speedTirednessRatioAnimationCurve.Evaluate(1.0f - currentStamina / maximumStamina);
                else
                    staminaFactor = 1.0f;

                if (calculateCarryingWeight)
                    carryingWeightFactor = speedCarryingWeightRatioAnimationCurve.Evaluate(currentCarryingWeight / maximumCarryingWeight);
                else
                    carryingWeightFactor = 1.0f;

                // Diving
                if (!inAirflow && canDive && meshRootTransform.localRotation.eulerAngles.x < 89.995f && meshRootTransform.localRotation.eulerAngles.x > divingStartAngle)
                {
                    diving = true;

                    if (slowingDown || fullStop)
                    {
                        currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed - slowDownAcceleration * Time.deltaTime, 0.0f, currentFlyingSpeed);
                        verticalAcceleration = slowDownAcceleration - g * Mathf.Cos((90.0f - meshRootTransform.localRotation.eulerAngles.x) * Mathf.Deg2Rad);
                    }
                    else
                    {
                        currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, maximumDivingSpeed);
                        currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + g * Mathf.Sin(meshRootTransform.localRotation.eulerAngles.x * Mathf.Deg2Rad) * Time.deltaTime, 0.0f, maximumDivingSpeed);
                        verticalAcceleration = -g * Mathf.Cos((90.0f - meshRootTransform.localRotation.eulerAngles.x) * Mathf.Deg2Rad);
                    }

                    flyingSpeed = currentFlyingSpeed * staminaFactor * carryingWeightFactor;
                    flyingVelocity = flyingDirection * flyingSpeed;

                    verticalSpeed += verticalAcceleration * Time.deltaTime;

                    targetCharacterPosition = rootTransform.position + flyingVelocity * Time.deltaTime + new Vector3(0.0f, Mathf.Clamp(verticalSpeed * Time.deltaTime - 0.5f * verticalAcceleration * Time.deltaTime * Time.deltaTime, -99999999.0f, 0.0f), 0.0f);
                }
                else
                {
                    if (!stopFlying)
                    {
                        diving = false;

                        if (verticalSpeed < -0.1f)
                            verticalSpeed += airDrag * Mathf.Cos((meshRootTransform.localRotation.eulerAngles.x) * Mathf.Deg2Rad) * Time.deltaTime;
                        else
                            verticalSpeed = 0.0f;

                        if (boosting)
                        {
                            if (calculateStaminaConsumption)
                                currentStamina = Mathf.Clamp(currentStamina - staminaDecreaseSpeed * Time.deltaTime, 0.0f, maximumStamina);

                            if (meshRootTransform.localRotation.eulerAngles.x > 270.0f)
                            {
                                if (currentFlyingSpeed > maximumFlyingSpeed)
                                    currentFlyingSpeed -= (decelerationAfterDiving + decelerationAfterDiving * Mathf.Sin((360.0f - meshRootTransform.localRotation.eulerAngles.x) * Mathf.Deg2Rad)) * Time.deltaTime;
                                else
                                    currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, maximumFlyingSpeed);
                            }
                            else
                            {
                                if (currentFlyingSpeed > maximumFlyingSpeed)
                                    currentFlyingSpeed -= decelerationAfterDiving * Time.deltaTime;
                                else
                                    currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, maximumFlyingSpeed);
                            }
                        }
                        else if (slowingDown || fullStop)
                        {
                            if (calculateStaminaConsumption)
                                currentStamina = Mathf.Clamp(currentStamina - staminaDecreaseSpeed * Time.deltaTime, 0.0f, maximumStamina);

                            if (meshRootTransform.localRotation.eulerAngles.x > 270.0f)
                            {
                                if (currentFlyingSpeed > normalFlyingSpeed)
                                    currentFlyingSpeed -= (decelerationAfterDiving + decelerationAfterDiving * Mathf.Sin((360.0f - meshRootTransform.localRotation.eulerAngles.x) * Mathf.Deg2Rad)) * Time.deltaTime;
                            }
                            else
                            {
                                if (currentFlyingSpeed > normalFlyingSpeed)
                                    currentFlyingSpeed -= decelerationAfterDiving * Time.deltaTime;
                            }

                            currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed - slowDownAcceleration * Time.deltaTime, 0.0f, currentFlyingSpeed);
                        }
                        else
                        {
                            if (calculateStaminaConsumption)
                                currentStamina = Mathf.Clamp(currentStamina - staminaDecreaseSpeed * Time.deltaTime, 0.0f, maximumStamina);

                            if (meshRootTransform.localRotation.eulerAngles.x > 270.0f)
                            {
                                if (currentFlyingSpeed > normalFlyingSpeed)
                                    currentFlyingSpeed -= (decelerationAfterDiving + decelerationAfterDiving * Mathf.Sin((360.0f - meshRootTransform.localRotation.eulerAngles.x) * Mathf.Deg2Rad)) * Time.deltaTime;
                                else
                                    currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, normalFlyingSpeed);
                            }
                            else
                            {
                                if (currentFlyingSpeed > normalFlyingSpeed)
                                    currentFlyingSpeed -= decelerationAfterDiving * Time.deltaTime;
                                else
                                    currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, normalFlyingSpeed);
                            }
                        }

                        flyingSpeed = currentFlyingSpeed * staminaFactor * carryingWeightFactor;
                        flyingVelocity = flyingDirection * flyingSpeed;

                        if (airflowSpeed > 0.01f)
                        {
                            airflowSpeed -= airflowFadeOutAcceleration * Time.deltaTime;

                            targetCharacterPosition = rootTransform.position + (flyingVelocity + Vector3.up * airflowSpeed) * Time.deltaTime;
                        }
                        else
                            targetCharacterPosition = rootTransform.position + flyingVelocity * Time.deltaTime;
                    }
                    else
                    {
                        currentStamina = Mathf.Clamp(currentStamina + staminaRecoverySpeed * Time.deltaTime, 0.0f, maximumStamina);

                        flyingSpeed = currentFlyingSpeed;
                        flyingVelocity = flyingDirection * flyingSpeed;

                        if (inAirflow)
                        {
                            airflowSpeed = Mathf.Clamp(airflowSpeed + airflowAcceleration * Time.deltaTime, 0.0f, airflowIntensity);

                            targetCharacterPosition = rootTransform.position + (flyingVelocity + Vector3.up * airflowSpeed) * Time.deltaTime;
                        }
                    }
                }

                rootTransform.position = targetCharacterPosition;
            }
        }
    }

    float RotationLerp(float angle, float targetAngle, float alpha)
    {
        if (angle > 270.0f && targetAngle < 90.0f)
        {
            increaseAngle = ((360.0f - angle) + targetAngle) * alpha;

            return (angle + increaseAngle) % 360.0f;
        }
        else if (angle < 90.0f && targetAngle > 270.0f)
        {
            increaseAngle = ((360.0f - targetAngle) + angle) * alpha;

            return (targetAngle + increaseAngle) % 360.0f;
        }
        else
        {
            increaseAngle = targetAngle - angle;

            if (targetAngle > angle && increaseAngle < 180.0f)
                return angle + increaseAngle * alpha;
            else
            {
                increaseAngle = angle - targetAngle;

                return angle - increaseAngle * alpha;
            }
        }
    }
}