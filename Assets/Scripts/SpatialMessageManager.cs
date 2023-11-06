using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[Serializable]
public class SerializableList<T>
{
    public List<T> List;
}

// Spatial Message 数据结构
[Serializable]
public class SpatialMessage
{
    public Vector3 Position;
    public string Timestamp;
    public string AudioFileName;
    public float Duration;
}

public class SpatialMessageManager : MonoBehaviour
{
    [SerializeField] private Transform cam;

    [SerializeField] private AudioManager audioManager;

    [SerializeField] private float createSpatialPointInterval = 1f;

    [SerializeField] private SpatialMessageController spatialMessagePrefab;

    [SerializeField] private TMP_Text recordButtonText;

    private SerializableList<SpatialMessage> spatialMessages = new();

    private DateTime startRecordingTime;

    private float lastCreateSpatialPointTime;

    private bool isRecordingSpatialMessage = false;

    private void Start()
    {
        // Step 0: 从手机存贮中读取 Spatial Message 数据
        spatialMessages.List = new();

        string json = PlayerPrefs.GetString("json", "empty");
        if (!json.Equals("empty"))
        {
            spatialMessages = JsonUtility.FromJson<SerializableList<SpatialMessage>>(json);
            Debug.Log($"Load spatial messages from disk: {json}");

            // Step 1: 根据 Spatial Message 数据，生成（重建）信息球
            ReconstructSpatialMessages();
        }
    }

    private void OnDestroy()
    {
        foreach (var spatialMessage in spatialMessages.List)
        {
            Debug.Log($"Spatial message: {spatialMessage}");
        }

        string json = JsonUtility.ToJson(spatialMessages);
        PlayerPrefs.SetString("json", json);
        Debug.Log($"Save spatial messages to disk: {json}");
    }

    private void ReconstructSpatialMessages()
    {
        foreach (var spatialMessage in spatialMessages.List)
        {
            var spatialMessageInstance = Instantiate(spatialMessagePrefab, spatialMessage.Position, Quaternion.identity);
            float scale = GetSpatialMessageScale(spatialMessage.Duration);
            spatialMessageInstance.transform.localScale = new(scale, scale, scale);
            spatialMessageInstance.SpatialMessage = spatialMessage;
        }
    }

    private void Update()
    {
        //if (Time.time - lastCreateSpatialPointTime > createSpatialPointInterval)
        //{
        //    CreateSpatialPoint();
        //}
    }

    //private void CreateSpatialMessage()
    //{
    //    SpatialMessage spatialPoint = new()
    //    {
    //        Position = cam.position,
    //        Timestamp = DateTime.Now
    //    };
    //    cameraTrace.Add(spatialPoint);
    //    lastCreateSpatialPointTime = Time.time;

    //    // Create spatial point visual
    //    Instantiate(isRecordingSpatialMessage ? audioSpatialPointPrefab : normalSpatialPointPrefab, spatialPoint.Position, Quaternion.identity);
    //}

    // Step 2: 开始录制 Spatial Message
    public void StartCreatingSpatialMessage()
    {
        // 开始录制语音
        audioManager.StartAudioRecording();

        // 记录正在录音状态
        isRecordingSpatialMessage = true;
        // 将 UI 按键变成 Stop
        recordButtonText.text = "Stop";
    }

    // Step 3: 结束录制 Spatial Message
    public void StopCreatingSpatialMessage()
    {
        // 检查是否正在录音
        if (!isRecordingSpatialMessage) return;

        // 结束语音录制
        audioManager.StopAudioRecording(out var audioFileName, out var clipDuration);
        Debug.Log($"stop recording with file name: {audioFileName}");
        // 创建一个新的 Spatial Message 对象
        Vector3 position = cam.position + cam.forward * 0.6f;
        SpatialMessage newMessage = new()
        {
            Position = position,    
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"),
            AudioFileName = audioFileName,
            Duration = clipDuration
        };
        spatialMessages.List.Add(newMessage);
        // 根据新的 Spatial Message 对象，创建它的可视化对象
        var spatialMessageInstance = Instantiate(spatialMessagePrefab, newMessage.Position, Quaternion.identity);
        float scale = GetSpatialMessageScale(newMessage.Duration);
        spatialMessageInstance.transform.localScale = new(scale, scale, scale);

        spatialMessageInstance.SpatialMessage = newMessage;

        // 记录录音结束状态
        isRecordingSpatialMessage = false;
        // 将 UI 按键变成 Record
        recordButtonText.text = "Record";
    }

    public void SwitchAudioRecording()
    {
        if (isRecordingSpatialMessage)
            StopCreatingSpatialMessage();
        else
            StartCreatingSpatialMessage();
    }

    public void OnCollideSpatialMessage(SpatialMessageController spatialMessageController)
    {
        audioManager.PlayAudio(spatialMessageController.SpatialMessage.AudioFileName);
    }

    public void ClearSpatialMessages()
    {
        var visuals = FindObjectsOfType<SpatialMessageController>();
        foreach (var visual in visuals)
        {
            Destroy(visual.gameObject);
        }
        spatialMessages.List.Clear();
    }

    private float GetSpatialMessageScale(float duration)
    {
        if (duration <= 5f)
            return 0.75f;

        if (duration > 5f && duration <= 10f)
            return 1f;

        if (duration > 10f)
            return 1.5f;

        return 1f;
    }
}
        