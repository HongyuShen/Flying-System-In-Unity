using UnityEngine;

public class CreatureFlyingSystem : MonoBehaviour
{
    [Header("Object References")]
    public Transform rootTransform;
    public Transform meshTransform;
    public Transform cameraTransform;

    [Header("Camera Attributes")]
    public float cameraLookAtOffsetY = 0.5f;

    [Header("Creature Attributes")]
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

    public bool canDive = true;
    [Range(0.0f, 90.0f)]
    public float divingStartAngle = 30.0f;
    public bool canFlyInAnyDirection = false;

    [Header("Custom Attributes")]
    public float g = 9.8f;
    public bool calculateStaminaConsumption = true;
    public float maximumStamina = 200.0f;
    public float staminaDecreaseSpeed = 0.25f;
    public float staminaDecreaseSpeedWhenBoosting = 1.5f;
    public AnimationCurve speedTirednessRatioAnimationCurve;

    public bool calculateCarryingWeight = true;
    public float maximumCarryingWeight = 100.0f;
    public AnimationCurve speedCarryingWeightRatioAnimationCurve;

    // Flying attributes
    [HideInInspector]
    public bool thirdPersonViewMode = true;

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
    private float idealFlyingSpeed;

    // Rotation Lerp variables
    private float increaseAngle;

    [HideInInspector]
    public float currentStamina;
    private float staminaFactor = 1.0f;

    [HideInInspector]
    public float currentCarryingWeight;
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

    public void SwitchToFirstPersonViewMode()
    {
        thirdPersonViewMode = false;
    }

    public void SwitchToThirdPersonViewMode()
    {
        thirdPersonViewMode = true;
    }

    public Quaternion GetRotation()
    {
        return meshTransform.rotation;
    }

    public void TakeOff()
    {
        inAir = true;
    }

    public void Grab()
    {

    }

