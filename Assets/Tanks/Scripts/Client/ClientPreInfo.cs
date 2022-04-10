using UnityEngine;

namespace Tanks.Client
{
    public class ClientPreInfo
    {
        public static string GetGuid()
        {
            if (PlayerPrefs.HasKey("local_guid"))
            {
                return PlayerPrefs.GetString("local_guid");
            }
            var guid = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("local_guid",guid);
            return guid;
        }
    }
}