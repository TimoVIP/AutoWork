using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QQRecovery
{
    public static class ValidateCrack
    {
        private static string urlbase { get; set; }
        private static string qq { get; set; }
        private static string mobile { get; set; }
        private static string code { get; set; }

        private static IWebDriver selenium { get; set; }

        public static void autoDrag(IWebDriver drive, int distance)
        {
            drive.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(1000);
            Thread.Sleep(2000);

            drive.SwitchTo().Frame(drive.FindElement(By.Id("tcaptcha_iframe")));

            var element = drive.FindElement(By.Id("tcaptcha_drag_button"));


            // 这里就是根据移动进行调试，计算出来的位置不是百分百正确的，加上一点偏移
            // distance -= element.size.get('width') / 2
            distance += 13;
           var  has_gone_dist = 0;
           var remaining_dist = distance;
            //distance += randint(-10, 10);
            var ratio = 0;
            var span = 0;
            // 按下鼠标左键
            Actions actions = new Actions(drive);
            actions.ClickAndHold(element).Perform();
            Thread.Sleep(500);
            while (remaining_dist > 0)
            {
                ratio = remaining_dist / distance;
                if (ratio < 0.2 || ratio > 0.8)
                {
                    span = new Random().Next(5, 8);
                }
                else
                {
                    span = new Random().Next(10, 16);
                }
                actions.MoveByOffset(span, new Random().Next(-5, 5)).Perform();
                remaining_dist -= span;
                has_gone_dist += span;
            }

            Thread.Sleep(new Random().Next(50, 200));
            actions.MoveByOffset(remaining_dist, new Random().Next(-5, 5)).Perform();
            actions.Release(element).Perform();
        }
        /*
        public static void generateTracks(int S,out )
        {
            S += 20;
            v = 0;
            t = 0.2;
            forward_tracks = [];
            current = 0;
            mid = S * 3 / 5; // 减速阀值
    while current < S:
        if current < mid:
            a = 2  # 加速度为+2
        else:
            a = -3  # 加速度-3
        s = v * t + 0.5 * a * (t * *2)
        v = v + a * t
        current += s
        forward_tracks.append(round(s))


    back_tracks = [-3, -3, -2, -2, -2, -2, -2, -1, -1, -1]
    return { 'forward_tracks': forward_tracks, 'back_tracks': back_tracks}

        }

    */

        public static List<double> getTrance(int distance)
        {
            List<double> trace = new List<double>();
            var faster_distance =(double) distance * 7 / 8;
            // 设置初始位置、初始速度、时间间隔
            double start = 0;
            double v0 = 0;
            double t = 0.2;
            double a;
            double move = 0;
            double v;
            // 当尚未移动到终点时
            while (start < distance)
            {
                if (start < faster_distance)
                {
                    // 设置加速度为2
                    a = 1.5;
                }
                else
                {
                    a = -3;
                }
                //移动的距离公式
                move = v0 * t + 1 / 2 * a * t * t;
                //此刻速度
                v = v0 + a * t;
                //重置初速度
                v0 = v;
                //重置起点
                start += move;
                //将移动的距离加入轨迹列表
                trace.Add(Math.Round(move));
            }
            return trace;
        }

        public static List<int> getTrance2(int distance)
        {
            List<int> trace = new List<int>();
            /*
            double current = 0;
            double mid = distance * 7 / 8;
            double t =new Random().Next(2,3) / 10;
            double a = 0;
            double v = 0;
            double v0 =0;
            while (current< distance)
            {
                if (current < mid)
                    a = 2;
                else
                    a = -3;
                v0 = v;
                v = v0 + a * t;
                current = v0 * t + 1 / 2 * a * t * t;
                trace.Add(current);
            }

            */

            distance += 15;
            double v = 0;
            double t = 0.5;
            double current = 0;
            double mid = distance * 0.6;
            double a = 0;
            while (current < distance)
            {
                if (current < mid)
                    a = 2;
                else
                    a = -3;
                double s = v * t + 0.5 * a * t * t;
                v = v + a * t;
                current += s;
                trace.Add((int)Math.Round(s));
            }

            trace.AddRange(new [] {  -3, -2, -2, -2, -2, -1, -1, -1 });
            return trace;
            /*
            var faster_distance =(double) distance * 7 / 8;
            // 设置初始位置、初始速度、时间间隔
            double start = 0;
            double v0 = 0;
            double t = 0.2;
            double a;
            double move = 0;
            double v;
            // 当尚未移动到终点时
            while (start < distance)
            {
                if (start < faster_distance)
                {
                    // 设置加速度为2
                    a = 1.5;
                }
                else
                {
                    a = -3;
                }
                //移动的距离公式
                move = v0 * t + 1 / 2 * a * t * t;
                //此刻速度
                v = v0 + a * t;
                //重置初速度
                v0 = v;
                //重置起点
                start += move;
                //将移动的距离加入轨迹列表
                trace.Add(Math.Round(move));
            }
            return trace;
            */
        }
    }
}
