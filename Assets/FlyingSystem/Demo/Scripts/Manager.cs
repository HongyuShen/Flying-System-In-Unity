using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    /*
    0: Default
    1: Duck
    2: Eagle
    3: Dragon
    4: Airliner
    5: Jet
    6: Helicopter
    7: Drone
    8: Flying Car
    9: Humanoid Aircraft
    10: Parachute
    11: Glider
    12: Paper Plane
    */
    public int possessedControllerId = 0;

    [Header("Creatures")]
    public BirdController duckBirdController;
    public EagleController eagleController;
    public BirdController dragonBirdController;

    [Header("Controllers")]
    public ThirdPersonCharacter thirdPersonCharacterController;

    public AircraftController airlinerAircraftController;
    public AircraftController jetAircraftController;
    public HelicopterController helicopterController;
    public DroneController droneController;
    public FlyingCarController flyingCarController;
    public HumanoidAircraftController humanoidAircraftController;
    public GliderController parachuteGliderController;
    public GliderController gliderController;
    public ViteAereaController viteAereaController;
    public GliderController paperAirPlaneGliderController;

    private bool enabledControl = false;

    private bool enabledCinemachine = false;
    public bool resetCameraLogic = false;

    private static bool mobileInputControl = false;

    [Header("UI")]
    public Canvas canvas;
    public GameObject pcImage, mobileImage, joystickGroup, rollButtonGroup;

    public Button switchToPCOrMobileModeButton;

    public Button turnOnOrOffButton;
    public Text turnOnOrOffButtonText;

    public Text upButtonText, downButtonText;

    public Button takeOffOrLandButton;
    public GameObject takeOffImage, landImage;

    void Start()
    {
        switchToPCOrMobileModeButton.onClick.AddListener(SwitchBetweenMobileAndPC);
        turnOnOrOffButton.onClick.AddListener(TurnOnOrOff);
        takeOffOrLandButton.onClick.AddListener(TakeOffOrLand);
    }

    void Update()
    {
        if (!enabledControl && Input.GetMouseButtonUp(0))
            LockMouse();

        CinemachineLogic();

        if (Input.GetKeyUp(KeyCode.R))
            ResetCamera();

        ResetCameraLogic();

        if (Input.GetKeyUp(KeyCode.M))
            SwitchBetweenMobileAndPC();
        else if (Input.GetKeyUp(KeyCode.H))
            HideOrUnhideUI();
        else if (Input.GetKeyUp(KeyCode.N))
            TurnOnOrOff();


        bool result = false;

        if (possessedControllerId == 4)
            result = airlinerAircraftController.takeOff;

        takeOffImage.SetActive(!result);
        landImage.SetActive(result);

        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            thirdPersonCharacterController.Deactivate();

            UseFlyer();
        }
    }

    public void UseFlyer()
    {
        if (thirdPersonCharacterController.activated)
        {
            thirdPersonCharacterController.Deactivate();

            if (possessedControllerId == 4) 
                airlinerAircraftController.transform.position = new Vector3(141.0f, 2.5f, 507.0f);
            else if (possessedControllerId == 5)
                jetAircraftController.transform.position = new Vector3(141.0f, 2.5f, 507.0f);

            enabledCinemachine = true;
        }
    }

    public void CinemachineLogic()
    {
        if (enabledCinemachine)
        {
            if (possessedControllerId == 1)
            {
                if (CinemachineLerp(duckBirdController.characterCamera.transform.position, duckBirdController.characterCamera.transform.rotation))
                    duckBirdController.Activate();
            }
            else if (possessedControllerId == 2)
            {
                if (CinemachineLerp(eagleController.characterCamera.transform.position, eagleController.characterCamera.transform.rotation))
                    eagleController.Activate();
            }
            else if (possessedControllerId == 3)
            {
                if (CinemachineLerp(dragonBirdController.characterCamera.transform.position, dragonBirdController.characterCamera.transform.rotation))
                    dragonBirdController.Activate();
            }
            else if (possessedControllerId == 4)
            {
                if (CinemachineLerp(airlinerAircraftController.characterCamera.transform.position, airlinerAircraftController.characterCamera.transform.rotation))
                    airlinerAircraftController.Activate();
            }
            else if (possessedControllerId == 5)
            {
                if (CinemachineLerp(jetAircraftController.characterCamera.transform.position, jetAircraftController.characterCamera.transform.rotation))
                    jetAircraftController.Activate();
            }
            else if (possessedControllerId == 6)
            {
                if (CinemachineLerp(helicopterController.characterCamera.transform.position, helicopterController.characterCamera.transform.rotation))
                    helicopterController.Activate();
            }
            else if (possessedControllerId == 7)
            {
                if (CinemachineLerp(droneController.characterCamera.transform.position, droneController.characterCamera.transform.rotation))
                    droneController.Activate();
            }
            else if (possessedControllerId == 8)
            {
                if (CinemachineLerp(flyingCarController.characterCamera.transform.position, flyingCarController.characterCamera.transform.rotation))
                    flyingCarController.Activate();
            }
            else if (possessedControllerId == 9)
            {
                if (CinemachineLerp(humanoidAircraftController.characterCamera.transform.position, humanoidAircraftController.characterCamera.transform.rotation))
                    humanoidAircraftController.Activate();
            }
            else if (possessedControllerId == 10)
            {
                if (CinemachineLerp(parachuteGliderController.characterCamera.transform.position, parachuteGliderController.characterCamera.transform.rotation))
                    parachuteGliderController.Activate();
            }
            else if (possessedControllerId == 11)
            {
                if (CinemachineLerp(gliderController.characterCamera.transform.position, gliderController.characterCamera.transform.rotation))
                    gliderController.Activate();
            }
            else if (possessedControllerId == 12)
            {
                if (CinemachineLerp(viteAereaController.characterCamera.transform.position, viteAereaController.characterCamera.transform.rotation))
                    viteAereaController.Activate();
            }
            else if (possessedControllerId == 13)
            {
                if (CinemachineLerp(paperAirPlaneGliderController.characterCamera.transform.position, paperAirPlaneGliderController.characterCamera.transform.rotation))
                    paperAirPlaneGliderController.Activate();
            }
        }
    }

    bool CinemachineLerp(Vector3 targetPosition, Quaternion targetRotation)
    {
        thirdPersonCharacterController.characterCameraTransform.position = Vector3.Lerp(thirdPersonCharacterController.characterCameraTransform.position, targetPosition, 4.0f * Time.deltaTime);
        thirdPersonCharacterController.characterCameraTransform.rotation = Quaternion.Lerp(thirdPersonCharacterController.characterCameraTransform.rotation, targetRotation, 4.0f * Time.deltaTime);

        if (Vector3.Distance(thirdPersonCharacterController.characterCameraTransform.position, targetPosition) < 0.01f)
        {
            enabledCinemachine = false;

            thirdPersonCharacterController.characterCamera.enabled = false;
            thirdPersonCharacterController.characterCameraTransform.localPosition = Vector3.zero;
            thirdPersonCharacterController.characterCameraTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            return true;
        }

        return false;
    }

    void ResetCamera()
    {
        if (!resetCameraLogic)
        {
            if (possessedControllerId == 1)
            {
                thirdPersonCharacterController.characterCameraTransform.position = duckBirdController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = duckBirdController.characterCamera.transform.rotation;

                duckBirdController.Deactivate();
            }
            else if (possessedControllerId == 2)
            {
                thirdPersonCharacterController.characterCameraTransform.position = eagleController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = eagleController.characterCamera.transform.rotation;

                eagleController.Deactivate();
            }
            else if (possessedControllerId == 3)
            {
                thirdPersonCharacterController.characterCameraTransform.position = dragonBirdController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = dragonBirdController.characterCamera.transform.rotation;

                dragonBirdController.Deactivate();
            }
            else if (possessedControllerId == 4)
            {
                thirdPersonCharacterController.characterCameraTransform.position = airlinerAircraftController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = airlinerAircraftController.characterCamera.transform.rotation;

                airlinerAircraftController.Deactivate();
            }
            else if (possessedControllerId == 5)
            {
                thirdPersonCharacterController.characterCameraTransform.position = jetAircraftController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = jetAircraftController.characterCamera.transform.rotation;

                jetAircraftController.Deactivate();
            }
            else if (possessedControllerId == 6)
            {
                thirdPersonCharacterController.characterCameraTransform.position = helicopterController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = helicopterController.characterCamera.transform.rotation;

                helicopterController.Deactivate();
            }
            else if (possessedControllerId == 7)
            {
                thirdPersonCharacterController.characterCameraTransform.position = droneController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = droneController.characterCamera.transform.rotation;

                droneController.Deactivate();
            }
            else if (possessedControllerId == 8)
            {
                thirdPersonCharacterController.characterCameraTransform.position = flyingCarController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = flyingCarController.characterCamera.transform.rotation;

                flyingCarController.Deactivate();
            }
            else if (possessedControllerId == 9)
            {
                thirdPersonCharacterController.characterCameraTransform.position = humanoidAircraftController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = humanoidAircraftController.characterCamera.transform.rotation;

                humanoidAircraftController.Deactivate();
            }
            else if (possessedControllerId == 10)
            {
                thirdPersonCharacterController.characterCameraTransform.position = parachuteGliderController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = parachuteGliderController.characterCamera.transform.rotation;

                parachuteGliderController.Deactivate();
            }
            else if (possessedControllerId == 11)
            {
                thirdPersonCharacterController.characterCameraTransform.position = gliderController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = gliderController.characterCamera.transform.rotation;

                gliderController.Deactivate();
            }
            else if (possessedControllerId == 12)
            {
                thirdPersonCharacterController.characterCameraTransform.position = viteAereaController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = viteAereaController.characterCamera.transform.rotation;

                viteAereaController.Deactivate();
            }
            else if (possessedControllerId == 13)
            {
                thirdPersonCharacterController.characterCameraTransform.position = paperAirPlaneGliderController.characterCamera.transform.position;
                thirdPersonCharacterController.characterCameraTransform.rotation = paperAirPlaneGliderController.characterCamera.transform.rotation;

                paperAirPlaneGliderController.Deactivate();
            }

            thirdPersonCharacterController.Activate();

            resetCameraLogic = true;
        }
    }

    void ResetCameraLogic()
    {
        if (resetCameraLogic)
        {
            if (ResetCameraLerp())
            {
                resetCameraLogic = false;

                possessedControllerId = 0;
            }
        }
    }

    bool ResetCameraLerp()
    {
        thirdPersonCharacterController.characterCameraTransform.localPosition = Vector3.Lerp(thirdPersonCharacterController.characterCameraTransform.localPosition, new Vector3(0.0f, 3.0f, -10.0f), 4.0f * Time.deltaTime);
        thirdPersonCharacterController.characterCameraTransform.localRotation = Quaternion.Lerp(thirdPersonCharacterController.characterCameraTransform.localRotation, Quaternion.Euler(0.0f, 0.0f, 0.0f), 4.0f * Time.deltaTime);

        if (Vector3.Distance(thirdPersonCharacterController.characterCameraTransform.localPosition, new Vector3(0.0f, 3.0f, -10.0f)) < 0.01f)
        {
            resetCameraLogic = false;

            thirdPersonCharacterController.characterCameraTransform.localPosition = new Vector3(0.0f, 3.0f, -10.0f);
            thirdPersonCharacterController.characterCameraTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            return true;
        }

        return false;
    }

    void SwitchBetweenMobileAndPC()
    {
        mobileInputControl = !mobileInputControl;

        pcImage.SetActive(mobileInputControl);
        mobileImage.SetActive(!mobileInputControl);
        joystickGroup.SetActive(mobileInputControl);
        rollButtonGroup.SetActive(possessedControllerId != 6 && possessedControllerId != 7 && possessedControllerId != 8 && possessedControllerId != 9);

        if (possessedControllerId == 6)
        {
            upButtonText.text = "[W]";
            downButtonText.text = "[S]";
        }
        else
        {
            upButtonText.text = "[Q]";
            downButtonText.text = "[E]";
        }

        if (mobileInputControl)
            UnlockMouse();
        else
            LockMouse();

        if (possessedControllerId == 2)
            airlinerAircraftController.mobileInputControl = !airlinerAircraftController.mobileInputControl;
        else if (possessedControllerId == 6)
            helicopterController.mobileInputControl = !helicopterController.mobileInputControl;
    }

    void LockMouse()
    {
        enabledControl = true;

        // Lock mouse cursor in the view
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockMouse()
    {
        enabledControl = true;

        // Lock mouse cursor in the view
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void HideOrUnhideUI()
    {
        canvas.enabled = !canvas.enabled;
    }

    void TurnOnOrOff()
    {
        bool result = false;

        if (possessedControllerId == 6)
            helicopterController.TurnOnOrOff();

        if (result)
            turnOnOrOffButtonText.text = "Off";
        else
            turnOnOrOffButtonText.text = "On";
    }

    void TakeOffOrLand()
    {
        if (possessedControllerId == 4)
            airlinerAircraftController.TakeOffOrLand();
    }

    public void MobileTurnLeft()
    {
        if (possessedControllerId == 1)
            eagleController.MobileTurnLeft();
        else if (possessedControllerId == 4)
            airlinerAircraftController.MobileTurnLeft();
        else if (possessedControllerId == 5)
            jetAircraftController.MobileTurnLeft();
        else if (possessedControllerId == 6)
            helicopterController.MobileTurnLeft();
    }

    public void MobileTurnRight()
    {
        if (possessedControllerId == 1)
            eagleController.MobileTurnRight();
        else if (possessedControllerId == 4)
            airlinerAircraftController.MobileTurnRight();
        else if (possessedControllerId == 5)
            jetAircraftController.MobileTurnRight();
        else if (possessedControllerId == 6)
            helicopterController.MobileTurnRight();
    }

    public void MobileUp()
    {
        if (possessedControllerId == 4)
            airlinerAircraftController.MobilePointUp();
        else if (possessedControllerId == 5)
            jetAircraftController.MobilePointUp();
        else if (possessedControllerId == 6)
            helicopterController.Ascend();
    }

    public void MobileDown()
    {
        if (possessedControllerId == 4)
            airlinerAircraftController.MobilePointDown();
        else if (possessedControllerId == 5)
            jetAircraftController.MobilePointDown();
        else if (possessedControllerId == 6)
            helicopterController.Descend();
    }

    public void MobileReleaseUp()
    {
        if (possessedControllerId == 6)
            helicopterController.MobileStopAscendOrDescend();
    }

    public void MobileReleaseDown()
    {
        if (possessedControllerId == 6)
            helicopterController.MobileStopAscendOrDescend();
    }

    public void MobileRollLeft()
    {
        if (possessedControllerId == 4)
            airlinerAircraftController.MobileRollLeft();
    }

    public void MobileRollRight()
    {
        if (possessedControllerId == 4)
            airlinerAircraftController.MobileRollRight();
    }
}