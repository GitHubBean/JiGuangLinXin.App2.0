using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.StringHelper;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.App20Interface.Controllers
{
    /// <summary>
    /// 管理社区的头像、相册等
    /// </summary>
    public class AlbumController : BaseController
    {
        private UserCore uCore = new UserCore();
        private VillageCore vCore = new VillageCore();
        private GroupAlbumCore gaCore = new GroupAlbumCore();
        private GroupAlbumPicCore picCore = new GroupAlbumPicCore();



        /// <summary>
        /// 创建社区相册
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Folder([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            Guid buildingId = obj.buildingId;
            string buildingName = obj.buildingName;
            string remark = obj.remark;
            string title = obj.title;
            string nikeName = obj.nikeName;


            Core_GroupAlbum album = new Core_GroupAlbum()
            {
                A_BuildingId = buildingId,
                A_BuildingName = buildingName,
                A_Count = 0,
                A_CoverImg = "",
                A_Desc = remark,
                A_Flag = 1,
                A_Id = Guid.NewGuid(),
                A_Rank = 999,
                A_State = 0,
                A_Time = DateTime.Now,
                A_Title = title,
                A_UId = uid,
                A_UName = nikeName
            };

            var r = gaCore.AddEntity(album);
            if (r != null)
            {
                rs.State = 0;
                rs.Msg = "ok";
                rs.Data = new
                {
                    aid = album.A_Id,
                    title = album.A_Title,
                    time = album.A_Time.ToString("yyyy-MM-dd")
                };
            }
            else
            {
                rs.Msg = "创建相册失败";
            }

            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 上传照片到相册
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage SharePic()
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            Guid buildingId = Guid.Parse(HttpContext.Current.Request.Form["buildingId"]);
            string buildingName = HttpContext.Current.Request.Form["buildingName"];
            string nikeName = HttpContext.Current.Request.Form["nikeName"];
            string cityName = HttpContext.Current.Request.Form["cityName"];
            string areaCode = HttpContext.Current.Request.Form["areaCode"];
            Guid albumId = Guid.Parse(HttpContext.Current.Request.Form["albumId"]);
            string coverImg = HttpContext.Current.Request.Form["coverImg"];

            //上传的所有图片
            var imgs = HttpContext.Current.Request.Files;
            if (imgs.Count > 0) //上传里面有图片
            {
                if (albumId == Guid.Empty)  //默认相册，先新建
                {

                    Core_GroupAlbum album = new Core_GroupAlbum()
                    {
                        A_BuildingId = buildingId,
                        A_BuildingName = buildingName,
                        A_Count = 0,
                        A_CoverImg = "",
                        A_Desc = "系统自动创建",
                        A_Flag = 1,
                        A_Id = Guid.NewGuid(),
                        A_Rank = 999,
                        A_State = 0,
                        A_Time = DateTime.Now,
                        A_Title = "默认相册",
                        A_UId = uid,
                        A_UName = nikeName
                    };

                    gaCore.AddEntity(album);
                    albumId = album.A_Id;
                }

                List<Core_GroupAlbumPic> picList = new List<Core_GroupAlbumPic>();
                for (int j = 0; j < imgs.Count; j++)
                {
                    HttpPostedFile img = imgs[j];

                    Core_GroupAlbumPic pic = new Core_GroupAlbumPic();
                    string fName = Guid.NewGuid().ToString("N") + Path.GetExtension(img.FileName);//DateTime.Now.ToString("yyyyMMdd_HHmmssffff") + rdm.GetRandomString(6) +Path.GetExtension(img.FileName);

                    //上传图片到服务器
                    if (UploadFileToServerPath(img, AttachmentFolderEnum.album.ToString(), fName) == FileUploadStateEnum.上传成功)
                    {
                        pic.P_AlbumId = albumId;
                        pic.P_AreaCode = areaCode;
                        pic.P_BuildingId = buildingId;
                        pic.P_BuildingName = buildingName;
                        pic.P_City = cityName;
                        pic.P_Desc = "";
                        pic.P_FileName = fName;
                        pic.P_FileNameOld = img.FileName;
                        pic.P_Folder = AttachmentFolderEnum.album.ToString();
                        pic.P_Id = Guid.NewGuid();
                        pic.P_Rank = j;
                        pic.P_Size = img.ContentLength;
                        pic.P_State = 0;
                        pic.P_Time = DateTime.Now;
                        pic.P_Title = "";
                        pic.P_UId = uid;
                        pic.P_UName = nikeName;


                        picList.Add(pic);
                    }
                }
                //入库
                if (picCore.AddEntities(picList))
                {
                    var al = gaCore.LoadEntity(p => p.A_Id == albumId);
                    if (string.IsNullOrEmpty(al.A_CoverImg))  //木有封面图片
                    {
                        var ft = picList.FirstOrDefault();
                        al.A_CoverImg = ft.P_Folder + "/" + ft.P_FileName;
                    }
                    al.A_Count += picList.Count();
                    gaCore.UpdateEntity(al);  //默认第一张上传的图片为封面图片

                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = picList.Select(o => new
                    {
                        pid = o.P_Id,
                        albumId = o.P_AlbumId,
                        url = StaticHttpUrl + o.P_FileName
                    });

                }
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 获取社区相册列表
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage AlbumList([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid buildingId = obj.buildingId;
            var aList =
                gaCore.LoadEntities(o => o.A_BuildingId == buildingId && o.A_State == 0)
                    .OrderBy(o => o.A_Time)
                    .Select(o => new
                    {
                        aid = o.A_Id,
                        title = o.A_Title,
                        coverImg = string.IsNullOrEmpty(o.A_CoverImg) ? "" : StaticHttpUrl + o.A_CoverImg
                    });

            if (!aList.Any())  //相册为空
            {
                rs.Data = new List<dynamic>()
                {
                   new { 
                       aid = Guid.Empty,
                    title = "默认相册",
                    coverImg = StaticHttpUrl + "default/pic_1.png"
                   }
                };
            }
            else
            {
                rs.Data = aList;
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 查询图片
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage PicListByAlbumId([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            dynamic obj = value;
            Guid buildingId = obj.buildingId;
            Guid albumId = obj.albumId;
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            var aList =
                picCore.LoadEntities(o => o.P_BuildingId == buildingId && o.P_State == 0 && o.P_AlbumId == albumId)
                    .OrderBy(o => o.P_Time)
                    .Select(o => new
                    {
                        pid = o.P_Id,
                        title = o.P_Title,
                        img = StaticHttpUrl + o.P_Folder + "/" + o.P_FileName,
                        uid = o.P_UId,
                        state = o.P_UId == uid ? 1 : 0
                    });

            rs.Data = aList;
            return WebApiJsonResult.ToJson(rs);
        }


        /// <summary>
        /// 管理员删除相册
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage RmoveAlbumById([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            Guid buildingId = obj.buildingId;
            Guid albumId = obj.albumId;


            var user =
                uCore.LoadEntity(
                    o =>
                        o.U_Id == uid && o.U_Status != (int)UserStatusEnum.冻结 &&
                        o.U_AuditingManager == (int)AuditingEnum.认证成功 && o.U_BuildingId == buildingId);
            if (user != null) //确实是个管理员
            {
                if (gaCore.DeleteByExtended(o => o.A_Id == albumId && o.A_BuildingId == buildingId) > 0)  //删除相册
                {
                    var del = picCore.DeleteByExtended(o => o.P_AlbumId == albumId && o.P_BuildingId == buildingId);  //删除相册的图片
                    if (del > -1)
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                    }
                }
                else
                {
                    rs.Msg = "删除失败";
                }
            }
            else
            {
                rs.Msg = "只有管理员才能删除群相册";
            }
            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 根据图片的ID删除（只有管理员、或者自己才可以删除图片）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage RmovePicById([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());
            dynamic obj = value;
            Guid picId = obj.picId;

            Guid buildingId = obj.buildingId;

            var user =
                uCore.LoadEntity(
                    o =>
                        o.U_Id == uid && o.U_Status != (int)UserStatusEnum.冻结 &&
                        o.U_AuditingManager == (int)AuditingEnum.认证成功 && o.U_BuildingId == buildingId);

            bool delRs = false;
            if (user != null) //是个管理员
            {
                delRs = picCore.DeleteEntityByWhere(o => o.P_Id == picId && o.P_BuildingId == buildingId);
            }
            else  //不是管理员就只能自己删除自己的图片了
            {

                delRs = picCore.DeleteEntityByWhere(o => o.P_Id == picId && o.P_UId == uid && o.P_BuildingId == buildingId);
            }
            if (delRs)
            {
                rs.State = 0;
                rs.Msg = "ok";

            }
            else
            {
                rs.Msg = "您没有权限删除此图片";
            }
            return WebApiJsonResult.ToJson(rs);
        }

        #region 群主管理小区的图片
        /// <summary>
        /// 社区管家更换社区头像
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Logo()
        {
            ResultViewModel rs = new ResultViewModel();
            Guid uid = Guid.Parse(Request.Headers.GetValues("uid").FirstOrDefault());

            var user =
                uCore.LoadEntity(
                    o =>
                        o.U_Id == uid && o.U_Status != (int)UserStatusEnum.冻结 &&
                        o.U_AuditingManager == (int)AuditingEnum.认证成功);
            if (user != null)  //确实是个管理员
            {
                HttpPostedFile img = HttpContext.Current.Request.Files["coverImg"];

                if (img != null)  //头像
                {

                    //CreateRandomStr rdm = new CreateRandomStr();
                    string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(img.FileName);//  DateTime.Now.ToString("yyyyMMdd_HHmmssffff") + rdm.GetRandomString(6) +Path.GetExtension(img.FileName);
                    //s上传到服务
                    var up = UploadFileToServerPath(img, AttachmentFolderEnum.community.ToString(), fileName);
                    if (up == FileUploadStateEnum.上传成功)
                    {
                        var building = vCore.LoadEntity(o => o.V_Id == user.U_BuildingId);
                        building.V_Img = AttachmentFolderEnum.community + "/" + fileName;
                        //修改社区头像
                        if (vCore.UpdateEntity(building))
                        {
                            rs.State = 0;
                            rs.Msg = "ok";
                            rs.Data = new { buildingImg = ConfigurationManager.AppSettings["ImgSiteUrl"] + building.V_Img };
                        }
                    }
                    else
                    {
                        rs.Msg = up.ToString();
                    }
                }
            }
            else
            {
                rs.Msg = "非管理员无权修改社区头像";
            }

            return WebApiJsonResult.ToJson(rs);
        }

        /// <summary>
        /// 群主更换小区的背景图片
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponseMessage UpdateBgImg([FromBody] JObject value)
        {
            ResultViewModel rs = new ResultViewModel();
            dynamic obj = value;

            string bgimg = obj.bgimg;  //选中的背景图片

            Guid uId = Guid.Parse(GetValueByHeader("uid"));
            Guid bId = obj.buildingId;


            var user = uCore.LoadEntity(o => o.U_Id == uId && o.U_BuildingId == bId && o.U_Status == (int)UserStatusEnum.正常 && o.U_AuditingManager == (int)AuditingEnum.认证成功);

            if (user == null)
            {
                rs.State = 1;
                rs.Msg = "您没有权限修改此图片";
                return WebApiJsonResult.ToJson(rs);
            }

            var building = vCore.LoadEntity(o => o.V_Id == bId && o.V_State == 0);
            if (building != null)
            {
                if (!bgimg.Equals(building.V_Remark))
                {
                    building.V_Remark = bgimg;
                    if (vCore.UpdateEntity(building))
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = new { bgImg = StaticHttpUrl + "default/" + bgimg };
                    }
                }
                else
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                    rs.Data = new { bgImg = StaticHttpUrl + "default/" + bgimg };
                }
            }
            else
            {
                rs.State = 1;
                rs.Msg = "小区不存在";
            }
            return WebApiJsonResult.ToJson(rs);
        }



        /// <summary>
        /// 社区中心，背景图片列表
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage BuildingBgImgList()
        {
            ResultViewModel rs = new ResultViewModel(0, "ok", null);
            DirectoryInfo dir = new DirectoryInfo(StaticFilePath + "default/buildingBgImg/");

            var files = dir.GetFiles().Select(o => new { imgurl = StaticHttpUrl + "default/buildingBgImg/" + o.Name, filename = o.Name });
            rs.Data = files;

            return WebApiJsonResult.ToJson(rs);
        }
        #endregion

    }
}
