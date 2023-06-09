using UnityEngine;

public class AircraftController : MonoBehaviour
{
    public Transform rootTransform;
    public Transform springArmTransform;
    public Camera characterCamera;

    public Transform rollRootTransform;

    public Rigidbody rootRigidbody;

    private AircraftFlyingSystem aircraftFlyingSystem;

    public bool activated = false;

    public float cameraSpeed = 300.0f;

    // Mobile variables
    public bool mobileInputControl = false;
    public float mobileCameraSpeed = 300.0f;
    private float screenCenterX;

    [Header("General Attributes")]
    public bool takeOff;
    public bool landed = true;
    public float maximumGroundMovementSpeed = 80.0f;
    public float groundAcceleration = 20.0f;
    public float groundTurningSpeed = 10.0f;
    public float slowDownAccelerationAfterLanding = 10.0f;

    [HideInInspector]
    public float currentGroundMovementSpeed;

    private float targetSpringArmRotationX, targetSpringArmRotationY;

    private bool movingForward = false;

    void Start()
    {
        if (activated)
            Activate();

        aircraftFlyingSystem = this.GetComponent<AircraftFlyingSystem>();

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

            MovementLogic();
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

        // Camera view follows the roll
        springArmTransform.rotation = Quaternion.Euler(targetSpringArmRotationX, targetSpringArmRotationY, rollRootTransform.rotation.eulerAngles.z);
    }

    void MobileCameraControlLogic()
    {
        // Temporarily use mouse to simulate the touch
        if (Input.GetMouseButton(0) && Input.mousePosition.x > screenCenterX)
        {
            targetSpringArmRotationX = springArmTransform.rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * mobileCameraSpeed * Time.deltaTime;
            targetSpringArmRotationY = springArmTransform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * mobileCameraSpeed * Time.deltaTime;
        }
        else
        {
            targetSpringArmRotationX = springArmTransform.rotation.eulerAngles.x;
            targetSpringArmRotationY = springArmTransform.rotation.eulerAngles.y;
        }

        // Only detects on mobile devices
        //if (Input.touchCount > 0)
        //{
        //    for (var i = 0; i < Input.touchCount; i++)
        //    {
        //        if (Input.GetTouch(i).position.x > screenCenterX && Input.GetTouch(i).phase == TouchPhase.Moved)
        //        {
        //            targetSpringArmRotationX = springArmTransform.rotation.eulerAngles.x - Input.GetTouch(i).deltaPosition.y * mobileCameraSpeed * Time.deltaTime;
        //            targetSpringArmRotationY = springArmTransform.rotation.eulerAngles.y + Input.GetTouch(i).deltaPosition.x * mobileCameraSpeed * Time.deltaTime;
        //        }
        //    }
        //}
        //else
        //{
        //    targetSpringArmRotationX = springArmTransform.rotation.eulerAngles.x;
        //    targetSpringArmRotationY = springArmTransform.rotation.eulerAngles.y;
        //}

        springArmTransform.rotation = Quaternion.Euler(targetSpringArmRotationX, targetSpringArmRotationY, rollRootTransform.rotation.eulerAngles.z);
    }

