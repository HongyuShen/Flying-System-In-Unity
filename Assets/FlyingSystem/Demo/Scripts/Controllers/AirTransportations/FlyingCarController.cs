using UnityEngine;

public class FlyingCarController : MonoBehaviour
{
    public Transform springArmTransform;
    public Camera characterCamera;

    public Rigidbody rootRigidbody;

    private FlyingVehicleFlyingSystem flyingVehicleFlyingSystem;

    public bool activated = false;

    public float cameraSpeed = 300.0f;

    public bool controlYawAndRollSeparately = true;

    // Mobile variables
    public bool mobileInputControl = false;
    public float mobileCameraSpeed = 300.0f;
    private float screenCenterX;

    [Header("Mobile")]
    public Joystick joystick;

    private float targetSpringArmRotationX, targetSpringArmRotationY;

    void Start()
    {
        if (activated)
            Activate();

        flyingVehicleFlyingSystem = this.GetComponent<FlyingVehicleFlyingSystem>();

        screenCenterX = screenCenterX = Screen.width / 2.0f;
    }

    void Update()
    {
        if (activated)
        {
            if (!mobileInputControl)
            {
                PCCameraControlLogic();
                PCInputControlLogic();
            }
            else
            {
                MobileCameraControlLogic();
                MobileInputControlLogic();
            }
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

    void PCCameraControlLogic()
    {
        targetSpringArmRotationX = springArmTransform.rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime;
        targetSpringArmRotationY = springArmTransform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime;

        springArmTransform.rotation = Quaternion.Euler(targetSpringArmRotationX, targetSpringArmRotationY, 0.0f);
    }

    void MobileCameraControlLogic()
    {

    }

    void PCInputControlLogic()
    {
        if (Input.GetKey(KeyCode.W))
            flyingVehicleFlyingSystem.AddForwardInput(1.0f);
        else if (Input.GetKey(KeyCode.S))
            flyingVehicleFlyingSystem.AddForwardInput(-1.0f);

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
            flyingVehicleFlyingSystem.StopMovingForward();

        // Hold down to turn left / right
        if (Input.GetKey(KeyCode.A))
            flyingVehicleFlyingSystem.AddYawInput(-1.0f);
        else if (Input.GetKey(KeyCode.D))
            flyingVehicleFlyingSystem.AddYawInput(1.0f);

        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
            flyingVehicleFlyingSystem.StopTurning();

        // Point up / down
        if (Input.GetKey(KeyCode.Q))
            flyingVehicleFlyingSystem.AddPitchInput(-1.0f);
        else if (Input.GetKey(KeyCode.E))
            flyingVehicleFlyingSystem.AddPitchInput(1.0f);

        if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E))
            flyingVehicleFlyingSystem.StopLookingUp();

        // Boost on / off
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            flyingVehicleFlyingSystem.boosting = !flyingVehicleFlyingSystem.boosting;
    }

    void MobileInputControlLogic()
    {

    }
}