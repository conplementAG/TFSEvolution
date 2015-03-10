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

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TFSWebService.Website.Helpers
{
    public static class SerializationService
    {
        /// <summary>
        ///     Converts an object to a byte[]. Needed for serialization.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>The byte array.</returns>
        public static byte[] ConvertToByteArray<T>(T obj) where T : class
        {
            if (obj == null)
            {
                return null;
            }

            if (obj is string)
            {
                return Convert.FromBase64String(obj as string);
            }

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        ///     Converts a given byte array to a list of work ite field changes.
        /// </summary>
        /// <param name="arrBytes">The byte array.</param>
        /// <returns>Teh converted List of workitem field changes.</returns>
        public static T ConvertFromByteArray<T>(byte[] arrBytes) where T : class
        {
            if (typeof (T) == typeof (string))
            {
                return Convert.ToBase64String(arrBytes) as T;
            }
            T result;
            BinaryFormatter binForm = new BinaryFormatter();
            using (MemoryStream memStream = new MemoryStream())
            {
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                result = (T) binForm.Deserialize(memStream);
            }
            return result;
        }
    }
}