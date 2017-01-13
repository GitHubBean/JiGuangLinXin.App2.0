using System;
using System.Linq;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.ViewModel;

namespace JiGuangLinXin.App.Core
{
    public class LuckyGiftHistoryCore : BaseRepository<Core_LuckyGiftHistory>
    {
        private LuckyGiftCore giftCore = new LuckyGiftCore();
        /// <summary>
        /// 查看红包 拆分记录
        /// </summary>
        /// <param name="giftId">红包id</param>
        /// <param name="uid">会员id</param>
        /// <returns></returns>
        public ResultMessageViewModel GetSendGiftDetail(Guid giftId, Guid uid)
        {

            ResultMessageViewModel result = new ResultMessageViewModel();
            var gift = giftCore.LoadEntity(o => o.LG_Id == giftId && o.LG_UserId == uid);
            if (gift == null)
            {
                result.State = 1;
                result.Msg = "红包不存在";
            }
            else
            {
                var list = base.LoadEntities(o => o.LH_GiftId == giftId);  //红包详情
                var rs = new
                {
                    gfitId = giftId,
                    money = gift.LG_Money,
                    count = gift.LG_Count,
                    remainCount = gift.LG_RemainCount,
                    receiveData = list
                };
                result.Data = rs;
            }

            return result;
        }
        /// <summary>
        /// 查看收到的所有红包
        /// </summary>
        /// <param name="uid">会员id</param>
        /// <returns></returns>
        public ResultMessageViewModel GetReceiveGiftDetail(Guid uid)
        {

            ResultMessageViewModel result = new ResultMessageViewModel();

            var list = base.LoadEntities(o => o.LH_UserId == uid && o.LH_Status == 0);  //红包详情
            var rs = new
            {
                money = list.Sum(o => o.LH_Money),//红包总额
                count = list.Count(),  //红包总数
                receiveData = list
            };
            result.Data = rs;


            return result;
        }


    }
}
