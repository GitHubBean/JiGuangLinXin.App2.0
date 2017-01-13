namespace JiGuangLinXin.App.Entities.ServicesModel
{
    /// <summary>
    /// 返回信息
    /// </summary>
    public struct ReturnInfo
    {
        /// <summary>
        /// 信息等级 0.正常 1.警告 2.错误
        /// </summary>
        public int State;
        /// <summary>
        /// 信息说明
        /// </summary>
        public string Message;
        /// <summary>
        /// 返回值
        /// </summary>
        public object Data;

        public ReturnInfo(object data)
        {
            State = 0;
            Message = null;
            Data = data;
        }
        public ReturnInfo(int state, string message)
        {
            State = state;
            Message = message;
            Data = null;
        }
        public ReturnInfo(int state, string error, object data)
        {
            State = state;
            Message = error;
            Data = data;
        }


        public static ReturnInfo Empty
        {
            get
            {
                return new ReturnInfo(2, "数据未赋值", null);
            }
        }
    }
}
