using UnityEngine;
using UnityEngine.EventSystems;

public class Ascend : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private HelicopterController helicopterController;

    void Start()
    {
        helicopterController = GameObject.Find("Helicopter").GetComponent<HelicopterController>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        helicopterController.MobileAscend();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        helicopterController.MobileStopAscendOrDescend();
    }
}