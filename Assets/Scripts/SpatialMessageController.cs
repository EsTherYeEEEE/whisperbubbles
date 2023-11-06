using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;

// Spatial Message 控制脚本
// 1. 控制信息球的相互吸引
// 2. 控制语音的播放
public class SpatialMessageController : MonoBehaviour
{
    public SpatialMessage SpatialMessage { get; set; }

    public string testTimestamp = "2023-11-01-17-46-01";

    private SpatialMessageController m_Target;

    private List<SpatialMessage> m_RelatedSpatialMessages = new();

    private void Start()
    {
#if UNITY_EDITOR
        SpatialMessage = new()
        {
            Position = Vector3.zero,
            Timestamp = testTimestamp,
            AudioFileName = "test",
            Duration = 10
        };
#endif
    }

    private void OnTriggerEnter(Collider other)
    {
        // 当信息球碰到别的信息球的时候
        if (other.TryGetComponent<SpatialMessageController>(out var spatialMessageController))
        {
            // 比较时间戳先后
            DateTime dateTimeA = GetDateTime(SpatialMessage.Timestamp);
            DateTime dateTimeB = GetDateTime(spatialMessageController.SpatialMessage.Timestamp);
            // 如果信息球的时间戳比碰到的信息球的时间戳晚
            if (dateTimeA.CompareTo(dateTimeB) > 0)
            {
                if (m_Target == null)
                {
                    // 被该信息球吸引
                    m_Target = spatialMessageController;
                    GetComponent<SmoothBallMovement>().target = spatialMessageController.transform;
                    GetComponent<BallFloat>().IsFloat = true;
                    //GetComponent<Collider>().enabled = false;
                }
                else
                {
                    DateTime dateTimeC = GetDateTime(m_Target.SpatialMessage.Timestamp);
                    // 如果新目标信息球的时间戳比就目标信息球的时间戳晚
                    if (dateTimeC.CompareTo(dateTimeB) > 0)
                    {
                        m_Target = spatialMessageController;
                        GetComponent<SmoothBallMovement>().target = spatialMessageController.transform; 
                    }
                }
            }
        }

        // 当玩家碰到信息球
        if (other.CompareTag("Player"))
        {
            if (m_Target != null) return;

            Debug.Log("Collide with player");
            m_RelatedSpatialMessages.Clear();
            Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<SpatialMessageController>(out var controller))
                {
                    m_RelatedSpatialMessages.Add(controller.SpatialMessage);
                    Debug.Log($"Collide with message with file name: {controller.SpatialMessage.AudioFileName}");
                }
            }
            //m_RelatedSpatialMessages.Add(SpatialMessage);
            Debug.Log($"Number of messages: {m_RelatedSpatialMessages.Count}");
            m_RelatedSpatialMessages.Sort((SpatialMessage message1, SpatialMessage message2) =>
            {
                DateTime message1DateTime = GetDateTime(message1.Timestamp);
                DateTime message2DateTime = GetDateTime(message2.Timestamp);
                if (message1DateTime.CompareTo(message2DateTime) < 0)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            });
            PlayAudioMessages();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 当玩家离开信息球范围
        if (other.CompareTag("Player"))
        {
            StopAudioMessages();
        }
    }

    private void PlayAudioMessages()
    {
        var audioManager = FindObjectOfType<AudioManager>();
        List<string> audioFileNames = new();
        List<float> durations = new();
        for (int i = 0; i < m_RelatedSpatialMessages.Count; i++)
        {
            SpatialMessage message = m_RelatedSpatialMessages[i];
            audioFileNames.Add(message.AudioFileName);
            durations.Add(message.Duration);
        }
        audioManager.PlayAudios(audioFileNames, durations);
    }

    private void StopAudioMessages()
    {
        var audioManager = FindObjectOfType<AudioManager>();
        audioManager.StopPlay();
    }

    static DateTime GetDateTime(string dateString)
    {
        string format = "yyyy-MM-dd-HH-mm-ss";
        CultureInfo provider = CultureInfo.InvariantCulture;

        try
        {
            DateTime result = DateTime.ParseExact(dateString, format, provider);
            return result;
        }
        catch (FormatException)
        {
            Console.WriteLine("{0} is not in the correct format.", dateString);
            return DateTime.Now;
        }
        
    }
}