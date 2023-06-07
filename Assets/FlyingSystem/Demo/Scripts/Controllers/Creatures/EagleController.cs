using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class EagleController : MonoBehaviour
{
    private Transform characterTransform;
    public Transform meshRootTransform;

    public Transform springArmTransform;
    public Camera characterCamera;
    private Transform characterCameraTransform;

    public Animator animator;

    public TrailRenderer leftWingTrailRenderer, rightWingTrailRenderer;

    public PostProcessVolume postProcessVolume;
    private DepthOfField depthOfField;
    private FloatParameter depthOfFieldFocusDistance;

    private CreatureFlyingSystem creatureFlyingSystem;

    private Airflow airflow;

    public bool activated = false;

    public float cameraSpeed = 300.0f;

    // Mobile variables
    public bool mobileInputControl = false;
    public float mobileCameraSpeed = 300.0f;
    private float screenCenterX;

    [Range(0.0f, 100.0f)]
    public float springArmSmoothingFactor = 0.25f;

    public float normalCameraY = 3.0f, normalCameraZ = -12.0f;
    public float divingZoomOutY = 3.0f, divingZoomOutZ = -15.0f;

    private bool hideWingTrails = false;

    public bool isGrabbing = false;
    private Transform targetGrabObjectTransform;
    private Rigidbody targetGrabObjectRigidbody;

    void Start()
    {
        if (activated)
            Activate();

        characterTransform = this.transform;
        characterCameraTransform = characterCamera.transform;

        postProcessVolume.profile.TryGetSettings<DepthOfField>(out depthOfField);

        depthOfFieldFocusDistance = new FloatParameter();

        creatureFlyingSystem = this.GetComponent<CreatureFlyingSystem>();

        screenCenterX = screenCenterX = Screen.width / 2.0f;
    }

    void Update()
    {
        if (activated)
        {
            CameraControlLogic();

            if (!mobileInputControl)
                PCInputControlLogic();
            else
                MobileInputControlLogic();
        }
    }

    public void Activate()
    {
        activated = true;
        characterCamera.enabled = true;
        characterCamera.GetComponent<AudioListener>().enabled = true;
    }

    public void Deactivate()
    {
        activated = false;
        characterCamera.enabled = false;
        characterCamera.GetComponent<AudioListener>().enabled = false;
    }

    void PCInputControlLogic()
    {
        // Take off / grab
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (creatureFlyingSystem.inAir)
            {
                if (isGrabbing)
                    Drop();
            }
            else
            {
                creatureFlyingSystem.TakeOff();

                animator.SetBool("FlyToIdle", false);
                animator.SetBool("IdleToFly", true);

                animator.SetBool("GlideToIdle", false);
            }
        }

        // Fly forward / stop
        if (Input.GetKey(KeyCode.W))
            creatureFlyingSystem.FlyForward();
        else if (Input.GetKey(KeyCode.S))
            creatureFlyingSystem.SlowDown();
        else if (Input.GetKeyUp(KeyCode.S))
            creatureFlyingSystem.StopSlowingDown();

        // Turn left / right
        creatureFlyingSystem.AddYawInput(Input.GetAxis("Mouse X"));

        // Camera effect for diving
        if (creatureFlyingSystem.diving)
        {
            characterCameraTransform.localPosition = Vector3.Lerp(characterCameraTransform.localPosition, new Vector3(0.0f, divingZoomOutY, divingZoomOutZ), 0.95f * Time.deltaTime);

            animator.SetBool("FlyToGlide", true);
            animator.SetBool("GlideToFly", false);

            if (!leftWingTrailRenderer.enabled)
            {
                hideWingTrails = false;

                leftWingTrailRenderer.enabled = true;
                rightWingTrailRenderer.enabled = true;
            }

            depthOfFieldFocusDistance.value = 10.0f - 6.0f * Mathf.Clamp(characterCameraTransform.localPosition.z / -7.0f, 0.0f, 1.0f);
            depthOfField.focusDistance.value = depthOfFieldFocusDistance;
        }
        else
        {
            characterCameraTransform.localPosition = Vector3.Lerp(characterCameraTransform.localPosition, new Vector3(0.0f, normalCameraY, normalCameraZ), 0.5f * Time.deltaTime);

            animator.SetBool("GlideToFly", true);
            animator.SetBool("FlyToGlide", false);

            if (!hideWingTrails)
            {
                hideWingTrails = true;

                Invoke("LateHideWingTrails", 2.0f);
            }

            depthOfFieldFocusDistance.value = 4.0f + 6.0f * Mathf.Clamp(characterCameraTransform.localPosition.z / -3.0f, 0.0f, 1.0f);
            depthOfField.focusDistance.value = depthOfFieldFocusDistance;
        }

        // Boost on / off
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            creatureFlyingSystem.boosting = !creatureFlyingSystem.boosting;
    }

    void MobileInputControlLogic()
    {

    }

    void CameraControlLogic()
    {
        springArmTransform.position = Vector3.Lerp(characterTransform.position, springArmTransform.position, springArmSmoothingFactor * Time.deltaTime);
        springArmTransform.rotation = Quaternion.Euler(springArmTransform.rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime, springArmTransform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime, 0.0f);
    }

    public void Drop()
    {
        isGrabbing = false;

        targetGrabObjectTransform.SetParent(null);

        targetGrabObjectRigidbody.useGravity = true;
        targetGrabObjectRigidbody.isKinematic = false;

        creatureFlyingSystem.currentCarryingWeight -= 3.0f;
    }

    public void MobileTurnLeft()
    {

    }

    public void MobileTurnRight()
    {

    }

    public void MobileRollLeft()
    {

    }

    public void MobileRollRight()
    {

    }

    void LateHideWingTrails()
    {
        leftWingTrailRenderer.enabled = false;
        rightWingTrailRenderer.enabled = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "Road")
        {
            if (creatureFlyingSystem.inAir && !isGrabbing)
            {
                creatureFlyingSystem.Land();

                animator.SetBool("GlideToIdle", true);

                animator.SetBool("FlyToIdle", true);
                animator.SetBool("IdleToFly", false);

                animator.SetBool("FlyToGlide", false);

                LateHideWingTrails();
            }
        }
        else if (collision.collider.name == "Weight")
        {
            // Grab
            isGrabbing = true;

            targetGrabObjectTransform = collision.transform;

            targetGrabObjectRigidbody = targetGrabObjectTransform.GetComponent<Rigidbody>();
            targetGrabObjectRigidbody.useGravity = false;
            targetGrabObjectRigidbody.isKinematic = true;

            targetGrabObjectTransform.SetParent(meshRootTransform);
            targetGrabObjectTransform.localPosition = new Vector3(0.0f, -1.787f, -2.172f);

            creatureFlyingSystem.currentCarryingWeight += 3.0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Airflow")
        {
            airflow = other.GetComponent<Airflow>();

            creatureFlyingSystem.AddAirflowForce(airflow.intensity, airflow.acceleration, airflow.fadeOutAcceleration);
            creatureFlyingSystem.stopFlying = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.name == "Airflow")
        {
            creatureFlyingSystem.EndAirflowForce();
            creatureFlyingSystem.stopFlying = false;
        }
    }
}