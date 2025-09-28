using UnityEngine;

public class TimePickup : MonoBehaviour
{
    public int timeValue = 3;

    const string playerTag = "Player";

    Timer timer;

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == playerTag)
        {
            PickUp();
        }
    }

    void PickUp()
    {
        timer.AddTime(timeValue);
        Destroy(gameObject);
    }
}
