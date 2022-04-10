using Tanks;
using Tanks.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks
{
    public class CreatedKeyboard : MonoBehaviour
    {
        [SerializeField] private GameObject[] keys;
        Vector3 startpos = new Vector3(-290, 60, 0);
        [SerializeField] private Font keyFont;
        [SerializeField] private int x = 1, y = 1;
        [SerializeField] private InputField _inputField;
        
        void Start()
        {
            
            keys = GameObject.FindGameObjectsWithTag("Key");
            keys[0].transform.localPosition = startpos;
            foreach (var key in keys)
            {
                if (x == 6)
                {
                    startpos.y -= 25;
                    x = 0;
                    startpos.x = -290;
                }

                key.AddComponent<AddClickEvent>();
                key.GetComponent<AddClickEvent>()._inputField = _inputField;
                key.transform.localPosition = startpos;
                if (key.name.Contains("Space") || key.name.Contains("Continue")||key.name.Contains("Clear"))
                {
                    key.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 25);
                    startpos.x += 150;
                }
                else if (key.name.Contains("Name"))
                {
                    key.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 25);
                    var transformLocalPosition = key.transform.localPosition;
                    transformLocalPosition.x += 50;
                    key.transform.localPosition = transformLocalPosition;
                    startpos.y -= 10;

                }
                else
                {
                    startpos.x += 75;
                    key.GetComponent<RectTransform>().sizeDelta = new Vector2(25, 25);
                }

                Text t = key.GetComponentInChildren<Text>();
                t.name = key.name;
                t.text = t.name;
                t.fontSize = 15;
                t.font = keyFont;
                t.alignByGeometry = true;
                t.alignment = TextAnchor.MiddleCenter;
                t.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
                t.transform.localScale = new Vector3(1, 1, 1);
                x++;
            }
        }
    }
}