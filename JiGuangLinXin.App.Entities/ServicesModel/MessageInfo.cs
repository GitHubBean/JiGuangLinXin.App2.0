namespace JiGuangLinXin.App.Entities.ServicesModel
{
    /// <summary>
    /// 推送信息
    /// </summary>
    public class MessageInfo
    {
        /// <summary>
        /// 信息类型
        /// </summary>
        public int Type;
        /// <summary>
        /// 返回值（字符串、实体类、集合）
        /// </summary>
        public object Data;

        public MessageInfo(int type, object data)
        {
            Type = type;
            Data = data;
        }
    }

    /// <summary>
    /// 推送信息
    /// </summary>
    public class MessageData
    {
        public string ID;
        public string Card;
        public decimal Money;
        public string Type;

        public MessageData(string id, string card, decimal money, string type)
        {
            ID = id;
            Card = card;
            Money = money;
            Type = type;
        }
    }
}
