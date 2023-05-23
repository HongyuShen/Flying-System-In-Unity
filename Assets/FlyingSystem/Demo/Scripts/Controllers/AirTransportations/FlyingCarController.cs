using UnityEngine;

public class FlyingCarController : MonoBehaviour
{
    public Transform springArmTransform;
    public Camera characterCamera;

    public Rigidbody rootRigidbody;

    private FlyingVehicleFlyingSystem flyingVehicleFlyingSystem;

    public bool activated = false;

    public float cameraSpeed = 300.0f;

    // Mobile variables
    public bool mobileInputControl = false;
    public float mobileCameraSpeed = 300.0f;
    private float screenCenterX;

    [Header("Mobile")]
    public Joystick joystick;

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
        if (Input.GetKey(KeyCode.A))
            flyingVehicleFlyingSystem.AddYawInput(-1.0f);
        else if (Input.GetKey(KeyCode.D))
            flyingVehicleFlyingSystem.AddYawInput(1.0f);

        
        if (Input.GetKey(KeyCode.W))
            flyingVehicleFlyingSystem.AddPitchInput(-1.0f);
        else if (Input.GetKey(KeyCode.S))
            flyingVehicleFlyingSystem.AddPitchInput(1.0f);
    }

    void MobileCameraControlLogic()
    {

    }

    void PCInputControlLogic()
    {

    }

    void MobileInputControlLogic()
    {

    }
}