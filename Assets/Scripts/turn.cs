using UnityEngine;

public class ClockAdjuster : MonoBehaviour
{
    public Transform hourHand;
    public Transform minuteHand;

    public float rotateSpeed = 50f;
    private bool adjusting = false;

    void Update()
    {
        // E tuþu ile mode aç/kapa
        if (Input.GetKeyDown(KeyCode.E))
        {
            adjusting = !adjusting;
        }

        // Saat ayarlama modu
        if (adjusting)
        {
            float h = Input.GetAxis("Horizontal"); // sað-sol
            float v = Input.GetAxis("Vertical");   // yukarý-aþaðý

            // Yelkovan
            if (h != 0)
                minuteHand.Rotate(0f, 0f, -h * rotateSpeed * Time.deltaTime);

            // Akrep
            if (v != 0)
                hourHand.Rotate(0f, 0f, -v * rotateSpeed * Time.deltaTime);
        }
    }
}
