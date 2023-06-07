using System.ComponentModel;
using UnityEngine;

public class GliderFlyingSystem : MonoBehaviour
{
    [Header("Object References")]
    public Transform rootTransform;
    public Transform rollRootTransform;
    public Transform meshRootTransform;

    [Header("Camera Attributes")]
    public bool cameraAlwaysLookAtMeshBack = false;

    [Header("General Attributes")]
    [Range(0.0f, 10.0f)]
    public float airDrag = 9.75f;

    [Header("Turning Attributes")]
    public float meshYawTurningSpeed = 45.0f;
    [Range(0.0f, 100.0f)]
    public float meshYawTurningSmoothingFactor = 2.0f;
    [Range(0.0f, 100.0f)]
    public float meshYawTurningRecoverySmoothingFactor = 0.5f;
    public float meshPitchTurningSpeed = 60.0f;
    [Range(0.0f, 100.0f)]
    public float meshPitchTurningSmoothingFactor = 4.0f;
    [Range(0.0f, 100.0f)]
    public float meshPitchTurningRecoverySmoothingFactor = 0.5f;
    public float maximumMeshRollAngle = 30.0f;
    public float meshRollTurningSpeed = 30.0f;
    [Range(0.0f, 100.0f)]
    public float meshRollTurningSmoothingFactor = 2.0f;
    [Range(0.0f, 100.0f)]
    public float meshRollTurningRecoverySmoothingFactor = 0.75f;

    [Header("Diving Attributes")]
    public bool canDive = true;
    public float maximumDivingSpeed = 500.0f;
    [Range(0.0f, 90.0f)]
    public float divingStartAngle = 30.0f;
    public float decelerationAfterDiving = 2.0f;

    [Header("Custom Attributes")]
    public float g = 9.8f;
    public bool calculateCarryingWeight = true;
    public float maximumCarryingWeight = 100.0f;
    public AnimationCurve speedCarryingWeightRatioAnimationCurve;

    // Flying attributes
    [HideInInspector]
    public bool isTurnedOff = false;

    [HideInInspector]
    public bool enabledFlyingLogic = true;

    [HideInInspector]
    public bool inAir = false;

    [HideInInspector]
    public bool inAirflow = false;

    [HideInInspector]
    public Vector3 flyingDirection;

    [HideInInspector]
    public float originalFlyingSpeed;

    [HideInInspector]
    public float flyingSpeed;

    [HideInInspector]
    public Vector3 flyingVelocity;

    private float currentFlyingSpeed;

    // Turning variables
    private float targetMeshLocalRotationX, targetMeshLocalRotationY, targetMeshLocalRotationZ;

    // Diving variables
    [HideInInspector]
    public bool diving = false;
    private float verticalAcceleration;
    private float verticalSpeed;
    private float sinComponent;

    // Airflow variables
    private float airflowSpeed;
    private float airflowIntensity;
    private float airflowAcceleration;
    private float airflowFadeOutAcceleration;

    [HideInInspector]
    public float currentCarryingWeight;
    private float carryingWeightFactor = 1.0f;

    void Update()
    {
        if (enabledFlyingLogic)
            Fly();
    }

    public void TakeOff(float launchSpeed)
    {
        originalFlyingSpeed = launchSpeed;
        currentFlyingSpeed = launchSpeed;

        inAir = true;
    }

    public void AddYawInput(float value)
    {
        targetMeshLocalRotationY += value * meshYawTurningSpeed * Time.deltaTime;
    }

    public void AddPitchInput(float value)
    {
        targetMeshLocalRotationX = Mathf.Clamp(targetMeshLocalRotationX + value * meshPitchTurningSpeed * Time.deltaTime, -89.995f, 89.995f);
    }

