using UnityEngine;

/// <summary>
/// Generates simple procedural audio clips for game SFX.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource audioSource;
    private AudioClip clickClip;
    private AudioClip matchClip;
    private AudioClip noMatchClip;
    private AudioClip winClip;

    private void Awake()
    {
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        GenerateClips();
    }

    public void PlayClick() => audioSource.PlayOneShot(clickClip, 0.4f);
    public void PlayMatch() => audioSource.PlayOneShot(matchClip, 0.6f);
    public void PlayNoMatch() => audioSource.PlayOneShot(noMatchClip, 0.4f);
    public void PlayWin() => audioSource.PlayOneShot(winClip, 0.7f);

    private void GenerateClips()
    {
        clickClip = CreateTone(800, 0.08f);
        matchClip = CreateTwoTone(600, 900, 0.15f);
        noMatchClip = CreateTone(300, 0.2f);
        winClip = CreateFanfare();
    }

    private AudioClip CreateTone(float freq, float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (float)i / samples;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
        }
        var clip = AudioClip.Create("tone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateTwoTone(float f1, float f2, float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        int half = samples / 2;
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = i < half ? f1 : f2;
            float envelope = 1f - (float)i / samples;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
        }
        var clip = AudioClip.Create("twotone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateFanfare()
    {
        int sampleRate = 44100;
        float duration = 0.6f;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        float[] notes = { 523f, 659f, 784f, 1047f }; // C5 E5 G5 C6
        int noteLen = samples / notes.Length;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            int noteIdx = Mathf.Min(i / noteLen, notes.Length - 1);
            float localT = (float)(i - noteIdx * noteLen) / noteLen;
            float envelope = 1f - localT * 0.5f;
            data[i] = Mathf.Sin(2f * Mathf.PI * notes[noteIdx] * t) * envelope * 0.4f;
        }
        var clip = AudioClip.Create("fanfare", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
