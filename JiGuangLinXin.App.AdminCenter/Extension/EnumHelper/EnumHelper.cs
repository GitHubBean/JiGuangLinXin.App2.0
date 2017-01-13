using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace JiGuangLinXin.App.AdminCenter.Extension.EnumHelper
{
    /// <summary>
    /// 枚举帮助类
    /// </summary>
    public class EnumHelper
    {

        #region 根据枚举的键值 组合 多选框list 单选框list集合
        /***
         * public enum BankEnum
        {
        [CustEnum(Letter="BC")]
        中国银行=0,

        [CustEnum(Letter="CBC")]
        中国建设银行=1,

        [CustEnum(Letter="ABC")]
        中国农业银行=2
        }
         * 
         * 
         * ***/
        /// <summary>
        ///  模式一：
        /// 根据枚举类型，返回 SelectListItem 集合，SelectListItem的Text对应枚举的Key,value对应枚举的value
        /// 如： new SelectListItem(){text='中国银行',value=1}
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="isAll"></param>
        /// <param name="selectedValue">默认选择项的，value值集合</param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> GetEnumKeysSelectListItems<T>(bool isAll = false, string selectedValue = "") where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new Exception("参数必须是枚举！");
            }
            //判断缓存中是否存在
            string cacheKey = string.Format("{0}_{1}_{2}_1", typeof(T).Name, isAll, selectedValue);
            if (HttpContext.Current != null && HttpContext.Current.Cache[cacheKey] != null)
                return (IEnumerable<SelectListItem>)HttpContext.Current.Cache[cacheKey];

            IEnumerable<SelectListItem> list;
            if (!string.IsNullOrEmpty(selectedValue))  //是否具备选中项
            {
                var arr = selectedValue.Split(',');
                list = System.Enum.GetValues(typeof(T)).Cast<T>().Select(v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = (Convert.ToInt32(v)).ToString(),
                    Selected = arr.Contains((Convert.ToInt32(v)).ToString())
                });

            }
            else
            {

                list = System.Enum.GetValues(typeof(T)).Cast<T>().Select(v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = (Convert.ToInt32(v)).ToString()
                });
            }

            //var enumKeysSelectListItems = list as SelectListItem[] ?? list.ToArray();
            var enumKeysSelectListItems = list.ToList();

            if (isAll)
            {
                enumKeysSelectListItems.Insert(0, new SelectListItem() { Text = "全部", Value = "-1" });
            }
            //写入缓存
            if (HttpContext.Current != null) HttpContext.Current.Cache[cacheKey] = enumKeysSelectListItems;
            return enumKeysSelectListItems;
        }


        /// <summary>
        /// 模式二：
        /// 根据枚举类型，返回 SelectListItem 集合，SelectListItem的Text为枚举的key，value对应为枚举的 自定义属性的成员,默认Title
        /// 如： new SelectListItem(){text='中国银行',value='BC'}
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="key">自定义属性的成员key</param>
        /// <param name="isAll">是否生成全部项</param>
        /// <param name="selectedValue">默认选择项的，value值集合</param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> GetAttributeValueItemSelectListItems<T>(string key = "Title", bool isAll = false, string selectedValue = "") where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new Exception("参数必须是枚举！");
            }

            //判断缓存中是否存在
            string cacheKey = string.Format("{0}_{1}_{2}_{3}_2", typeof(T).Name, isAll, key, selectedValue);
            if (HttpContext.Current != null && HttpContext.Current.Cache[cacheKey] != null)
                return (IEnumerable<SelectListItem>)HttpContext.Current.Cache[cacheKey];


            List<SelectListItem> ret = new List<SelectListItem>();
            if (isAll)
            {
                ret.Add(new SelectListItem() { Text = "全部", Value = "-1" });
            }

            Type oType = typeof(T);
            Array array = typeof(T).GetEnumValues();  //获取枚举所有的值

            CustEnumAttribute att;
            var prop = typeof(CustEnumAttribute).GetProperty(key);  //反射获取自定义属性中的成员
            string tempValue;
            bool tempSelected = false;
            if (!string.IsNullOrEmpty(selectedValue))  //是否具有选中项
            {
                var arr = selectedValue.Split(',');
                foreach (object t in array)
                {
                    att = GetCustomAttribute<CustEnumAttribute>(t as System.Enum);   //根据当前的枚举项获得他的自定义属性
                    if (att != null)
                    {
                        tempValue = prop.GetValue(att, null).ToString();
                        ret.Add(new SelectListItem() { Text = t.ToString(), Value = tempValue, Selected = arr.Contains(tempValue) });
                    }
                }
            }
            else
            {
                foreach (object t in array)
                {
                    att = GetCustomAttribute<CustEnumAttribute>(t as System.Enum);   //根据当前的枚举项获得他的自定义属性
                    if (att != null)
                    {
                        tempValue = prop.GetValue(att, null).ToString();
                        ret.Add(new SelectListItem() { Text = t.ToString(), Value = tempValue });
                    }
                }
            }

            //写入缓存
            if (HttpContext.Current != null) HttpContext.Current.Cache[cacheKey] = ret;

            //var titles = EnumHelper.GetItemAttributeList<T>().OrderBy(t => t.Value.Order);
            //foreach (var t in titles)
            //{
            //    if (!isAll && (!t.Value.IsDisplay || t.Key.ToString() == "None"))
            //        continue;

            //    if (t.Key.ToString() == "None" && isAll)
            //    {
            //        ret.Add(new SelectListItem() { Text = "全部", Value = "-1" });
            //    }
            //    else
            //    {
            //        ret.Add(new SelectListItem() { Text = t.Key.ToString(), Value = t.Value.GetType().GetProperty("Letter").GetValue(t.Value).ToString() });
            //    }
            //}

            return ret;
        }
        /// <summary>
        /// 模式三：
        /// 根据枚举类型，返回 SelectListItem 集合，SelectListItem的Text为枚举的 自定义属性的成员,默认Title，value对应枚举的value
        /// 如： new SelectListItem(){text='BC',value='1'}
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="key">自定义属性的成员key</param>
        /// <param name="isAll">是否生成全部项</param>
        /// <param name="selectedValue">默认选择项的，value值集合</param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> GetAttributeKeyItemSelectListItems<T>(string key = "Title", bool isAll = false, string selectedValue = "") where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new Exception("参数必须是枚举！");
            }

            //判断缓存中是否存在
            string cacheKey = string.Format("{0}_{1}_{2}_{3}_3", typeof(T).Name, isAll, key, selectedValue);
            if (HttpContext.Current != null && HttpContext.Current.Cache[cacheKey] != null)
                return (IEnumerable<SelectListItem>)HttpContext.Current.Cache[cacheKey];


            List<SelectListItem> ret = new List<SelectListItem>();
            if (isAll)
            {
                ret.Add(new SelectListItem() { Text = "全部", Value = "-1" });
            }

            Type oType = typeof(T);
            Array array = typeof(T).GetEnumValues();  //获取枚举所有的值

            CustEnumAttribute att;
            var prop = typeof(CustEnumAttribute).GetProperty(key);  //反射获取自定义属性中的成员
            string tempValue;
            bool tempSelected = false;
            if (!string.IsNullOrEmpty(selectedValue))  //是否具有选中项
            {
                var arr = selectedValue.Split(',');
                string valStr = string.Empty;
                foreach (object t in array)
                {
                    valStr = Convert.ToInt32(t).ToString();
                    att = GetCustomAttribute<CustEnumAttribute>(t as System.Enum);   //根据当前的枚举项获得他的自定义属性
                    if (att != null)
                    {
                        tempValue = prop.GetValue(att, null).ToString();
                        ret.Add(new SelectListItem() { Text = tempValue, Value = valStr, Selected = arr.Contains(valStr) });
                    }
                }
            }
            else
            {
                foreach (object t in array)
                {
                    att = GetCustomAttribute<CustEnumAttribute>(t as System.Enum);   //根据当前的枚举项获得他的自定义属性
                    if (att != null)
                    {
                        tempValue = prop.GetValue(att, null).ToString();
                        ret.Add(new SelectListItem() { Text = tempValue, Value = Convert.ToInt32(t).ToString() });
                    }
                }
            }

            //写入缓存
            if (HttpContext.Current != null) HttpContext.Current.Cache[cacheKey] = ret;

            return ret;
        }

        /// <summary>
        /// 根据自定义枚举生成selectList集合
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="enumModel">数据模式，模式1：【枚举键值对】、模式2：【枚举key-->枚举属性值】、模式3：【枚举属性值-->枚举key】</param>
        /// <param name="custAttrKey">若数据模式选择 2,3时，需提供枚举自定义属性的key</param>
        /// <param name="showAll">是否显示“全部”项</param>
        /// <param name="selectedValue">默认选择项的，value值集合</param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> GetSelectListItemsByEnum<TEnum>(SelectListItemModelEnum enumModel = SelectListItemModelEnum.KeyValue, string custAttrKey = "Title", bool showAll = false, string selectedValue = "") where TEnum : struct
        {
            IEnumerable<SelectListItem> listItems;
            switch (enumModel)  //数据源模式
            {
                case SelectListItemModelEnum.KeyValue:  //枚举键值对
                    listItems = EnumHelper.GetEnumKeysSelectListItems<TEnum>(showAll, selectedValue);
                    break;
                case SelectListItemModelEnum.KeyAttribute: //枚举key-->枚举属性值
                    listItems = EnumHelper.GetAttributeValueItemSelectListItems<TEnum>(custAttrKey, showAll, selectedValue);
                    break;
                case SelectListItemModelEnum.AttributeVlue: //枚举属性值-->枚举key
                    listItems = EnumHelper.GetAttributeKeyItemSelectListItems<TEnum>(custAttrKey, showAll, selectedValue);
                    break;
                default:
                    listItems = EnumHelper.GetEnumKeysSelectListItems<TEnum>(showAll, selectedValue);
                    break;
            }
            return listItems;
        }

        public static Dictionary<object, CustEnumAttribute> GetItemAttributeList<T>(System.Enum language = null) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new Exception("参数必须是枚举！");
            }
            Type oType = typeof(T);
            Dictionary<object, CustEnumAttribute> ret = new Dictionary<object, CustEnumAttribute>();

            Array array = typeof(T).GetEnumValues();

            CustEnumAttribute att;
            foreach (object t in array)
            {
                att = GetCustomAttribute<CustEnumAttribute>(t as System.Enum);
                if (att != null)
                    ret.Add(t, att);
            }
            return ret;
        }
        #endregion

        /// <summary>
        /// 转换如："enum1,enum2,enum3"字符串到枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="obj">枚举字符串</param>
        /// <returns></returns>
        public static T Parse<T>(string obj)
        {
            if (string.IsNullOrEmpty(obj))
                return default(T);
            else
                return (T)System.Enum.Parse(typeof(T), obj);
        }
        public static T TryParse<T>(string obj, T defT = default(T))
        {
            try
            {
                return Parse<T>(obj);
            }
            catch
            {
                return defT;
            }
        }

        /// <summary>
        /// 根据银行的 “字母缩写” 获取该银行枚举值 
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        //public static BankEnum GetBankEnumValueByCustAttribute(string letter)
        //{
        //    //string temp = GetCustomAttribute<CustEnumAttribute>().Letter;

        //    //var list = System.Enum.GetValues(typeof (BankEnum)).Cast<BankEnum>().Select(v => new
        //    //{
        //    //    key=v.ToString(),
        //    //    value = (int)v
        //    //});

        //    var list = System.Enum.GetValues(typeof(BankEnum));  //获取银行的所有 “value”
        //    foreach (var item in list)
        //    {
        //        if (letter.Equals(EnumHelper.GetCustomAttribute<CustEnumAttribute>((BankEnum)item).Letter)) //判断当前 缩写是否相等
        //        {
        //            return (BankEnum)item;
        //        }
        //    }
        //    return BankEnum.中国银行;
        //}

        /// <summary>
        /// 根据枚举的描述值，返回指定枚举对象
        /// 如：EnumHelper.GetEnumByCustAttribute《BankEnum》("Letter","ICBC")
        /// 
        /// </summary>
        /// <typeparam name="TEnum">枚举对象</typeparam>
        /// <param name="attrKey">自定义属性的Key</param>
        /// <param name="attrValue">自定义属性的Value</param>
        /// <returns></returns>
        public static TEnum GetEnumByCustAttribute<TEnum>(string attrKey, string attrValue) where TEnum : struct
        {

            if (!typeof(TEnum).IsEnum)
            {
                throw new Exception("参数必须是枚举！");
            }

            //判断缓存中是否存在
            string cacheKey = string.Format("{0}_{1}_{2}_4", typeof(TEnum).Name, attrKey, attrValue);
            if (HttpContext.Current != null && HttpContext.Current.Cache[cacheKey] != null)
                return (TEnum)HttpContext.Current.Cache[cacheKey];


            var list = System.Enum.GetValues(typeof(TEnum));  //获取银行的所有 “value”


            foreach (var item in list)
            {
                var attr = GetCustomAttribute<CustEnumAttribute>(item as System.Enum);

                if (attrValue.Equals(attr.GetType().GetProperty(attrKey).GetValue(attr, null))) //判断当前 缩写是否相等
                {
                    //写入缓存
                    if (HttpContext.Current != null) HttpContext.Current.Cache[cacheKey] = item;
                    return (TEnum)item;
                }
            }



            return default(TEnum);
        }



        /// <summary>
        /// 获取枚举项的Attribute
        /// </summary>
        /// <typeparam name="T">自定义的Attribute</typeparam>
        /// <param name="source">枚举</param>
        /// <returns>返回枚举,否则返回null</returns>
        public static T GetCustomAttribute<T>(System.Enum source) where T : Attribute
        {
            Type sourceType = source.GetType();
            string sourceName = System.Enum.GetName(sourceType, source);
            FieldInfo field = sourceType.GetField(sourceName);
            object[] attributes = field.GetCustomAttributes(typeof(T), false);
            foreach (object attribute in attributes)
            {
                if (attribute is T)
                    return (T)attribute;
            }
            return null;
        }

        /// <summary>
        ///获取DescriptionAttribute描述
        /// </summary>
        /// <param name="source">枚举</param>
        /// <returns>有description标记，返回标记描述，否则返回null</returns>
        public static string GetDescription(System.Enum source)
        {
            var attr = GetCustomAttribute<System.ComponentModel.DescriptionAttribute>(source);
            if (attr == null)
                return null;

            return attr.Description;
        }

    }

    /// <summary>
    /// 自定义 枚举的 “CustEnum” 属性 
    /// </summary>
    public class CustEnumAttribute : Attribute
    {
        private bool _IsDisplay = true;

        /// <summary>
        /// 构造时，暂时只用到 字母缩写（在此处可扩展）
        /// </summary>
        /// <param name="letter"></param>
        public CustEnumAttribute(string letter)
        {
            Letter = letter;
        }
        //是否显示
        public bool IsDisplay { get { return _IsDisplay; } set { _IsDisplay = value; } }
        //标题
        public string Title { get; set; }
        //描述
        public string Description { get; set; }
        //字母缩写
        public string Letter { get; set; }

        //枚举相近的一些词
        public string[] Synonyms { get; set; }
        //分类
        public int Category { get; set; }
        //排序号
        public int Order { get; set; }
    }

}