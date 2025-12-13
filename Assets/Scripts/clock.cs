using UnityEngine;

public class ClockController : MonoBehaviour
{
    public Transform hourHand;   // Akrep
    public Transform minuteHand; // Yelkovan

    public float rotateSpeed = 30f;
    public float interactDistance = 3f;

    private bool isAdjusting = false;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        // Etkileşim alanı
        if (!isAdjusting && distance < interactDistance)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                isAdjusting = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        // Ayarlama modu aktifken
        if (isAdjusting)
        {
            AdjustClock();

            if (Input.GetKeyDown(KeyCode.E))
            {
                isAdjusting = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void AdjustClock()
    {
        float h = Input.GetAxis("Horizontal"); // sağ-sol
        float v = Input.GetAxis("Vertical");   // yukarı-aşağı

        // Yelkovan döner (dakika)
        if (h != 0)
        {
            minuteHand.Rotate(0f, 0f, -h * rotateSpeed * Time.deltaTime);
        }

        // Akrep döner (saat)
        if (v != 0)
        {
            hourHand.Rotate(0f, 0f, -v * rotateSpeed * Time.deltaTime);
        }
    }
}
