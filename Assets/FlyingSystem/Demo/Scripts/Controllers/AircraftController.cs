using UnityEngine;

public class AircraftController : MonoBehaviour
{
    public Transform springArmTransform;
    public Transform cameraTransform;

    public Transform rollRootTransform;

    private AirTransportationFlyingSystem airTransportationFlyingSystem;

    public bool activated = false;

    public float cameraSpeed = 300.0f;

    [Range(0.0f, 100.0f)]
    public float springArmSmoothingFactor = 0.25f;

    public float groundMovementSpeed = 100.0f;

    void Start()
    {
        airTransportationFlyingSystem = this.GetComponent<AirTransportationFlyingSystem>();
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

        }
    }

    void PCInputControlLogic()
    {
        if (!airTransportationFlyingSystem.inAir)
        {
            if (Input.GetKeyUp(KeyCode.Space) && !airTransportationFlyingSystem.inAir)
                airTransportationFlyingSystem.TakeOff();
        }
        else
        {
            // Hold down to turn left / right
            if (Input.GetKey(KeyCode.A))
                airTransportationFlyingSystem.AddYawInput(-1.0f);
            else if (Input.GetKey(KeyCode.D))
                airTransportationFlyingSystem.AddYawInput(1.0f);

            if (Input.GetKeyUp(KeyCode.A))
                airTransportationFlyingSystem.StopYawInput();
            else if (Input.GetKeyUp(KeyCode.D))
                airTransportationFlyingSystem.StopYawInput();

            // Point down / up
            if (Input.GetKey(KeyCode.Q))
                airTransportationFlyingSystem.AddPitchInput(-1.0f);
            else if (Input.GetKey(KeyCode.E))
                airTransportationFlyingSystem.AddPitchInput(1.0f);

            // Roll to have a sharp turn left / right
            if (Input.GetKey(KeyCode.Z))
                airTransportationFlyingSystem.AddRollInput(-1.0f);
            else if (Input.GetKey(KeyCode.C))
                airTransportationFlyingSystem.AddRollInput(1.0f);
        }

        if (Input.GetKey(KeyCode.W))
            airTransportationFlyingSystem.MoveForward();
        else if (Input.GetKey(KeyCode.S))
            airTransportationFlyingSystem.SlowDown();
        else if (Input.GetKeyUp(KeyCode.S))
            airTransportationFlyingSystem.StopSlowingDown();
    }

    void MobileInputControlLogic()
    {

    }

    void CameraControlLogic()
    {
        // Camera view follows the roll
        springArmTransform.rotation = Quaternion.Euler(springArmTransform.rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime, springArmTransform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime, rollRootTransform.rotation.eulerAngles.z);
    }
}