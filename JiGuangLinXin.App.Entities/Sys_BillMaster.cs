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
    
    public partial class Sys_BillMaster
    {
        public int B_Id { get; set; }
        public System.Guid B_UId { get; set; }
        public string B_Phone { get; set; }
        public int B_Type { get; set; }
        public int B_Module { get; set; }
        public Nullable<System.Guid> B_OrderId { get; set; }
        public string B_Title { get; set; }
        public decimal B_Money { get; set; }
        public string B_Remark { get; set; }
        public System.DateTime B_Time { get; set; }
        public int B_Flag { get; set; }
        public int B_Status { get; set; }
    }
}
