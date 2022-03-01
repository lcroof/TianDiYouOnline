using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataPublic
{
    /// <summary>
    /// 加密解密类
    /// 通过AES加密内容后使用RSA进行加密解密AES加密内容，再通过AES密钥解密原文
    /// 发送方只获取公钥，加密方法写在接受方
    /// </summary>
    public static class CryptoDecrypt
    {
        private static string rsaPublicKey;
        private static string rsaPrivateKey;
        private static RSACryptoServiceProvider rsa;
        private static AesCryptoServiceProvider aes;
        private static int keySize = 1024;
        private static byte[] splitChar = DataPublics.GetByteByInt(256);
        private static string splitString = Encoding.UTF8.GetString(splitChar);

        /// <summary>
        /// RSA公钥
        /// </summary>
        public static string RsaPublicKey
        {
            get
            {
                if (rsaPublicKey.IsNullOrEmpty())
                {
                    rsaPublicKey = @"<RSAKeyValue><Modulus>6+VpSJkT+J5k5xPU0u49zV4av8J7FRS+DTYWP5zIY9WXxqZ5Ksx26resx5n3L3IXaAmtJ/e3zJm/TpYoIa0tB04zWOJvzdT+ZYvu5pIoz/JHeFY9lrC5sOJlp8dPMf/Gr1mzk/vL49sLqsLkJ6POFFNnX/z8kjPAVIX3fTvj3iE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
                }
                return rsaPublicKey;
            }
            set
            {
                rsaPublicKey = value;
            }
        }

        /// <summary>
        /// 加密解密
        /// </summary>
        public static void Initialize()
        {
            rsa = new RSACryptoServiceProvider(keySize);
            aes = new AesCryptoServiceProvider();

            rsaPublicKey = rsa.ToXmlString(false);
            rsaPrivateKey = rsa.ToXmlString(true);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
        }

        /// <summary>
        /// 获取加密RSA公钥
        /// </summary>
        /// <returns></returns>
        public static string RSAEncryptRSAPublicKey()
        {
            rsa.FromXmlString(@"<RSAKeyValue><Modulus>6+VpSJkT+J5k5xPU0u49zV4av8J7FRS+DTYWP5zIY9WXxqZ5Ksx26resx5n3L3IXaAmtJ/e3zJm/TpYoIa0tB04zWOJvzdT+ZYvu5pIoz/JHeFY9lrC5sOJlp8dPMf/Gr1mzk/vL49sLqsLkJ6POFFNnX/z8kjPAVIX3fTvj3iE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
            byte[] originTextArray = Convert.FromBase64String(rsaPublicKey);
            byte[] encodeTextArray = rsa.Encrypt(originTextArray, RSAEncryptionPadding.OaepSHA1);
            string resultString = Convert.ToBase64String(encodeTextArray);

            return resultString;
        }

        /// <summary>
        /// RSA加密方法
        /// </summary>
        /// <param name="encryptString">AES加密后的密文</param>
        /// <param name="isFromClient">是否从客户端发送</param>
        public static string RSAEncrypt(string encryptString, bool isFromClient)
        {
            string publicKey = rsaPublicKey;
            int bufferSize = (rsa.KeySize / 8 - 42);
            byte[] buffer = new byte[bufferSize];//待加密块
            byte[] originTextArray = Convert.FromBase64String(encryptString);
            byte[] resultArray = new byte[0];
            List<byte[]> encodeTextArray = new List<byte[]>();

            if (!isFromClient)
            {
                publicKey = @"<RSAKeyValue><Modulus>6+VpSJkT+J5k5xPU0u49zV4av8J7FRS+DTYWP5zIY9WXxqZ5Ksx26resx5n3L3IXaAmtJ/e3zJm/TpYoIa0tB04zWOJvzdT+ZYvu5pIoz/JHeFY9lrC5sOJlp8dPMf/Gr1mzk/vL49sLqsLkJ6POFFNnX/z8kjPAVIX3fTvj3iE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            }

            rsa.FromXmlString(publicKey);


            using (MemoryStream msInput = new MemoryStream(originTextArray))
            {
                using (MemoryStream msOutput = new MemoryStream())
                {
                    int readLen;
                    while ((readLen = msInput.Read(buffer, 0, bufferSize)) > 0)
                    {

                        byte[] dataToEnc = new byte[readLen];
                        Array.Copy(buffer, 0, dataToEnc, 0, readLen);

                        //byte[] encData = rsa.Encrypt(dataToEnc, false);
                        encodeTextArray.Add(rsa.Encrypt(dataToEnc, RSAEncryptionPadding.OaepSHA1));
                    }
                }
            }

            foreach (byte[] data in encodeTextArray)
            {
                resultArray = resultArray.Concat(data).ToArray();
            }

            return Convert.ToBase64String(resultArray);
        }

        /// <summary>
        /// RSA解密方法
        /// </summary>
        /// <param name="decryptString">加密后的密文</param>
        /// <param name="isFromClient">是否从客户端发送</param>
        /// <returns></returns>
        public static string RSADecrypt(string decryptString, bool isFromClient)
        {
            string privateKey = rsaPrivateKey;
            string resultString = String.Empty;
            byte[] resultArray = new byte[0];
            byte[] encodeTextArray = Convert.FromBase64String(decryptString);
            int bufferSize = (rsa.KeySize / 8);
            byte[] buffer = new byte[bufferSize];//待加密块

            List<byte[]> decodeTextArray = new List<byte[]>();

            if (!isFromClient)
            {
                privateKey = @"<RSAKeyValue><Modulus>6+VpSJkT+J5k5xPU0u49zV4av8J7FRS+DTYWP5zIY9WXxqZ5Ksx26resx5n3L3IXaAmtJ/e3zJm/TpYoIa0tB04zWOJvzdT+ZYvu5pIoz/JHeFY9lrC5sOJlp8dPMf/Gr1mzk/vL49sLqsLkJ6POFFNnX/z8kjPAVIX3fTvj3iE=</Modulus><Exponent>AQAB</Exponent><P>78eIuJDyBv3K4ozAMXQmZ1c//sy5CRkR9ryQSCFfOQnUgypCavMkKx8OkYoz+EJLD/hdb32bQ/Wk353jlshIPw==</P><Q>+9qgfGHlmzaKHpoBsESjK9Wo60dbTnkAVI+o6PP/r513rZd9cSmV6vIOi3rswloDDbJecjbLIZIAeu2SExlBnw==</Q><DP>VDVXJrqAxUPMRNP5i2SnOBPEPc/YDAHp9SUz5qERuA5ju8zTlovdX+eATkyjA8UEZ49qAQWzyaIzTPU+QG4GMQ==</DP><DQ>a/Wax9a7Mt6dNGL2CFZTz82F3F05MZlC3/y4/irh1qjcjWgb2K9n2U+rlnS6GtG072EfhTwhtSE93XQLIu63pw==</DQ><InverseQ>KrGHwtwrnalyphp2swJDXqWSLl/AjdqqKZabaHCJfMJkE3jLSLJu6oh7yK5KPkL5c78Z24q/6YbR5T/GMM0pWw==</InverseQ><D>6kpxNiSrvDt9VjDRKSpsgYOcp6Z+XqK6XB7DHC0YrmdfFnMTVxm6ZkdFOP3Hjv0PGEq8Cz0y9OgjJOdoNoJse7O5JZR2z4QEQ0fDnwUKTpm4pIg19fucqRvDq51TE0Ac8XyiKx/I9oBopbT//ToxMH8AvDFIjhGFJR3IbwBqhc0=</D></RSAKeyValue>";
            }

            rsa.FromXmlString(privateKey);

            using (MemoryStream msInput = new MemoryStream(encodeTextArray))
            {
                using (MemoryStream msOutput = new MemoryStream())
                {
                    int readLen;
                    while ((readLen = msInput.Read(buffer, 0, bufferSize)) > 0)
                    {

                        byte[] dataToEnc = new byte[readLen];
                        Array.Copy(buffer, 0, dataToEnc, 0, readLen);

                        //byte[] encData = rsa.Encrypt(dataToEnc, false);
                        decodeTextArray.Add(rsa.Decrypt(dataToEnc, RSAEncryptionPadding.OaepSHA1));
                    }
                }
            }

            foreach (byte[] data in decodeTextArray)
            {
                resultArray = resultArray.Concat(data).ToArray();
            }

            bool needSplit = false;
            for (int i = resultArray.Length - 1; i >= resultArray.Length - 35; i--)
            {
                if (resultArray[i] == 255)
                {
                    if (!resultString.IsNullOrEmpty())
                    {
                        resultString += splitString;
                    }
                    resultString += Convert.ToBase64String(resultArray.Skip(i + 1).Take(16).ToArray());
                    needSplit = true;
                }
                if (i == resultArray.Length - 35 && needSplit)
                {
                    return resultString += splitString + Convert.ToBase64String(resultArray.Take(resultArray.Length - 34).ToArray());
                }
            }


            resultString = Convert.ToBase64String(resultArray);

            return resultString;
        }

        /// <summary>
        /// AES解密方法
        /// </summary>
        /// <param name="decryptString">AES加密后的密文</param>
        /// <param name="isFromClient">是否从客户端发送</param>
        /// <returns></returns>
        public static string AESDecrypt(string decryptString, bool isFromClient)
        {
            string actualString = string.Empty;

            //先获取AES密钥，RSA方面需要进行解码
            string[] str = RSADecrypt(decryptString, isFromClient).Split(splitString);
            aes.Key = Convert.FromBase64String(str[1]);
            aes.IV = Convert.FromBase64String(str[0]);

            for (int i = 2; i < str.Length; i++)
            {
                actualString += str[i];
            }
            
            byte[] encodeTextArray = Convert.FromBase64String(actualString);

            byte[] originTextArray = aes.CreateDecryptor().TransformFinalBlock(encodeTextArray, 0, encodeTextArray.Length);
            string resultString = Convert.ToBase64String(originTextArray);

            return resultString;
        }

        /// <summary>
        /// AES加密方法
        /// </summary>
        /// <param name="encryptString">原文</param>
        /// <param name="isFromClient">是否从客户端发送</param>
        /// <returns></returns>
        public static string AESEncrypt(string encryptString, bool isFromClient)
        {
            //获取AES加密偏移量和密码
            aes.GenerateIV();
            aes.GenerateKey();

            byte[] originTextArray = Convert.FromBase64String(encryptString);
            byte[] actual = aes.CreateEncryptor().TransformFinalBlock(originTextArray, 0, originTextArray.Length).Concat(splitChar).Concat(aes.Key).Concat(splitChar).Concat(aes.IV).ToArray();
            //使用RSA加密后返回
            string resultString = RSAEncrypt(Convert.ToBase64String(actual), isFromClient);

            return resultString;
        }
    }
}
