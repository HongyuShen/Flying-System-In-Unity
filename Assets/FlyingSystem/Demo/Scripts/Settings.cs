using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public static bool enabledControl = false;

    public static bool mobileInputControl = false;

    public GameObject mobileUIroot;

    public Button switchToPCOrMobileModeButton;

    void Start()
    {
        switchToPCOrMobileModeButton.onClick.AddListener(SwitchBetweenMobileAndPC);
    }

    void Update()
    {
        //print(Input.mousePosition);
        if (!enabledControl && Input.GetMouseButtonUp(0))
        {
            LockMouse();
        }

        if (Input.GetKeyUp(KeyCode.M))
            SwitchBetweenMobileAndPC();
    }

    void SwitchBetweenMobileAndPC()
    {
        mobileInputControl = !mobileInputControl;
        mobileUIroot.SetActive(mobileInputControl);

        if (mobileInputControl)
            UnlockMouse();
        else
            LockMouse();
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
}