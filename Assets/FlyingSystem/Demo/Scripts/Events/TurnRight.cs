using UnityEngine;
using UnityEngine.EventSystems;

public class TurnRight : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private HelicopterController helicopterController;

    private bool buttonDown = false;

    void Start()
    {
        helicopterController = GameObject.Find("Helicopter").GetComponent<HelicopterController>();
    }

    void Update()
    {
        if (buttonDown)
            helicopterController.MobileTurnRight();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonDown = false;
    }
}