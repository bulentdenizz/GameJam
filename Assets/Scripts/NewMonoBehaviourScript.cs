using UnityEngine;

public class ClockByPlayerPosition : MonoBehaviour
{
    public Transform player;
    public Transform hourHand;
    public Transform minuteHand;

    public float minZ = 0f;
    public float maxZ = 10f;

    public float minX = 0f;
    public float maxX = 10f;

    private bool isLocked = false;

    void Update()
    {
        if (!isLocked)
        {
            UpdateHourHand();
            UpdateMinuteHand();
        }

    }

    void UpdateHourHand()
    {
        float z = Mathf.Clamp(player.position.z, minZ, maxZ);
        float t = Mathf.InverseLerp(minZ, maxZ, z);

        float hours = Mathf.Lerp(0f, 12f, t);
        float hourAngle = hours * 30f;

        hourHand.localRotation = Quaternion.Euler(0, 0, -hourAngle);
    }

    void UpdateMinuteHand()
    {
        float x = Mathf.Clamp(player.position.x, minX, maxX);
        float minutes = Mathf.Lerp(0f, 60f, Mathf.InverseLerp(minX, maxX, x));

        float minuteAngle = minutes * 6f;
        minuteHand.localRotation = Quaternion.Euler(0, 0, -minuteAngle);
    }

    public void LockClock()
    {
        isLocked = true;
    }
}