    void PCInputControlLogic()
    {
        if (Input.GetKeyUp(KeyCode.Space))
            TakeOffOrLand();

        if (!aircraftFlyingSystem.inAir)
        {
            if (Input.GetKey(KeyCode.W))
                GoundMoveForward(1.0f);
            else if (Input.GetKey(KeyCode.S))
                GoundMoveForward(-1.0f);

            if (Input.GetKeyUp(KeyCode.W))
                StopMovingForward();

            if (Input.GetKey(KeyCode.A))
                GoundTurnRight(-1.0f);
            else if (Input.GetKey(KeyCode.D))
                GoundTurnRight(1.0f);
        }
        else
        {
            // Hold down to turn left / right
            if (Input.GetKey(KeyCode.A))
                aircraftFlyingSystem.AddYawInput(-1.0f);
            else if (Input.GetKey(KeyCode.D))
                aircraftFlyingSystem.AddYawInput(1.0f);

            if (Input.GetKeyUp(KeyCode.A))
                aircraftFlyingSystem.StopYawInput();
            else if (Input.GetKeyUp(KeyCode.D))
                aircraftFlyingSystem.StopYawInput();

            // Point up / down
            if (Input.GetKey(KeyCode.Q))
                aircraftFlyingSystem.AddPitchInput(1.0f);
            else if (Input.GetKey(KeyCode.E))
                aircraftFlyingSystem.AddPitchInput(-1.0f);

            // Roll to have a sharp turn left / right
            if (Input.GetKey(KeyCode.Z))
                aircraftFlyingSystem.AddRollInput(-1.0f);
            else if (Input.GetKey(KeyCode.C))
                aircraftFlyingSystem.AddRollInput(1.0f);
        }

        //if (Input.GetKey(KeyCode.W))
        //    aircraftFlyingSystem.MoveForward();
        //else if (Input.GetKey(KeyCode.S))
        //    aircraftFlyingSystem.SlowDown();
        //else if (Input.GetKeyUp(KeyCode.S))
        //    aircraftFlyingSystem.StopSlowingDown();
    }

    void MobileInputControlLogic()
    {

    }

    public void GoundMoveForward(float value)
    {
        movingForward = true;

        if (!(currentGroundMovementSpeed > maximumGroundMovementSpeed))
        {
            currentGroundMovementSpeed = Mathf.Clamp(currentGroundMovementSpeed + groundAcceleration * Time.deltaTime, 0.0f, maximumGroundMovementSpeed);
            rootTransform.position += rootTransform.forward * value * currentGroundMovementSpeed * Time.deltaTime;
        }
    }

    public void StopMovingForward()
    {
        movingForward = false;
    }

    public void GoundTurnRight(float value)
    {
        rootTransform.Rotate(rootTransform.up * value * groundTurningSpeed * Time.deltaTime);
    }

    public void TakeOffOrLand()
    {
        if (!aircraftFlyingSystem.inAir)
        {
            if (aircraftFlyingSystem.TakeOff(currentGroundMovementSpeed))
            {
                rootRigidbody.isKinematic = true;
                rootRigidbody.useGravity = false;
                
                landed = false;
            }
        }
        else
        {
            aircraftFlyingSystem.Land();

            rootRigidbody.isKinematic = false;
            rootRigidbody.useGravity = true;

            movingForward = false;
            landed = true;
        }

        takeOff = aircraftFlyingSystem.inAir;
    }

    void MovementLogic()
    {
        if (landed)
        {
            if (!movingForward)
            {
                currentGroundMovementSpeed = Mathf.Clamp(currentGroundMovementSpeed - slowDownAccelerationAfterLanding * Time.deltaTime, 0.0f, currentGroundMovementSpeed);
                rootTransform.position += rootTransform.forward * currentGroundMovementSpeed * Time.deltaTime;
            }
            else
            {
                if (currentGroundMovementSpeed > maximumGroundMovementSpeed)
                {
                    currentGroundMovementSpeed -= slowDownAccelerationAfterLanding * Time.deltaTime;
                    rootTransform.position += rootTransform.forward * currentGroundMovementSpeed * Time.deltaTime;
                }
            }
        }
    }

    public void MobileTurnLeft()
    {
        aircraftFlyingSystem.AddYawInput(-1.0f);
    }

    public void MobileTurnRight()
    {
        aircraftFlyingSystem.AddYawInput(1.0f);
    }

    public void MobilePointUp()
    {
        aircraftFlyingSystem.AddPitchInput(1.0f);
    }

    public void MobilePointDown()
    {
        aircraftFlyingSystem.AddPitchInput(-1.0f);
    }

    public void MobileRollLeft()
    {
        aircraftFlyingSystem.AddRollInput(-1.0f);
    }

    public void MobileRollRight()
    {
        aircraftFlyingSystem.AddRollInput(1.0f);
    }
}