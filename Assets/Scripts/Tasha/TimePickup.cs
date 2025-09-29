using UnityEngine;

public class TimePickup : MonoBehaviour
{
    public int timeValue = 3;
    public AudioClip sfx;

    bool pickedUp = false;

    const string playerTag = "Player";

    Timer timer;
    AudioSource audioSource;

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == playerTag && !pickedUp)
        {
            pickedUp = true;
            PickUp();
        }
    }

    void PickUp()
    {
        timer.AddTime(timeValue);
        audioSource.PlayOneShot(sfx);
        AudioSource.PlayClipAtPoint(sfx, Camera.main.transform.position);
        gameObject.SetActive(false);
    }
}
