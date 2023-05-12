using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ThirdPersonBirdController : MonoBehaviour
{
    private Transform characterTransform;

    private Transform springArmTransform;
    private Transform cameraTransform;

    public PostProcessVolume postProcessVolume;
    private DepthOfField depthOfField;
    private FloatParameter depthOfFieldFocusDistance;

    public CreatureFlyingSystem creatureFlyingSystem;

    public float cameraSpeed = 300.0f;

    public float groundMovementSpeed = 1.0f;

    void Start()
    {
        characterTransform = this.transform;

        springArmTransform = this.transform.GetChild(0).transform;
        springArmTransform.parent = null;
        cameraTransform = springArmTransform.GetChild(0).transform;
        
        postProcessVolume.profile.TryGetSettings<DepthOfField>(out depthOfField);
        
        depthOfFieldFocusDistance = new FloatParameter();
    }

    void Update()
    {
        if (Settings.enabledControl)
        {
            CameraControlLogic();

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
            else if (Input.GetKey(KeyCode.S))
                creatureFlyingSystem.slowingDown = false;

            // Boost on / off
            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
                creatureFlyingSystem.boosting = !creatureFlyingSystem.boosting;

            // Turn left / right
            creatureFlyingSystem.AddHorizontalMovement(Input.GetAxis("Mouse X"));

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
    }

    void CameraControlLogic()
    {
        springArmTransform.position = characterTransform.position;
        springArmTransform.rotation = Quaternion.Euler(springArmTransform.rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime, springArmTransform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime, 0.0f);
    }
}