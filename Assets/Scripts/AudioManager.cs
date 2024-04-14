using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour
{
    #region Variables
    public bool stopSoundsWhenPaused = true;
    public float pitchRampTime = 1f;
    public float volumeRampTime = 3f;
    public List<Sound> sounds = new List<Sound>() { new Sound() };
    private List<string> blockedSounds = new List<string>();
    private Dictionary<AudioSource, string> lastPlayedSounds = new Dictionary<AudioSource, string>();
    private Coroutine _pitchChangeCo;
    private Coroutine _volumeChangeCo;
    #endregion

    #region Static (Pausing)
    private static List<AudioManager> statAllManagers = new List<AudioManager>();

    public static void PauseAll()
    {
        foreach (AudioManager AMan in statAllManagers)
        {
            AMan.PauseSounds();
        }
    }

    public static void ResumeAll()
    {
        foreach (AudioManager AMan in statAllManagers)
        {
            AMan.ResumeSounds();
        }
    }

    private void SubscribeToPauseList()
    {
        UnsubscribeFromPauseList();

        statAllManagers.Add(this);
    }

    private void UnsubscribeFromPauseList()
    {
        for (int i = 0; i < statAllManagers.Count; ++i)
        {
            if (statAllManagers[i] == null || statAllManagers[i] == this)
            {
                statAllManagers.RemoveAt(i);
                --i;
            }
        }
    }
    #endregion

    #region Events
    void Awake()
    {
        SubscribeToPauseList();
    }

    private void OnDestroy()
    {
        UnsubscribeFromPauseList();
    }
    #endregion

    #region SoundFunctions
    /// <summary> Play the sound from the start (good for landing). </summary>
    public void PlaySound(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Start();
            lastPlayedSounds[sound.Source] = soundName;
        }
    }

    /// <summary> Play the sound if it isn't playing currently (good for triggering every frame). </summary>
    public void PlayOrContinueSound(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Play();
            lastPlayedSounds[sound.Source] = soundName;
        }
    }

    /// <summary> Play the sound and set it to loop (good for events that only trigger at the start and end). </summary>
    public void LoopSound(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Start();
            sound.Looping = true;
            lastPlayedSounds[sound.Source] = soundName;
        }
    }

    public void PlayAndRampPitch(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Play();
            lastPlayedSounds[sound.Source] = soundName;

            if (_pitchChangeCo != null)
                StopCoroutine(_pitchChangeCo);

            _pitchChangeCo = StartCoroutine(ChangePitch(sound.Source, sound.MinPitch, sound.MaxPitch, pitchRampTime));
        }
    }

    public void RampPitchAndLoop(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Play();
            sound.Looping = true;
            lastPlayedSounds[sound.Source] = soundName;

            if (_pitchChangeCo != null)
                StopCoroutine(_pitchChangeCo);

            _pitchChangeCo = StartCoroutine(ChangePitch(sound.Source, sound.MinPitch, sound.MaxPitch, pitchRampTime));
        }
    }

    public void PlayAndDropPitch(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Play();
            lastPlayedSounds[sound.Source] = soundName;

            if (_pitchChangeCo != null)
                StopCoroutine(_pitchChangeCo);

            _pitchChangeCo = StartCoroutine(ChangePitch(sound.Source, sound.MaxPitch, sound.MinPitch, pitchRampTime));
        }
    }

    public void DropPitchAndLoop(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Play();
            sound.Looping = true;
            lastPlayedSounds[sound.Source] = soundName;

            if (_pitchChangeCo != null)
                StopCoroutine(_pitchChangeCo);

            _pitchChangeCo = StartCoroutine(ChangePitch(sound.Source, sound.MaxPitch, sound.MinPitch, pitchRampTime));
        }
    }

    public void SetPitch(string soundName, float pitch)
    {
        Sound sound = GetSound(soundName);

        if (sound != null && IsLastPlayed(sound, soundName))
        {
            pitch = Mathf.Clamp(pitch, 0.5f, 1.5f);
            sound.Source.pitch = pitch;
        }
    }

    public void SetVolume(string soundName, float volume)
    {
        Sound sound = GetSound(soundName);

        if (sound != null && IsLastPlayed(sound, soundName))
        {
            volume = Mathf.Clamp(volume, 0f, 1f);
            sound.Source.volume = volume;
        }
    }

    public void Mute(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null && IsLastPlayed(sound, soundName))
        {
            sound.Source.mute = true;
        }
    }

    public void UnMute(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null && IsLastPlayed(sound, soundName))
        {
            sound.Source.mute = false;
        }
    }

    public void UnMuteFadeIn(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null && sound.Source.mute == false)
            return;

        if (sound != null && IsLastPlayed(sound, soundName))
        {
            sound.Source.mute = false;
        }

        if (_volumeChangeCo != null)
            StopCoroutine(_volumeChangeCo);

        _volumeChangeCo = StartCoroutine(ChangeVolume(sound.Source, 0f, 1f, volumeRampTime));
    }

    public void SetRampTime(float time)
    {
        pitchRampTime = time;
    }

    private IEnumerator ChangeVolume(AudioSource source, float startVolume, float endVolume, float duration)
    {
        source.volume = startVolume;

        float startTime = Time.time;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            source.volume = Mathf.Lerp(startVolume, endVolume, t);
            yield return wait;
        }

        source.volume = endVolume;
        _volumeChangeCo = null;
    }

    private IEnumerator ChangePitch(AudioSource source, float startPitch, float endPitch, float duration)
    {
        source.pitch = startPitch;

        float startTime = Time.time;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            source.pitch = Mathf.Lerp(startPitch, endPitch, t);
            yield return wait;
        }

        source.pitch = endPitch;
        _pitchChangeCo = null;
    }

    /// <summary> Stop the sound immediately (good for cancelling other sounds when hit). </summary>
    public void StopSound(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null && IsLastPlayed(sound, soundName))
        {
            sound.Stop();
        }
    }

    /// <summary> Stop the sound immediately (good for cancelling other sounds when hit). </summary>
    public void StopAllSounds()
    {
        foreach (Sound s in sounds)
        {
            s.Stop();
        }
    }

    /// <summary> Pause the sound (can only be resumed by PlayOrContinue). </summary>
    public void PauseSound(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null && IsLastPlayed(sound, soundName))
        {
            sound.Pause();
        }
    }

    /// <summary> Turn looping off for the sound so it stops after this playthrough (other half of loop). </summary>
    public void StopLoopingSound(string soundName)
    {
        Sound sound = GetSound(soundName);

        if (sound != null && IsLastPlayed(sound, soundName))
        {
            sound.Looping = false;
        }
    }

    /// <summary> True if the audio source of this sound is not playing a different sound. </summary>
    private bool IsLastPlayed(Sound sound, string soundName)
    {
        return (lastPlayedSounds.ContainsKey(sound.Source) == false || lastPlayedSounds[sound.Source] == soundName);
    }

    public void BlockSound(string soundName)
    {
        if (blockedSounds.Contains(soundName))
            return;

        Sound sound = GetSound(soundName);

        if (sound != null)
        {
            sound.Stop();
            blockedSounds.Add(soundName);
        }
    }

    public void UnBlockSound(string soundName)
    {
        blockedSounds.Remove(soundName);
    }

    private Sound GetSound(string soundName)
    {
        Sound sound = null;
        foreach (Sound s in sounds)
        {
            if (s.name == soundName)
                sound = s;
        }
        return sound;
    }

    /// <summary> Pause sounds when the game is paused. </summary>
    public void PauseSounds()
    {
        if (stopSoundsWhenPaused == false)
            return;

        foreach (Sound s in sounds)
        {
            if (s.CurrentlyPlaying)
                s.Pause();
        }
    }

    /// <summary> Resume sounds when the game is un-paused. </summary>
    public void ResumeSounds()
    {
        foreach (Sound s in sounds)
        {
            s.Resume();
        }
    }
    #endregion

    [System.Serializable]
    public class Sound
    {
        public string name;
        [SerializeField]
        private List<AudioClip> clips;
        [SerializeField]
        private AudioSource source;
        public AudioSource Source => source;
        [SerializeField]
        private bool randomPitch = true;
        [SerializeField]
        [Range(0.5f, 1.5f)]
        [FormerlySerializedAs("randomPitchMin")]
        private float pitchMin = 1;
        public float MinPitch => pitchMin;
        [SerializeField]
        [Range(0.5f, 1.5f)]
        [FormerlySerializedAs("randomPitchMax")]
        private float pitchMax = 1;
        public float MaxPitch => pitchMax;

        public Sound()
        {
            pitchMin = 1;
            pitchMax = 1;
        }

        public void Start()
        {
            source.clip = RandomClip;
            source.Stop();
            if (randomPitch)
                source.pitch = RandomPitch;
            source.Play();
            Looping = false;
        }

        public void Play()
        {
            if (CurrentlyPlaying)
                Resume();
            else
                Start();
        }

        public void Pause()
        {
            if (source != null)
                source.Pause();
        }

        public void Resume()
        {
            if (source != null)
                source.UnPause();
        }

        public void Stop()
        {
            source.Stop();
            Looping = false;
        }

        public bool CurrentlyPlaying
        {
            get { return source == null ? false : source.isPlaying; }
        }

        public bool Looping
        {
            get { return source.loop; }
            set { source.loop = value; }
        }

        private float RandomPitch
        {
            get { return Random.Range(pitchMin, pitchMax); }
        }

        private AudioClip RandomClip
        {
            get
            {
                if (clips.Count > 0)
                    return clips[Random.Range(0, clips.Count)];
                return source.clip;
            }
        }
    }
}
