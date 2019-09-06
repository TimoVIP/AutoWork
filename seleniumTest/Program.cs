using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading;

namespace seleniumTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ChromeDriver selenium = new ChromeDriver();
            //selenium.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(200);
            //selenium.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromMilliseconds(200);

            selenium.Navigate().GoToUrl("https://www.baidu.com/");
            Console.WriteLine(selenium.PageSource);
            Thread.Sleep(1000);

            selenium.Navigate().GoToUrl("http://home.sina.com/");            
            Console.WriteLine(selenium.PageSource);
            Thread.Sleep(1000);

            Console.ReadLine();
        }
    }
}
