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
    
    public partial class Core_InviteReply
    {
        public int R_Id { get; set; }
        public System.Guid R_InviteId { get; set; }
        public System.Guid R_UId { get; set; }
        public string R_Content { get; set; }
        public System.DateTime R_Time { get; set; }
        public int R_Status { get; set; }
        public string R_Remark { get; set; }
        public string R_UPhone { get; set; }
    }
}