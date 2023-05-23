using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class EagleController : MonoBehaviour
{
    private Transform characterTransform;

    public Transform springArmTransform;
    public Camera characterCamera;
    private Transform characterCameraTransform;

    public PostProcessVolume postProcessVolume;
    private DepthOfField depthOfField;
    private FloatParameter depthOfFieldFocusDistance;

    private CreatureFlyingSystem creatureFlyingSystem;

    public bool activated = false;

    public float cameraSpeed = 300.0f;

    // Mobile variables
    public bool mobileInputControl = false;
    public float mobileCameraSpeed = 300.0f;
    private float screenCenterX;

    [Range(0.0f, 100.0f)]
    public float springArmSmoothingFactor = 0.25f;

    public float groundMovementSpeed = 1.0f;

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
                creatureFlyingSystem.Grab();
            else
                creatureFlyingSystem.TakeOff();
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
            characterCameraTransform.localPosition = Vector3.Lerp(characterCameraTransform.localPosition, new Vector3(0.0f, 0.75f, -7.0f), 0.95f * Time.deltaTime);

            depthOfFieldFocusDistance.value = 10.0f - 7.0f * Mathf.Clamp(characterCameraTransform.localPosition.z / -7.0f, 0.0f, 1.0f);
            depthOfField.focusDistance.value = depthOfFieldFocusDistance;
        }
        else
        {
            characterCameraTransform.localPosition = Vector3.Lerp(characterCameraTransform.localPosition, new Vector3(0.0f, 0.75f, -3.0f), 0.5f * Time.deltaTime);

            depthOfFieldFocusDistance.value = 3.0f + 7.0f * Mathf.Clamp(characterCameraTransform.localPosition.z / -3.0f, 0.0f, 1.0f);
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
}