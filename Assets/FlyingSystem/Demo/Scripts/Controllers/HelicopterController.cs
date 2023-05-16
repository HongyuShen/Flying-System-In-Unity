using UnityEngine;
using UnityEngine.UI;

public class HelicopterController : MonoBehaviour
{
    public Transform springArmTransform;
    public Transform cameraTransform;

    private AirTransportationFlyingSystem airTransportationFlyingSystem;

    public bool activated = false;

    public float cameraSpeed = 300.0f;

    [Range(0.0f, 100.0f)]
    public float springArmSmoothingFactor = 0.25f;

    public float groundMovementSpeed = 100.0f;

    private bool draggingMouse = false;

    private float accumulatedDeltaMousePositionX, accumulatedDeltaMousePositionY;

    [Header("Mobile")]
    public Joystick joystick;
    public Button ascendButton, descendButton;

    void Start()
    {
        airTransportationFlyingSystem = this.GetComponent<AirTransportationFlyingSystem>();
    }

    void Update()
    {
        if (Settings.enabledControl && activated)
        {
            if (!Settings.mobileInputControl)
            {
                if (!draggingMouse)
                    CameraControlLogic();

                PCInputControlLogic();
            }
            else
            {
                MobileInputControlLogic();
            }
        }
    }

    void PCInputControlLogic()
    {
        // Hold down to turn left / right
        if (Input.GetKey(KeyCode.A))
            airTransportationFlyingSystem.AddYawInput(-1.0f);
        else if (Input.GetKey(KeyCode.D))
            airTransportationFlyingSystem.AddYawInput(1.0f);

        // Hold down to ascend / descend
        if (Input.GetKey(KeyCode.W))
            airTransportationFlyingSystem.AddPitchInput(1.0f);
        else if (Input.GetKey(KeyCode.S))
            airTransportationFlyingSystem.AddPitchInput(-1.0f);

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
            airTransportationFlyingSystem.VerticalSlowDown();

        // Hold down mouse left button and drag to move
        if (Input.GetMouseButtonDown(0))
            draggingMouse = true;

        if (draggingMouse)
        {
            accumulatedDeltaMousePositionX += Mathf.Clamp(Input.GetAxis("Mouse X"), -1.0f, 1.0f);
            accumulatedDeltaMousePositionY += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1.0f, 1.0f);

            airTransportationFlyingSystem.AddHelicopterYawInput(new Vector2(accumulatedDeltaMousePositionX, accumulatedDeltaMousePositionY));
        }

        if (Input.GetMouseButtonUp(0))
        {
            draggingMouse = false;
            accumulatedDeltaMousePositionX = 0.0f;
            accumulatedDeltaMousePositionY = 0.0f;
            airTransportationFlyingSystem.StopYawInput();
        }
    }

    void MobileInputControlLogic()
    {
        if (joystick.isMoving)
        {
            draggingMouse = true;
            accumulatedDeltaMousePositionX += joystick.inputAxisX;
            accumulatedDeltaMousePositionY += joystick.inputAxisY;

            airTransportationFlyingSystem.AddHelicopterYawInput(new Vector2(accumulatedDeltaMousePositionX, accumulatedDeltaMousePositionY));
        }
        else
        {
            if (draggingMouse)
            {
                draggingMouse = false;
                accumulatedDeltaMousePositionX = 0.0f;
                accumulatedDeltaMousePositionY = 0.0f;
                airTransportationFlyingSystem.StopYawInput();
            }
        }
    }

    void CameraControlLogic()
    {
        // Camera view follows the roll
        springArmTransform.rotation = Quaternion.Euler(springArmTransform.rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime, springArmTransform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime, 0.0f);
    }

    public void MobileTurnLeft()
    {
        airTransportationFlyingSystem.AddYawInput(-1.0f);
    }

    public void MobileTurnRight()
    {
        airTransportationFlyingSystem.AddYawInput(1.0f);
    }

    public void MobileAscend()
    {
        airTransportationFlyingSystem.AddPitchInput(1.0f);
    }

    public void MobileDescend()
    {
        airTransportationFlyingSystem.AddPitchInput(-1.0f);
    }

    public void MobileStopAscendOrDescend()
    {
        airTransportationFlyingSystem.VerticalSlowDown();
    }
}