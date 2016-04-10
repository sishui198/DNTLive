﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;

namespace 剪贴板
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 获取文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTxt_Click(object sender, EventArgs e)
        {
            var dataStr = Clipboard.GetText();
            if (!string.IsNullOrEmpty(dataStr))
            {
                CreateDirectory("Text");
                string name = string.Format(@"Text\{0}.txt", GetNewName());
                File.WriteAllText(name, dataStr, Encoding.UTF8);
                MessageBox.Show(string.Format("操作成功，请看Text文件夹！", "逆天友情提醒"));
                OpenDirectory();
            }
            else
            {
                MessageBox.Show("剪贴板文本内容为空！", "逆天友情提醒");
            }
        }

        /// <summary>
        /// 剪贴板写入处理过的内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPage_Click(object sender, EventArgs e)
        {
            var dataStr = GetHtmlStr();
            if (!string.IsNullOrEmpty(dataStr))
            {
                MessageBox.Show("操作成功，请看打开的页面！", "逆天友情提醒");
                OutputHtml(dataStr);
            }
            else
            {
                MessageBox.Show("剪贴板图文内容为空！", "逆天友情提醒");
            }
        }

        /// <summary>
        /// 读取剪贴板并保存剪贴板内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImg_Click(object sender, EventArgs e)
        {
            var dataStr = GetHtmlStr();
            var imgObj = GetImgObj();
            if (!string.IsNullOrEmpty(dataStr))
            {
                int i = 0;
                MessageBox.Show(string.Format("成功提取{0}个图片", DownloadImg(dataStr, i)), "逆天友情提醒");
            }
            else if (imgObj != null)//非HTML的单张图片
            {
                CreateDirectory("Images");
                imgObj.Save(string.Format(@"Images\{0}.png", GetNewName()), ImageFormat.Png);
                MessageBox.Show("操作成功，请看Images文件夹！", "逆天友情提醒");
                OpenDirectory();
            }
            else
            {
                MessageBox.Show("剪贴板图片信息为空！", "逆天友情提醒");
            }
        }

        /// <summary>
        /// 批量下载图片
        /// </summary>
        /// <param name="dataStr">页面字符串</param>
        /// <param name="i">成功条数</param>
        /// <returns></returns>
        private static int DownloadImg(string dataStr, int i)
        {
            var collection = Regex.Matches(dataStr, @"<img([^>]*)\s*src=('|\"")([^'\""]+)('|\"")", RegexOptions.ECMAScript);
            WebClient webClient = new WebClient();
            foreach (Match item in collection)
            {
                string imgPath = item.Groups[3].Value;
                try
                {
                    CreateDirectory("Images");
                    webClient.DownloadFile(imgPath, string.Format(@"Images\{0}.png", Path.GetFileName(imgPath)));//剪贴板的图片没有相对路径
                    i++;
                }
                catch (Exception ex) { File.WriteAllText("log.dnt", ex.ToString(), Encoding.UTF8); }
            }
            return i;
        }

        /// <summary>
        /// 清除剪贴板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbl1_Click(object sender, EventArgs e)
        {
            ClearClipboard();
        }

        #region 公用方法
        /// <summary>
        /// 非HTML的单张图片
        /// </summary>
        /// <returns></returns>
        private static Bitmap GetImgObj()
        {
            var data = Clipboard.GetDataObject();
            if (data.GetDataPresent(DataFormats.Bitmap, true))
            {
                return data.GetData(DataFormats.Bitmap, true) as Bitmap;
            }
            return null;
        }

        /// <summary>
        /// HTML字符串
        /// </summary>
        /// <returns></returns>
        private static string GetHtmlStr()
        {
            var data = Clipboard.GetDataObject();
            if (data.GetDataPresent(DataFormats.Html, true))
            {
                return data.GetData(DataFormats.Html, true).ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 输出HTML文件
        /// </summary>
        /// <param name="dataStr"></param>
        private static void OutputHtml(string dataStr)
        {
            CreateDirectory("Page");
            string name = string.Format(@"Page\{0}.html", GetNewName());
            File.WriteAllText(name, dataStr.Substring(dataStr.IndexOf("<html")), Encoding.UTF8);//除去版权信息
            Process.Start(name);
        }

        /// <summary>
        /// 打开目录
        /// </summary>
        private static void OpenDirectory()
        {
            Process.Start("explorer.exe ", Directory.GetCurrentDirectory());//打开目录
        }

        /// <summary>
        /// 生成新名称-就不用 Guid 了，普通用户看见了会怕
        /// </summary>
        /// <returns></returns>
        private static string GetNewName()
        {
            return DateTime.Now.ToString("yyyy-MM-dd~HH.mm.ss.fff");
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        private static void CreateDirectory(string name)
        {
            if (!Directory.Exists(name))
            {
                Directory.CreateDirectory(name);
            }
        }

        /// <summary>
        /// 清除剪贴板
        /// </summary>
        private void ClearClipboard()
        {
            Clipboard.Clear();
        }

        #endregion
    }
}
