using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace JiGuangLinXin.App.Provide.ImgHelper
{
    public static class ImgZoomHelper
    {
      /// <summary>
      /// 通用图片处理方法(水印)
      /// </summary>
        /// <param name="fromUrl">图片来源绝对地址（不能和保存地址相同）</param>
        /// <param name="saveUrl">图片保存绝对地址（不能和来源地址相同）</param>
      /// <param name="width">目标图片宽最大值</param>
        /// <param name="height">目标图片高最大值</param>
      /// <param name="wText">图片水印文字</param>
      /// <param name="wUrl">图片水印图片绝对地址</param>
      public static void MakeThumbnail(string fromUrl, string saveUrl, Double width, Double height, string wText, string wUrl)
      {
          //从文件取得图片对象，并使用流中嵌入的颜色管理信息
          Image image = Image.FromFile(fromUrl);
          //缩略图宽、高
          Double Width = image.Width, Height = image.Height;
          //宽大于模版的横图
          if (image.Width > image.Height || image.Width == image.Height)
          {
              if (image.Width > width)
              {
                  //宽按模版，高按比例缩放
                  Width = width;
                  Height = image.Height * (Width / image.Width);
              }
          }
          //高大于模版的竖图
          else
          {
              if (image.Height > height)
              {
                  //高按模版，宽按比例缩放
                  Height = height;
                  Width = image.Width * (Height / image.Height);
              }
          }
          //取得图片大小
          Size size = new Size((int)Width, (int)Height);
          //新建一个bmp图片
          Image bitmap = new Bitmap(size.Width, size.Height);
          //新建一个画板
          Graphics g = Graphics.FromImage(bitmap);
          //设置高质量插值法
          g.InterpolationMode = InterpolationMode.High;
          //设置高质量,低速度呈现平滑程度
          g.SmoothingMode = SmoothingMode.HighQuality;
          //清空一下画布
          g.Clear(Color.White);
          //在指定位置画图
          g.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height),
          new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
          ///文字水印
          //Graphics G = Graphics.FromImage(bitmap);
          //Font f = new Font("Microsoft Yahei", 10);
          //Brush b = new SolidBrush(Color.Black);
          //G.DrawString("www.heejaa.com", f, b, 10, 10);
          //G.Dispose();
          ///图片水印
          //Image WaterImage = System.Drawing.Image.FromFile(System.Web.HttpContext.Current.Server.MapPath("pic/1.gif"));
          //Graphics wi = Graphics.FromImage(bitmap);
          //wi.DrawImage(WaterImage, new Rectangle(bitmap.Width - WaterImage.Width, bitmap.Height - WaterImage.Height, WaterImage.Width, WaterImage.Height), 0, 0, WaterImage.Width, WaterImage.Height, GraphicsUnit.Pixel);
          //WaterImage.Dispose();
          //wi.Dispose();
          //WaterImage.Dispose();
          //保存缩略图
          bitmap.Save(saveUrl, ImageFormat.Jpeg);
          g.Dispose();
          image.Dispose();
          bitmap.Dispose();
      }


      /// <summary>
      /// 缩放图像
      /// </summary>
      /// <param name="originalImagePath">图片原始路径</param>
      /// <param name="thumNailPath">保存路径</param>
      /// <param name="width">缩放图的宽</param>
      /// <param name="height">缩放图的高</param>
      /// <param name="model">缩放模式</param>
      public static void ImgZoom(string originalImagePath, string thumNailPath, int width, int height, string model = "W")
      {
          System.Drawing.Image originalImage = System.Drawing.Image.FromFile(originalImagePath);

          int thumWidth = width;      //缩略图的宽度
          int thumHeight = height;    //缩略图的高度

          int x = 0;
          int y = 0;

          int originalWidth = originalImage.Width;    //原始图片的宽度
          int originalHeight = originalImage.Height;  //原始图片的高度

          switch (model)
          {
              case "HW":      //指定高宽缩放,可能变形
                  thumHeight = height;
                  thumWidth = width;
                  break;
              case "W":       //指定宽度,高度按照比例缩放
                  thumHeight = originalImage.Height * width / originalImage.Width;
                  break;
              case "H":       //指定高度,宽度按照等比例缩放
                  thumWidth = originalImage.Width * height / originalImage.Height;
                  break;
              case "Cut":
                  if ((double)originalImage.Width / (double)originalImage.Height > (double)thumWidth / (double)thumHeight)
                  {
                      originalHeight = originalImage.Height;
                      originalWidth = originalImage.Height * thumWidth / thumHeight;
                      y = 0;
                      x = (originalImage.Width - originalWidth) / 2;
                  }
                  else
                  {
                      originalWidth = originalImage.Width;
                      originalHeight = originalWidth * height / thumWidth;
                      x = 0;
                      y = (originalImage.Height - originalHeight) / 2;
                  }
                  break;
              default:
                  break;
          }

          //新建一个bmp图片
          System.Drawing.Image bitmap = new System.Drawing.Bitmap(thumWidth, thumHeight);

          //新建一个画板
          System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap);

          //设置高质量查值法
          graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

          //设置高质量，低速度呈现平滑程度
          graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

          //清空画布并以透明背景色填充
          graphic.Clear(System.Drawing.Color.Transparent);

          //在指定位置并且按指定大小绘制原图片的指定部分
          graphic.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, thumWidth, thumHeight), new System.Drawing.Rectangle(x, y, originalWidth, originalHeight), System.Drawing.GraphicsUnit.Pixel);

          try
          {
              bitmap.Save(thumNailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
          }
          catch (Exception ex)
          {
              throw ex;
          }
          finally
          {
              originalImage.Dispose();
              bitmap.Dispose();
              graphic.Dispose();
          }
      }


    }
}
