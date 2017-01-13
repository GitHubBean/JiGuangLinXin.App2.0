using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace JiGuangLinXin.App.Provide.ImgHelper
{
    /// <summary>
    /// 验证码
    /// </summary>
   public static class ValidateCode
    {
        /// <summary>
        /// 產生圖形驗證碼。
        /// </summary>
        /// <param name="MemoryStream">記憶體資料流。</param>
        /// <param name="Code">傳出驗證碼。</param>
        /// <param name="CodeLength">驗證碼字元數。</param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="FontSize"></param>
        public static void CreateValidateCodeImage(MemoryStream MemoryStream, out String Code, int CodeLength, int Width, int Height, int FontSize)
        {
            Bitmap oBmp;
            oBmp = CreateValidateCodeImage(out Code, CodeLength, Width, Height, FontSize);
            try
            {
                oBmp.Save(MemoryStream, ImageFormat.Png);
            }
            finally { oBmp.Dispose(); }
        }
        /// <summary>
        /// 產生圖形驗證碼。
        /// </summary>
        /// <param name="Code">傳出驗證碼。</param>
        /// <param name="CodeLength">驗證碼字元數。</param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="FontSize"></param>
        /// <returns></returns>
        public static Bitmap CreateValidateCodeImage(out String Code, int CodeLength, int Width, int Height, int FontSize)
        {
            String sCode = String.Empty;
            //顏色列表，用於驗證碼、噪線、噪點
            Color[] oColors ={ 
             System.Drawing.Color.Black,
             System.Drawing.Color.Red,
             System.Drawing.Color.Blue,
             System.Drawing.Color.Green,
             System.Drawing.Color.Orange,
             System.Drawing.Color.Brown,
             System.Drawing.Color.Brown,
             System.Drawing.Color.DarkBlue
            };
            //字體列表，用於驗證碼
            string[] oFontNames = { "Times New Roman", "MS Mincho", "Book Antiqua", "Gungsuh", "PMingLiU", "Impact" };
            //驗證碼的字元集，去掉了一些容易混淆的字元
            char[] oCharacter = {
			'2',
			'3',
			'4',
			'5',
			'6',
			'8',
			'9',
			'A',
			'B',
			'C',
			'D',
			'E',
			'F',
			'G',
			'H',
			'J',
			'K',
			'L',
			'M',
			'N',
			'P',
			'R',
			'S',
			'T',
			'W',
			'X',
			'Y'
		};
            Random oRnd = new Random();
            Bitmap oBmp = null;
            Graphics oGraphics = null;
            int N1 = 0;
            System.Drawing.Point oPoint1 = default(System.Drawing.Point);
            System.Drawing.Point oPoint2 = default(System.Drawing.Point);
            string sFontName = null;
            Font oFont = null;
            Color oColor = default(Color);

            //生成驗證碼字串
            for (N1 = 0; N1 <= CodeLength - 1; N1++)
            {
                sCode += oCharacter[oRnd.Next(oCharacter.Length)];
            }

            oBmp = new Bitmap(Width, Height);
            oGraphics = Graphics.FromImage(oBmp);
            oGraphics.Clear(System.Drawing.Color.White);
            try
            {
                for (N1 = 0; N1 <= 4; N1++)
                {
                    //畫噪線
                    oPoint1.X = oRnd.Next(Width);
                    oPoint1.Y = oRnd.Next(Height);
                    oPoint2.X = oRnd.Next(Width);
                    oPoint2.Y = oRnd.Next(Height);
                    oColor = oColors[oRnd.Next(oColors.Length)];
                    oGraphics.DrawLine(new Pen(oColor), oPoint1, oPoint2);
                }

                for (N1 = 0; N1 <= sCode.Length - 1; N1++)
                {
                    //畫驗證碼字串
                    sFontName = oFontNames[oRnd.Next(oFontNames.Length)];
                    oFont = new Font(sFontName, FontSize, FontStyle.Italic);
                    oColor = oColors[oRnd.Next(oColors.Length)];
                    oGraphics.DrawString(sCode[N1].ToString(), oFont, new SolidBrush(oColor), Convert.ToSingle(N1) * FontSize + 10, Convert.ToSingle(8));
                }

                for (int i = 0; i <= 30; i++)
                {
                    //畫噪點
                    int x = oRnd.Next(oBmp.Width);
                    int y = oRnd.Next(oBmp.Height);
                    Color clr = oColors[oRnd.Next(oColors.Length)];
                    oBmp.SetPixel(x, y, clr);
                }

                Code = sCode;
                return oBmp;
            }
            finally
            {
                oGraphics.Dispose();
            }
        }
        ///  <summary>  
        ///  创建随机码图片  
        ///  </summary>  
        ///  <param  name="len">随机码</param>  
        public static Bitmap CreateImage(string randomcode)
        {

            int randAngle = 45;     //随机转动角度  
            int mapwidth = (int)(randomcode.Length * 16);
            Bitmap map = new Bitmap(mapwidth, 28);  //创建图片背景，设置其长宽  
            Graphics graph = Graphics.FromImage(map);
            graph.Clear(Color.AliceBlue);
            graph.DrawRectangle(new Pen(Color.Black, 0), 0, 0, map.Width - 1, map.Height - 1);//画一个边框  

            Random rand = new Random();

            // 生成背景噪点  
            Pen blackPen = new Pen(Color.LightGray, 0);
            for (int i = 0; i < 50; i++)
            {
                int x = rand.Next(0, map.Width);
                int y = rand.Next(0, map.Height);
                graph.DrawRectangle(blackPen, x, y, 1, 1);
            }

            //验证码旋转，防止机器识别  
            char[] chars = randomcode.ToCharArray();//拆散字符串成单字符数组  

            //文字距中  
            StringFormat format = new StringFormat(StringFormatFlags.NoClip);
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            // 定义随机颜色列表  
            Color[] c = { Color.Black, Color.Red, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };
            // 定义随机字体字体  
            string[] font = { "Verdana", "Microsoft Sans Serif", "Comic Sans MS", "Arial", "宋体" };

            for (int i = 0; i < chars.Length; i++)
            {
                int cindex = rand.Next(7);
                int findex = rand.Next(5);

                Font f = new System.Drawing.Font(font[findex], 16, System.Drawing.FontStyle.Bold); // 字体样式(参数2为字体大小)  
                Brush b = new System.Drawing.SolidBrush(c[cindex]);

                Point dot = new Point(11, 11);  // 括号内数值越大，字符间距越大  
                float angle = rand.Next(0, randAngle);  // 转动的度数，如果将0改为-randAngle，那么旋转角度为-45度～45度  

                graph.TranslateTransform(dot.X, dot.Y);
                graph.RotateTransform(angle);
                graph.DrawString(chars[i].ToString(), f, b, 2, 6, format); // 第4、5个参数控制左、上间距  
                graph.RotateTransform(-angle);
                graph.TranslateTransform(2, -dot.Y);
            }
            return map;
        }
        ///  <summary>  
        ///  生成随机码  
        ///  </summary>  
        ///  <param  name="length">随机码个数</param>  
        ///  <returns></returns>  
        public static string CreateRandomCode(int length)
        {
            int rand;
            char code;
            string randomcode = String.Empty;

            //生成一定长度的验证码  
            System.Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                rand = random.Next();

                if (rand % 3 == 0)
                {
                    code = (char)('A' + (char)(rand % 26));
                }
                else
                {
                    code = (char)('0' + (char)(rand % 10));
                }

                randomcode += code.ToString();
            }
            return randomcode;
        }
    }
}
