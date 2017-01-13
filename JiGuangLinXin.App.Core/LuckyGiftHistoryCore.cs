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
        /// �鿴��� ��ּ�¼
        /// </summary>
        /// <param name="giftId">���id</param>
        /// <param name="uid">��Աid</param>
        /// <returns></returns>
        public ResultMessageViewModel GetSendGiftDetail(Guid giftId, Guid uid)
        {

            ResultMessageViewModel result = new ResultMessageViewModel();
            var gift = giftCore.LoadEntity(o => o.LG_Id == giftId && o.LG_UserId == uid);
            if (gift == null)
            {
                result.State = 1;
                result.Msg = "���������";
            }
            else
            {
                var list = base.LoadEntities(o => o.LH_GiftId == giftId);  //�������
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
        /// �鿴�յ������к��
        /// </summary>
        /// <param name="uid">��Աid</param>
        /// <returns></returns>
        public ResultMessageViewModel GetReceiveGiftDetail(Guid uid)
        {

            ResultMessageViewModel result = new ResultMessageViewModel();

            var list = base.LoadEntities(o => o.LH_UserId == uid && o.LH_Status == 0);  //�������
            var rs = new
            {
                money = list.Sum(o => o.LH_Money),//����ܶ�
                count = list.Count(),  //�������
                receiveData = list
            };
            result.Data = rs;


            return result;
        }


    }
}
