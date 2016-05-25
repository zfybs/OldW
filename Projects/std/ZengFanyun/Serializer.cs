using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace std_ez
{
    /// <summary>
    /// 将任意一个有 Serializable标记的类以二进制转换器将类中所有数据与字符串间的相互序列化。
    /// 即可以将类中的数据（包括数组）序列化为字符，还可以将序列化的字符反序列化为一个类。
    /// </summary>
    public class StringSerializer
    {
        /// <summary>
        /// Encode arbitrary .NET serialisable object
        /// into binary data encodes as base64 string.
        /// </summary>
        public static string Encode64(object obj)
        {
            // serialize into binary stream
            BinaryFormatter f = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            f.Serialize(stream, obj);
            stream.Position = 0;

            // 将二进制数据编码为base64的字符串
            int n = (int) stream.Length;
            byte[] buf = new byte[n - 1 + 1];
            stream.Read(buf, 0, n);
            // 如果想将二进制字节数组转直接换成字符串，可以使用具有8位编码的字符集转换，但不能使用其它字符集，比如Unicode、GB2312.
            return Convert.ToBase64String(buf);
        }

        /// <summary>
        /// Decode arbitrary .NET serialisable object
        /// from binary data encoded as base64 string.
        /// </summary>
        public static dynamic Decode64(string s64)
        {
            // decode string back to binary data:
            MemoryStream s = new MemoryStream(Convert.FromBase64String(s64));
            s.Position = 0;

            // deserialize:
            BinaryFormatter f = new BinaryFormatter();
            //f.AssemblyFormat = FormatterAssemblyStyle.Simple;
            // add this line below to avoid the "unable to find assembly" issue:
            f.Binder = new ZengfyLinkBinder();
            return f.Deserialize(s);
        }

        /// <summary>
        /// 为了解决SerializationException，方法之一是确保此assembly放置在与acad.exe 或 revit.exe相同的文件夹中，
        /// 另一个方法就是实现一个像这样的类。
        /// </summary>
        /// <remarks>
        ///  Resolve System.Runtime.Serialization.SerializationException, Message =
        /// "Unable to find assembly 'StoreData, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'."
        /// One solution is to ensure that assembly resides in same directory as acad.exe or revit.exe,
        /// the other is to implement a class such as this, cf.
        /// http://www.codeproject.com/soap/Serialization_Samples.asp
        /// </remarks>
        private sealed class ZengfyLinkBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
            }
        }
    }

    /// <summary>
    /// 在.NET中，我们可以将对象序列化从而保存对象的状态到内存或者磁盘文件中，或者分布式应用程序中用于系统通信，，这样就有可能做出一个“对象数据库”了。
    /// 一般来说，二进制序列化的效率要高，所获得的字节数最小。
    /// </summary>
    /// <remarks></remarks>
    public class BinarySerializer
    {
        /// <summary>
        /// 将任意一个声明为Serializable的类或者其List等集合中的数据，以二进制的格式保存到对应的流文件中。
        /// </summary>
        /// <param name="fs">推荐使用FileStream对象。此方法中不会对Stream对象进行Close。</param>
        /// <param name="Data">要进行保存的可序列化对象</param>
        /// <remarks></remarks>
        public static void EnCode(Stream fs, object Data)
        {
            BinaryFormatter bf = new BinaryFormatter(); // 最关键的对象，用来进行类到二进制的序列化与反序列化操作
            bf.Serialize(fs, Data);
        }

        /// <summary>
        /// 从二进制流文件中，将其中的二进制数据反序列化为对应的类或集合对象。
        /// </summary>
        /// <param name="fs">推荐使用FileStream对象。此方法中不会对Stream对象进行Close。</param>
        /// <returns>此二进制流文件所对应的可序列化对象</returns>
        /// <remarks></remarks>
        public static dynamic DeCode(Stream fs)
        {
            BinaryFormatter bf = new BinaryFormatter();
            object dt = bf.Deserialize(fs);
            return dt;
        }
    }
}