    public void AddRollInput(float value)
    {
        targetMeshLocalRotationZ = Mathf.Clamp(targetMeshLocalRotationZ - value * meshRollTurningSpeed * Time.deltaTime, -maximumMeshRollAngle, maximumMeshRollAngle);
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
                rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, targetMeshLocalRotationY, rollRootTransform.localRotation.eulerAngles.z), meshRollTurningSmoothingFactor * Time.deltaTime);
                rollRootTransform.localRotation = Quaternion.Lerp(rollRootTransform.localRotation, Quaternion.Euler(0.0f, rollRootTransform.localRotation.eulerAngles.y, targetMeshLocalRotationZ), meshRollTurningSmoothingFactor * Time.deltaTime);
                meshRootTransform.localRotation = Quaternion.Lerp(meshRootTransform.localRotation, Quaternion.Euler(targetMeshLocalRotationX, 0.0f, 0.0f), meshPitchTurningSmoothingFactor * Time.deltaTime);

                if (!inAirflow && canDive)
                {
                    if (meshRootTransform.localRotation.eulerAngles.x < 89.995f && meshRootTransform.localRotation.eulerAngles.x > divingStartAngle)
                    {
                        if (!diving)
                        {
                            diving = true;
                            originalFlyingSpeed = currentFlyingSpeed;
                        }

                        currentFlyingSpeed = Mathf.Clamp(currentFlyingSpeed + g * Mathf.Sin(meshRootTransform.localRotation.eulerAngles.x * Mathf.Deg2Rad) * Time.deltaTime, 0.0f, maximumDivingSpeed);
                    }
                    else
                    {
                        diving = false;

                        if (verticalSpeed < -0.1f)
                            verticalSpeed += airDrag * Mathf.Cos((meshRootTransform.localRotation.eulerAngles.x) * Mathf.Deg2Rad) * Time.deltaTime;
                        else
                            verticalSpeed = 0.0f;

                        if (meshRootTransform.localRotation.eulerAngles.x > 270.0f)
                        {
                            if (currentFlyingSpeed > originalFlyingSpeed)
                                currentFlyingSpeed -= (decelerationAfterDiving + decelerationAfterDiving * Mathf.Sin((360.0f - meshRootTransform.localRotation.eulerAngles.x) * Mathf.Deg2Rad)) * Time.deltaTime;
                        }
                        else
                        {
                            if (currentFlyingSpeed > originalFlyingSpeed)
                                currentFlyingSpeed -= decelerationAfterDiving * Time.deltaTime;
                        }
                    }
                }

                if (calculateCarryingWeight)
                    carryingWeightFactor = speedCarryingWeightRatioAnimationCurve.Evaluate(currentCarryingWeight / maximumCarryingWeight);
                else
                    carryingWeightFactor = 1.0f;

                flyingDirection = meshRootTransform.forward;

                if (inAirflow)
                {
                    flyingSpeed = currentFlyingSpeed;
                    flyingVelocity = flyingDirection * flyingSpeed;

                    airflowSpeed = Mathf.Clamp(airflowSpeed + airflowAcceleration * Time.deltaTime, 0.0f, airflowIntensity);

                    rootTransform.position += (flyingVelocity + Vector3.up * airflowSpeed) * Time.deltaTime;
                }
                else
                {
                    if (flyingDirection.y > 0.001f && currentFlyingSpeed < originalFlyingSpeed + 0.001f)
                        flyingDirection = new Vector3(flyingDirection.x, 0.0f, flyingDirection.z);

                    flyingSpeed = currentFlyingSpeed;
                    flyingVelocity = flyingDirection * flyingSpeed;

                    verticalAcceleration = -g + airDrag * Mathf.Cos((meshRootTransform.localRotation.eulerAngles.x) * Mathf.Deg2Rad);
                    verticalSpeed += verticalAcceleration * Time.deltaTime;

                    if (airflowSpeed > 0.01f)
                    {
                        airflowSpeed -= airflowFadeOutAcceleration * Time.deltaTime;

                        rootTransform.position += flyingVelocity * Time.deltaTime + new Vector3(0.0f, Mathf.Clamp(verticalSpeed * Time.deltaTime - 0.5f * verticalAcceleration * Time.deltaTime * Time.deltaTime, -99999999.0f, 0.0f), 0.0f) + (flyingVelocity + Vector3.up * airflowSpeed) * Time.deltaTime;
                    }
                    else
                        rootTransform.position += flyingVelocity * Time.deltaTime + new Vector3(0.0f, Mathf.Clamp(verticalSpeed * Time.deltaTime - 0.5f * verticalAcceleration * Time.deltaTime * Time.deltaTime, -99999999.0f, 0.0f), 0.0f);
                }
            }
        }
    }
}