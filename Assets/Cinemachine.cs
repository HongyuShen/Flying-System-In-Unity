using UnityEngine;

public class Cinemachine : MonoBehaviour
{
    public Vector3[] stagePositionArray = new Vector3[14];

    private Transform cameraTransform;

    private int currentIndex = 0;

    void Start()
    {
        cameraTransform = this.transform;
        cameraTransform.position = stagePositionArray[0];
        cameraTransform.rotation = Quaternion.Euler(25.0f, 0.0f, 0.0f);
    }

    void Update()
    {
        if (currentIndex < stagePositionArray.Length)
        {
            if (Vector3.Distance(cameraTransform.position, stagePositionArray[currentIndex]) < 0.1f)
                currentIndex += 1;
            else
            {
                if (currentIndex == 5)
                {
                    cameraTransform.position += (stagePositionArray[currentIndex] - cameraTransform.position).normalized * 150.0f * Time.deltaTime;
                    cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.Euler(25.0f, 180.0f, 0.0f), 10.0f * Time.deltaTime);
                }
                else if (currentIndex == 14 || currentIndex == 15 || currentIndex == 16)
                    cameraTransform.position += (stagePositionArray[currentIndex] - cameraTransform.position).normalized * 45.0f * Time.deltaTime;
                else if (currentIndex == 17)
                {
                    cameraTransform.position += (stagePositionArray[currentIndex] - cameraTransform.position).normalized * 200.0f * Time.deltaTime;
                    cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.Euler(45.0f, 90.0f, 0.0f), 10.0f * Time.deltaTime);
                }
                else
                    cameraTransform.position += (stagePositionArray[currentIndex] - cameraTransform.position).normalized * 15.0f * Time.deltaTime;
            }
        }
    }
}