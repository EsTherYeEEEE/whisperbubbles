using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public List<AudioClip> m_Clips = new();

    private AudioSource m_AudioSource;

    private void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();

        StartCoroutine(PlayClipsInARow());
    }

    private IEnumerator PlayClipsInARow()
    {
        yield return null;

        for (int i = 0; i < m_Clips.Count; i++)
        {
            m_AudioSource.clip = m_Clips[i];
            m_AudioSource.Play();

            while (m_AudioSource.isPlaying)
            {
                yield return null;
            }
        }
    }
}
