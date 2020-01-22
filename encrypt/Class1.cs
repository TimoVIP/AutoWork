using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace encrypt
{
    public static class Class1
    {
        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Encrypt(string publicKey, string content)
        {
            if (publicKey.Length==0)
            {
                foreach (byte b in Properties.Resources.publicKey)
                {
                    publicKey += (char)b;
                }
                //publicKey = System.Text.Encoding.Default.GetBytes(Properties.Resources.publicKey);
                
            }
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);

            var encryptString = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(content), false));

            return encryptString;
        }

        public static string Encrypt(string content)
        {
            return Encrypt("", content);
        }
    }
}
