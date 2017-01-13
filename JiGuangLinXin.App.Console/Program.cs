using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using JiGuangLinXin.App.App20Interface;
using JiGuangLinXin.App.Core;
using JiGuangLinXin.App.Entities;
using JiGuangLinXin.App.Entities.BaseEnum;
using JiGuangLinXin.App.Entities.ViewModel;
using JiGuangLinXin.App.Log;
using JiGuangLinXin.App.Provide.EncryptHelper;
using JiGuangLinXin.App.Provide.ImgHelper;
using JiGuangLinXin.App.Provide.JsonHelper;
using JiGuangLinXin.App.Provide.StringHelper;
using JiGuangLinXin.App.Services;

namespace JiGuangLinXin.App.Console
{
    public class Abc
    {
        public dynamic demo { get; set; }
    }

    class Program
    {
        public static string GetOrderNumber()
        {
            string Number = DateTime.Now.ToString("yyMMddHHmmss");//yyyyMMddHHmmssms
            return Number + Next(1000, 1).ToString();
        }
        private static int Next(int numSeeds, int length)
        {


            // Create a byte array to hold the random value.  
            byte[] buffer = new byte[length];
            // Create a new instance of the RNGCryptoServiceProvider.  
            System.Security.Cryptography.RNGCryptoServiceProvider Gen = new System.Security.Cryptography.RNGCryptoServiceProvider();
            // Fill the array with a random value.  
            Gen.GetBytes(buffer);
            // Convert the byte to an uint value to make the modulus operation easier.  
            uint randomResult = 0x0;//这里用uint作为生成的随机数  
            for (int i = 0; i < length; i++)
            {
                randomResult |= ((uint)buffer[i] << ((length - 1 - i) * 8));
            }
            // Return the random number mod the number  
            // of sides. The possible values are zero-based  
            return (int)(randomResult % numSeeds);
        }
        public static string GetTimeStamp()
        {
            Thread.Sleep(1000);
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        public static string GuidTo16String()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
                i *= ((int)b + 1);
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        public static void SyncB()
        {
            System.Console.WriteLine("同步开始");
            Thread.Sleep(3000);
            System.Console.WriteLine("同步结束");
            //for (int i = 0; i < 20; i++)
            //{
            //    System.Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff"));
            //    Thread.Sleep(100);
            //}
        }


        public static async void AsyncA()
        {
            System.Console.WriteLine("异步开始");
            await Task.Delay(3000);
            System.Console.WriteLine("异步结束");

        }




        public static void Sync()
        {
            System.Console.WriteLine("同步开始");

            //do something 发邮箱
            //do something  写日志
            //do something 发短信

            System.Console.WriteLine("同步结束");
        }


        public static async Task Async()
        {
            System.Console.WriteLine("异步开始");

            //写日志方法、发短信、发邮箱

            Task task1 = Task.Run(() =>
            {
                //do something 发邮箱
                Thread.Sleep(3000);
            });
            await task1;

            Task task2 = Task.Run(() =>
            {
                //do something  写日志
                Thread.Sleep(3000);
            });
            await task2;

            Task task3 = Task.Run(() =>
            {
                //do something 发短信
                Thread.Sleep(3000);
            });
            await task3;

            //            Task.WaitAll()

            System.Console.WriteLine("异步结束");
        }

        private static Core_PrizeDetail CalcRate(int count)
        {
            var pd = new Core_PrizeDetail();

            List<Core_PrizeDetail> list = new List<Core_PrizeDetail>();

            list.Add(new Core_PrizeDetail() { PD_Award = "iPhone6", PD_LuckGift = 0, PD_OwnerCard = 0, PD_Rate = 0 });
            list.Add(new Core_PrizeDetail() { PD_Award = "500业主卡", PD_LuckGift = 0, PD_OwnerCard = 500, PD_Rate = 0.2 * count * 0.2 });
            list.Add(new Core_PrizeDetail() { PD_Award = "300业主卡", PD_LuckGift = 0, PD_OwnerCard = 300, PD_Rate = 0.2 * count * 0.1 });
            list.Add(new Core_PrizeDetail() { PD_Award = "业主红包5元", PD_LuckGift = 5, PD_OwnerCard = 0, PD_Rate = 0.2 * count * 0.3 });
            list.Add(new Core_PrizeDetail() { PD_Award = "100元业主卡", PD_LuckGift = 0, PD_OwnerCard = 100, PD_Rate = 0.2 * count * 0.2 });
            list.Add(new Core_PrizeDetail() { PD_Award = "VR眼镜", PD_LuckGift = 0, PD_OwnerCard = 0, PD_Rate = 0.2 * count * 0.1 });
            list.Add(new Core_PrizeDetail() { PD_Award = "业主红包10元", PD_LuckGift = 10, PD_OwnerCard = 0, PD_Rate = 0.2 * count * 0.1 });
            list.Add(new Core_PrizeDetail() { PD_Award = "谢谢参与", PD_LuckGift = 0, PD_OwnerCard = 0, PD_Rate = 1 - 0.2 * count });
            //if (count == 1)  //20% 几率中奖
            //{
            //    list[1].PD_Rate = 0.2 * 0.1;
            //    list[2].PD_Rate = 0.2 * 0.2;
            //    list[3].PD_Rate = 0.2 * 0.3;
            //    list[4].PD_Rate = 0.2 * 0.1;
            //    list[5].PD_Rate = 0.2 * 0.2;
            //    list[6].PD_Rate = 0.2 * 0.1;
            //    list[7].PD_Rate = 0.8;
            //} 

            Random rdm = new Random(Guid.NewGuid().GetHashCode());

            int temp = 0;
            int seek = rdm.Next(1, 101);  //取一个随机数
            for (int i = 0; i < list.Count; i++)
            {
                temp += Convert.ToInt32(list[i].PD_Rate * 100);
                if (temp >= seek)
                {
                    System.Console.WriteLine(string.Format("seek:{0},temp:{1}", seek, temp));
                    return list[i];
                }
            }
            return null;
        }

        // 时间戳转为C#格式时间
        private static DateTime StampToDateTime(string timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp );
            TimeSpan toNow = new TimeSpan(lTime);

            return dateTimeStart.Add(toNow);
        }

