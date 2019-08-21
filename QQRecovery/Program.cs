using BaseFun;
using OpenCvSharp;
using OpenCvSharp.Util;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QQRecovery
{
    class Program
    {
        static IWebDriver selenium;
        static void Main(string[] args)
        {
            //test
           //List<double> _list =  ValidateCrack.getTrance2(400);

            

            while (true)
            {
                string qq_num = new Random().Next(100000000, 999999999).ToString();

                //打开网址，获取验证码
                ChromeOptions opt = new ChromeOptions();
                //隐藏
                //opt.AddArgument("--headless");
                //opt.AddArgument("--disable-gpu");
                if (selenium == null)
                {
                    selenium = new ChromeDriver(opt);
                }
                //测试网易
                /*
                selenium.Navigate().GoToUrl("https://reg.mail.163.com/unireg/call.do?cmd=register.entrance");
                selenium.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(100);
                Thread.Sleep(200);
                selenium.FindElement(By.ClassName("a2")).Click();
                Thread.Sleep(200);
                selenium.SwitchTo().Frame(selenium.FindElement(By.Id("regVipFrameId")));

                //selenium.FindElement(By.ClassName("a2")).Click();
                //Thread.Sleep(200);

                IWebElement ele_target = selenium.FindElement(By.ClassName("yidun_bg-img"));
                IWebElement ele_template = selenium.FindElement(By.ClassName("yidun_jigsaw"));
                string str_target = ele_target.GetAttribute("src");
                string str_template = ele_template.GetAttribute("src");
                */

                
                selenium.Navigate().GoToUrl("https://aq.qq.com/cn2/findpsw/mobile_v2/mobile_web_find_input_account?find_type=1&source_id=3268");
                selenium.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(100);
                selenium.FindElement(By.Id("input_find_qq")).SendKeys(qq_num);
                selenium.FindElement(By.XPath("//*[@id='account_input']/div[1]/button")).Click();

                //获取图片到本地
                selenium.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(2000);
                Thread.Sleep(2000);
                selenium.SwitchTo().Frame(selenium.FindElement(By.Id("tcaptcha_iframe")));
                IWebElement ele_target = selenium.FindElement(By.Id("slideBg"));
                string str_target = ele_target.GetAttribute("src");
                IWebElement ele_template = selenium.FindElement(By.Id("slideBlock"));
                string str_template = ele_template.GetAttribute("src");


                
                string file_name = Environment.CurrentDirectory + "/tmp/{0}" + DateTime.Now.ToString("yyyyMMddHHmmssfff")+ ".{1}";
                string target = string.Format(file_name, "target","jpg");
                string template = string.Format(file_name, "template", "png"); 
                string target_new =string.Format(file_name, "target_new", "jpg"); 
                WebClient client = new WebClient();
                client.DownloadFile(str_template, template);
                client.DownloadFile(str_target, target);
                //获取坐标

                Thread.Sleep(1000);

                //RunTemplateMatch(target, template);

                //RunTmlMatch(target, template);

                
                Mat target_rgb = Cv2.ImRead(target);

                Mat target_gray = target_rgb.CvtColor(ColorConversionCodes.BGR2GRAY);
                Mat template_rgb = Cv2.ImRead(template, 0);
                Mat res = target_gray.MatchTemplate(template_rgb, TemplateMatchModes.CCoeffNormed);
                double minValues, maxValues;
                Point minLocations, maxLocations;
                res.MinMaxLoc(out minValues, out maxValues, out minLocations, out maxLocations);
                string msg = $"min:{minValues} max:{maxValues}; minx:{minLocations.X}, miny:{minLocations.Y}; maxx:{maxLocations.X},maxy:{maxLocations.Y}";
                WriteLine(msg);

                Cv2.Rectangle(target_rgb, minLocations, new Point(minLocations.X + template_rgb.Cols, minLocations.Y + template_rgb.Rows), Scalar.Red, 2);
                target_rgb.SaveImage(target_new);
                //Cv2.ImShow(target, target_rgb);
                //Cv2.WaitKey(10);
                File.Delete(target);
                File.Delete(template);


                //拖动
                //比例
                double per_target = (double)ele_target.Size.Width / (double)target_rgb.Width;
                double per_template = (double)ele_template.Size.Width / (double)template_rgb.Width;

                //int distance = Convert.ToInt16(minLocations.X * per_target - 30+11);//不准

                //int distance = Convert.ToInt16(( minLocations.X + 0.5 * template_rgb.Width) * per);//不准

                int distance = Convert.ToInt16((minLocations.X - 0.5 * template_rgb.Width + 13) * per_target);//很接近了

                //int distance = Convert.ToInt16(minLocations.X * per_target - (0.5 * template_rgb.Width + 13) * per_template );//不准

                //ValidateCrack.autoDrag(selenium, distance);



                //var element = selenium.FindElement(By.Id("tcaptcha_drag_button"));//按钮
                var element = selenium.FindElement(By.ClassName("yidun_slider"));//按钮
                 
                Actions actions = new Actions(selenium);

                //actions.DragAndDropToOffset(element, distance, 0).Build().Perform();//直接移动过去

                actions.ClickAndHold(element).Perform();

                List<int> list = ValidateCrack.getTrance2(distance);
                int c = 0;
                foreach (var item in list)
                {
                    c++;
                    Debug.WriteLine($"{c}:{item}");
                    actions.MoveByOffset(item, 0);
                }
                //actions.Perform();
                actions.Release(element).Perform();
                //是否成功 没有成功 刷新一下，继续识别 成功后继续下一下操作


                



                Thread.Sleep(1000);
            }
            //Console.ReadLine();
        }


        private static void WriteLine(string v)
        {
            //throw new NotImplementedException();
            appSittingSet.Log(v);
            Console.WriteLine(v);
        }

    }
}