    public void Land()
    {
        inAir = false;
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

    public void StopSlowingDown() {
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
            flyingDirection = (rootTransform.position + new Vector3(0.0f, cameraLookAtOffsetY, 0.0f) - cameraTransform.position).normalized;

            targetMeshLocalRotation = Quaternion.LookRotation(flyingDirection, meshTransform.up);

            relativeRotationY = 0.0f;
            rotationYAlignmentPercentage = 0.0f;

            if (meshTransform.localRotation.eulerAngles.y > 270.0f && targetMeshLocalRotation.eulerAngles.y < 90.0f)
                deltaRotationY = 360.0f - meshTransform.localRotation.eulerAngles.y + targetMeshLocalRotation.eulerAngles.y;
            else if (meshTransform.localRotation.eulerAngles.y < 90.0f && targetMeshLocalRotation.eulerAngles.y > 270.0f)
                deltaRotationY = 360.0f - targetMeshLocalRotation.eulerAngles.y + meshTransform.localRotation.eulerAngles.y;
            else
                deltaRotationY = Mathf.Abs(targetMeshLocalRotation.eulerAngles.y - meshTransform.localRotation.eulerAngles.y);

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

    public void Fly()
    {
        if (enabledFlyingLogic)
        {
            if (inAir)
            {
                if (!slowingDown)
                {
                    rotationYComponent = meshHorizontalTurningSpeed * Time.deltaTime;

                    currentMeshLocalRotationY = RotationLerp(meshTransform.localRotation.eulerAngles.y, targetMeshLocalRotation.eulerAngles.y, rotationYComponent);

                    if (!alignedToTargetDirection)
                    {
                        relativeRotationY += increaseAngle;

                        rotationYAlignmentPercentage = Mathf.Clamp(relativeRotationY / deltaRotationY, 0.0f, 1.0f);

                        if (rotationYAlignmentPercentage > 0.985f)
                            alignedToTargetDirection = true;

                        targetMeshLocalRotationZ = RotationLerp(meshTransform.localRotation.eulerAngles.z, turningDirection * meshMaximumTurningRotationZ, rotationYAlignmentPercentage * meshRotationZSmoothingFactor);
                    }
                    else
                    {
                        targetMeshLocalRotationZ = RotationLerp(meshTransform.localRotation.eulerAngles.z, 0.0f, rotationYComponent);
                    }

                    meshTransform.localRotation = Quaternion.Euler(RotationLerp(meshTransform.localRotation.eulerAngles.x, targetMeshLocalRotation.eulerAngles.x, meshVerticalTurningSpeed * Time.deltaTime), currentMeshLocalRotationY, targetMeshLocalRotationZ);
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
                if (meshTransform.localRotation.eulerAngles.x < 90.0f && meshTransform.localRotation.eulerAngles.x > divingStartAngle)
                {
                    diving = true;

                    if (boosting)
                        currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, maximumFlyingSpeed);
                    else if (slowingDown || fullStop)
                    {
                        currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed - slowDownAcceleration * Time.deltaTime, 0.0f, currentFlyingSpeed);
                        verticalAcceleration = slowDownAcceleration - g * Mathf.Cos((90.0f - meshTransform.localRotation.eulerAngles.x) * Mathf.Deg2Rad);
                    }
                    else
                    {
                        idealFlyingSpeed = normalFlyingSpeed + g * Mathf.Sin(meshTransform.localRotation.eulerAngles.x * Mathf.Deg2Rad);

                        if (currentFlyingSpeed < idealFlyingSpeed)
                            currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, idealFlyingSpeed);
                        else
                            currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed - slowDownAcceleration * Time.deltaTime, idealFlyingSpeed, maximumFlyingSpeed);

                        verticalAcceleration = -g * Mathf.Cos((90.0f - meshTransform.localRotation.eulerAngles.x) * Mathf.Deg2Rad);
                    }

                    currentFlyingSpeed *= staminaFactor * carryingWeightFactor;

                    flyingVelocity = meshTransform.forward * currentFlyingSpeed;

                    verticalSpeed = verticalSpeed + verticalAcceleration * Time.deltaTime;

                    targetCharacterPosition = rootTransform.position + flyingVelocity * Time.deltaTime + new Vector3(0.0f, Mathf.Clamp(verticalSpeed * Time.deltaTime - 0.5f * verticalAcceleration * Time.deltaTime * Time.deltaTime, -99999999.0f, 0.0f), 0.0f);
                }
                else
                {
                    diving = false;

                    if (verticalSpeed < -0.1f)
                        verticalSpeed += airDrag * Time.deltaTime;
                    else
                        verticalSpeed = 0.0f;

                    if (boosting)
                    {
                        if (calculateStaminaConsumption)
                            currentStamina -= staminaDecreaseSpeedWhenBoosting * Time.deltaTime;

                        currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, maximumFlyingSpeed);
                    }
                    else if (slowingDown || fullStop)
                    {
                        if (calculateStaminaConsumption)
                            currentStamina -= staminaDecreaseSpeedWhenBoosting * Time.deltaTime;

                        currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed - slowDownAcceleration * Time.deltaTime, 0.0f, currentFlyingSpeed);
                    }
                    else
                    {
                        if (calculateStaminaConsumption)
                            currentStamina -= staminaDecreaseSpeed * Time.deltaTime;

                        idealFlyingSpeed = normalFlyingSpeed + verticalSpeed;

                        if (currentFlyingSpeed < idealFlyingSpeed)
                            currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, idealFlyingSpeed);
                        else
                            currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed - slowDownAcceleration * Time.deltaTime, idealFlyingSpeed, maximumFlyingSpeed);
                    }

                    currentFlyingSpeed *= staminaFactor * carryingWeightFactor;

                    flyingVelocity = meshTransform.forward * currentFlyingSpeed;

                    targetCharacterPosition = rootTransform.position + flyingVelocity * Time.deltaTime;
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