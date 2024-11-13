using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using SimpleJSON; // ����ʹ�� Newtonsoft.Json

public class AudioRecorder : MonoBehaviour
{
    private AudioClip audioClip;
    private bool isRecording = false;
    private int sampleRate = 16000; // Azure ����ʹ�� 16 kHz

    // Azure ��������Ĳ���
    private string speechServiceApiKey = "CmIZZlLAnlhc0bmOsyrbkDAJeWte4XGZo2LMZ341dDPgSKeqKNXwJQQJ99AKACYeBjFXJ3w3AAAYACOG5830";
    private string speechServiceRegion = "eastus"; // ���� "eastus"


    // Azure �ı���������Ĳ���
    private string textAnalyticsApiKey = "7NSiVNOtwqWEivRwxp8YamqwA34HM50GcKr0DlBkyA45HqiKy9vfJQQJ99AKACYeBjFXJ3w3AAAaACOGkynP";
    private string textAnalyticsEndpoint = "https://v2tsentiment1.cognitiveservices.azure.com/";

    public void StartRecording()
    {
        if (!isRecording)
        {
            audioClip = Microphone.Start(null, false, 10, sampleRate);
            isRecording = true;
            Debug.Log("��ʼ¼��...");
        }
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            Microphone.End(null);
            isRecording = false;
            Debug.Log("ֹͣ¼����");
            StartCoroutine(ProcessAudio());
        }
    }

    private IEnumerator ProcessAudio()
    {
        // �ȴ�¼������
        yield return null;

        // �� AudioClip ת��Ϊ WAV ��ʽ���ֽ�����
        byte[] audioData = WavUtility.FromAudioClip(audioClip);

        // ����Ƶ���ݷ��͵� Azure ����ת�ı�����
        yield return StartCoroutine(AzureSpeechToText(audioData));
    }

    private IEnumerator AzureSpeechToText(byte[] audioData)
    {
        string token = ""; // �����֤����
        yield return StartCoroutine(GetAzureToken((result) => token = result));

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("��ȡ Azure ����ʧ�ܡ�");
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
            Debug.Log("����ת�ı���Ӧ��" + responseText);

            // ������Ӧ����ȡת¼�ı�
            string recognizedText = "";
            var jsonResponse = JSON.Parse(responseText);
            if (jsonResponse != null)
            {
                recognizedText = jsonResponse["DisplayText"];
                Debug.Log("ʶ����ı���" + recognizedText);

                // ������з���
                yield return StartCoroutine(AzureSentimentAnalysis(recognizedText));
            }
        }
        else
        {
            Debug.LogError("����ת�ı�����" + request.error);
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
            Debug.LogError("��ȡ���ƴ���" + request.error);
            callback(null);
        }
    }

    private IEnumerator AzureSentimentAnalysis(string text)
    {
        string url = $"{textAnalyticsEndpoint}text/analytics/v3.0/sentiment";

        // ������������
        var postData = new
        {
            documents = new[]
            {
                new { language = "zh-Hans", id = "1", text = text }
            }
        };

        // �������������л�Ϊ JSON
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
            Debug.Log("��з�����Ӧ��" + responseText);

            // ������ʹ����з������
            ParseSentimentResponse(responseText);
        }
        else
        {
            Debug.LogError("��з�������" + request.error);
        }
    }

    private void ParseSentimentResponse(string jsonResponse)
    {
        var json = JSON.Parse(jsonResponse);
        if (json != null)
        {
            var sentiment = json["documents"][0]["sentiment"];
            var confidenceScores = json["documents"][0]["confidenceScores"];
            Debug.Log("��У�" + sentiment);
            Debug.Log("���Ŷȷ���������=" + confidenceScores["positive"] +
                      "������=" + confidenceScores["neutral"] +
                      "������=" + confidenceScores["negative"]);

            // ������и���Ӧ�ó���
            UpdateGameBasedOnSentiment(sentiment);
        }
    }

    private void UpdateGameBasedOnSentiment(string sentiment)
    {
        if (sentiment == "positive")
        {
            // ʵ�ֻ�����е���Ӧ
            Debug.Log("�û��е�������");
        }
        else if (sentiment == "neutral")
        {
            // ʵ��������е���Ӧ
            Debug.Log("�û��е����ԡ�");
        }
        else if (sentiment == "negative")
        {
            // ʵ��������е���Ӧ
            Debug.Log("�û��е�������");
        }
    }
}
