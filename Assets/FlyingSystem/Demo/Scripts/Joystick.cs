using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Transform joystickBackgroundTransform, joystickControllerTransform;

    private bool reset = false;
    private float maxMovingDistance;

    public bool isMoving = false;
    private int direction = 0;

    public float x, y;
    public float inputAxisX, inputAxisY;

    void Start()
    {
        joystickBackgroundTransform = this.transform.GetChild(0).GetChild(0);
        joystickControllerTransform = this.transform.GetChild(0).GetChild(1);

        maxMovingDistance = 77.5f;
    }

    void Update()
    {
        if (reset)
        {
            joystickControllerTransform.position = new Vector3(joystickControllerTransform.position.x - (joystickControllerTransform.position.x - joystickBackgroundTransform.position.x) * 10.0f * Time.deltaTime, joystickControllerTransform.position.y - (joystickControllerTransform.position.y - joystickBackgroundTransform.position.y) * 10.0f * Time.deltaTime, 0.0f);

            if (joystickControllerTransform.position.x < (joystickBackgroundTransform.position.x + 1.0f) && (joystickBackgroundTransform.position.x - 1f) < joystickControllerTransform.position.x)
            {
                joystickControllerTransform.position = new Vector3(joystickBackgroundTransform.position.x, joystickBackgroundTransform.position.y, 0f);
                reset = false;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isMoving == false)
            isMoving = true;

        if (isMoving)
        {
            float realtimeDistance = (eventData.position.x - joystickBackgroundTransform.position.x) * (eventData.position.x - joystickBackgroundTransform.position.x) + (eventData.position.y - joystickBackgroundTransform.position.y) * (eventData.position.y - joystickBackgroundTransform.position.y);

            if (realtimeDistance <= Mathf.Pow(maxMovingDistance, 2.0f))
            {
                joystickControllerTransform.position = new Vector3(eventData.position.x, eventData.position.y, 0.0f);

                if (eventData.position.x > joystickBackgroundTransform.position.x)
                {
                    if (eventData.position.y > joystickBackgroundTransform.position.y)
                        direction = 1;
                    else
                        direction = 4;
                }
                else
                {
                    if (eventData.position.y > joystickBackgroundTransform.position.y)
                        direction = 2;
                    else
                        direction = 3;
                }
            }
            else
            {
                float relativeX = Mathf.Sqrt((maxMovingDistance * maxMovingDistance) / (1.0f + ((eventData.position.x - joystickBackgroundTransform.position.x) / (eventData.position.y - joystickBackgroundTransform.position.y)) * ((eventData.position.x - joystickBackgroundTransform.position.x) / (eventData.position.y - joystickBackgroundTransform.position.y))));

                if (eventData.position.x > joystickBackgroundTransform.position.x)
                {
                    if (eventData.position.y > joystickBackgroundTransform.position.y)
                    {
                        direction = 1;
                        joystickControllerTransform.position = new Vector3(joystickBackgroundTransform.position.x + relativeX * Mathf.Abs((eventData.position.x - joystickBackgroundTransform.position.x) / (eventData.position.y - joystickBackgroundTransform.position.y)), joystickBackgroundTransform.position.y + relativeX, 0.0f);
                    }
                    else
                    {
                        direction = 4;
                        joystickControllerTransform.position = new Vector3(joystickBackgroundTransform.position.x + relativeX * Mathf.Abs((eventData.position.x - joystickBackgroundTransform.position.x) / (eventData.position.y - joystickBackgroundTransform.position.y)), joystickBackgroundTransform.position.y - relativeX, 0.0f);
                    }
                }
                else
                {
                    if (eventData.position.y > joystickBackgroundTransform.position.y)
                    {
                        direction = 2;
                        joystickControllerTransform.position = new Vector3(joystickBackgroundTransform.position.x - relativeX * Mathf.Abs((eventData.position.x - joystickBackgroundTransform.position.x) / (eventData.position.y - joystickBackgroundTransform.position.y)), joystickBackgroundTransform.position.y + relativeX, 0.0f);
                    }
                    else
                    {
                        direction = 3;
                        joystickControllerTransform.position = new Vector3(joystickBackgroundTransform.position.x - relativeX * Mathf.Abs((eventData.position.x - joystickBackgroundTransform.position.x) / (eventData.position.y - joystickBackgroundTransform.position.y)), joystickBackgroundTransform.position.y - relativeX, 0.0f);
                    }
                }
            }

            x = (joystickControllerTransform.position.x - joystickBackgroundTransform.position.x);
            y = joystickControllerTransform.position.y - joystickBackgroundTransform.position.y;

            inputAxisX = x / maxMovingDistance;
            inputAxisY = y / maxMovingDistance;

            reset = false;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isMoving = false;
        reset = true;
    }
}