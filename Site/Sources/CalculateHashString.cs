using System;
using System.Security.Cryptography;
using System.Text;


namespace Site.Sources
{
    public class CalculateHashString
    {
        public string Calculate(string value)
        {
            byte[] tmpSource;
            byte[] hash;

            // создание байтового массива
            tmpSource = ASCIIEncoding.ASCII.GetBytes(value);

            // вычисление хеш
            hash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);

            return ByteArrayToString(hash);
        }

        private static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new(arrInput.Length);
            for (i = 0; i < arrInput.Length - 1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
    }
}