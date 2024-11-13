using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using SimpleJSON; // 或者使用 Newtonsoft.Json

public class AudioRecorder : MonoBehaviour
{
    private AudioClip audioClip;
    private bool isRecording = false;
    private int sampleRate = 16000; // Azure 建议使用 16 kHz

    // Azure 语音服务的参数
    private string speechServiceApiKey = "CmIZZlLAnlhc0bmOsyrbkDAJeWte4XGZo2LMZ341dDPgSKeqKNXwJQQJ99AKACYeBjFXJ3w3AAAYACOG5830";
    private string speechServiceRegion = "eastus"; // 例如 "eastus"


    // Azure 文本分析服务的参数
    private string textAnalyticsApiKey = "7NSiVNOtwqWEivRwxp8YamqwA34HM50GcKr0DlBkyA45HqiKy9vfJQQJ99AKACYeBjFXJ3w3AAAaACOGkynP";
    private string textAnalyticsEndpoint = "https://v2tsentiment1.cognitiveservices.azure.com/";

    public void StartRecording()
    {
        if (!isRecording)
        {
            audioClip = Microphone.Start(null, false, 10, sampleRate);
            isRecording = true;
            Debug.Log("开始录音...");
        }
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            Microphone.End(null);
            isRecording = false;
            Debug.Log("停止录音。");
            StartCoroutine(ProcessAudio());
        }
    }

    private IEnumerator ProcessAudio()
    {
        // 等待录音结束
        yield return null;

        // 将 AudioClip 转换为 WAV 格式的字节数组
        byte[] audioData = WavUtility.FromAudioClip(audioClip);

        // 将音频数据发送到 Azure 语音转文本服务
        yield return StartCoroutine(AzureSpeechToText(audioData));
    }

    private IEnumerator AzureSpeechToText(byte[] audioData)
    {
        string token = ""; // 身份验证令牌
        yield return StartCoroutine(GetAzureToken((result) => token = result));

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("获取 Azure 令牌失败。");
            yield break;
        }

        string url = $"https://{speechServiceRegion}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language=zh-CN";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(audioData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Content-Type", "audio/wav; codecs=audio/pcm; samplerate=16000");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("语音转文本响应：" + responseText);

            // 解析响应并提取转录文本
            string recognizedText = "";
            var jsonResponse = JSON.Parse(responseText);
            if (jsonResponse != null)
            {
                recognizedText = jsonResponse["DisplayText"];
                Debug.Log("识别的文本：" + recognizedText);

                // 进行情感分析
                yield return StartCoroutine(AzureSentimentAnalysis(recognizedText));
            }
        }
        else
        {
            Debug.LogError("语音转文本错误：" + request.error);
        }
    }

    private IEnumerator GetAzureToken(System.Action<string> callback)
    {
        string fetchTokenUri = $"https://{speechServiceRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken";

        UnityWebRequest request = new UnityWebRequest(fetchTokenUri, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Ocp-Apim-Subscription-Key", speechServiceApiKey);
        request.SetRequestHeader("Content-Length", "0");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string token = request.downloadHandler.text;
            callback(token);
        }
        else
        {
            Debug.LogError("获取令牌错误：" + request.error);
            callback(null);
        }
    }

    private IEnumerator AzureSentimentAnalysis(string text)
    {
        string url = $"{textAnalyticsEndpoint}text/analytics/v3.0/sentiment";

        // 创建请求数据
        var postData = new
        {
            documents = new[]
            {
                new { language = "zh-Hans", id = "1", text = text }
            }
        };

        // 将请求数据序列化为 JSON
        string jsonData = JsonUtility.ToJson(postData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Ocp-Apim-Subscription-Key", textAnalyticsApiKey);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("情感分析响应：" + responseText);

            // 解析并使用情感分析结果
            ParseSentimentResponse(responseText);
        }
        else
        {
            Debug.LogError("情感分析错误：" + request.error);
        }
    }

    private void ParseSentimentResponse(string jsonResponse)
    {
        var json = JSON.Parse(jsonResponse);
        if (json != null)
        {
            var sentiment = json["documents"][0]["sentiment"];
            var confidenceScores = json["documents"][0]["confidenceScores"];
            Debug.Log("情感：" + sentiment);
            Debug.Log("置信度分数：积极=" + confidenceScores["positive"] +
                      "，中性=" + confidenceScores["neutral"] +
                      "，消极=" + confidenceScores["negative"]);

            // 根据情感更新应用程序
            UpdateGameBasedOnSentiment(sentiment);
        }
    }

    private void UpdateGameBasedOnSentiment(string sentiment)
    {
        if (sentiment == "positive")
        {
            // 实现积极情感的响应
            Debug.Log("用户感到积极！");
        }
        else if (sentiment == "neutral")
        {
            // 实现中性情感的响应
            Debug.Log("用户感到中性。");
        }
        else if (sentiment == "negative")
        {
            // 实现消极情感的响应
            Debug.Log("用户感到消极。");
        }
    }
}
