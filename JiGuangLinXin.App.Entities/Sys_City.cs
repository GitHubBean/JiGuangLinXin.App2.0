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
    
    public partial class Sys_City
    {
        public Sys_City()
        {
            this.Sys_District = new HashSet<Sys_District>();
        }
    
        public int C_PK_ID { get; set; }
        public int C_ProvinceID { get; set; }
        public string C_Name { get; set; }
        public string C_AreaCode { get; set; }
        public string C_LevelCode { get; set; }
        public Nullable<int> C_Sort { get; set; }
        public string C_Pinyin { get; set; }
        public int C_Hot { get; set; }
    
        public virtual Sys_Province Sys_Province { get; set; }
        public virtual ICollection<Sys_District> Sys_District { get; set; }
    }
}
