using UnityEngine;

public class Settings : MonoBehaviour
{
    public static bool enabledControl = false;

    void Update()
    {
        //print(Input.mousePosition);
        if (!enabledControl && Input.GetMouseButtonUp(0))
        {
            LockMouse();
        }
    }

    void LockMouse()
    {
        enabledControl = true;

        // Lock mouse cursor in the view
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}