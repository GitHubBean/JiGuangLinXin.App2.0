using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.EncryptHelper;
using Newtonsoft.Json.Linq;
using JiGuangLinXin.App.BusinessSystem.Extension;


namespace JiGuangLinXin.App.BusinessSystem.Controllers
{
    public class BusinessController : BaseController
    {
        private BusinessCore busCore = new BusinessCore();
        private BusinessEnteringApplyCore applyCore = new BusinessEnteringApplyCore();
        private AttachmentCore attCore = new AttachmentCore();
        private BusinessVillageCore bvCore = new BusinessVillageCore();
        /// <summary>
        /// 编辑社区服务
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel EditService([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid busId = obj.busId;
            string serviceImg = obj.serviceImg;
            string desc = obj.desc;
            string mobPhone = obj.mobPhone;
            string tel = obj.tel;


            var info = busCore.LoadEntity(o => o.B_Id == busId);
            if (info != null)
            {
                info.B_ServiceImg = serviceImg;
                info.B_ServiceItem = desc;
                info.B_Phone = mobPhone + "," + tel;
                if (busCore.UpdateEntity(info))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            else
            {

                rs.Msg = "商家服务不存在";
            }

            return rs;
        }

        /// <summary>
        /// 编辑社区基本信息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel EditBaseInfo([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid busId = obj.busId;
            string address = obj.address;
            string desc = obj.desc;
            int isHot = obj.isHot;
            int isReturn = obj.isReturn;
            int isAuthentic = obj.isAuthentic;
            int isFree = obj.isFree;
            int isFamous = obj.isFamous;


            IEnumerable<dynamic> images = obj.images;// 商家，图片附件



            var info = busCore.LoadEntity(o => o.B_Id == busId);
            if (info != null)
            {
                info.B_Desc = desc;
                info.B_Address = address;

                info.B_IsHot = isHot;
                info.B_IsFamous = isFamous;
                info.B_IsFree = isFree;
                info.B_IsAuthentic = isAuthentic;
                info.B_IsReturns = isReturn;


                if (busCore.EditBusInfo(info, images))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }
            else
            {

                rs.Msg = "商家服务不存在";
            }

            return rs;
        }
        /// <summary>
        /// 商家详情
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Show([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid busId = obj.busId;

            var info = busCore.LoadEntity(o => o.B_Id == busId);
            if (info != null)
            {
                //商家图片
                var images = attCore.LoadEntities(o => o.A_PId == busId).OrderBy(o => o.A_Rank).Select(am => new
                {
                    am.A_FileNameOld,
                    am.A_FileName,
                    am.A_Size,
                    am.A_Folder,
                    am.A_Rank
                });
                //服务社区
                var villList =
                    bvCore.LoadEntities(o => o.BV_BusinessId == busId).GroupBy(o => o.BV_CityName).Select(o => new
                    {
                        cityName = o.Key.ToString(),
                        count = o.Count()
                    });


                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    staticImgUrl = StaticHttpUrl,
                    images,
                    villList,
                    logo = StaticHttpUrl + info.B_Logo,
                    busName = info.B_NickName,
                    servicePhone = info.B_Phone,
                    authPhone = info.B_LoginPhone,
                    address = info.B_Address,
                    isHot = info.B_IsHot,
                    isReturn = info.B_IsReturns,
                    isAuthentic = info.B_IsAuthentic,
                    isFree = info.B_IsFree,
                    isFamous = info.B_IsFamous,
                    desc = info.B_Desc,

                    trueName = info.B_TrueName,
                    cardNo = info.B_CardId.Replace(info.B_CardId.Substring(10, 5), "******"),

                    serviceImg = info.B_ServiceImg,
                    serviceItem = info.B_ServiceItem
                };
            }
            else
            {
                rs.Msg = "商家服务不存在";
            }

            return rs;
        }

        /// <summary>
        /// 根据商家的名称、电话搜索
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel QueryByKeys([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            dynamic obj = value;
            string key = obj.keys;

            rs.Data =
                busCore.LoadEntities(
                    o =>
                        o.B_Status != (int)UserStatusEnum.冻结 &&
                        (o.B_NickName.Contains(key) || o.B_LoginPhone.Contains(key)))
                    .OrderByDescending(o => o.B_RegisterDate)
                    .Take(20).Select(o => new
                    {
                        busId = o.B_Id,
                        busName = o.B_NickName,
                        busPhone = o.B_LoginPhone,
                        busRole = o.B_Role,
                        busLogo = StaticHttpUrl + o.B_Logo,
                        address = o.B_Address
                    });
            return rs;
        }
        /// <summary>
        /// 商家服务类型
        /// </summary>
        /// <returns></returns>
        public ResultMessageViewModel ServiceTypeList()
        {
            LifestyleTypeCore typeCore = new LifestyleTypeCore();
            ResultMessageViewModel rs = new ResultMessageViewModel(0, "ok", null);
            var types = typeCore.LoadEntities(o => o.T_State == 0).OrderBy(o => o.T_Rank);
            rs.Data = types.Select(o => new
            {
                tid = o.T_Id,
                tname = o.T_Title,
                img = StaticHttpUrl + o.T_CoverImg,
                remark = o.T_Remark
            });
            return rs;
        }

        /// <summary>
        /// 商家服务，列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel SuccessList([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            string querySTime = obj.querySTime;
            string queryETime = obj.queryETime;
            string title = obj.title;
            int typeId = obj.typeId;

            int pn = obj.pn;
            int rows = obj.rows;

            Expression<Func<Core_Business, Boolean>> exp = o => o.B_AuditingState == (int)AuditingEnum.认证成功;  //筛选条件


            if (typeId > 0)
            {
                exp = exp.And(o => o.B_Category == typeId);
            }
            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.B_RegisterDate > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.B_RegisterDate < et);
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.B_NickName.Contains(title));
            }
            LifestyleTypeCore mtCore = new LifestyleTypeCore();
            var typeList = mtCore.LoadEntities().ToList();
            var list = busCore.LoadEntities(exp).OrderByDescending(o => o.B_RegisterDate).Skip(pn * rows).Take(rows).Select(o => new
            {

                busId = o.B_Id,
                busName = o.B_NickName,
                busPhone = o.B_LoginPhone,
                busRole = o.B_Role,
                busLogo = o.B_Logo,
                address = o.B_Address,
                remark = o.B_Remark,
                time = o.B_RegisterDate,

                busType = o.B_Category,
                state = o.B_Status


            });

            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    category = typeList.Select(o => new
                    {
                        o.T_Id,
                        o.T_Title
                    }),
                    list
                };
            }
            else
            {
                rs.Msg = "没有更多数据";
            }
            return rs;
        }


        /// <summary>
        /// 审核中、审核失败的列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel AudingList([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;

            string querySTime = obj.querySTime;
            string queryETime = obj.queryETime;
            string title = obj.title;
            int state = obj.state;

            int pn = obj.pn;
            int rows = obj.rows;

            Expression<Func<Core_BusinessEnteringApply, Boolean>> exp = o => true;  //筛选条件

            if (state > 0)
            {
                exp = exp.And(o => o.BA_State == state);
            }

            if (!string.IsNullOrEmpty(querySTime))
            {
                DateTime st;
                if (DateTime.TryParse(querySTime, out  st))
                {
                    exp = exp.And(o => o.BA_Time > st);
                }
            }

            if (!string.IsNullOrEmpty(queryETime))
            {
                DateTime et;
                if (DateTime.TryParse(queryETime, out  et))
                {
                    exp = exp.And(o => o.BA_Time < et);
                }
            }

            if (!string.IsNullOrEmpty(title))
            {
                exp = exp.And(o => o.BA_NickName.Contains(title));
            }
            LifestyleTypeCore mtCore = new LifestyleTypeCore();
            var typeList = mtCore.LoadEntities().ToList();
            var list = applyCore.LoadEntities(exp).OrderByDescending(o => o.BA_Time).Skip(pn * rows).Take(rows).Select(o => new
            {
                o.BA_Address,
                o.BA_Logo,
                o.BA_BusLicenseImg,
                o.BA_CardImg,
                o.BA_OrganizationCodeImg,
                o.BA_Time,
                o.BA_CheckTips,
                o.BA_Id,
                o.BA_BusId,
                o.BA_NickName,
                o.BA_Phone,
                o.BA_State,
                o.BA_CheckTime,
                o.BA_TrueName,
                o.BA_CardId
            });

            if (list.Any())
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    imgBaseUrl = StaticHttpUrl,
                    typeList,
                    list
                };
            }
            else
            {
                rs.Msg = "没有更多数据";
            }
            return rs;
        }
        /// <summary>
        /// 商家审核成功，查看其申请资料
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel AudingByBusId([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid busId = obj.busId;

            var o = applyCore.LoadEntity(a => a.BA_State == (int)AuditingEnum.认证成功 && a.BA_BusId == busId);

            if (o != null)
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    imgBaseUrl = StaticHttpUrl,
                    o.BA_Address,
                    o.BA_Logo,
                    o.BA_BusLicenseImg,
                    o.BA_CardImg,
                    o.BA_OrganizationCodeImg,
                    o.BA_Time,
                    o.BA_CheckTips,
                    o.BA_Id,
                    o.BA_BusId,
                    o.BA_NickName,
                    o.BA_Phone,
                    o.BA_State,
                    o.BA_CheckTime,
                    o.BA_TrueName,
                    o.BA_CardId
                };
            }
            else
            {
                rs.Msg = "没有更多数据";
            }
            return rs;
        }


        /// <summary>
        /// 重置商家登录密码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel ResetPwd([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid busId = obj.busId;


            var o = busCore.LoadEntity(a => a.B_Id == busId);

            if (o != null)
            {
                string pwd = Md5Extensions.MD5Encrypt(string.Format("a{0}{1}", o.B_LoginPhone, o.B_PwdCode));
                o.B_LoginPwd = pwd;
                if (busCore.UpdateEntity(o))
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new
                    {
                        newpwd = "a" + o.B_LoginPhone
                    };
                }

            }
            else
            {
                rs.Msg = "没有更多数据";
            }
            return rs;
        }

        /// <summary>
        /// 冻结商家
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ResultMessageViewModel Forzen([FromBody] JObject value)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            dynamic obj = value;
            Guid busId = obj.busId;
            //int state = obj.state;
            var info = busCore.LoadEntity(o => o.B_Id == busId);

            if (info != null)
            {
                //冻结商家
                if (info.B_Status == (int)UserStatusEnum.冻结)
                {
                    info.B_Status = (int) UserStatusEnum.正常;
                }
                else
                {
                    info.B_Status = (int)UserStatusEnum.冻结;
                }
                //info.B_Status = state;// (int)UserStatusEnum.冻结;
                if (busCore.UpdateEntity(info)) //更改状态
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
            }

            return rs;
        }
    }
}
