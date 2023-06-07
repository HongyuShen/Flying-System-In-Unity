using UnityEngine;

public class GliderController : MonoBehaviour
{
    public Transform springArmTransform;
    public Camera characterCamera;

    public Rigidbody rootRigidbody;

    private GliderFlyingSystem gliderFlyingSystem;

    private Airflow airflow;

    public bool activated = false;

    public float cameraSpeed = 300.0f;

    // Mobile variables
    public bool mobileInputControl = false;
    public float mobileCameraSpeed = 300.0f;
    private float screenCenterX;

    private float targetSpringArmRotationX, targetSpringArmRotationY;

    void Start()
    {
        if (activated)
            Activate();

        gliderFlyingSystem = this.GetComponent<GliderFlyingSystem>();

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
        this.transform.position = new Vector3(146.0f, 150.0f, 388.0f);
        gliderFlyingSystem.TakeOff(5.0f);
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
        // Hold down to turn left / right
        if (Input.GetKey(KeyCode.A))
            gliderFlyingSystem.AddYawInput(-1.0f);
        else if (Input.GetKey(KeyCode.D))
            gliderFlyingSystem.AddYawInput(1.0f);

        // Point up / down
        if (Input.GetKey(KeyCode.Q))
            gliderFlyingSystem.AddPitchInput(-1.0f);
        else if (Input.GetKey(KeyCode.E))
            gliderFlyingSystem.AddPitchInput(1.0f);

        // Roll to have a sharp turn left / right
        if (Input.GetKey(KeyCode.Z))
            gliderFlyingSystem.AddRollInput(-1.0f);
        else if (Input.GetKey(KeyCode.C))
            gliderFlyingSystem.AddRollInput(1.0f);
    }

    void MobileInputControlLogic()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Airflow")
        {
            airflow = other.GetComponent<Airflow>();
            gliderFlyingSystem.AddAirflowForce(airflow.intensity, airflow.acceleration, airflow.fadeOutAcceleration);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.name == "Airflow")
            gliderFlyingSystem.EndAirflowForce();
    }
}