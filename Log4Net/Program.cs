using log4net;
using log4net.Config;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4Net
{
    class Program
    {
        static void Main(string[] args)
        {

            //ILoggerRepository repository = LogManager.CreateRepository("NETCoreRepository");
            //XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            //ILog log = LogManager.GetLogger(repository.Name, "NETCorelog4net");

            //log.Info("NETCorelog4net log");
            //log.Info("test log");
            //log.Error("error");
            //log.Info("linezero");
            //Console.ReadKey();

            ILog log = log4net.LogManager.GetLogger("Test");
            log.Error("错误", new Exception("发生了一个异常"));//错误
            log.Fatal("严重错误", new Exception("发生了一个致命错误"));//严重错误
            log.Info("信息"); //记录一般信息
            log.Debug("调试信息");//记录调试信息
            log.Warn("警告");//记录警告信息
            Console.WriteLine("日志记录完毕。");
            Console.Read();
        }
    }
}
