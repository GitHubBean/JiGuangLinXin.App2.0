namespace JiGuangLinXin.App.AdminCenter.Extension.EnumHelper
{
    /// <summary>
    /// 多选框、单选框列表枚举集合
    /// </summary>
    public enum SelectListItemModelEnum
    {
        /// <summary>
        /// 模式一：text=枚举键,value=枚举值
        /// </summary>
        KeyValue,
        /// <summary>
        /// 模式二：text=枚举键，value=枚举属性值
        /// </summary>
        KeyAttribute,
        /// <summary>
        /// 模式三:text=枚举属性值,value=枚举值
        /// </summary>
        AttributeVlue
    }
}
