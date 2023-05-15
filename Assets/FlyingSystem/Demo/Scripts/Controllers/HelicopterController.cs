using UnityEngine;

public class HelicopterController : MonoBehaviour
{
    private Transform characterTransform;

    public Transform springArmTransform;
    public Transform cameraTransform;

    public Transform rollRootTransform;

    private AirTransportationFlyingSystem airTransportationFlyingSystem;

    public bool activated = false;

    public bool mobileInputControl = false;

    public float cameraSpeed = 300.0f;

    [Range(0.0f, 100.0f)]
    public float springArmSmoothingFactor = 0.25f;

    public float groundMovementSpeed = 100.0f;

    private bool draggingMouse = false;

    private float accumulatedDeltaMousePositionX, accumulatedDeltaMousePositionY;

    private bool mobileUIVisible = false;

    void Start()
    {
        characterTransform = this.transform;

        airTransportationFlyingSystem = this.GetComponent<AirTransportationFlyingSystem>();
    }

    void Update()
    {
        if (Settings.enabledControl && activated)
        {
            if (!draggingMouse)
                CameraControlLogic();

            if (!mobileInputControl)
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
            accumulatedDeltaMousePositionX += Input.GetAxis("Mouse X");
            accumulatedDeltaMousePositionY += Input.GetAxis("Mouse Y");

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

    }

    void CameraControlLogic()
    {
        // Camera view follows the roll
        springArmTransform.rotation = Quaternion.Euler(springArmTransform.rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime, springArmTransform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime, 0.0f);
    }
}