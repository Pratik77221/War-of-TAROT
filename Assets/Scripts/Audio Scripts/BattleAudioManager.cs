using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum SoundType
{

    DIE,
    HEAVYHIT,
    HOOKPUNCH,
    SPECIALATTACK,
    RUN,

    DEATH,
    JUSTICE,
    STRENGTH,
    DEVIL,
    FOOL,
    HERMIT,
    MAGICIAN,
    STAR,
    TOWER,
    WORLD

}

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundList;
    public static AudioManager instance { get; private set;}
    private AudioSource audioSource;

    void Awake()
    {
        instance = this;
        PlayBattleMusic();
    }

    void Start()
    {
     audioSource = GetComponent<AudioSource>();   
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        instance.audioSource.PlayOneShot(instance.soundList[(int)sound], volume);

    }

    public void PlayBattleMusic()
    {
        AudioSource battleMusic = GameObject.Find("Battle Music").GetComponent<AudioSource>();
        battleMusic.Play();
    }

    public void PlayIdleMusic()
    {

        AudioSource idleMusic = GameObject.Find("Idle Music").GetComponent<AudioSource>();
        idleMusic.Play();

    }
}
