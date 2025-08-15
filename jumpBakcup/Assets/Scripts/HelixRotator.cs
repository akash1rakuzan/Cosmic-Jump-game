using UnityEngine;

public class HelixRotator : MonoBehaviour
{
    public static float sensitivity = 10f;

    void Update()
    {
        if (GameManager.gameOver || GameManager.levelWin || GameManager.gameJustStarted)
            return;

        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxisRaw("Mouse X");
            transform.Rotate(0, -mouseX * sensitivity * Time.deltaTime, 0);
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            float xDeltaPos = Input.GetTouch(0).deltaPosition.x;
            transform.Rotate(0, -xDeltaPos * sensitivity * Time.deltaTime, 0);
        }
    }
}
