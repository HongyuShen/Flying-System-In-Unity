using UnityEngine;

public class AircraftController : MonoBehaviour
{
    private Transform characterTransform;

    public Transform springArmTransform;
    public Transform cameraTransform;

    public Transform rollRootTransform;

    private AirTransportationFlyingSystem airTransportationFlyingSystem;

    public bool possessed = false;

    public float cameraSpeed = 300.0f;

    public float groundMovementSpeed = 100.0f;

    void Start()
    {
        characterTransform = this.transform;

        airTransportationFlyingSystem = this.GetComponent<AirTransportationFlyingSystem>();
    }

    void Update()
    {
        if (Settings.enabledControl && possessed)
        {
            CameraControlLogic();

            if (!airTransportationFlyingSystem.inAir)
            {
                if (Input.GetKeyUp(KeyCode.Space) && !airTransportationFlyingSystem.inAir)
                    airTransportationFlyingSystem.TakeOff();
            }
            else
            {
                if (Input.GetKey(KeyCode.A))
                    airTransportationFlyingSystem.AddHorizontalMovement(-1.0f);
                else if (Input.GetKey(KeyCode.D))
                    airTransportationFlyingSystem.AddHorizontalMovement(1.0f);

                if (Input.GetKeyUp(KeyCode.A))
                    airTransportationFlyingSystem.StopHorizontalMovement();
                else if (Input.GetKeyUp(KeyCode.D))
                    airTransportationFlyingSystem.StopHorizontalMovement();

                if (Input.GetKey(KeyCode.Q))
                    airTransportationFlyingSystem.AddVerticalMovement(-1.0f);
                else if (Input.GetKey(KeyCode.E))
                    airTransportationFlyingSystem.AddVerticalMovement(1.0f);

                if (Input.GetKey(KeyCode.Z))
                    airTransportationFlyingSystem.AddRollMovement(-1.0f);
                else if (Input.GetKey(KeyCode.C))
                    airTransportationFlyingSystem.AddRollMovement(1.0f);
            }

            if (Input.GetKey(KeyCode.W))
                airTransportationFlyingSystem.MoveForward();
            else if (Input.GetKey(KeyCode.S))
                airTransportationFlyingSystem.SlowDown();
            else if (Input.GetKeyUp(KeyCode.S))
                airTransportationFlyingSystem.slowingDown = false;
        }
    }

    void CameraControlLogic()
    {
        springArmTransform.position = characterTransform.position;

        // Camera view follows the roll
        springArmTransform.rotation = Quaternion.Euler(springArmTransform.rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime, springArmTransform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime, rollRootTransform.rotation.eulerAngles.z);
    }
}