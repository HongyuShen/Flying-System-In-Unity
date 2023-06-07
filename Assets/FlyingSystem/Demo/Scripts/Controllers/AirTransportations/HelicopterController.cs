using UnityEngine;

public class HelicopterController : MonoBehaviour
{
    public Transform springArmTransform;
    public Camera characterCamera;

    public Rigidbody rootRigidbody;

    public Transform topRotorTransform;
    public Transform tailRotorTransform;

    private HelicopterFlyingSystem helicopterFlyingSystem;

    public bool activated = false;

    public float cameraSpeed = 300.0f;

    // Mobile variables
    public bool mobileInputControl = false;
    public float mobileCameraSpeed = 300.0f;
    private float screenCenterX;

    public bool canMoveOnGround = false;
    public float groundMovementSpeed = 100.0f;

    private bool draggingMouse = false;

    private float accumulatedDeltaMousePositionX, accumulatedDeltaMousePositionY;

    [Header("Mobile")]
    public Joystick joystick;

    void Start()
    {
        if (activated)
            Activate();

        helicopterFlyingSystem = this.GetComponent<HelicopterFlyingSystem>();

        screenCenterX = screenCenterX = Screen.width / 2.0f;
    }

    void Update()
    {
        if (activated)
        {
            if (!mobileInputControl)
            {
                if (!draggingMouse)
                    PCCameraControlLogic();

                PCInputControlLogic();
            }
            else
            {
                MobileCameraControlLogic();
                MobileInputControlLogic();
            }

            topRotorTransform.Rotate(Vector3.forward * 1280.0f * Time.deltaTime);
            tailRotorTransform.Rotate(Vector3.forward * 1280.0f * Time.deltaTime);
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
        springArmTransform.rotation = Quaternion.Euler(springArmTransform.rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime, springArmTransform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime, 0.0f);
    }

    void MobileCameraControlLogic()
    {
        // Temporarily use mouse to simulate the touch
        if (Input.GetMouseButton(0) && Input.mousePosition.x > screenCenterX)
        {
            springArmTransform.Rotate(Vector3.up * mobileCameraSpeed * Input.GetAxis("Mouse X") * Time.deltaTime);
            springArmTransform.Rotate(-Vector3.right * mobileCameraSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime);
        }

        // Only detects on mobile devices
        if (Input.touchCount > 0)
        {
            for (var i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).position.x > screenCenterX && Input.GetTouch(i).phase == TouchPhase.Moved)
                {
                    springArmTransform.Rotate(Vector3.up * mobileCameraSpeed * Input.GetTouch(i).deltaPosition.x * Time.deltaTime);
                    springArmTransform.Rotate(-Vector3.right * mobileCameraSpeed * Input.GetTouch(i).deltaPosition.y * Time.deltaTime);
                }
            }
        }
    }

    void PCInputControlLogic()
    {
        // Hold down to turn left / right
        if (Input.GetKey(KeyCode.A))
            helicopterFlyingSystem.AddYawInput(-1.0f);
        else if (Input.GetKey(KeyCode.D))
            helicopterFlyingSystem.AddYawInput(1.0f);

        // Hold down to ascend / descend
        if (Input.GetKey(KeyCode.W))
            Ascend();
        else if (Input.GetKey(KeyCode.S))
            Descend();

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
            helicopterFlyingSystem.VerticalSlowDown();

        // Hold down mouse left button and drag to move
        if (Input.GetMouseButtonDown(0))
            draggingMouse = true;

        if (draggingMouse)
        {
            accumulatedDeltaMousePositionX += Mathf.Clamp(Input.GetAxis("Mouse X"), -1.0f, 1.0f);
            accumulatedDeltaMousePositionY += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1.0f, 1.0f);

            helicopterFlyingSystem.AddHorizontalInput(new Vector2(accumulatedDeltaMousePositionX, accumulatedDeltaMousePositionY));
        }

        if (Input.GetMouseButtonUp(0))
        {
            draggingMouse = false;
            accumulatedDeltaMousePositionX = 0.0f;
            accumulatedDeltaMousePositionY = 0.0f;
            helicopterFlyingSystem.StopYawInput();
        }

        // Boost on / off
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            helicopterFlyingSystem.boosting = !helicopterFlyingSystem.boosting;
    }

    void MobileInputControlLogic()
    {
        if (joystick != null)
        {
            if (joystick.isMoving)
            {
                draggingMouse = true;
                accumulatedDeltaMousePositionX += joystick.inputAxisX;
                accumulatedDeltaMousePositionY += joystick.inputAxisY;

                helicopterFlyingSystem.AddHorizontalInput(new Vector2(accumulatedDeltaMousePositionX, accumulatedDeltaMousePositionY));
            }
            else
            {
                if (draggingMouse)
                {
                    draggingMouse = false;
                    accumulatedDeltaMousePositionX = 0.0f;
                    accumulatedDeltaMousePositionY = 0.0f;
                    helicopterFlyingSystem.StopYawInput();
                }
            }
        }
    }

    public bool TurnOnOrOff()
    {
        if (helicopterFlyingSystem.isTurnedOff)
            helicopterFlyingSystem.TurnOn();
        else
            helicopterFlyingSystem.TurnOff();

        return helicopterFlyingSystem.isTurnedOff;
    }

    public void MobileTurnLeft()
    {
        helicopterFlyingSystem.AddYawInput(-1.0f);
    }

    public void MobileTurnRight()
    {
        helicopterFlyingSystem.AddYawInput(1.0f);
    }

    public void Ascend()
    {
        if (rootRigidbody.useGravity)
            rootRigidbody.useGravity = false;

        helicopterFlyingSystem.AddVerticalInput(1.0f);
    }

    public void Descend()
    {
        if (rootRigidbody.useGravity)
            rootRigidbody.useGravity = false;

        rootRigidbody.velocity = Vector3.zero;

        helicopterFlyingSystem.AddVerticalInput(-1.0f);
    }

    public void MobileStopAscendOrDescend()
    {
        helicopterFlyingSystem.VerticalSlowDown();
    }
}