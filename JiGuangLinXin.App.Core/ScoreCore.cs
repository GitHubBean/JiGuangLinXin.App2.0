using System;
using System.Collections.Generic;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Provide.JsonHelper;

namespace JiGuangLinXin.App.Core
{
    public class ScoreCore : BaseRepository<Core_Score>
    {
        /// <summary>
        /// ����������ֵ
        /// </summary>
        /// <param name="uid">�û�ID</param>
        /// <param name="score">���Ļ���</param>
        /// <param name="title">��ֵ����</param>
        /// <param name="times">�ҽ�ʱ��Σ���ʽ��2016-07-28 11:00,2016-07-28 11:01|2016-07-28 09��00,2016-07-28 09��01</param>
        /// <param name="limitCount">ÿ�նһ�����</param>
        /// <returns></returns>
        public ResultMessageViewModel CellphoneTraffic(Guid uid, int score, string title, string times, int limitCount)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            rs.State = 1;

            bool flag = false;//�Ƿ���Զҽ�
            //�ȿ����Ƿ����ڿ������ʱ����ڶҽ�
            DateTime now = DateTime.Now;
            var timeArr = times.Split('|'); //���ʱ��ο��Զҽ�
            foreach (var ti in timeArr)
            {
                var timestra = ti.Split(',');
                if (now > Convert.ToDateTime(timestra[0]) && now < Convert.ToDateTime(timestra[1]))
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                rs.Msg = "��δ���ҽ�ʱ��";
                return rs;
            }


            int orderId = -1;
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                //�ж��û��Ƿ���ڣ��Լ������Ƿ��㹻
                var user = db.Core_User.FirstOrDefault(o => o.U_Id == uid && o.U_Status != (int)UserStatusEnum.���� && o.U_AuditingState == (int)AuditingEnum.��֤�ɹ�);
                if (user == null)
                {
                    rs.Msg = "�û�������";
                    return rs;
                }
                DateTime dt = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                if (db.Core_ScoreExchange.Count(o => o.E_Module == (int)EventH5ModuleEnum.ǩ�������� && o.E_Time > dt) >= limitCount)
                {
                    rs.Msg = "�����ѱ�����";
                    return rs;
                }


