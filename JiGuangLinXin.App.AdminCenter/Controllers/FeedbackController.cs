using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JiGuangLinXin.App.AdminCenter.Extension.Filters;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities.BaseEnum;
using Webdiyer.WebControls.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{


    /// <summary>
    /// 投诉反馈
    /// </summary>
    public class FeedbackController : BaseController
    {
        private FeedbackCore fbCore = new FeedbackCore();

        /// <summary>
        /// 投诉
        /// </summary>
        /// <returns></returns>
        public ActionResult Complaint(int pn = 0, int rows = 10)
        {
            var list = fbCore.LoadEntities(o => o.F_Flag == (int)FeedbackEnum.举报).OrderBy(o => o.F_Status).ThenBy(o => o.F_Time).ToPagedList(pn, rows);
            return View(list);
        }

        /// <summary>
        /// 处理反馈
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        [HttpPost]
        public string Check(int id, string content)
        {
            var obj = fbCore.LoadEntity(o => o.F_Id == id && o.F_Status == 0);
            if (obj != null && !string.IsNullOrEmpty(content.Trim()))
            {
                obj.F_Status = 1;
                obj.F_Reply = content;
                obj.F_ReplyTime = DateTime.Now;
                if (fbCore.UpdateEntity(obj))
                {
                    return "回复成功";
                }
            }
            return "记录不存在";
        }



        /// <summary>
        /// 建议
        /// </summary>
        /// <returns></returns>
        public ActionResult Advice(int pn = 1, int rows = 10)
        {
            var list = fbCore.LoadEntities(o => o.F_Flag == (int)FeedbackEnum.反馈).OrderBy(o => o.F_Status).ThenBy(o => o.F_Time).ToPagedList(pn, rows);
            return View(list);
        }

        /// <summary>
        /// 用户便民 付款成功，交易失败
        /// </summary>
        /// <param name="pn"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public ActionResult Order(int pn = 1, int rows = 10)
        {
            var list = fbCore.LoadEntities(o => o.F_Flag == (int)FeedbackEnum.系统).OrderBy(o => o.F_Status).ThenBy(o => o.F_Time).ToPagedList(pn, rows);
            return View(list);
        }


        /// <summary>
        /// 忽略订单反馈
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        [HttpPost]
        public string OrderIgnore(int id, string content)
        {
            var obj = fbCore.LoadEntity(o => o.F_Id == id && o.F_Status == 0);

            if (obj != null)
            {
                obj.F_Status = 1;
                obj.F_Reply = content;
                obj.F_ReplyTime = DateTime.Now;
                if (fbCore.UpdateEntity(obj))
                {
                    return "ok";
                }
            }
            return "记录不存在";
        }

        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="id">ID 反馈记录</param>
        /// <param name="content">审核备注</param>
        /// <param name="source">订单来源</param>
        /// <param name="orderNo">订单号</param>
        /// <returns></returns>
        [JsonException]
        [HttpPost]
        public string OrderBackmoney(int id, string content, int  source, string orderNo)
        {
            var obj = fbCore.LoadEntity(o => o.F_Id == id && o.F_Status == 0);

            if (obj != null)
            {
                obj.F_Status = 1;
                obj.F_Reply = content;
                obj.F_ReplyTime = DateTime.Now;
                if (fbCore.BackOrderMoney(id,content,source ,orderNo))
                {
                    return "ok";
                }
            }
            return "记录不存在";
        }
    }
}
