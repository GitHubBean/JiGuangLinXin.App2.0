using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using JiGuangLinXin.App.AdminCenter.Extension.EnumHelper;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Provide.JsonHelper;
using Webdiyer.WebControls.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Controllers
{
    /// <summary>
    /// 小区楼盘
    /// </summary>
    public class VillageBuildingController : BaseController
    {
        private BuildingCore buildCore = new BuildingCore();
        private AttachmentCore attaCore = new AttachmentCore();
        private BuidingCubeCore cubeCore = new BuidingCubeCore();

        #region 商家楼盘
        /// <summary>
        /// 所有楼盘
        /// </summary>
        /// <returns></returns>
        public ActionResult Building(int id = 1, int size = 10, string key = "")
        {
            Expression<Func<Core_Building, Boolean>> expr = t => true;
            if (!string.IsNullOrWhiteSpace(key))
            {
                expr = t => t.B_CityName.Contains(key.Trim());
            }

            var list = buildCore.LoadEntities(expr).OrderByDescending(o => o.B_Top).ThenBy(o => o.B_Clicks).ToPagedList(id, size);

            return View(list);
        }
        /// <summary>
        /// 新增楼盘
        /// </summary>
        /// <returns></returns>
        public ActionResult BuildingEdit(Guid? id)
        {
            Core_Building build = null;
            IEnumerable<SelectListItem> BuildingFlagList = null;
            IEnumerable<SelectListItem> BuildingTypeList = null;
            if (id.HasValue)
            {
                build = buildCore.LoadEntity(o => o.B_Id == id);
                if (build != null)
                {
                    BuildingFlagList = EnumHelper.GetEnumKeysSelectListItems<BuildingFlagEnum>(selectedValue: build.B_Flag.ToString());
                    //BuildingTypeList = EnumHelper.GetEnumKeysSelectListItems<BuildingTypeEnum>(selectedValue: build.B_TypeId.ToString());
                }
            }
            if (build == null)
            {
                BuildingFlagList = EnumHelper.GetEnumKeysSelectListItems<BuildingFlagEnum>();
                BuildingTypeList = EnumHelper.GetEnumKeysSelectListItems<BuildingTypeEnum>();
                build = new Core_Building() { B_BTime = DateTime.Now, B_ETime = DateTime.Now };
            }

            TempData["BuildingFlag"] = BuildingFlagList;
            TempData["BuildingType"] = BuildingTypeList;
            return View(build);
        }
        /// <summary>
        /// 保存楼盘编辑信息
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult BuildingEdit(Core_Building obj)
        {
            obj.B_Time = DateTime.Now;
            if (obj.B_Id == Guid.Empty) //新增
            {
                obj.B_Id = Guid.NewGuid();
                buildCore.AddEntity(obj);
            }
            else //修改
            {
                buildCore.UpdateEntity(obj);
            }

            return RedirectToAction("Landscape",new {id=obj.B_Id});  //下一步，上传景观漫游图片
        }
        /// <summary>
        /// 变更楼盘冻结状态
        /// </summary>
        /// <param name="id">楼盘id</param>
        /// <param name="status">状态ID</param>
        /// <returns></returns>
        [HttpPost]
        public string BuildingForzen(Guid id, int status)
        {
            string rs = "ok";
            var obj = buildCore.LoadEntity(o => o.B_Id == id);
            obj.B_Status = status;
            if (!buildCore.UpdateEntity(obj))  //更新活动状态
            {
                rs = "error";
            }
            return rs;
        }


        #endregion

        #region 楼盘漫游景观
        /// <summary>
        /// 漫游景观
        /// </summary>
        /// <returns></returns>
        public ActionResult Landscape(Guid? id)
        {
            if (id.HasValue)
            {
                var obj = buildCore.LoadEntity(o => o.B_Id == id);
                if (obj != null)
                {
                    ViewBag.BuildId = id;
                    ViewBag.BuildingName = obj.B_Name;
                    var imgList = attaCore.LoadEntities(o => o.A_PId == id && o.A_Folder == AttachmentFolderEnum.landscape.ToString()).OrderBy(o => o.A_Rank).ToList();
                    return View(imgList);
                }
            }
            return RedirectToAction("Building");
        }
        /// <summary>
        /// 上传景观漫游图片
        /// </summary>
        /// <param name="imgJson"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LandscapeSave(string imgJson, Guid? bId)
        {
            SaveAttachment(imgJson, AttachmentFolderEnum.landscape.ToString());

            return RedirectToAction("HouseType", new { id = bId });  //下一步，户型管理
        }


        /// <summary>
        /// 批量上传景观漫游图片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LandscapeUpload()
        {
            string avatarPaht = Server.MapPath("~/cache/" + AttachmentFolderEnum.landscape);
            if (!Directory.Exists(avatarPaht))
            {
                Directory.CreateDirectory(avatarPaht);
            }
            if (Request.Files.Count < 1)
            {
                return Json(new List<object> { new { status = "ERROR", message = "没有选择要上传的文件" } }, JsonRequestBehavior.AllowGet);
            }

            // HttpPostedFileBase files = Request.Files;  //上传的文件
            int maxattachsize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ImgSize"]);// 最大上传大小，默认是5M
            string upext = "jpg,jpeg,gif,png,bmp";    // 上传扩展名
            var msg = new List<object>();

            for (int i = 1; i <= Request.Files.Count; i++)
            {
                var file = Request.Files[i - 1];
                // 取上载文件后缀名
                string extension = GetFileExt(file.FileName);
                if (new Byte[file.ContentLength].Length > maxattachsize)
                {
                    return Json(new List<object> { new { status = "ERROR", message = "上传的文件过大，已超出限制！" } }, JsonRequestBehavior.AllowGet);
                }
                if (("," + upext + ",").IndexOf("," + extension + ",") < 0)
                {
                    return Json(new List<object> { new { status = "ERROR", message = "上传的文件非法！" } }, JsonRequestBehavior.AllowGet);
                }

                // 生成随机文件名
                //Random random = new Random(DateTime.Now.Millisecond);
                string filename = Guid.NewGuid().ToString("N") + "." + extension;// string.Format("cover{0}.{1}", DateTime.Now.ToString("yyyyMMddhhmmss") + random.Next(1000), extension);
                file.SaveAs(avatarPaht + "/" + filename);

                msg.Add(new { status = "ok", url = "/cache/" + AttachmentFolderEnum.landscape + "/" + filename, oldname = file.FileName, size = file.ContentLength, title = "漫游景观" + i, fname = filename });
            }

            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 楼盘户型
        /// <summary>
        /// 楼盘户型
        /// </summary>
        /// <param name="id">楼盘ID</param>
        /// <returns></returns>
        public ActionResult HouseType(Guid? id)
        {
            //return View(new List<Core_BuidingCube>());
            if (id.HasValue)
            {
                var obj = buildCore.LoadEntity(o => o.B_Id == id);
                if (obj != null)
                {
                    //string folder = Enum.GetName(typeof(AttachmentFolderEnum),AttachmentFolderEnum.landscape);
                    //var list = attaCore.LoadEntities(o => o.A_PId == obj.B_Id && o.A_Folder == folder).OrderBy(o=>o.A_Rank); //获取所有户型图片附件
                    //ViewBag.BuildingId = obj.B_Id;

                    //查询楼盘所有户型
                    var houseTypeList = cubeCore.LoadEntities(o => o.BC_BuildingId == obj.B_Id).OrderBy(o => o.BC_Rank).ToList();
                    ViewBag.HouseName = obj.B_Name;
                    ViewBag.HouseId = obj.B_Id;
                    return View(houseTypeList);
                }

            }
            return RedirectToAction("Building");
        }

        /// <summary>
        /// 楼盘户型编辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult HouseTypeEdit(Core_BuidingCube obj)
        {
            obj.BC_Time = DateTime.Now;
            if (obj.BC_Id != Guid.Empty)
            {
                cubeCore.UpdateEntity(obj);
            }
            else
            {
                obj.BC_Id = Guid.NewGuid();
                cubeCore.AddEntity(obj);
            }

            return RedirectToAction("HouseTypeCube", "VillageBuilding",new {id=obj.BC_Id,n=obj.BC_Title});
        }
        /// <summary>
        /// 户型编辑
        /// </summary>
        /// <param name="buildingId">楼盘iD</param>
        /// <param name="id">户型ID</param>
        /// <returns></returns>
        public ActionResult HouseTypeEdit(Guid? buildingId, Guid? id)
        {
            if (id.HasValue)  //户型ID不为空，标示修改户型
            {
                var obj = cubeCore.LoadEntity(o => o.BC_Id == id);
                if (obj != null)
                {
                    //  var rs = cubeCore.LoadEntities(o=>o.BC_BuildingId == obj.B_Id).OrderByDescending(o=>o.BC_Rank);
                    return View(obj);
                }
            }
            else
            {
                if (buildingId.HasValue)  //户型ID为空，楼盘ID不为空，标示新增户型
                {
                    return View(new Core_BuidingCube() { BC_BuildingId = (Guid)buildingId });
                }
            }
            throw new Exception("编辑户型，参数非法");
        }
        /// <summary>
        /// 户型全景图
        /// </summary>
        /// <param name="id">楼盘户型ID</param>
        /// <returns></returns>
        public ActionResult HouseTypeCube(Guid? id)
        {
            if (id.HasValue)
            {
                var rs = attaCore.LoadEntities(o => o.A_PId == (Guid)id).OrderBy(o => o.A_Rank).ToList();
                ViewBag.CubeId = id;
                return View(rs);
            }

            throw new Exception("编辑户型全景图片，参数非法");
        }

        /// <summary>
        /// 批量上传景户型全景图片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult HouseTypeCubeUpload()
        {
            string avatarPaht = Server.MapPath("~/cache/" + AttachmentFolderEnum.cube);
            if (!Directory.Exists(avatarPaht))
            {
                Directory.CreateDirectory(avatarPaht);
            }
            if (Request.Files.Count < 1)
            {
                return Json(new List<object> { new { status = "ERROR", message = "没有选择要上传的文件" } }, JsonRequestBehavior.AllowGet);
            }

            // HttpPostedFileBase files = Request.Files;  //上传的文件
            int maxattachsize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ImgSize"]);// 最大上传大小，默认是5M
            string upext = "jpg,jpeg,gif,png,bmp";    // 上传扩展名
            var msg = new List<object>();

            for (int i = 1; i <= Request.Files.Count; i++)
            {
                var file = Request.Files[i - 1];
                // 取上载文件后缀名
                string extension = GetFileExt(file.FileName);
                if (new Byte[file.ContentLength].Length > maxattachsize)
                {
                    return Json(new List<object> { new { status = "ERROR", message = "上传的文件过大，已超出限制！" } }, JsonRequestBehavior.AllowGet);
                }
                if (("," + upext + ",").IndexOf("," + extension + ",") < 0)
                {
                    return Json(new List<object> { new { status = "ERROR", message = "上传的文件非法！" } }, JsonRequestBehavior.AllowGet);
                }

                // 生成随机文件名
                //Random random = new Random(DateTime.Now.Millisecond);
                string filename = Guid.NewGuid().ToString("N") + "." + extension;// string.Format("cover{0}.{1}", DateTime.Now.ToString("yyyyMMddhhmmss") + random.Next(1000), extension);
                file.SaveAs(avatarPaht + "/" + filename);

                msg.Add(new { status = "ok", url = "/cache/" + AttachmentFolderEnum.cube + "/" + filename, oldname = file.FileName, size = file.ContentLength, title = "图" + i, fname = filename });
            }

            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 上传景观漫游图片
        /// </summary>
        /// <param name="imgJson"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult HouseTypeCubeSave(string imgJson)
        {
            SaveAttachment(imgJson, AttachmentFolderEnum.cube.ToString());

            return RedirectToAction("Building"); 
        }

        /// <summary>
        /// 更新楼盘户型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public string HouseTypeDelete(Guid id, int status)
        {
            string rs = "ok";
            var obj = cubeCore.LoadEntity(o => o.BC_Id == id);
            obj.BC_Status = status;
            if (!cubeCore.UpdateEntity(obj))  //更新楼盘户型状态
            {
                rs = "error";
            }
            return rs;
        }
        #endregion

        /// <summary>
        /// 所有小区
        /// </summary>
        /// <returns></returns>
        public ActionResult Village()
        {
            return View();
        }
        /// <summary>
        /// 所有小区
        /// </summary>
        /// <returns></returns>
        public ActionResult VillageEdit()
        {
            return View();
        }

        #region 楼盘封面
        /// <summary>
        /// 楼盘封面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BuidingCover()
        {
            string avatarPaht = Server.MapPath("~/cache/building/");
            if (!Directory.Exists(avatarPaht))
            {
                Directory.CreateDirectory(avatarPaht);
            }
            if (Request.Files.Count < 1)
            {
                return Json(new List<object> { new { status = "ERROR", message = "没有选择要上传的文件" } }, JsonRequestBehavior.AllowGet);
            }

            var file = Request.Files[0];  //上传的文件
            int maxattachsize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ImgSize"]);// 最大上传大小，默认是5M
            string upext = "jpg,jpeg,gif,png,bmp";    // 上传扩展名
            // 取上载文件后缀名
            string extension = GetFileExt(file.FileName);
            if (new Byte[file.ContentLength].Length > maxattachsize)
            {
                return Json(new List<object> { new { status = "ERROR", message = "上传的文件过大，已超出限制！" } }, JsonRequestBehavior.AllowGet);
            }
            if (("," + upext + ",").IndexOf("," + extension + ",") < 0)
            {
                return Json(new List<object> { new { status = "ERROR", message = "上传的文件非法！" } }, JsonRequestBehavior.AllowGet);
            }

            // 生成随机文件名
            Random random = new Random(DateTime.Now.Millisecond);
            string filename = string.Format("cover{0}.{1}", DateTime.Now.ToString("yyyyMMddhhmmss") + random.Next(1000), extension);
            if (!System.IO.Directory.Exists(avatarPaht)) System.IO.Directory.CreateDirectory(avatarPaht);  //文件是否存在
            file.SaveAs(avatarPaht + filename);

            return Json(new List<object> { new { status = "OK", message = "牛逼，成功了", url = "/abc/s.jpg" } }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        string GetFileExt(string FullPath)
        {
            if (FullPath != "") return FullPath.Substring(FullPath.LastIndexOf('.') + 1).ToLower();
            else return "";
        }

        /// <summary>
        /// 保存附件图片
        /// </summary>
        /// <param name="imgJson"></param>
        /// <param name="folder"></param>
        private void SaveAttachment(string imgJson ,string folder)
        {
            if (!string.IsNullOrEmpty(imgJson))
            {
                var objList = JsonSerialize.Instance.JsonToObjectArray(imgJson);
                List<Sys_Attachment> attList = new List<Sys_Attachment>();
                Guid pid = Guid.Empty;
                foreach (var item in objList)
                {
                    pid = Guid.Parse(item["pid"].ToString());
                    Sys_Attachment obj = new Sys_Attachment()
                    {
                        A_FileName = item["fname"].ToString(),
                        A_FileNameOld = item["oldname"].ToString(),
                        A_Folder = folder,
                        A_Id = Guid.NewGuid(),
                        A_PId = pid,
                        A_Rank = int.Parse(item["rank"].ToString()),
                        A_Remark = item["title"].ToString(),
                        A_Size = int.Parse(item["size"].ToString()),
                        A_Time = DateTime.Now,
                        A_Type = (int)AttachmentTypeEnum.图片
                    };
                    attList.Add(obj);
                }
                attaCore.DeleteByExtended(o => o.A_PId == pid);  //批量删除之前的附件图片
                if (!attaCore.AddEntities(attList)) //附件添加失败
                {
                    throw new Exception("楼盘图片上传失败");
                }

            }
        }
    }
}
