//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace JiGuangLinXin.App.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class view_topicTicketHistory
    {
        public System.Guid U_Id { get; set; }
        public string U_NickName { get; set; }
        public string U_Logo { get; set; }
        public int U_Age { get; set; }
        public int U_Sex { get; set; }
        public System.Guid U_BuildingId { get; set; }
        public string U_BuildingName { get; set; }
        public string U_ChatID { get; set; }
        public System.Guid T_TopicId { get; set; }
        public string T_TopicName { get; set; }
        public int T_Id { get; set; }
        public System.DateTime C_Time { get; set; }
        public System.DateTime T_DatetTime { get; set; }
        public Nullable<System.Guid> T_ReceiveUserId { get; set; }
        public Nullable<System.DateTime> T_ReceiveTime { get; set; }
        public string receiveFlag { get; set; }
    }
}