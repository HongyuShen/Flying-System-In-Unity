using UnityEngine;

public class BirdController : MonoBehaviour
{
    private Transform characterTransform;

    public Transform springArmTransform;
    public Camera characterCamera;

    public Animator animator;

    private CreatureFlyingSystem creatureFlyingSystem;

    private Airflow airflow;

    public bool activated = false;

    public float cameraSpeed = 300.0f;

    // Mobile variables
    public bool mobileInputControl = false;
    public float mobileCameraSpeed = 300.0f;
    private float screenCenterX;

    [Range(0.0f, 100.0f)]
    public float springArmSmoothingFactor = 0.25f;

    public float groundMovementSpeed = 1.0f;

    void Start()
    {
        if (activated)
            Activate();
        //animator.SetBool("IdleToFly", true);
        characterTransform = this.transform;

        creatureFlyingSystem = this.GetComponent<CreatureFlyingSystem>();
        
        screenCenterX = screenCenterX = Screen.width / 2.0f;
    }

    void Update()
    {
        if (activated)
        {
            CameraControlLogic();

            if (!mobileInputControl)
                PCInputControlLogic();
            else
                MobileInputControlLogic();
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

    void PCInputControlLogic()
    {
        // Take off / grab
        if (Input.GetKeyUp(KeyCode.Space))
        {
            creatureFlyingSystem.TakeOff();

            animator.SetBool("FlyToIdle", false);
            animator.SetBool("IdleToFly", true);
        }

        if (Input.GetKeyUp(KeyCode.L))
        {
            creatureFlyingSystem.Land();

            animator.SetBool("IdleToFly", false);
            animator.SetBool("FlyToIdle", true);
        }

        // Fly forward / stop
        if (Input.GetKey(KeyCode.W))
            creatureFlyingSystem.FlyForward();
        else if (Input.GetKey(KeyCode.S))
            creatureFlyingSystem.SlowDown();
        else if (Input.GetKeyUp(KeyCode.S))
            creatureFlyingSystem.StopSlowingDown();

        // Turn left / right
        creatureFlyingSystem.AddYawInput(Input.GetAxis("Mouse X"));

        if (creatureFlyingSystem.canDive)
        {
            if (creatureFlyingSystem.diving)
            {
                animator.SetBool("FlyToGlide", true);
                animator.SetBool("GlideToFly", false);
            }
            else
            {
                animator.SetBool("GlideToFly", true);
                animator.SetBool("FlyToGlide", false);
            }
        }

        // Boost on / off
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            creatureFlyingSystem.boosting = !creatureFlyingSystem.boosting;
    }

    void MobileInputControlLogic()
    {

    }

    void CameraControlLogic()
    {
        springArmTransform.position = Vector3.Lerp(characterTransform.position, springArmTransform.position, springArmSmoothingFactor * Time.deltaTime);
        springArmTransform.rotation = Quaternion.Euler(springArmTransform.rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime, springArmTransform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime, 0.0f);
    }

    public void Grab()
    {

    }

    public void MobileTurnLeft()
    {

    }

    public void MobileTurnRight()
    {

    }

    public void MobileRollLeft()
    {

    }

    public void MobileRollRight()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "Road")
        {
            if (creatureFlyingSystem.inAir)
            {
                creatureFlyingSystem.Land();

                animator.SetBool("GlideToIdle", true);

                animator.SetBool("FlyToIdle", true);
                animator.SetBool("IdleToFly", false);

                animator.SetBool("FlyToGlide", false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Airflow")
        {
            airflow = other.GetComponent<Airflow>();

            creatureFlyingSystem.AddAirflowForce(airflow.intensity, airflow.acceleration, airflow.fadeOutAcceleration);
            creatureFlyingSystem.stopFlying = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.name == "Airflow")
        {
            creatureFlyingSystem.EndAirflowForce();
            creatureFlyingSystem.stopFlying = false;
        }
    }
}