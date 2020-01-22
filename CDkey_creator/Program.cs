using encrypt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CDkey_creator
{
    class Program
    {
        static String stringtable = "abcdefghijkmnpqrstuvwxyz23456789";
        static String password = "dak3le2";

        //从byte转为字符表索引所需要的位数
        static int convertByteCount = 5;
        static void Main(string[] args)
        {
            //ShowTime();
            //Console.WriteLine("=======================");
            //create((byte)1, 100, 12, password);

            //VerifyCode("c8dksqjamaba");
            //VerifyCode("4a36g5npamna");
            //VerifyCode("4a36g5naamna");
            //VerifyCode("dafe33234g435");
            //VerifyCode("ga8ehxsq6dja");

            //string ss= Console.ReadLine();
            //VerifyCode(ss);
            //ss = Console.ReadLine();

            //Tuple<string, string> t = GenerateRSAKeys();

            //string s = Encrypt(t.Item1, "timo");
            //Console.WriteLine(s);
            //s = Decrypt(t.Item2, s);
            //Console.WriteLine(s);










            string path_publicKey = $"{ Environment.CurrentDirectory}/publicKey.key";
            string path_privateKey = $"{ Environment.CurrentDirectory}/privateKey.key";

            string publicKey = File.ReadAllText(path_publicKey);
            string privateKey = File.ReadAllText(path_privateKey);

            //string s1 = Class1.Encrypt("Timo1234");
            //Console.WriteLine(s1);
            //s1 = Decrypt(privateKey, s1);



            string s = Encrypt(publicKey,Console.ReadLine());
            Console.WriteLine(s);
            s = Decrypt(privateKey, s);
            Console.WriteLine(s);

            Console.ReadLine();
        }

        /**
             * 生成兑换码
             * 这里每一次生成兑换码的最大数量为int的最大值即2147483647
             * @param time
             * @param id
             * @param count
             * @return
             */
        public static byte[] create(byte groupid, int codecount, int codelength, String password)
        {
            //8位的数据总长度
            int fullcodelength = codelength * convertByteCount / 8;
            //随机码对时间和id同时做异或处理
            //类型1，id4，随机码n,校验码1 
            int randcount = fullcodelength - 6;//随机码有多少个

            //如果随机码小于0 不生成
            if (randcount <= 0)
            {
                return null;
            }
            for (int i = 0; i < codecount; i++)
            {
                //这里使用i作为code的id
                //生成n位随机码
                byte[] randbytes = new byte[randcount];
                for (int j = 0; j < randcount; j++)
                {
                    randbytes[j] = (byte)(new Random().Next(1, 10) * Byte.MaxValue);
                }

                //存储所有数据
                ByteHapper byteHapper = ByteHapper.CreateBytes(fullcodelength);
                byteHapper.AppendNumber(groupid).AppendNumber(i).AppendBytes(randbytes);

                //计算校验码 这里使用所有数据相加的总和与byte.max 取余
                byte verify = (byte)(byteHapper.GetSum() % Byte.MaxValue);
                byteHapper.AppendNumber(verify);

                //使用随机码与时间和ID进行异或
                for (int j = 0; j < 5; j++)
                {
                    byteHapper.bytes[j] = (byte)(byteHapper.bytes[j] ^ (byteHapper.bytes[5 + j % randcount]));
                }

                //使用密码与所有数据进行异或来加密数据
                byte[] passwordbytes = System.Text.Encoding.ASCII.GetBytes(password);
                for (int j = 0; j < byteHapper.bytes.Length; j++)
                {
                    byteHapper.bytes[j] = (byte)(byteHapper.bytes[j] ^ passwordbytes[j % passwordbytes.Length]);
                }

                //这里存储最终的数据
                byte[] bytes = new byte[codelength];

                //按6位一组复制给最终数组
                for (int j = 0; j < byteHapper.bytes.Length; j++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        int sourceindex = j * 8 + k;
                        int targetindex_x = sourceindex / convertByteCount;
                        int targetindex_y = sourceindex % convertByteCount;
                        byte placeval = (byte)Math.Pow(2, k);
                        byte val = (byte)((byteHapper.bytes[j] & placeval) == placeval ? 1 : 0);
                        //复制每一个bit
                        bytes[targetindex_x] = (byte)(bytes[targetindex_x] | (val << targetindex_y));
                    }
                }

                StringBuilder result = new StringBuilder();
                //编辑最终数组生成字符串
                for (int j = 0; j < bytes.Length; j++)
                {
                    //result.append(stringtable.charAt(bytes[j]));
                    result.Append(stringtable.Substring(bytes[j], 1));
                }
                //System.out.println("out string : " + result.toString());
                Console.WriteLine(result.ToString());
            }
            ShowTime();
            return null;
        }

        /**
         * 验证兑换码
         * @param code
         */
        public static void VerifyCode(String code)
        {
            byte[] bytes = new byte[code.Length];

            //首先遍历字符串从字符表中获取相应的二进制数据
            for (int i = 0; i < code.Length; i++)
            {
                //byte index = (byte)stringtable.indexOf(code.charAt(i));
                //bytes[i] = index;
                bytes[i] = (byte)code[i];
            }

            //还原数组
            int fullcodelength = code.Length * convertByteCount / 8;
            int randcount = fullcodelength - 6;//随机码有多少个

            byte[] fullbytes = new byte[fullcodelength];
            for (int j = 0; j < fullbytes.Length; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    int sourceindex = j * 8 + k;
                    int targetindex_x = sourceindex / convertByteCount;
                    int targetindex_y = sourceindex % convertByteCount;

                    byte placeval = (byte)Math.Pow(2, targetindex_y);
                    byte val = (byte)((bytes[targetindex_x] & placeval) == placeval ? 1 : 0);

                    fullbytes[j] = (byte)(fullbytes[j] | (val << k));
                }
            }

            //解密，使用密码与所有数据进行异或来加密数据
            //byte[] passwordbytes = password.getBytes();
            byte[] passwordbytes = System.Text.Encoding.ASCII.GetBytes(password);
            for (int j = 0; j < fullbytes.Length; j++)
            {
                fullbytes[j] = (byte)(fullbytes[j] ^ passwordbytes[j % passwordbytes.Length]);
            }

            //使用随机码与时间和ID进行异或
            for (int j = 0; j < 5; j++)
            {
                fullbytes[j] = (byte)(fullbytes[j] ^ (fullbytes[5 + j % randcount]));
            }

            //获取校验码 计算除校验码位以外所有位的总和
            int sum = 0;
            for (int i = 0; i < fullbytes.Length - 1; i++)
            {
                sum += fullbytes[i];
            }
            byte verify = (byte)(sum % Byte.MaxValue);

            //校验
            if (verify == fullbytes[fullbytes.Length - 1])
            {
                Console.WriteLine(code + " : verify success!");
            }
            else
            {
                Console.WriteLine(code + " : verify failed!");
            }

        }



        public static void ShowTime()
        {
            Console.WriteLine(DateTime.Now);
        }






        /// <summary>
        /// 产生公钥及私钥
        /// </summary>
        /// <returns></returns>
        private static Tuple<string,string> GenerateRSAKeys()
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();

            var publicKey = rsa.ToXmlString(false);
            var privateKey = rsa.ToXmlString(true);

            string path_publicKey = $"{ Environment.CurrentDirectory}/publicKey.key";
            string path_privateKey = $"{ Environment.CurrentDirectory}/privateKey.key";
            //FileStream fs = File.OpenWrite(path_privateKey);
            //FileInfo fi = new FileInfo(path_privateKey);
            //fi.a

            StreamWriter sw = File.CreateText(path_privateKey);
            sw.Write(privateKey);
            sw.Close();

            sw = File.CreateText(path_publicKey);
            sw.Write(publicKey);
            sw.Close();

            return Tuple.Create(publicKey, privateKey);
        }


        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string Encrypt(string publicKey, string content)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);

            var encryptString = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(content), false));

            return encryptString;
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="encryptedContent"></param>
        /// <returns></returns>
        private static string Decrypt(string privateKey, string encryptedContent)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);

            var decryptString = Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(encryptedContent), false));

            return decryptString;
        }




    }
}

