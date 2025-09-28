using System;
using UnityEngine;

public class WinGame : MonoBehaviour
{
    const string playerTag = "Player";

    public static Action OnGameWon;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == playerTag)
        {
            OnGameWon();
        }
    }
}
