﻿namespace JiguangLinXin.App.SpecialEvent.Models
{
    /// <summary>
    /// 返回的结果集
    /// </summary>
    public class ResultViewModel
    {
        public ResultViewModel()
        {
            State = 1;
            Msg = "请求数据异常，请检查网络";
            Data = null;
        }
        public ResultViewModel(int state,string msg ,object data)
        {
            State = state;
            Msg = msg;
            Data = data;
        }
        /// <summary>
        /// 接口处理结果 0正常 1异常
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 接口处理描述信息：异常信息
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 数据结果集，JSON字符串
        /// </summary>
        public object Data { get; set; }
    }
}