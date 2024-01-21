using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebsiteBlogCMS.Classes
{
    public static class CryptHelper
    {
        static byte[] bytes = ASCIIEncoding.ASCII.GetBytes("BlogASCI");

        public static string Encrypt(string toEncrypt)
        {
            if(string.IsNullOrEmpty(toEncrypt))
            {
                throw new Exception("The string which need to be encrypted cannot be null.");
            }

            using(DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            using(MemoryStream ms = new MemoryStream())
            using(CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write))
            {
                using(StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(toEncrypt);
                }
                byte[] encrypted = ms.ToArray();
                return Convert.ToBase64String(encrypted);
            }
        }

        public static string Decrypt(string toDecrypt)
        {
            if (string.IsNullOrEmpty(toDecrypt))
            {
                throw new Exception("The string which need to be decrypted cannot be null.");
            }

            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(toDecrypt)))
            using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}