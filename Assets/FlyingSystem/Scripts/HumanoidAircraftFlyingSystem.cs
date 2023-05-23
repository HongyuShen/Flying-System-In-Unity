using UnityEngine;

public class HumanoidAircraftFlyingSystem : MonoBehaviour
{
    [Header("Object References")]
    public Transform rootTransform;

    public Transform springArmTransform;
    public Transform springArmLocalReferenceTransform;
    public Transform cameraTransform;

    public Transform rollRootTransform;
    public Transform meshTransform;

    [Header("Camera Attributes")]
    public float cameraLookAtOffsetY = 0.5f;

    [Header("Flying Mode Attributes")]
    public bool freezeMeshRotationX = false;

    [Header("General Attributes")]
    public float normalFlyingSpeed = 20.0f;
    public float maximumFlyingSpeed = 35.0f;
    public float boostAcceleration = 12.0f;
    public float slowDownAcceleration = 10.0f;
    [Range(0.0f, 10.0f)]
    public float airDrag = 2.5f;

    [Header("Turning Attributes")]
    [Range(0.0f, 100.0f)]
    public float meshYawTurningSmoothingFactor = 4.0f;

    [Header("Horizontal Movement Attributes")]
    public float maximumMeshPitchAngle = 25.0f;
    [Range(0.0f, 100.0f)]
    public float meshPitchTurningSmoothingFactor = 2.0f;
    [Range(0.0f, 100.0f)]
    public float meshPitchTurningRecoverySmoothingFactor = 0.5f;

    public float maximumMeshRollAngle = 25.0f;
    [Range(0.0f, 100.0f)]
    public float meshRollTurningSmoothingFactor = 2.0f;
    [Range(0.0f, 100.0f)]
    public float meshRollTurningRecoverySmoothingFactor = 0.5f;

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
    public float maximumCarryingWeight = 1000.0f;
    public AnimationCurve speedCarryingWeightRatioAnimationCurve;

    // Flying attributes
    [HideInInspector]
    public bool isTurnedOff = false;

    [HideInInspector]
    public bool enabledFlyingLogic = true;

    [HideInInspector]
    public bool inAir = false;

    [HideInInspector]
    public bool hoverMode = false;

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

    // Turning variables
    private float targetMeshLocalRotationX, targetMeshLocalRotationY, targetMeshLocalRotationZ;

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
        flyingDirection.x += value;
    }

    public void AddPitchInput(float value)
    {
        flyingDirection.y += value;
    }

    public void AddWeight(float increaseValue)
    {
        currentCarryingWeight += Mathf.Clamp(currentCarryingWeight + increaseValue, 0.0f, maximumCarryingWeight);
    }

    void Fly()
    {
        if (enabledFlyingLogic)
        {
            if (Mathf.Abs(flyingDirection.x) > 0.01f || Mathf.Abs(flyingDirection.y) > 0.01f)
            {
                // Rotation
                flyingDirection = flyingDirection.normalized;

                if (flyingDirection.x > 0.0f)
                    targetMeshLocalRotationZ = -maximumMeshRollAngle;
                else
                    targetMeshLocalRotationZ = maximumMeshRollAngle;

                targetMeshLocalRotationZ *= Mathf.Abs(flyingDirection.x);

                if (!freezeMeshRotationX)
                {
                    if (DeltaAngle(springArmTransform.rotation.eulerAngles.y, meshTransform.rotation.eulerAngles.y) < 90.0f)
                    {
                        if (flyingDirection.y > 0.0f)
                            targetMeshLocalRotationX = maximumMeshPitchAngle;
                        else
                            targetMeshLocalRotationX = -maximumMeshPitchAngle;
                    }
                    else
                    {
                        if (flyingDirection.y > 0.0f)
                            targetMeshLocalRotationX = -maximumMeshPitchAngle;
                        else
                            targetMeshLocalRotationX = maximumMeshPitchAngle;
                    }

                    targetMeshLocalRotationX *= Mathf.Abs(flyingDirection.y);
                }
                else
                {
                    targetMeshLocalRotationX = 0.0f;
                }

                targetMeshLocalRotationY = springArmTransform.rotation.eulerAngles.y;

                rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, targetMeshLocalRotationY, rollRootTransform.localRotation.eulerAngles.z), meshYawTurningSmoothingFactor * Time.deltaTime);
                rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, rollRootTransform.localRotation.eulerAngles.y, targetMeshLocalRotationZ), meshRollTurningSmoothingFactor * Time.deltaTime);
                meshTransform.localRotation = Quaternion.Lerp(meshTransform.localRotation, Quaternion.Euler(targetMeshLocalRotationX, 0.0f, 0.0f), meshPitchTurningSmoothingFactor * Time.deltaTime);

                // Position
                springArmLocalReferenceTransform.localPosition = new Vector3(flyingDirection.x, 0.0f, flyingDirection.y) * 10000.0f;

                flyingDirection = (springArmLocalReferenceTransform.position - rootTransform.position);

                if (!hoverMode)
                    flyingDirection.y = 0.0f;

                flyingDirection = flyingDirection.normalized;

                rootTransform.position += flyingDirection * normalFlyingSpeed * Time.deltaTime;

                flyingDirection = Vector3.zero;
            }
            else
            {
                rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, rollRootTransform.localRotation.eulerAngles.y, 0.0f), meshRollTurningRecoverySmoothingFactor * Time.deltaTime);
                meshTransform.localRotation = Quaternion.Lerp(meshTransform.localRotation, Quaternion.Euler(0.0f, 0.0f, 0.0f), meshPitchTurningRecoverySmoothingFactor * Time.deltaTime);
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