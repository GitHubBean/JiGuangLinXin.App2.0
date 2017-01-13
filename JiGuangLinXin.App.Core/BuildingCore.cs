using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Extensions;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using Microsoft.Win32.SafeHandles;

namespace JiGuangLinXin.App.Core
{
    public class BuildingCore : BaseRepository<Core_Building>
    {

        /// <summary>
        /// ����¥��
        /// </summary>
        /// <param name="info"></param>
        /// <param name="images"></param>
        /// <param name="cube"></param>
        /// <returns></returns>
        public bool Add(Core_Building info, IEnumerable<dynamic> images, IEnumerable<dynamic> cube)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                //1���ȫ����¼
                foreach (var o in cube)
                {
                    Core_BuidingCube cb = new Core_BuidingCube();
                    cb.BC_BuildingId = info.B_Id;
                    cb.BC_CoverImg = o.coverImg;
                    cb.BC_Rank = o.rank;
                    cb.BC_Status = 0;
                    cb.BC_Title = o.title;
                    cb.BC_Time = info.B_Time;
                    cb.BC_Id = Guid.NewGuid();
                    db.Core_BuidingCube.Add(cb);  //��ӻ���

                    //����ͼƬ
                    IEnumerable<dynamic> temp = o.images;
                    if (temp.Count() == 6)
                    {
                        foreach (var img in o.images)
                        {
                            Sys_Attachment att = new Sys_Attachment();
                            att.A_Id = Guid.NewGuid();
                            att.A_PId = cb.BC_Id;
                            att.A_FileName = img.A_FileName;
                            att.A_FileNameOld = img.A_FileNameOld;
                            att.A_Folder = img.A_Folder;
                            att.A_Size = img.A_Size;
                            att.A_Rank = img.A_Rank;
                            att.A_Time = cb.BC_Time;
                            db.Sys_Attachment.Add(att);  //��ӻ��͵�ͼƬ
                        }
                    }
                }


                //2���¥��ͼƬ
                foreach (var img in images)
                {
                    Sys_Attachment att = new Sys_Attachment();
                    att.A_Id = Guid.NewGuid();
                    att.A_PId = info.B_Id;
                    att.A_FileName = img.A_FileName;
                    att.A_FileNameOld = img.A_FileNameOld;
                    att.A_Folder = img.A_Folder;
                    att.A_Size = img.A_Size;
                    att.A_Rank = img.A_Rank;
                    att.A_Time = info.B_Time;

                    db.Sys_Attachment.Add(att);  //���¥�̵�ͼƬ
                }

                //3���¥����Ϣ
                db.Core_Building.Add(info);

                //4���ڷ���¥�̺������Ҫ���ƽ̨�˵�
                if (info.B_HongbaoCount > 0 && info.B_HongbaoMoney > 0)
                {

                    //5ƽ̨��ˮ
                    Sys_BillMaster billMaster = new Sys_BillMaster()
                    {
                        B_Flag = (int)BillFlagEnum.ƽ̨��ˮ,
                        B_Module = (int)BillEnum.¥�̺��,
                        B_Money = info.B_HongbaoCount * info.B_HongbaoCount,
                        B_OrderId = info.B_Id,
                        B_Phone = info.B_BusPhone,
                        B_Remark = "",
                        B_Status = 0,
                        B_Time = info.B_Time,
                        B_Title = info.B_HongbaoTips,
                        B_UId = info.B_AdminId,
                        B_Type = (int)MemberRoleEnum.�̼�
                    };
                    db.Sys_BillMaster.Add(billMaster);
                }



