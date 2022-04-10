using System;
using Tanks.Client;
using Tanks.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tanks
{
    public class AddClickEvent : MonoBehaviour
    {
        public Button btn;

        public InputField _inputField;

        [SerializeField] private PlayerName m_PlayerName;

        // Start is called before the first frame update
        void Start()
        {
            m_PlayerName = GameManager._GameManager.NetworkPlayerStateComponent.GetComponent<PlayerName>();
            btn = gameObject.GetComponent<Button>();
            if (btn.name.Contains("Back"))
            {
                btn.onClick.AddListener(Backspace);
            }
            else if (btn.name.Equals("Space"))
            {
                btn.onClick.AddListener(delegate { InputText(" "); });
            }
            else if (btn.name.Equals("Continue"))
            {
                btn.onClick.AddListener(NameInputted);
            }
            else if (btn.name.Contains("Name"))
            {
                _inputField.text = String.Empty;
                btn.onClick.AddListener(NameSet);
                NameSet();
            }
            else if (btn.name.Contains("Clear"))
            {
                btn.onClick.AddListener(ClearName);

            }
            else
            {
                btn.onClick.AddListener(delegate { InputText(gameObject.name); });
            }
        }

        void ClearName()
        {
            _inputField.text = String.Empty;
        }
        void NameSet()
        {
            m_PlayerName.NameChoose();
            _inputField.text = GameManager._GameManager.Name;
        }

        private void NameInputted()
        {
            if (_inputField.text != null)
            {
                GameManager._GameManager.Name = _inputField.text;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }

        void InputText(string _name)
        {
            _inputField.text += _name;
        }

        void Backspace()
        {
            var remove = _inputField.text;
            remove = remove.Substring(0, remove.Length - 1);
            _inputField.text = remove;
        }
    }
}