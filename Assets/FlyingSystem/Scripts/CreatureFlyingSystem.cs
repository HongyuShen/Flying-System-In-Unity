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
    public float normalFlyingSpeed = 15.0f;
    public float maximumFlyingSpeed = 40.0f;
    public float boostAcceleration = 12.0f;
    public float slowDownAcceleration = 6.0f;
    [Range(0.0f, 10.0f)]
    public float airDrag = 2.5f;
    public float horizontalTurningSpeed = 2.0f;
    public float verticalTurningSpeed = 10.0f;
    public float maximumTurningRotationZ = 30.0f;
    [Range(0.0f, 1.0f)]
    public float rotationZSmoothingFactor = 0.25f;

    public bool canDive = true;
    [Range(0.0f, 90.0f)]
    public float divingStartAngle = 20.0f;
    public bool canFlyInAnyDirection = false;

    [Header("Custom Attributes")]
    public float g = 9.8f;
    public bool calculateStaminaConsumption = true;
    public AnimationCurve speedStaminaRatioAnimationCurve;

    public bool calculateCarryingWeight = true;
    public float maximumCarryingWeight = 100.0f;
    public AnimationCurve speedCarryingWeightRatioAnimationCurve;

    // Flying attributes
    [HideInInspector]
    public bool enabledFlyingLogic = true;

    [HideInInspector]
    public bool inAir = false;

    [HideInInspector]
    public Vector3 flyingDirection;

    [HideInInspector]
    private float currentFlyingSpeed;

    [HideInInspector]
    public Vector3 flyingVelocity;

    [HideInInspector]
    public bool flyingInNormalSpeed = false;
    [HideInInspector]
    public bool boosting = false;
    [HideInInspector]
    public bool slowingDown = false, stop = false;

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
        stop = false;
    }

    public void SlowDown()
    {
        boosting = false;
        flyingInNormalSpeed = false;
        slowingDown = true;
        stop = false;
    }

    public void FullStop()
    {
        boosting = false;
        flyingInNormalSpeed = false;
        slowingDown = false;
        stop = true;
    }

    public void AddHorizontalMovement(float value)
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
                    rotationYComponent = horizontalTurningSpeed * Time.deltaTime;

                    currentMeshLocalRotationY = RotationLerp(meshTransform.localRotation.eulerAngles.y, targetMeshLocalRotation.eulerAngles.y, rotationYComponent);

                    if (!alignedToTargetDirection)
                    {
                        relativeRotationY += increaseAngle;

                        rotationYAlignmentPercentage = Mathf.Clamp(relativeRotationY / deltaRotationY, 0.0f, 1.0f);

                        if (rotationYAlignmentPercentage > 0.985f)
                            alignedToTargetDirection = true;

                        targetMeshLocalRotationZ = RotationLerp(meshTransform.localRotation.eulerAngles.z, turningDirection * maximumTurningRotationZ, rotationYAlignmentPercentage * rotationZSmoothingFactor);
                    }
                    else
                    {
                        targetMeshLocalRotationZ = RotationLerp(meshTransform.localRotation.eulerAngles.z, 0.0f, rotationYComponent);
                    }

                    meshTransform.localRotation = Quaternion.Euler(RotationLerp(meshTransform.localRotation.eulerAngles.x, targetMeshLocalRotation.eulerAngles.x, verticalTurningSpeed * Time.deltaTime), currentMeshLocalRotationY, targetMeshLocalRotationZ);
                }

                // Diving
                if (meshTransform.localRotation.eulerAngles.x < 90.0f && meshTransform.localRotation.eulerAngles.x > divingStartAngle)
                {
                    diving = true;

                    if (boosting)
                        currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, maximumFlyingSpeed);
                    else if (slowingDown || stop)
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
                        currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, maximumFlyingSpeed);
                    else if (slowingDown || stop)
                        currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed - slowDownAcceleration * Time.deltaTime, 0.0f, currentFlyingSpeed);
                    else
                    {
                        idealFlyingSpeed = normalFlyingSpeed + verticalSpeed;

                        if (currentFlyingSpeed < idealFlyingSpeed)
                            currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + boostAcceleration * Time.deltaTime, 0.0f, idealFlyingSpeed);
                        else
                            currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed - slowDownAcceleration * Time.deltaTime, idealFlyingSpeed, maximumFlyingSpeed);
                    }
                    
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