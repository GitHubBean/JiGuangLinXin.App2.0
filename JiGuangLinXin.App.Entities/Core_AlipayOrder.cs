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
    
    public partial class Core_AlipayOrder
    {
        public System.Guid A_Id { get; set; }
        public string A_OrderNo { get; set; }
        public System.Guid A_UId { get; set; }
        public string A_Phone { get; set; }
        public decimal A_Money { get; set; }
        public System.DateTime A_Time { get; set; }
        public int A_Status { get; set; }
        public string A_Remark { get; set; }
    
        public virtual Core_AlipayOrder Core_AlipayOrder1 { get; set; }
        public virtual Core_AlipayOrder Core_AlipayOrder2 { get; set; }
    }
}