                return db.SaveChanges() > 0;//�����ύ
            }
            return false;
        }

        /// <summary>
        /// �û���������
        /// </summary>
        /// <param name="bId">¥��ID</param>
        /// <param name="uId">�û�ID</param>
        /// <returns></returns>
        public ResultMessageViewModel Apply(Guid bId, Guid uId)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                //�û��Ƿ����
                var user = db.Core_User.FirstOrDefault(o => o.U_Id == uId && o.U_Status != (int)UserStatusEnum.����);
                if (user == null)
                {
                    rs.Msg = "�û�������";
                    return rs;
                }
                //¥���Ƿ����
                var building = db.Core_Building.FirstOrDefault(o => o.B_Id == bId && o.B_Status == 0);
                if (building == null)
                {
                    rs.Msg = "¥�̻������";
                    return rs;
                }
                if (building.B_BTime < DateTime.Now && building.B_ETime > DateTime.Now)
                {
                    rs.Msg = "¥�̻�����Ѿ�����";
                    return rs;
                }
                var his = db.Core_ActivityApply.Where(o => o.AA_UId == user.U_Id && o.AA_ActivityId == building.B_Id);
                if (his.Any())
                {
                    rs.Msg = "���Ѿ��μӣ������ظ�����";
                    return rs;
                }


                Core_ActivityApply apply = new Core_ActivityApply()
                {
                    AA_ActivityId = building.B_Id,
                    AA_Status = 0,
                    AA_Time = DateTime.Now,
                    AA_UId = user.U_Id,
                    AA_UPhone = user.U_LoginPhone,
                    AA_Remark = user.U_TrueName + "#" + user.U_BuildingName
                };
                db.Core_ActivityApply.Add(apply);
                if (db.SaveChanges() > 0)
                {
                    rs.Msg = "ok";
                    rs.State = 0;
                }

            }
            return rs;
        }


        /// <summary>
        /// ¥�̻���л������Ŷ
        /// </summary>
        /// <param name="bId"></param>
        /// <param name="uId"></param>
        /// <returns></returns>
        public ResultMessageViewModel Activity(Guid bId, Guid uId)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {

                //¥���Ƿ����
                var building = db.Core_Building.FirstOrDefault(o => o.B_Id == bId && o.B_Status == 0);
                if (building == null)
                {
                    rs.Msg = "¥�̻������";
                    return rs;
                }
                //�û��Ƿ����
                var user = db.Core_User.FirstOrDefault(o => o.U_Id == uId && o.U_Status != (int)UserStatusEnum.����);
                if (user == null)
                {

                    rs.Msg = "ok";
                    rs.State = 0;


                    rs.Data = new
                    {
                        bId = building.B_Id,
                        title = building.B_ActivityTitle,
                        stime = building.B_BTime,
                        etime = building.B_ETime,
                        coverImg = building.B_ActivityImg,
                        address = building.B_Address,
                        content = building.B_ActivityContent,
                        buildingName = building.B_Name,
                        logo = building.B_Logo,

                        hbFlag = 0,//0δ��ȡ��� 1������ȡ��� 2֮ǰ��ȡ�����
                        hbMoney = 0,//������,

                        uid = "",
                        uphone = "",
                        unickname = ""
                    };

                    return rs;
                }
                //if (building.B_BTime < DateTime.Now && building.B_ETime > DateTime.Now)
                //{
                //    rs.Msg = "¥�̻�����Ѿ�����";
                //    return rs;
                //}
                //var his = db.Core_ActivityApply.Where(o => o.AA_UId == user.U_Id && o.AA_ActivityId == building.B_Id);
                //if (his.Any())
                //{
                //    rs.Msg = "���Ѿ��μӣ������ظ�����";
                //    return rs;
                //}

                DateTime dt = DateTime.Now;
                decimal hbMoney = 0;
                int hbFlag = 0;

                //�鿴�Ƿ��Ѿ���ȡ
                var isReceive =
                    db.Core_LuckyGiftHistory.FirstOrDefault(o => o.LH_UserId == user.U_Id && o.LH_GiftId == building.B_Id);
                if (isReceive != null)  //����ȡ
                {
                    hbMoney = isReceive.LH_Money;
                    hbFlag = 2;
                }
                else if (building.B_HongbaoRemain > 0) //û����ȡ�����к��δ����ȡ
                {

                    building.B_HongbaoRemain -= 1;  //��������ݼ�

                    //����û�������¼
                    Core_LuckyGiftHistory history = new Core_LuckyGiftHistory()
                    {
                        LH_CreateTime = dt,
                        LH_GiftDetailId = null,
                        LH_GiftId = building.B_Id,
                        LH_Id = Guid.NewGuid(),
                        LH_Money = building.B_HongbaoMoney,
                        LH_Remark = string.Format("��ȡ¥�̡�{0}-{1}��-��{2}�� Ԫ����", building.B_Name, building.B_ActivityTitle, building.B_HongbaoMoney.ToString("N")),
                        LH_Status = 0,
                        LH_UserId = user.U_Id,
                        LH_UserNickName = user.U_NickName,
                        LH_UserPhone = user.U_LoginPhone,
                        LH_Flag = (int)LuckGiftTypeEnum.�¼��Ƽ����
                    };
                    db.Core_LuckyGiftHistory.Add(history);

                    //����˵���¼
                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                        B_Module = (int)BillEnum.¥�̺��,
                        B_Money = building.B_HongbaoMoney,
                        B_OrderId = history.LH_Id,
                        B_Phone = user.U_LoginPhone,
                        B_Remark = history.LH_Remark,
                        B_Status = 0,
                        B_Time = dt,
                        B_Title = building.B_HongbaoTips,
                        B_UId = user.U_Id,
                        B_Type = (int)MemberRoleEnum.��Ա
                    };
                    db.Core_BillMember.Add(bill);

                    //�ۼ��û����
                    var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == user.U_Id);
                    balance.B_Balance += building.B_HongbaoMoney;


                    hbMoney = building.B_HongbaoMoney;
                    hbFlag = 1;
                }
                building.B_Clicks += 1;  //�ۼ������
                if (db.SaveChanges() > 0)
                {

                    rs.Msg = "ok";
                    rs.State = hbFlag;


                    rs.Data = new
                    {
                        bId = building.B_Id,
                        title = building.B_ActivityTitle,
                        stime = building.B_BTime,
                        etime = building.B_ETime,
                        coverImg = building.B_ActivityImg,
                        address = building.B_Address,
                        content = building.B_ActivityContent,
                        buildingName = building.B_Name,
                        logo = building.B_Logo,

                        hbFlag,//0δ��ȡ��� 1������ȡ��� 2֮ǰ��ȡ�����
                        hbMoney,//������,

                        uid = user.U_Id,
                        uphone = user.U_LoginPhone,
                        unickname = user.U_NickName
                    };
                }

            }
            return rs;
        }

    }
}
