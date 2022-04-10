using TMPro;
using UnityEngine;
using Tanks;
using UnityEngine.UI;

namespace Tanks.Shared
{
    public class PlayerName : MonoBehaviour
    {
        private NetworkName m_Name;
        [SerializeField] private NameGenerate m_NameData;


        // Start is called before the first frame update
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            m_Name = gameObject.GetComponent<NetworkName>() == null ? gameObject.AddComponent<NetworkName>() : gameObject.GetComponent<NetworkName>();
            NameChoose();
        }

        public string PName
        {
            get => m_Name.Name.Value;
            set => m_Name.Name.Value = value;
        }

        public void NameChoose()
        {
            PName = m_NameData.Names[Random.Range(0, m_NameData.Names.Length - 1)];
        }

        public void NameChoose(string a)
        {
            PName = m_NameData.Names[Random.Range(0, m_NameData.Names.Length - 1)];
            GameManager._GameManager.Name = PName;
        }
    }
}