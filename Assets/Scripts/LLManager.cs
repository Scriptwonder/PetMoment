using UnityEngine;
using OpenAI;
using OpenAI.Assistants;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI.Chat;
using OpenAI.Models;
using System.Linq;
using System.Text;
using Unity.Netcode;
using Unity.Tutorials.Core.Editor;
using Mono.Cecil;

public class LLManager : NetworkBehaviour
{
    [SerializeField] private OpenAIAuthentication auth;

    [SerializeField] private PetManager petManager;

    private Model model;

    [SerializeField]
    private string currentContext;
    //[SerializeField]
    //private  TextAsset labelsAsset;

    private string[] labelIndexes;

    List<Message> messages = new List<Message>();

    List<Message> outputMessages = new List<Message>();

    private static LLManager instance;

    private StringBuilder sb = new StringBuilder();

    private int currentConversationBucket = 0;

    private int currentMessageIndex = 0;

    private int conversationBucketLimit = 5;

    

    public static LLManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<LLManager>();
                
                // Create new instance if none exists
                if (instance == null)
                {
                    GameObject go = new GameObject("LLManager");
                    instance = go.AddComponent<LLManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        //var authToken = await LoginAsync();
        //auth = new OpenAIAuthentication("yourownkey");
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    
    }

    void Start()
    {
        Setup();

    }

    void Update()
    {

    }
        

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Setup()
    {
        var api = new OpenAIClient(auth);
        model = await api.ModelsEndpoint.GetModelDetailsAsync("gpt-4o");
        var assistant = await api.AssistantsEndpoint.CreateAssistantAsync(
            new CreateAssistantRequest(
                model,
                name: "PetMoment Assistant",
                description: "PetMoment Assistant",
                instructions: "You are a helpful pet assistant that will guide and manage team dynamics in a group meeting setting."
            )
        );
        StartNewMessage();
        if (currentContext.Length == 0) {
            currentContext = "You are a helpful pet assistant that will guide and manage team dynamics in a group meeting setting.";
        }
        InputMessage(currentContext);
        InputMessage("You will be given a conversation text in the format of '[SpeakerID]:Text', and I want you to diagnose in the following format: ");
        InputMessage("The given test may be based on a current time bucket, so only a segment of the conversation will be given one time. But it will be continuous conversation about how to solve a problem");
        InputMessage("You should output the current action that should be done by you, which is to intervene or not, and the reason for your decision.");
        InputMessage("You will be given several options of the interaction to choose from, and it is based on Thomas-Kilmann Conflict Management Model. here are the options:");
        //given the option:
        InputMessage("1. Default: Do nothing, let the conversation continue.");
        InputMessage("2. Happy: Based on the conversation, you are happy with the current conversation and you want to continue.");
        InputMessage("3. Upset: Based on the conversation, you are upset with the current situation and show your upset to the group.");
        InputMessage("4. Angry: Based on the conversation, you are angry with the current situation and show your anger to the group.");
        InputMessage("5. Surprised: Based on the conversation, you are shocked and stumbled.");
        InputMessage("6. Cheerful: Based on the conversation, you are happy and want to dance.(Team Dynamics go pretty well)");
        InputMessage("7. Supportive: Based on the conversation, you are happy and want to clap.(Team Dynamics go pretty well)");
        InputMessage("Given these options, select the best one that would fit to act at this current bucket of time.");
        InputMessage("In the following answers, you should always given the answer with only a number at the first letter, and the reason for your decision at the second index.");
        await SendMessage();


        //DissectMessage(outputMessages.Last());
    }

    public async void AddMessage(string message) {
        //InputMessage(message);
        currentConversationBucket++;
        sb.AppendFormat("{0}: {1}\n", currentMessageIndex++ , message);
        //Debug.Log(currentConversationBucket);
        if (currentConversationBucket >= conversationBucketLimit) {
            //Debug.Log("Conversation Bucket Limit Reached");
            currentConversationBucket = 0;
            InputMessage("Here is the conversation text, give your answer with only a number at the first letter, and the reason for your decision at the end: ");
            InputMessage(sb.ToString());
            sb.Clear();
            await SendMessage();
            petManager.AnalyzeAnimationServerRpc(outputMessages.Last().ToString());
        }
    }

    //every message will go through this method
    public async Task SendMessage()
    {
        var api = new OpenAIClient(auth);
        var chatRequest = new ChatRequest(messages, model);
        var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        var choice = response.FirstChoice;
        Debug.Log($"[{choice.Index}] {choice.Message.Role}: {choice.Message} | Finish Reason: {choice.FinishReason}");
        outputMessages.Add(choice.Message);
    }

    public void DissectMessage(Message message) {
        labelIndexes = message.ToString().Split(", ");
        foreach (string labelIndex in labelIndexes) {
            Debug.Log(labelIndex);
        }
    }

    public void InputMessage(string message) {
        messages.Add(new Message(Role.User, message));
    }

    public void StartNewMessage() {
        messages.Clear();
    }

    public bool FindLabelIndex(string labelIndex) {
        if (labelIndexes == null) {
            return false;
        }
        foreach (string label in labelIndexes) {
            if (label == labelIndex) {
                return true;
            }
        }
        return false;
    }

    public async void ConcludeMessage() {
        InputMessage("Based on our previous history, give a detailed summary of the conversation and its team dynamics.");
        await SendMessage();
        petManager.SummaryTextServerRpc(outputMessages.Last().ToString());
    }
}
