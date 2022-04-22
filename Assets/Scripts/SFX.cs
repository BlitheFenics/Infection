using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    public AudioSource audio;
    public AudioClip jump, interact;
    public static SFX instance;

    private void Awake()
    {
        instance = this;
    }
}