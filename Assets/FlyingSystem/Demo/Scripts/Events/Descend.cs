using UnityEngine;
using UnityEngine.EventSystems;

public class Descend : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private HelicopterController helicopterController;

    void Start()
    {
        helicopterController = GameObject.Find("Helicopter").GetComponent<HelicopterController>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        helicopterController.MobileDescend();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        helicopterController.MobileStopAscendOrDescend();
    }
}