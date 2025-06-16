//Author: Small Hedge Games
//Updated: 13/06/2024

using System;
using UnityEngine;
using UnityEngine.Audio;

namespace SmallHedge.SoundManager
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private SoundsSO SO;
        private static SoundManager instance = null;
        private AudioSource audioSource;

        private void Awake()
        {
            if(!instance)
            {
                instance = this;
                audioSource = GetComponent<AudioSource>();
            }
        }

        public static void PlaySound(SoundType sound, AudioSource source = null, float volume = 1)
        {
            SoundList soundList = instance.SO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if(source)
            {
                source.outputAudioMixerGroup = soundList.mixer;
                source.clip = randomClip;
                source.volume = volume * soundList.volume;
                source.Play();
            }
            else
            {
                instance.audioSource.outputAudioMixerGroup = soundList.mixer;
                instance.audioSource.PlayOneShot(randomClip, volume * soundList.volume);
            }
        }

        public static void PlaySoundAtPosition(SoundType sound, Vector3 position, float volume = 1f, float minDistance = 1f, float maxDistance = 25f)
        {
            SoundList soundList = instance.SO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            GameObject tempGO = new GameObject("TempAudio");
            tempGO.transform.position = position;

            AudioSource aSource = tempGO.AddComponent<AudioSource>();
            aSource.outputAudioMixerGroup = soundList.mixer;
            aSource.clip = randomClip;
            aSource.volume = volume * soundList.volume;

            aSource.spatialBlend = 1f; 
            aSource.minDistance = minDistance; 
            aSource.maxDistance = maxDistance;
            aSource.rolloffMode = AudioRolloffMode.Linear; 

            aSource.Play();
            Destroy(tempGO, randomClip.length);
        }
    }


    [Serializable]
    public struct SoundList
    {
        [HideInInspector] public string name;
        [Range(0, 1)] public float volume;
        public AudioMixerGroup mixer;
        public AudioClip[] sounds;
    }
}