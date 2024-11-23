using Mono.Cecil;
using Unity.Netcode;
using UnityEngine;

namespace GUIPetProject
{
    public class GUIPetMoment : MonoBehaviour
    {
        private NetworkManager m_NetworkManager;
        [SerializeField] private PetManager petManager;

        void Awake()
        {
            m_NetworkManager = GetComponent<NetworkManager>();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(100, 100, 400, 400));
            if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
                StatusButtons();
            }

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            if (GUILayout.Button("Host")) m_NetworkManager.StartHost();
            if (GUILayout.Button("Client")) m_NetworkManager.StartClient();
            if (GUILayout.Button("Server")) m_NetworkManager.StartServer();
        }

        void StatusLabels()
        {
            var mode = m_NetworkManager.IsHost ?
                "Host" : m_NetworkManager.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                m_NetworkManager.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        void StatusButtons() {
            if (GUILayout.Button("F or Press to Record"))
            {
                if (petManager != null) {
                    petManager.ButtonClick();
                }
            }
        }
    }
}