        // DateTime时间格式转换为Unix时间戳格式
        private static int DateTimeToStamp(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }


        /// <summary>
        /// unix时间戳转换成日期
        /// </summary>
        /// <param name="unixTimeStamp">时间戳（秒）</param>
        /// <returns></returns>
        public static DateTime UnixTimestampToDateTime(long timestamp)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0,DateTimeKind.Utc);
            return start.AddMilliseconds(timestamp);
        }
        static void Main(string[] args)
        {


            for (int i = 5; i < 7; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    LogHelper log = new LogHelper("D:\\TempStatic\\log\\pro"+i, LogType.Daily);

                    if (i==5)
                    {
                        log.Write("0000000000***"+j, LogLevel.Error);
                    }
                    else
                    {
                        log.Write("11111111111***" + j, LogLevel.Error);
                    }
                    Thread.Sleep(50);
                }
            }
            System.Console.WriteLine("ok");

            System.Console.ReadKey();




            var i1 = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;

            System.Console.WriteLine(i1);
            System.Console.WriteLine(UnixTimestampToDateTime(1472719437143).ToString());
            System.Console.WriteLine(new DateTime(621355968000000000).ToString("yyyy-MM-dd HH:mm:ss"));
            System.Console.WriteLine(DateTime.Now.ToUniversalTime().ToString());
              i1 = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;

            var i3 = (DateTime.Now.ToLocalTime().Ticks - 621355968000000000) / 10000;
            var i2 = DateTime.Now.ToUniversalTime().Ticks;

            System.Console.WriteLine((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);  //精确到秒，若要毫秒可以/10000  ，客户端jS:Math.round(new Date().getTime()/1000) 精确到秒
            System.Console.ReadKey();
            var cks = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
            System.Console.WriteLine(cks);

            var adas = DESProvider.EncryptString("10");
            int accs = 0;
            decimal abc = 8m;
            System.Console.WriteLine(int.TryParse(abc.ToString(),out accs));

            System.Console.WriteLine(int.TryParse("8.5", out accs));


            System.Console.ReadKey();
            var rto = new { };

            double dd = 10000000000000000000000d;

            dd += 1;

            System.Console.WriteLine("{0:G50}", dd);

            System.Console.ReadKey();

            decimal accc = decimal.Parse("0.66");
            float adc = 0.5f * 0.5f;
            decimal acs = accc * 8;
            System.Console.WriteLine(acs);
            System.Console.ReadKey();


            //var cc2 = Guid.Parse("49A63CD0-681A-43BD-B052-2960E91372A0").ToString("N");

            string jr = "{\"openid\":\"odfclxnpdwcvhlngabv\",\"sex\":\"2\",\"privilege\":[]}";

            dynamic ob = JsonSerialize.Instance.JsonToObject<Dictionary<string, object>>(jr);


            System.Console.ReadKey();

            WaterImageManage water = new WaterImageManage();



            string filepath = @"D:\linxinAppStatic\attachment\tagFun\";

            //获得所有的模版图片 
            //var files = Directory.GetFiles(filepath, "*.jpg");
            DirectoryInfo folder = new DirectoryInfo(filepath);
            var files = folder.GetFiles();

            Random rdm = new Random(Guid.NewGuid().GetHashCode());
            string fn = files[rdm.Next(0, files.Count())].Name;  //随机娶一个图片

            string tf = water.DrawWords(fn, "龙湖西苑青青雅舍东原D7", float.Parse("0.9"), ImagePosition.Building, filepath);
            System.Console.WriteLine(tf);
            System.Console.ReadKey();


            string a = "35.2941";
            decimal b = 0;
            decimal.TryParse(a, out b);

            System.Console.WriteLine(Convert.ToInt32(b));

            System.Console.WriteLine();
            System.Console.WriteLine(Convert.ToDecimal((b / Convert.ToDecimal(0.85)).ToString("F0")));


            System.Console.WriteLine("1#iPhone6".Substring(0, "1#iPhone6".IndexOf('#')));
            System.Console.ReadKey();
            int ccount = 0;
            for (int i = 0; i < 5; i++)
            {
                var ii = CalcRate(1);
                if (ii == null)
                {

                }
                else
                {

                    System.Console.WriteLine(string.Format("{0} - {1}", ii.PD_Award, ii.PD_Rate));
                    if (ii.PD_Award != "谢谢参与")
                    {
                        ccount++;
                    }

                }
                Thread.Sleep(100);
            }
            System.Console.WriteLine("---------预期中奖数:20------------总共中奖次数：" + ccount);
            System.Console.WriteLine("1#iPhone6".Substring("1#iPhone6".IndexOf('#')));
            System.Console.ReadKey();

            //var ar = new int[] { 2, 0, 3, 2, 4, 0, 1, 3, 2, 3, 3 };
            //string ph = "";
            ////new int[] { 2, 0, 3, 2, 4, 0, 1, 3, 2, 3, 3 }.ToList().ForEach((c, a) => ph += new int[] { 2, 0, 3, 2, 4, 0, 1, 3, 2, 3, 3 }[c]);
            //System.Console.WriteLine(string.Join("", new[] { 2, 0, 3, 2, 4, 0, 1, 3, 2, 3, 3 }.Select(i => new[] { 8, 2, 1, 0, 3 }[i])));

            //string name = "张三";
            //int age = 20;
            //string s1 = string.Format("{0},{1}", name, age);
            //string s2 = string.Format("姓名={0},年龄={1}", name, age);
            //string s3 = string.Format("{0,15},{1:d3}", name, age);
            //string s4 = string.Format("{0,15},{1,10:d3}", name, age);
            //System.Console.WriteLine("{0},{1},{2},{3}", s1, s2, s3, s4);

            string pao = string.Join("", new[] { 2, 0, 3, 2, 4, 0, 1, 3, 2, 3, 3 }.Select(i => new[] { 8, 2, 1, 0, 3 }[i]));
            Debug.WriteLine(pao);

            //string.Join("", ar.ToList().Select((i, o) => new { }));
            System.Console.ReadKey();

            string logoUrl = "http://120.76.101.244:8122/";
            System.Console.WriteLine(logoUrl.Substring(logoUrl.IndexOf("avatar")));

            System.Console.WriteLine(DESProvider.DecryptString("OdzQezoPBQU="));
            System.Console.ReadKey();

            Tuple<int, string, bool> tuple =
        new Tuple<int, string, bool>(1, "cat", true);
            // Access tuple properties.
            if (tuple.Item1 == 1)
            {
                System.Console.WriteLine(tuple.Item1);
            }
            if (tuple.Item2 == "dog")
            {
                System.Console.WriteLine(tuple.Item2);
            }
            if (tuple.Item3)
            {
                System.Console.WriteLine(tuple.Item3);
            }
            System.Console.ReadKey();




            Stopwatch timer = new Stopwatch();

            timer.Start();//开始计算时间 
            //Sync();
            //for (int i = 0; i < 10; i++)
            //{
            //    System.Console.WriteLine("同步"+i);
            //    Thread.Sleep(100);
            //}
            //timer.Stop();//结束点，另外stopwatch还有Reset方法，可以重置。
            System.Console.WriteLine(timer.Elapsed + " 毫秒：" + timer.Elapsed);//显示时间

            ////////////////////
            //timer.Restart();
            Async();
            //for (int i = 0; i < 10; i++)
            //{
            //    System.Console.WriteLine("异步" + i);
            //    Thread.Sleep(100);
            //}
            System.Console.WriteLine(timer.Elapsed + " 毫秒：" + timer.Elapsed);//显示时间



            System.Console.ReadKey();


            System.Console.WriteLine("220 / 100 = " + 220 / 100);


            Sys_AdSystem oc = new Sys_AdSystem();

            System.Console.WriteLine((object)oc == null);

            for (int i = 0; i < 10; i++)
            {
                System.Console.WriteLine(GuidTo16String());
                Thread.Sleep(100);
            }
            System.Console.ReadKey();
            System.Console.WriteLine(Math.Abs(-123));


            string[] strarr = { "f3dfe86ce09c47b1ad3b7dfafdfbc155" };

            var tem = strarr.Select(o => "lx_" + Guid.Parse(o)).ToArray();
            foreach (var s in tem)
            {
                System.Console.WriteLine(s.ToString());
            }
            System.Console.ReadKey();

            System.Diagnostics.Debug.WriteLine("Value is : " + 123);

            System.Console.ReadKey();
            for (int i = 0; i < 20; i++)
            {
                System.Console.WriteLine(("刷卡打飞机阿斯科利的房价ad搜房卡上的纠纷；阿斯蒂芬金克拉三等奖发生打架阿斯蒂芬asdf23asd54f6asd4f654a65sdf413as1dfvas23d1fv3a146s5df4v56asd4fsad32f123ad1s321f23as1d23f1as2d31f23a1sdsad疯狂阿桑德拉反馈来看" + Guid.NewGuid()).GetHashCode().ToString("D10"));
                Thread.Sleep(100);
            }
            System.Console.ReadKey();
            var pro = new
            {
                flag = 4,
                targetUrl = "http://i.cqjglx.com/html/classroom/index.html",
                proId = ""
            };

            var ccs = JsonSerialize.Instance.ObjectToJson(pro);


            var aaa = ",61763331".Split(',');


            //System.Console.WriteLine(Md5Extensions.MD5Encrypt("a183802027092771"));


            var ids =
                            new UserCore().LoadEntities(
                                o => o.U_AuditingManager == (int)AuditingEnum.认证成功 && o.U_Status != (int)UserStatusEnum.冻结).ToList()
                                .Select(o => o.U_Id.ToString("N").ToLower())
                                .ToArray();  // 查询所有的管理员ID


            System.Console.ReadKey();

            System.Console.WriteLine(Math.Pow(10, 6));
            int min = Convert.ToInt32(Math.Pow(10, 5));
            int max = Convert.ToInt32(Math.Pow(10, 6));
            for (int i = 0; i < 200; i++)
            {

                System.Console.WriteLine(new Random((int)DateTime.Now.Ticks).Next(min, max));

                Thread.Sleep(100);
            }
            System.Console.ReadKey();

            BuildingViewModel vm = new BuildingViewModel()
            {
                landscape = "保利中心，保利地产于江北嘴CBD倾力打造的重庆商务地标，未来发展前途不可估量。项目三低一高配置，低密生态引领都市办公热潮：2.09超低容积率（江北嘴其他写字楼容积率基本都在7-10之间），为企业带来高端生态揽江办公体验；",
                location = "距离地铁3、5号线交汇站或珠江新城中轴地下快速公交系统步行距离8分钟，步行至广州大道五羊新村站5分钟，53条公交线路辐射广州；开车起步即达广州大道、黄埔大道，快速接驳体育东、天河北商圈，8分钟可达新光快速。",
                planning = "保利中心以全生态商务办公空间，为金融企业量身定做保利中心占地2万方，临江写字楼总建筑面积8.2万方，采用南北通透板式设计。南望270°江景，北望中心园林及商务广场，在寸土寸金的珠江新城独享健康生态办公环境。",
                property = "保利中心：超高人均绿化占有率，30%的绿化率让您享受更舒适、更健康的办公环境。项目四周双公园环绕，一线临江。塔子山公园、溉澜溪体育公园，超180°的江景视野，尽揽城市风光。"
            };

            var jsSst = JsonSerialize.Instance.ObjectToJson(vm);






            BuildingViewModel vm2 = new BuildingViewModel()
            {
                landscape = "城市锦上项目总用地面积250余亩，总建筑面积37万平米，主要由 湖湾度假公寓、湖湾假日美宅、湖滨风情商业街区、酒店半山低密 度配套、铂金五星级度假酒店、五星级温泉养生spa多种业态组成。",
                location = "城市锦上紧临弥勒的湖泉生态园，背靠白腊园森林公园，东临黑腊 沼，被大小龙潭两个水体环绕，背靠弥勒目前最大的18洞及36洞高 尔夫球场。",
                planning = "城市锦上位于凤城一路与明光路十字向西50米，是由西安经发城市 发展有限公司开发，项目地处经开区门户核心区域，项目周边生活 配套齐全。东侧与经发学校仅一墙之隔，西邻朱宏路850亩汉城湖 遗址公园仅200米，南隔城市金腰带北。",
                property = "城市锦上紧临弥勒的湖泉生态园，背靠白腊园森林公园，东临黑腊 沼，被大小龙潭两个水体环绕，背靠弥勒目前最大的18洞及36洞高 尔夫球场。"
            };

            var jsSst1 = JsonSerialize.Instance.ObjectToJson(vm2);






            BuildingViewModel vm3 = new BuildingViewModel()
            {
                landscape = "富邦中心占据重庆主城最核心地段，石桥铺、大坪、沙坪坝三大百 亿级商圈优势叠加,立体式交通网络，迅速畅达全城，强势整合城市 资源，中心之上再造中心。",
                location = "轨道1号线、2号线环伺项目，距石桥铺轻轨站步行不到10分钟；在 建轨道5号线将贯穿项目；车行10分钟到达江北商圈、杨家坪商圈、 南坪商圈、解放碑商圈。",
                planning = "繁华之上，再塑繁华。再高九路投资狂潮，扼守区域产业升级制高 点，狙击时代财富！城市未来，向这里看起！",
                property = "富邦中心集合SHHO、LOFT、控制云铺、临街商铺全能物业形态， 首创商务、娱乐、休闲、办公、餐饮多重功能全新体验式商务体。"
            };

            var jsSst2 = JsonSerialize.Instance.ObjectToJson(vm3);





            BuildingViewModel vm4 = new BuildingViewModel()
            {
                landscape = "世界只有一个九寨沟，九寨沟只有一个“九寨•云顶”。以约3000多亩的超大体量，近10种截然不不同的休闲业态打造一个纯粹高端的休闲度假复合体，这就是“九寨云顶”不可消防的独特之处。",
                location = "四川宏义实业集团有限公司斥巨资打造的“九寨云顶”项目，位于九寨沟县城西南方保华乡海池，约20000米山腹之上。因紧邻闻名世界的旅游圣地九寨沟。该项目得天地造化，享绝世地理。",
                planning = @"“九寨云顶”具备世界顶级休闲度假旅游圣地的属性，已成为阿坝
州政府旅游发展规划的重点项目之一，“九寨云顶”的建成势必成为
带动九寨沟产业升级和旅游经济二次腾飞的龙头。
",
                property = @"定制低密度住宅是典型的面向社会塔尖人群的高端住宅产品。在“九
寨云顶”，这种能带给人们更多选择机会合对个性尊重的一对一式服务
也能实现。
。"
            };

            var jsSst3 = JsonSerialize.Instance.ObjectToJson(vm4);




            System.Console.ReadKey();


            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("partner", "2088121602726575");
            sParaTemp.Add("_input_charset", Com.Alipay.Config.Input_charset.ToLower());
            sParaTemp.Add("service", "single_trade_query");
            //sParaTemp.Add("trade_no", order);//使用支付宝交易号查询
            sParaTemp.Add("out_trade_no", "C994EB9B63444B1CB22A04FEB9F40FB8"); //使用商户订单号查询

            string sHtmlText = Com.Alipay.Submit.BuildRequest(sParaTemp);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sHtmlText);


            System.Console.ReadKey();

            double iii = 192.356565;
            System.Console.WriteLine(iii);
            System.Console.ReadKey();
            var inf = new Abc();
            inf.demo.abc = 1;
            inf.demo.def = 2;

            System.Console.WriteLine(inf.demo.abc);

            System.Console.ReadKey();

            BusinessCore bcore = new BusinessCore();


            //Juhe.Update("D:\\juheDept.txt");

            //var list = bcore.LoadEntities().Select(o => new {o.B_Id}).ToList();

            IEnumerable<Core_Building> list = new List<Core_Building>()
            {
                new Core_Building(){B_Id = Guid.NewGuid(),B_Name = "1"},
                new Core_Building(){B_Id = Guid.NewGuid(),B_Name = "2"},
                new Core_Building(){B_Id = Guid.NewGuid(),B_Name = "3"},
                new Core_Building(){B_Id = Guid.NewGuid(),B_Name = "4"},
                new Core_Building(){B_Id = Guid.NewGuid(),B_Name = "56"},
                new Core_Building(){B_Id = Guid.NewGuid(),B_Name = "6"},
            };

            StringBuilder sb = new StringBuilder();
            var ll = list.Select(o => new { bid = o.B_Id }).AsEnumerable();


            // 期望得到一个字符串 b_id逗号分割 如： d675a17563c14f1e96598edecf08c137，d675a17563c14f1e96598edecf08c133
            System.Console.WriteLine(list);
            System.Console.ReadKey();

            System.Console.WriteLine(Md5Extensions.MD5Encrypt("a183802027092771"));

            //DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970,1,1));
            //DateTime dtNow = DateTime.Parse(DateTime.Now.ToString());
            //TimeSpan toNow = dtNow.Subtract(dtStart);
            //string timeStamp = toNow.TotalMilliseconds.ToString();
            //            System.Console.WriteLine(DateTime.Now.ToString());

            LuckyGiftCore ccc = new LuckyGiftCore();
            for (int i = 0; i < 50; i++)
            {
                CreateRandomStr dr = new CreateRandomStr();

                System.Console.WriteLine(dr.GetRandomString(6));
                Thread.Sleep(200);
            }

            //double money = 8555554545.561662632626;
            //System.Console.WriteLine(Guid.NewGuid().ToString("N"));

            System.Console.ReadKey();

            //System.Console.WriteLine(AttachmentFolderEnum.landscape);
            //System.Console.WriteLine("ok".Equals("OK", StringComparison.InvariantCultureIgnoreCase));
            //System.Console.WriteLine( DateTime.Now.ToString("yyyyMMdd_HHmmssffff"));

            //System.Console.ReadKey();
            //System.Console.WriteLine("ok".Equals(StringExt.removeHtml("OK\r\n\r\n\r\n  "), StringComparison.InvariantCultureIgnoreCase));

            //Core_ChinapayOrder obj = new Core_ChinapayOrder() {A_billDate="003"};
            //System.Console.WriteLine(obj.A_billDate);
            //System.Console.WriteLine(obj.A_billType);

            //ChangeOjb(ref obj);

            //System.Console.WriteLine(obj.A_billDate);
            //System.Console.WriteLine(obj.A_billType);

            //GetBag(9,9);

            //GetBag1(8,8);
            ////////////////////////
            /*
            int count = 10;
            double money = 6;
            for (int i = count; i > 0; i--)
            {
                if (money > 0)
                {
                    if (i == 1)
                    {
                        System.Console.WriteLine(money);
                    }
                    else
                    {
                        if (money - 0.01 * count > 0)
                        {

                            //money -= 0.01 * count;
                            Random random = new Random();
                            double cur = Math.Round(random.NextDouble() * (money - 0.01 * count - 0.01) + 0.01, 2);

                            System.Console.WriteLine(cur);

                            money -= cur;
                        }
                        else
                        {
                            money -= 0.01;
                            System.Console.WriteLine("0.01");
                        }

                    }
                }
                else
                {
                    money -= 0.01;
                    System.Console.WriteLine("0.01");
                }
            }*/
            ////////////////////
            //double[] hb = WeiXin.Hongbao(count,money);

            //for (int i = 0; i < 100; i++)
            //{
            //    System.Console.WriteLine(CalcGift(10, 5));
            //    ;
            //}
            //for (int i = 0; i < count; i++)
            //{
            //    System.Console.WriteLine(hb[i]);
            //}



            //System.Console.WriteLine(HuanXin.AccountQunJoin("124778987309236808", "lx18580465179"));

            //System.Console.WriteLine(HuanXin.CreateUser("lxyx999"));

            //IList<string> list = new[] { "a", "b", "cc", "a", "b", "cc", "a", "b", "cc", "bb" };
            //var l = list.Select((o, i) => new
            //{
            //    key = o.ToString(),
            //    star = i > 8 ? 3.5 : (i > 4 ? 4 : (i > 1 ? 4.5 : 5))
            //});

            //foreach (var obj in l)
            //{
            //    System.Console.WriteLine(obj.key + "***" + obj.star);
            //}


            //string a = "{\"beijing\":{\"zone\":\"海淀\",\"zone_en\":\"haidian\"}}";
            //string a = "[{\"a\":\"123\"},{\"a\":\"abc\"}]";
            System.Console.WriteLine(DESProvider.EncryptString("123456"));

            // dynamic tk = JsonSerialize.Instance.JsonToObject(a);
            //System.Console.WriteLine(Md5Extensions.MD5Encrypt("222"));DESProvider.DesDecrypt("HUX+7VtHgb0=")
            //System.Console.WriteLine(DESProvider.DecryptString("wVVLgz75kdU="));

            ////object id = null;
            ////System.Console.WriteLine(JsonSerialize.Instance.ObjectToJson(new {state="1",msg="sdsd",data=id}));

            string pwd = "123456";
            string phone = "6666";
            Guid c = Guid.NewGuid();
            string cc = Md5Extensions.MD5Encrypt(pwd + phone);
            //System.Console.WriteLine(cc);
            //AuditingVillageCore audCore = new AuditingVillageCore();
            //System.Console.WriteLine(audCore.CreateHuanxinGroup("LX00000"));

            string json = "{\"phone\":\"15696861956\",\"code\":\"wVVLgz75kdU=\",\"time\":\"2016/2/29 13:48:54\",\"sendCount\":0}";
            //string json = "{\n    count = 2;\n    money = 2;\n}";

            dynamic oo = JsonSerialize.Instance.JsonToObject(json);
            //System.Console.WriteLine(oo.phone);

            string rs = DESProvider.EncryptString("123456");

            string r = DESProvider.Encrypt("abc123456");

            System.Console.WriteLine(rs);
            System.Console.ReadKey();
        }


        public static double CalcGift(double money, int count)
        {
            if (money - 0.01 * count > 0)
            {
                double cur = GetRandom(money - 0.01 * count, 0.01);
                while (cur > (money / count) * 2)
                {
                    System.Console.WriteLine("************" + cur + "***********");
                    cur = GetRandom(money - 0.01 * count, 0.01);
                }
                return cur;
            }
            return 0.01;
        }
        /// <summary>
        /// 获取随机数
        /// </summary>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static double GetRandom(double max, double min)
        {
            Thread.Sleep(300);
            Random random = new Random((int)DateTime.Now.Ticks);
            return Math.Round(random.NextDouble() * (max - min) + min, 2);
        }



        public static void ChangeOjb(ref Core_ChinapayOrder obj)
        {
            obj.A_billType = "ccccc";
        }

        private static void GetBag(double sumMoney, int bagNum)
        {
            double tempMoney = sumMoney;
            double[] bagMoney = new double[bagNum];
            double avg = sumMoney / bagNum;
            for (int i = 0; i < bagNum - 1; i++)
            {
                bagMoney[i] = GetMoney(tempMoney);
                tempMoney = tempMoney - bagMoney[i];
            }
            bagMoney[bagNum - 1] = tempMoney;
            for (int i = 0; i < bagMoney.Length; i++)
            {
                System.Console.WriteLine(String.Format("第{0}份红包为：{1}元", i + 1, bagMoney[i]));
            }
        }
        private static double GetMoney(double sumMoney)
        {

            int max = Convert.ToInt32(Math.Floor(sumMoney / 2)) * 100;
            string rs = "";
            if (max == 0)
            {
                rs = "0.01";
            }
            else
            {
                rs = (new Random().Next(1, max) * 0.01).ToString("F2");
            }

            //string rs = (.ToString("F2");

            return Convert.ToDouble(rs);
        }




        ////////////////
        private static void GetBag1(int sumMoney, int bagNum)
        {
            int tempMoney = sumMoney;
            int[] bagMoney = new int[bagNum];
            int avg = sumMoney / bagNum;
            for (int i = 0; i < bagNum - 1; i++)
            {
                bagMoney[i] = GetMoney1(tempMoney);
                tempMoney = tempMoney - bagMoney[i];
            }
            bagMoney[bagNum - 1] = tempMoney;
            for (int i = 0; i < bagMoney.Length; i++)
            {
                System.Console.WriteLine(String.Format("第{0}份红包为：{1}元", i + 1, bagMoney[i]));
            }
        }
        private static int GetMoney1(int sumMoney)
        {
            return new Random().Next(1, sumMoney / 2);
        }
    }


    /// <summary>
    /// 微信类
    /// </summary>
    public class WeiXin
    {
        /// <summary>
        /// 红包随机分配
        /// </summary>
        /// <param name="personNumber">红包个数</param>
        /// <param name="money">金额</param>
        /// <returns></returns>
        public static double[] Hongbao(int personNumber, double money)
        {
            Random rand = new Random();
            double fen = money;
            double[] hb = new double[personNumber];
            double rm = 0D;

            // 预分配
            for (int i = 0; i < personNumber; i++) hb[i] = 0.01;

            fen -= (personNumber * 0.01);

            if (fen > 0.01)
            {
                // 随机分配
                while (fen > 0)
                {
                    rm = GetRandomNumber(0.01, fen);
                    hb[rand.Next(0, personNumber)] += rm;
                    fen -= rm;
                }
            }

            return hb;
        }

        /// <summary>
        /// 返回介于minimum和maximum之间的随机数
        /// </summary>
        /// <param name="minimum">最小值</param>
        /// <param name="maximum">最大值</param>
        /// <returns></returns>
        public static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return Math.Round(random.NextDouble() * (maximum - minimum) + minimum, 2);
        }
    }


}
