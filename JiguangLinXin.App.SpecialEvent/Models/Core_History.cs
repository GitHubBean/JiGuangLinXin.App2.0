//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace JiguangLinXin.App.SpecialEvent.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Core_History
    {
        public int H_Id { get; set; }
        public string H_Ip { get; set; }
        public string H_UA { get; set; }
        public System.Guid H_VillageId { get; set; }
        public string H_VillageName { get; set; }
        public Nullable<System.DateTime> H_Time { get; set; }
        public int H_Source { get; set; }
        public string H_Notes { get; set; }
    }
}