using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource AudioSource => m_AudioSource;

    private SaveWavScript saveWavScript;

    private AudioSource m_AudioSource;

    private AudioClip m_RecordClip;

    private bool m_MicConnected;

    private int minFreq, maxFreq;

    private bool m_IsRecording;

    private int m_NumOfAudioToPlay;

    private Coroutine m_Coroutine;

    private float m_StartRecordingTime;

    private List<AudioClip> m_AudioClips = new();

    private Dictionary<string, float> m_AudioDurationDict = new();

    private void Start()
    {
        if (Microphone.devices.Length <= 0)
        {
            m_MicConnected = false;
        }
        else
        {
            m_MicConnected = true;
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);
            if (minFreq == 0 && maxFreq == 0)
            {
                maxFreq = 44100;
            }
        }

        m_AudioSource = GetComponent<AudioSource>();
        saveWavScript = GetComponent<SaveWavScript>();
        saveWavScript.OnGetClip.AddListener(OnGetClip);
    }

    public void StartAudioRecording()
    {
        if (!m_MicConnected) return;

        if (!Microphone.IsRecording(null))
        {
            m_StartRecordingTime = Time.time;
            m_RecordClip = Microphone.Start(null, false, 60, maxFreq);
        }
    }

    public void StopAudioRecording(out string audioFileName, out float duration)
    {
#if !UNITY_EDITOR
        string fileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".wav";
        SaveWavScript.SavWav.Save(fileName, m_RecordClip);
        audioFileName = fileName;
        duration = Time.time - m_StartRecordingTime;
#else
        audioFileName = "Empty";
        duration = Time.time - m_StartRecordingTime;
#endif
        Microphone.End(null);
    }

    public void ReplayAudio()
    {
        m_AudioSource.clip = m_RecordClip;
        m_AudioSource.Play();
    }

    public void PlayAudio(string audioFileName)
    {
        string path = Path.Combine(Application.persistentDataPath, audioFileName);
        saveWavScript.LoadAudioClip(path, AudioType.WAV);
    }

    public void PlayAudios(List<string> audioFileNames, List<float> durations)
    {
        m_NumOfAudioToPlay = audioFileNames.Count;
        m_AudioClips.Clear();
        int i = 0;
        foreach (string audioFileName in audioFileNames)
        {
            float duration = durations[i];
            string path = Path.Combine(Application.persistentDataPath, audioFileName);
            m_AudioDurationDict[path] = duration;
            saveWavScript.LoadAudioClip(path, AudioType.WAV);
            i++;
        }
    }

    public void StopPlay()
    {
        StopCoroutine(m_Coroutine);
        m_AudioSource.Stop();
    }

    private void OnGetClip(AudioClip audioClip, string path)
    {
        Debug.Log($"OnGetClip: {path}");
        float duration = m_AudioDurationDict[path];
        int sampleRate = audioClip.frequency;
        int samplesWithinDuration = Mathf.CeilToInt(duration) * sampleRate;

        float[] samples = new float[samplesWithinDuration * audioClip.channels];
        audioClip.GetData(samples, 0);

        AudioClip newClip = AudioClip.Create("NewClip", samplesWithinDuration, audioClip.channels, sampleRate, false);
        newClip.SetData(samples, 0);

        m_AudioClips.Add(newClip);
        if (m_AudioClips.Count == m_NumOfAudioToPlay)
        {
            Debug.Log($"Start to play all audios: {m_AudioClips.Count}");
            m_Coroutine = StartCoroutine(PlayAudioInARow());
        }
    }

    private IEnumerator PlayAudioInARow()
    {
        yield return null;
        for (int i = 0; i < m_AudioClips.Count; i++)
        {
            Debug.Log($"Play i: {i}, duration: {m_AudioClips[i].length}");
            m_AudioSource.clip = m_AudioClips[i];
            m_AudioSource.Play();

            while (m_AudioSource.isPlaying)
            {
                //Debug.Log("aaaaaaa");
                yield return null;
            }  
        }
    }
}
