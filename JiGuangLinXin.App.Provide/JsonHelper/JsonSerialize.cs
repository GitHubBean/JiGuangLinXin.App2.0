using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiGuangLinXin.App.Provide.JsonHelper
{
    public class JsonSerialize
    {
        public readonly static JsonSerialize Instance = new JsonSerialize();

        private JsonSerialize()
        {
        }

        /// <summary>
        /// 数据实体转化为JSON数据
        /// </summary>
        /// <param name="obj">要转化的数据实体</param>
        /// <returns>JSON格式字符串</returns>
        public string ObjectToJson<T>(T obj)
        {
            if (obj != null)
            {
                return JsonConvert.SerializeObject(obj);
            }
            return string.Empty;
        }

        /// <summary>
        /// 将JSON数据转化为C#数据实体
        /// </summary>
        /// <param name="json">符合JSON格式的字符串</param>
        /// <returns>T类型的对象</returns>
        public T JsonToObject<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (System.Exception tx)
            {
                throw new System.Exception(tx.Message);
            }
        }
        /// <summary>
        /// 将字符串转换为 动态对象
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public dynamic JsonToObject(string json)
        {
            try
            {
                //JArray ja = (JArray)JsonConvert.DeserializeObject(json);  //对象集合
                //if (ja.Count > 0)
                //{
                //    JObject o = (JObject)ja[0];  //若存在，取第一个元素
                //    return o;
                //}

                return JsonConvert.DeserializeObject(json);
            }
            catch (System.Exception ex)
            {

                throw new System.Exception(ex.Message);
            }

            return null;
        }
        /// <summary>
        /// 将字符串转换为 动态对象集合
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public JArray JsonToObjectArray(string json)
        {
            try
            {
                JArray ja = (JArray)JsonConvert.DeserializeObject(json);  //对象集合
                return ja;
            }
            catch (System.Exception ex)
            {

                throw new System.Exception(ex.Message);
            }

            return null;
        }
    }
}