                var scoreInfo = db.Core_Score.FirstOrDefault(o => o.S_AccountId == uid);
                if (scoreInfo != null && scoreInfo.S_Score >= score)  //�����㹻
                {
                    //1�۳�����
                    scoreInfo.S_Score -= score;

                    //2 ��Ӷҽ���¼
                    var ex = new Core_ScoreExchange()
                    {
                        E_BuildingId = user.U_BuildingId,
                        E_BuildingName = user.U_BuildingName,
                        E_Id = 0,
                        E_Module = (int)EventH5ModuleEnum.ǩ��������, //Ĭ�ϣ������������������չ��д��ö�ټ���
                        E_OrderNo = "",//������ֵ�ɹ�����Ҫ�޸Ĵ˶���������,
                        E_Phone = user.U_LoginPhone,
                        E_Role = (int)MemberRoleEnum.��Ա,
                        E_Score = score,
                        E_Status = -1,  //-1��ʶδ���ʣ�0�ɹ� ��1ʧ��
                        E_Time = DateTime.Now,
                        E_Flag = (int)FilmFlagEnum.����,
                        E_Title = title,
                        E_UId = uid
                    };

                    db.Core_ScoreExchange.Add(ex);

                    //3 ����˵���¼
                    //Core_BillMember bill = new Core_BillMember()
                    //{
                    //    B_Flag = (int)BillFlagModuleEnum.�ٷ�ƽ̨,
                    //    B_Money = -score,
                    //    B_OrderId = order.O_Id,
                    //    B_Phone = order.O_Phone,
                    //    B_Remark = order.O_Remark,
                    //    B_Status = 0,
                    //    B_Time = order.O_Time,
                    //    B_Title = Enum.GetName(typeof(PaymentTypeEnum), order.O_Type) + ":" + order.O_OrderNo,
                    //    B_UId = order.O_UId,
                    //    B_Type = (int)MemberRoleEnum.��Ա
                    //};

                    if (db.SaveChanges() > 0)
                    {
                        rs.State = 0;
                        rs.Msg = "ok";
                        rs.Data = JsonSerialize.Instance.ObjectToJson(new
                        {
                            phone = user.U_LoginPhone, //�û��绰
                            billId = ex.E_Id //�ҽ���¼��ID
                        });
                    }

                }
                else
                {
                    rs.Msg = "ǩ���㲻��";
                }

            }
            return rs;
        }
    }

    public class ScoreExchangeCore : BaseRepository<Core_ScoreExchange>
    {
        /// <summary>
        /// �ϻ����
        /// </summary>
        /// <param name="uid">ҵ��ID</param>
        /// <returns></returns>
        public ResultMessageViewModel PlaySlot(Guid uid)
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            rs.State = 1;
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                var user = db.Core_User.FirstOrDefault(o => o.U_Id == uid && o.U_Status != (int)UserStatusEnum.����);
                if (user == null)
                {
                    rs.Msg = "�û�������";
                    return rs;
                }
                else if (user.U_AuditingState != (int)AuditingEnum.��֤�ɹ�)
                {
                    rs.Msg = "����ͨ��ʵ����֤";
                    return rs;
                }
                if (db.Core_ScoreExchange.Any(o => o.E_Module == (int)EventH5ModuleEnum.�ϻ����͵�ӰƱ && o.E_UId == uid))
                {
                    rs.Msg = "���Ѳμӹ����λ";
                    return rs;
                }

                var info = new Core_ScoreExchange()
                {
                    E_BuildingId = user.U_BuildingId,
                    E_BuildingName = user.U_BuildingName,
                    E_Id = 0,
                    E_Module = (int)EventH5ModuleEnum.�ϻ����͵�ӰƱ,
                    E_OrderNo = "", //������ֵ�ɹ�����Ҫ�޸Ĵ˶���������,
                    E_Phone = user.U_LoginPhone,
                    E_Role = (int)MemberRoleEnum.��Ա,
                    E_Score = 0,
                    E_Status = 0,
                    E_Time = DateTime.Now,
                    E_UId = uid
                };

                #region �����н�����

                //�ȼ���ó����еĸ�������ĺ�

                //��ӰƱ
                var events =
                    db.Core_ScoreExchange.Where(o => o.E_Module == (int)EventH5ModuleEnum.�ϻ����͵�ӰƱ);
                //.Count(o => o.E_Module == (int) EventH5ModuleEnum.�ϻ����͵�ӰƱ && o.E_Flag == 1);
                //ÿ��С��������
                var tickets = events.Count(o => o.E_BuildingId == user.U_BuildingId && o.E_Flag == 1); //��ӰƱ����
                var cards = events.Count(o => o.E_BuildingId == user.U_BuildingId && o.E_Flag == 2); //ҵ��������
                var giftInfo = events.Where(o => o.E_BuildingId == user.U_BuildingId && o.E_Flag == 3); //�������
                int gift = 0;
                if (giftInfo.Any())
                {
                    gift = giftInfo.Sum(o => o.E_Score);
                }


                var rdm = new Random(Guid.NewGuid().GetHashCode());
                List<int> nums = new List<int>();

                //��ʼ���ץ��
                while (true)
                {

                    if (tickets > 100 && cards > 100 && gift > 1500)
                    {
                        rs.Msg = "�̫�𱬣���Ʒ�Ѿ����꿩��";
                        return rs;
                    }
                    //1��ӰƱ 2ҵ���� 3���
                    int rnum = rdm.Next(0, 100); //����������������н�����
                    if (tickets <= 100 && rnum < 30)
                    {
                        nums.Add(1);
                    }
                    if (cards <= 100 && rnum >= 30 && rnum < 60)
                    {
                        nums.Add(2);
                    }
                    if (gift <= 1500 && rnum >= 60)
                    {
                        nums.Add(3);
                    }
                    if (nums.Count > 0)
                    {
                        break;
                    }

                }

                int tem = rdm.Next(0, nums.Count);
                int flag = nums[tem]; //�ڼ�������

                //if (tem < 40)
                //{
                //    flag = 1;
                //    info.E_Title = "��õ�ӰƱһ��";
                //    info.E_Flag = flag;
                //}
                //else if (tem < 80)
                //{
                //    flag = 2;
                //}
                //else
                //{
                //    flag = 3;
                //}


                info.E_Flag = flag;
                if (flag == 1)
                {
                    info.E_Title = "��õ�ӰƱһ��";
                    info.E_Score = 30; //30Ԫ/�� ��ӰƱ ������ֶη�������Ҳ������

                    rs.Msg =
                        "��ϲ�����е�ӰƱ1�ţ���������������ȡ��ӰƱ" + tickets + "�ţ���100�š��Ͻ������ھ�Ҳ��פ���ţ�����ȡ��ӰƱ�ﵽ80�ţ���������ۿ���Ѱ�����Ӱ��#" + user.U_LoginPhone;
                }
                else if (flag == 2)
                {
                    info.E_Title = "���ҵ����һ��";
                    info.E_Score = 30;

                    Guid bid = Guid.Parse("309FFB15-3CAA-4C92-9F1E-96EDAF5A81FD"); //�ų���ԭD7С��
                    if (user.U_BuildingId != bid)
                    {
                        var ban = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == user.U_Id);
                        if (ban != null)
                        {
                            //�ۼ�ҵ�������
                            ban.B_CouponMoney += 30;


                            Core_BillMember bill = new Core_BillMember()
                            {
                                B_Time = info.E_Time,
                                B_Title = string.Format("����ϻ��������{0}Ԫҵ����", 30),
                                B_Money = 30,
                                B_UId = user.U_Id,
                                B_Phone = user.U_LoginPhone,
                                B_Module = (int)BillEnum.ƽ̨ҵ����,
                                B_OrderId = null,
                                B_Type = (int)MemberRoleEnum.��Ա,
                                B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                                B_Status = 0
                            };
                            //����˵�
                            db.Core_BillMember.Add(bill);
                        }
                    }

                    rs.Msg = "��ϲ������30Ԫ�ֽ�ҵ����1�ţ���������������ȡҵ����" + cards + "�ţ���100�š��Ͻ������ھ�Ҳ��פ���ţ����л���ۿ���Ѱ�����Ӱ���Ͻ��ж������ɣ�#" + user.U_LoginPhone;
                }
                else if (flag == 3)
                {
                    //�ú���ֵ���һ�������� (����1�����5)
                    info.E_Score = rdm.Next(1, 3);
                    info.E_Title = "���" + info.E_Score + "Ԫ���";

                    //��ӻ�Ա�˵���
                    Core_BillMember bill = new Core_BillMember()
                    {
                        B_Time = info.E_Time,
                        B_Title = string.Format("����ϻ��������{0}Ԫ���", info.E_Score),
                        B_Money = info.E_Score,
                        B_UId = user.U_Id,
                        B_Phone = user.U_LoginPhone,
                        B_Module = (int)BillEnum.���,
                        B_OrderId = null,
                        B_Type = (int)MemberRoleEnum.��Ա,
                        B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                        B_Status = 0

                    };
                    db.Core_BillMember.Add(bill);

                    //�ۼ����
                    var balance = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == user.U_Id);
                    balance.B_Balance += info.E_Score; // Convert.ToDecimal(info.E_Score);
                }

                #endregion

                db.Core_ScoreExchange.Add(info);
                if (db.SaveChanges() > 0)
                {
                    rs.State = 0;
                    rs.Data = new
                    {
                        flag,
                        title = info.E_Title,
                        phone = user.U_LoginPhone,
                        num = 80 - tickets - 1
                    }; //JsonSerialize.Instance.ObjectToJson();
                }

            }
            return rs;
        }

        /// <summary>
        /// ����һ�����۵Ľӿڣ���С���н���ҵ����ҵ�������ʺţ���ͨ��ʵ�忨�һ���
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public ResultMessageViewModel Danteng()
        {
            ResultMessageViewModel rs = new ResultMessageViewModel();
            rs.State = 1;
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                Guid bid = Guid.Parse("309FFB15-3CAA-4C92-9F1E-96EDAF5A81FD"); //�ų���ԭD7С��
                var list = db.Core_ScoreExchange.Where(o => o.E_BuildingId != bid && o.E_Flag == (int)FilmFlagEnum.ҵ���� && o.E_Status == 0);
                foreach (var item in list)
                {
                    var ban = db.Core_Balance.FirstOrDefault(o => o.B_AccountId == item.E_UId);
                    if (ban != null)
                    {
                        //�ۼ�ҵ�������
                        ban.B_CouponMoney += item.E_Score;


                        Core_BillMember bill = new Core_BillMember()
                        {
                            B_Time = item.E_Time,
                            B_Title = string.Format("����ϻ��������{0}Ԫҵ����", item.E_Score),
                            B_Money = item.E_Score,
                            B_UId = item.E_UId,
                            B_Phone = item.E_Phone,
                            B_Module = (int)BillEnum.ƽ̨ҵ����,
                            B_OrderId = null,
                            B_Type = (int)MemberRoleEnum.��Ա,
                            B_Flag = (int)BillFlagEnum.��ͨ��ˮ,
                            B_Status = 0
                        };
                        //����˵�
                        db.Core_BillMember.Add(bill);

                        item.E_Status = 1;
                    }
                }
                if (db.SaveChanges() > 0)
                {
                    rs.State = 0;
                    rs.Msg = "ok";
                }
                return rs;
            }
        }
    }

    public class ScoreHistoryCore : BaseRepository<Core_ScoreHistory>
    {
    }
}
