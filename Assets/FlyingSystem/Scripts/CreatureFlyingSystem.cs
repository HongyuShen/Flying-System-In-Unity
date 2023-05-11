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
    public float flyingSpeed = 15.0f;
    public float maximumFlyingSpeed = 20.0f;
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
    [Range(0.0f, 90.0f)]
    public float divingEndAngle = 90.0f;
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
    public Vector3 flyingVelocity;

    [HideInInspector]
    public bool flappingWings = false;

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

    // Rotation Lerp variables
    private float increaseAngle;

    void Start()
    {

    }

    void Update()
    {
        Fly();
    }

    public void TakeOff()
    {
        inAir = true;
    }

    public void Grab()
    {

    }

    public void AddForwardMovement(float value)
    {
        flappingWings = true;

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

    void AddVerticalMovement(float value)
    {
        //targetMeshRotationX = targetMeshRotationX - verticalTurningSpeed * value;
        //targetMeshRotationX = Mathf.Clamp(targetMeshRotationX - verticalTurningSpeed * value, -maximumDivingAngle, maximumDivingAngle);
    }

    public void Fly()
    {
        if (enabledFlyingLogic)
        {
            if (inAir)
            {
                if (flappingWings)
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

                    // To maintain horizontal flying, lift must be equal to g
                    if (meshTransform.localRotation.eulerAngles.x < 90.0f && meshTransform.localRotation.eulerAngles.x > divingStartAngle)
                    {
                        diving = true;

                        flyingVelocity = meshTransform.forward * flyingSpeed;

                        verticalAcceleration = -g * Mathf.Cos((90.0f - meshTransform.localRotation.eulerAngles.x) * Mathf.Deg2Rad);
                        verticalSpeed += verticalAcceleration * Time.deltaTime;

                        targetCharacterPosition = rootTransform.position + flyingVelocity * Time.deltaTime + new Vector3(0.0f, verticalSpeed * Time.deltaTime - 0.5f * verticalAcceleration * Time.deltaTime * Time.deltaTime, 0.0f);
                    }
                    else
                    {
                        diving = false;
                            
                        if (verticalSpeed < -0.1f)
                            verticalSpeed += airDrag * Time.deltaTime;
                        else
                            verticalSpeed = 0.0f;

                        flyingVelocity = meshTransform.forward * flyingSpeed + new Vector3();
                        targetCharacterPosition = rootTransform.position + flyingVelocity * Time.deltaTime;
                        //targetCharacterPosition = Vector3.Lerp(targetCharacterPosition, rootTransform.position + flyingVelocity * Time.deltaTime, 0.1f * Time.deltaTime);
                    }

                    rootTransform.position = targetCharacterPosition;
                    //rootTransform.position = Vector3.Lerp(rootTransform.position, targetCharacterPosition, 0.125f);
                }
                else
                {

                }
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