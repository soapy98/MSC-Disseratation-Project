namespace Tanks.Server
{
    public class GameStateRelayObj
    {
        private static object m_RelayObject = null;

        public static object GetRelayObj()
        {
            var obj = m_RelayObject;
            m_RelayObject = null;
            return obj;
        }
        public static object RelayObjectSet
        {
            set => m_RelayObject = value;
        }
    }
}