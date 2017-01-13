using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Provide.StringHelper;

namespace JiGuangLinXin.App.Core
{
    public class OwnerCardCodeCore : BaseRepository<Sys_OwnerCardCode>
    {

        /// <summary>
        /// 批量制卡
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool BatchBuildCardCode(Sys_OwnerCard obj)
        {
            using (LinXinApp20Entities db = new LinXinApp20Entities())
            {
                if (obj.OC_TotalCount > 0)
                {
                    for (int i = 0; i < obj.OC_TotalCount; i++)
                    {
                        Sys_OwnerCardCode item = new Sys_OwnerCardCode();  //单个卡的密钥

                        item.C_BatchNo = obj.OC_BatchNo;
                        item.C_Code = new CreateRandomStr().BuildRandomNumber(16); //GuidTo16String();//Guid.NewGuid().ToString("N");
                        item.C_Flag = obj.OC_Flag;
                        item.C_Id = Guid.NewGuid();
                        item.C_Money = obj.OC_Money;
                        item.C_State = 0;
                        item.C_Time = obj.OC_Time;
                        item.C_PId = obj.OC_Id;

                        db.Entry(item).State = EntityState.Added;
                    }
                    db.Entry(obj).State = EntityState.Added;
                    return db.SaveChanges() > 0;
                }
            }

            return false;
        }
        public string GuidTo16String()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
                i *= ((int)b + 1);
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }


    }

    public class OwnerCardCore : BaseRepository<Sys_OwnerCard>
    {
    }

}
