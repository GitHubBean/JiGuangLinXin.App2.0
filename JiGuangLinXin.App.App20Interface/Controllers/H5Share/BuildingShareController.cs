using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using JiGuangLinXin.App.App20Interface.Extension;
using JiGuangLinXin.App.App20Interface.Models;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.JsonHelper;

namespace JiGuangLinXin.App.App20Interface.Controllers.H5Share
{
    public class BuildingShareController : Controller
    {
        //
        // GET: /BuildingShare/

        private BuildingCore bCore = new BuildingCore();
        private AttachmentCore attCore = new AttachmentCore();
        string StaticHttpUrl = ConfigurationManager.AppSettings["ImgSiteUrl"];



        public ActionResult Index(Guid? bId)
        {
            if (!bId.HasValue)
            {
                bId = Guid.Parse("D4624D81-C158-47B0-9049-004E26252E96");
            }

            var tips = new string[] { "", "百万红包发放中！", "VR全景体验！", "璀璨面世！", "VR眼镜发放中！" };


            var info = bCore.LoadEntity(o => o.B_Id == bId && o.B_Status == 0);

            if (info != null)
            {

                ViewBag.Tips = tips[new Random().Next(0, 5)];
                ViewBag.StaticUrl = StaticHttpUrl;

                dynamic content = JsonSerialize.Instance.JsonToObject<dynamic>(info.B_Content);  //基础信息内容

                BuidingCubeCore bcCore = new BuidingCubeCore();

                //全景看房
                var cubeList = bcCore.LoadEntities(o => o.BC_Status == 0 && o.BC_BuildingId == info.B_Id).OrderBy(o => o.BC_Rank).ToList().Select(o => new
                {
                    cubeId = o.BC_Id,
                    title = o.BC_Title,
                    coverImg = StaticHttpUrl + o.BC_CoverImg,
                    imgList = attCore.LoadEntities(a => a.A_PId == o.BC_Id).OrderBy(b => b.A_Rank).ToList().Select(c => new
                    {
                        imgUrl = StaticHttpUrl + c.A_Folder + "/" + c.A_FileName
                    })
                });
                //加载楼盘所有附件图片

                var attList = attCore.LoadEntities(o => o.A_PId == info.B_Id).ToList();


                //景观漫游
                var landscapeImg =
                    attList.Where(o => o.A_Folder == "landscape")
                        .OrderBy(o => o.A_Rank)
                        .ToList()
                        .Select(c => new
                        {
                            imgUrl = StaticHttpUrl + c.A_Folder + "/" + c.A_FileName
                        });

                //区位展示
                var locationImg =
                    attList.Where(o => o.A_Folder == "location")
                        .OrderBy(o => o.A_Rank)
                        .ToList()
                        .Select(c => new
                        {
                            imgUrl = StaticHttpUrl + c.A_Folder + "/" + c.A_FileName
                        });
                //建筑规划
                var planningImg =
                    attList.Where(o => o.A_Folder == "planning")
                        .OrderBy(o => o.A_Rank)
                        .ToList()
                        .Select(c => new
                        {
                            imgUrl = StaticHttpUrl + c.A_Folder + "/" + c.A_FileName
                        });


                //物业配套
                var propertyImg =
                    attList.Where(o => o.A_Folder == "property")
                        .OrderBy(o => o.A_Rank)
                        .ToList()
                        .Select(c => new
                        {
                            imgUrl = StaticHttpUrl + c.A_Folder + "/" + c.A_FileName
                        });


                ViewBag.Data = JsonSerialize.Instance.ObjectToJson(new
                {
                    bid = info.B_Id,
                    title = info.B_Name,
                    coverImg = StaticHttpUrl + info.B_CovereImg,
                    videoImg = StaticHttpUrl + info.B_VideoImg,
                    videoUrl = StaticHttpUrl + info.B_Video,
                    desc = info.B_Desc,

                    adTitle = info.B_AdTitle,
                    adUrl = info.B_AdUrl,
                    adCartoon = string.IsNullOrEmpty(info.B_AdCartoonImg) ? "" : StaticHttpUrl + info.B_AdCartoonImg,
                    adBgImg = string.IsNullOrEmpty(info.B_AdBgImg) ? "" : StaticHttpUrl + info.B_AdBgImg,

                    hongbao = info.B_HongbaoRemain,

                    landscape = content.landscape,
                    landscapeImg,

                    location = content.location,
                    locationImg,

                    property = content.property,
                    propertyImg,

                    planning = content.planning,
                    planningImg,

                    cubeList,

                    linkPhone = info.B_BusPhone,

                });
            }
            else
            {
                return Content("数据不存在");
            }


            return View(info);
        }

    }
}
