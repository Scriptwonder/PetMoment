using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class AudioUploader : MonoBehaviour
{
    public string serverUrl = "http://127.0.0.1:5000/analyze"; // 替换为你的服务器URL

    void Start()
    {
        StartRecording(); // 在场景启动时自动开始录音
    }

    // 开始录音
    private void StartRecording()
    {
        AudioClip audioClip = Microphone.Start(null, false, 10, 16000); // 在Quest 2上录制10秒，采样率为16kHz
        if (audioClip == null)
        {
            Debug.LogError("Failed to start recording.");
            return;
        }
        StartCoroutine(WaitForRecording(audioClip));
    }

    private IEnumerator WaitForRecording(AudioClip clip)
    {
        yield return new WaitForSeconds(10); // 等待录音结束
        Microphone.End(null);
        string filePath = Path.Combine(Application.persistentDataPath, "audio.wav");
        SaveWavFile(clip, filePath);
        StartCoroutine(UploadAudio(filePath)); // 自动上传录制完成的音频
    }

    // 将录制的音频保存为WAV文件
    private void SaveWavFile(AudioClip clip, string filePath)
    {
        byte[] wavData = WavUtility.FromAudioClip(clip);
        File.WriteAllBytes(filePath, wavData);
        Debug.Log("Audio saved to " + filePath);
    }

    // 上传音频到服务器
    private IEnumerator UploadAudio(string filePath)
    {
        byte[] audioData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                Debug.Log("Response: " + www.downloadHandler.text);
                // 可以在此处添加VR界面上显示结果的逻辑
            }
        }
    }
}
