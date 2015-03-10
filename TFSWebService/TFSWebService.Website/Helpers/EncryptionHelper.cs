#region License and Terms

// /***************************************************************************
// Copyright (c) 2015 Conplement AG
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  
// ***************************************************************************/

#endregion

using System.IO;
using System.Security.Cryptography;

namespace TFSWebService.Website.Helpers
{
    public interface IEncryptionHelper
    {
        byte[] Encrypt(byte[] plainText);
        byte[] Decrypt(byte[] cipher);
    }

    public class EncryptionHelper : IEncryptionHelper
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly RijndaelManaged _rijndaelManaged;

        public EncryptionHelper(byte[] key, byte[] iv)
        {
            _key = key;
            _iv = iv;
            _rijndaelManaged = new RijndaelManaged();
        }

        public byte[] Encrypt(byte[] plainText)
        {
            using (var memoryStream = new MemoryStream())
            {
                var encryptor = _rijndaelManaged.CreateEncryptor(_key, _iv);

                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainText, 0, plainText.Length);
                    cryptoStream.FlushFinalBlock();

                    return memoryStream.ToArray();
                }
            }
        }

        public byte[] Decrypt(byte[] cipher)
        {
            using (var memoryStream = new MemoryStream(cipher))
            {
                var decryptor = _rijndaelManaged.CreateDecryptor(_key, _iv);

                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        cryptoStream.CopyTo(resultStream);

                        return resultStream.ToArray();
                    }
                }
            }
        }
    }
}