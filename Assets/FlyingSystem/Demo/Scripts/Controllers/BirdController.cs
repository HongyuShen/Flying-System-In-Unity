using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class BirdController : MonoBehaviour
{
    private Transform characterTransform;

    public Transform springArmTransform;
    public Transform cameraTransform;

    public PostProcessVolume postProcessVolume;
    private DepthOfField depthOfField;
    private FloatParameter depthOfFieldFocusDistance;

    private CreatureFlyingSystem creatureFlyingSystem;

    public bool activated = false;

    private bool thirdPersonViewMode = true;

    public float cameraSpeed = 300.0f;

    [Range(0.0f, 100.0f)]
    public float springArmSmoothingFactor = 0.25f;

    public float groundMovementSpeed = 1.0f;

    void Start()
    {
        characterTransform = this.transform;

        postProcessVolume.profile.TryGetSettings<DepthOfField>(out depthOfField);

        depthOfFieldFocusDistance = new FloatParameter();

        creatureFlyingSystem = this.GetComponent<CreatureFlyingSystem>();
    }

    void Update()
    {
        if (Settings.enabledControl && activated)
        {
            CameraControlLogic();

            if (!Settings.mobileInputControl)
                PCInputControlLogic();
            else
                MobileInputControlLogic();

            //if (Input.GetKeyUp(KeyCode.M))
            //{
            //    mobileUIVisible = !mobileUIVisible;

            //    if(mobileUIVisible)

            //    else

            //}
        }
    }

    void PCInputControlLogic()
    {
        // Switch between first-person and third-person view mode
        if (Input.GetKeyUp(KeyCode.T))
        {
            thirdPersonViewMode = !thirdPersonViewMode;

            if (!thirdPersonViewMode)
                creatureFlyingSystem.SwitchToFirstPersonViewMode();
            else if (thirdPersonViewMode)
                creatureFlyingSystem.SwitchToThirdPersonViewMode();
        }

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

        // Boost on / off
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            creatureFlyingSystem.boosting = !creatureFlyingSystem.boosting;

        // Turn left / right
        creatureFlyingSystem.AddYawInput(Input.GetAxis("Mouse X"));

        // Camera effect for diving
        if (creatureFlyingSystem.diving)
        {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, new Vector3(0.0f, 0.75f, -7.0f), 0.95f * Time.deltaTime);

            depthOfFieldFocusDistance.value = 10.0f - 7.0f * Mathf.Clamp(cameraTransform.localPosition.z / -7.0f, 0.0f, 1.0f);
            depthOfField.focusDistance.value = depthOfFieldFocusDistance;
        }
        else
        {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, new Vector3(0.0f, 0.75f, -3.0f), 0.5f * Time.deltaTime);

            depthOfFieldFocusDistance.value = 3.0f + 7.0f * Mathf.Clamp(cameraTransform.localPosition.z / -3.0f, 0.0f, 1.0f);
            depthOfField.focusDistance.value = depthOfFieldFocusDistance;
        }
    }

    void MobileInputControlLogic()
    {

    }

    void CameraControlLogic()
    {
        springArmTransform.position = Vector3.Lerp(characterTransform.position, springArmTransform.position, springArmSmoothingFactor * Time.deltaTime);
        springArmTransform.rotation = Quaternion.Euler(springArmTransform.rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime, springArmTransform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime, 0.0f);
    }
}