using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace kate.shared.Helpers
{
    public enum ObjectType : byte
    {
        nullType,
        boolType,
        byteType,
        uint16Type,
        uint32Type,
        uint64Type,
        sbyteType,
        int16Type,
        int32Type,
        int64Type,
        charType,
        stringType,
        singleType,
        doubleType,
        decimalType,
        dateTimeType,
        byteArrayType,
        charArrayType,
        otherType,
        bSerializableType
    }

    /// <summary> SerializationWriter.  Extends BinaryWriter to add additional data types,
    /// handle null strings and simplify use with ISerializable. </summary>
    public class SerializationWriter : BinaryWriter
    {
        public SerializationWriter(Stream s)
            : base(s, Encoding.UTF8)
        {
        }

        /// <summary> Static method to initialise the writer with a suitable MemoryStream. </summary>
        public static SerializationWriter GetWriter()
        {
            MemoryStream ms = new MemoryStream(1024);
            return new SerializationWriter(ms);
        }

        /// <summary> Writes a string to the buffer.  Overrides the base implementation so it can cope with nulls </summary>
        public override void Write(string str)
        {
            if (str == null)
            {
                Write((byte)ObjectType.nullType);
            }
            else
            {
                Write((byte)ObjectType.stringType);
                base.Write(str);
            }
        }

        /// <summary> Writes a byte array to the buffer.  Overrides the base implementation to
        /// send the length of the array which is needed when it is retrieved </summary>
        public override void Write(byte[] b)
        {
            if (b == null)
            {
                Write(-1);
            }
            else
            {
                int len = b.Length;
                Write(len);
                if (len > 0) base.Write(b);
            }
        }

        /// <summary> Writes a char array to the buffer.  Overrides the base implementation to
        /// sends the length of the array which is needed when it is read. </summary>
        public override void Write(char[] c)
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                int len = c.Length;
                Write(len);
                if (len > 0) base.Write(c);
            }
        }

        /// <summary> Writes a DateTime to the buffer. <summary>
        public void Write(DateTime dt)
        {
            Write(dt.ToUniversalTime().Ticks);
        }

        /// <summary> Writes a generic ICollection (such as an IList<T>) to the buffer. </summary>
        public void Write<T>(List<T> c) where T : bSerializable
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                int count = c.Count;
                Write(count);
                for (int i = 0; i < count; i++)
                    c[i].WriteToStream(this);
            }
        }

        /// <summary> Writes a generic IDictionary to the buffer. </summary>
        public void Write<T, U>(IDictionary<T, U> d)
        {
            if (d == null)
            {
                Write(-1);
            }
            else
            {
                Write(d.Count);
                foreach (KeyValuePair<T, U> kvp in d)
                {
                    WriteObject(kvp.Key);
                    WriteObject(kvp.Value);
                }
            }
        }
        public void WriteBVal<T, U>(IDictionary<T, U> d) where U : bSerializable
        {
            if (d == null)
            {
                Write(-1);
            }
            else
            {
                Write(d.Count);
                foreach (KeyValuePair<T, U> kvp in d)
                {
                    WriteObject(kvp.Key);
                    kvp.Value.WriteToStream(this);
                }
            }
        }

        /// <summary> Writes an arbitrary object to the buffer.  Useful where we have something of type "object"
        /// and don't know how to treat it.  This works out the best method to use to write to the buffer. </summary>
        public void WriteObject(object obj)
        {
            if (obj == null)
            {
                Write((byte)ObjectType.nullType);
            }
            else
            {
                switch (obj.GetType().Name)
                {
                    case "Boolean":
                        Write((byte)ObjectType.boolType);
                        Write((bool)obj);
                        break;

                    case "Byte":
                        Write((byte)ObjectType.byteType);
                        Write((byte)obj);
                        break;

                    case "UInt16":
                        Write((byte)ObjectType.uint16Type);
                        Write((ushort)obj);
                        break;

                    case "UInt32":
                        Write((byte)ObjectType.uint32Type);
                        Write((uint)obj);
                        break;

                    case "UInt64":
                        Write((byte)ObjectType.uint64Type);
                        Write((ulong)obj);
                        break;

                    case "SByte":
                        Write((byte)ObjectType.sbyteType);
                        Write((sbyte)obj);
                        break;

                    case "Int16":
                        Write((byte)ObjectType.int16Type);
                        Write((short)obj);
                        break;

                    case "Int32":
                        Write((byte)ObjectType.int32Type);
                        Write((int)obj);
                        break;

                    case "Int64":
                        Write((byte)ObjectType.int64Type);
                        Write((long)obj);
                        break;

                    case "Char":
                        Write((byte)ObjectType.charType);
                        base.Write((char)obj);
                        break;

                    case "String":
                        Write((byte)ObjectType.stringType);
                        base.Write((string)obj);
                        break;

                    case "Single":
                        Write((byte)ObjectType.singleType);
                        Write((float)obj);
                        break;

                    case "Double":
                        Write((byte)ObjectType.doubleType);
                        Write((double)obj);
                        break;

                    case "Decimal":
                        Write((byte)ObjectType.decimalType);
                        Write((decimal)obj);
                        break;

                    case "DateTime":
                        Write((byte)ObjectType.dateTimeType);
                        Write((DateTime)obj);
                        break;

                    case "Byte[]":
                        Write((byte)ObjectType.byteArrayType);
                        base.Write((byte[])obj);
                        break;

                    case "Char[]":
                        Write((byte)ObjectType.charArrayType);
                        base.Write((char[])obj);
                        break;

                    default:
                        Write((byte)ObjectType.otherType);
                        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter b = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        b.AssemblyFormat = FormatterAssemblyStyle.Simple;
                        b.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
                        b.Serialize(BaseStream, obj);
                        break;
                } // switch
            } // if obj==null
        } // WriteObject

        /// <summary> Adds the SerializationWriter buffer to the SerializationInfo at the end of GetObjectData(). </summary>
        public void AddToInfo(SerializationInfo info)
        {
            byte[] b = ((MemoryStream)BaseStream).ToArray();
            info.AddValue("X", b, typeof(byte[]));
        }

        public void WriteRawBytes(byte[] b)
        {
            base.Write(b);
        }

        public void WriteByteArray(byte[] b)
        {
            if (b == null)
            {
                Write(-1);
            }
            else
            {
                int len = b.Length;
                Write(len);
                if (len > 0) base.Write(b);
            }
        }

        public void WriteUTF8(string str)
        {
            WriteRawBytes(Encoding.UTF8.GetBytes(str));
        }
    }

    /// <summary> SerializationReader.  Extends BinaryReader to add additional data types,
    /// handle null strings and simplify use with ISerializable. </summary>
    public class SerializationReader : BinaryReader
    {
        public SerializationReader(Stream s)
            : base(s, Encoding.UTF8)
        {
        }

        /// <summary> Static method to take a SerializationInfo object (an input to an ISerializable constructor)
        /// and produce a SerializationReader from which serialized objects can be read </summary>.
        public static SerializationReader GetReader(SerializationInfo info)
        {
            byte[] byteArray = (byte[])info.GetValue("X", typeof(byte[]));
            MemoryStream ms = new MemoryStream(byteArray);
            return new SerializationReader(ms);
        }

        /// <summary> Reads a string from the buffer.  Overrides the base implementation so it can cope with nulls. </summary>
        public override string ReadString()
        {
            if (0 == ReadByte()) return null;
            return base.ReadString();
        }

        /// <summary> Reads a byte array from the buffer, handling nulls and the array length. </summary>
        public byte[] ReadByteArray()
        {
            int len = ReadInt32();
            if (len > 0) return ReadBytes(len);
            if (len < 0) return null;
            return new byte[0];
        }

        /// <summary> Reads a char array from the buffer, handling nulls and the array length. </summary>
        public char[] ReadCharArray()
        {
            int len = ReadInt32();
            if (len > 0) return ReadChars(len);
            if (len < 0) return null;
            return new char[0];
        }

        /// <summary> Reads a DateTime from the buffer. </summary>
        public DateTime ReadDateTime()
        {
            long ticks = ReadInt64();
            if (ticks < 0) throw new AbandonedMutexException("oops");
            return new DateTime(ticks, DateTimeKind.Utc);
        }

        /// <summary> Reads a generic list from the buffer. </summary>
        public IList<T> ReadBList<T>() where T : kate.shared.Helpers.bSerializable, new()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IList<T> d = new List<T>(count);
            if (count < 1) return d;
            SerializationReader sr = new SerializationReader(BaseStream);

            for (int i = 0; i < count; i++)
            {
                T obj = new T();
                obj.ReadFromStream(sr);
                d.Add(obj);
            }

            return d;
        }

        /// <summary> Reads a generic list from the buffer. </summary>
        public IList<T> ReadList<T>()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IList<T> d = new List<T>(count);
            if (count < 1) return d;
            for (int i = 0; i < count; i++) d.Add((T)ReadObject());
            return d;
        }

        /// <summary> Reads a generic Dictionary from the buffer. </summary>
        public IDictionary<T, U> ReadDictionary<T, U>()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IDictionary<T, U> d = new Dictionary<T, U>();
            if (count < 1) return d;
            for (int i = 0; i < count; i++) d[(T)ReadObject()] = (U)ReadObject();
            return d;
        }
        public IDictionary<T, U> ReadBValDictionary<T, U>() where U : bSerializable, new()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            SerializationReader sr = new SerializationReader(BaseStream);
            IDictionary<T, U> d = new Dictionary<T, U>();
            if (count < 1) return d;
            for (int i = 0; i < count; i++)
            {
                U obj = new U();
                obj.ReadFromStream(sr);
                d[(T)ReadObject()] = obj;
            }
            return d;
        }

        /// <summary> Reads an object which was added to the buffer by WriteObject. </summary>
        public object ReadObject()
        {
            ObjectType t = (ObjectType)ReadByte();
            switch (t)
            {
                case ObjectType.boolType:
                    return ReadBoolean();
                case ObjectType.byteType:
                    return ReadByte();
                case ObjectType.uint16Type:
                    return ReadUInt16();
                case ObjectType.uint32Type:
                    return ReadUInt32();
                case ObjectType.uint64Type:
                    return ReadUInt64();
                case ObjectType.sbyteType:
                    return ReadSByte();
                case ObjectType.int16Type:
                    return ReadInt16();
                case ObjectType.int32Type:
                    return ReadInt32();
                case ObjectType.int64Type:
                    return ReadInt64();
                case ObjectType.charType:
                    return ReadChar();
                case ObjectType.stringType:
                    return base.ReadString();
                case ObjectType.singleType:
                    return ReadSingle();
                case ObjectType.doubleType:
                    return ReadDouble();
                case ObjectType.decimalType:
                    return ReadDecimal();
                case ObjectType.dateTimeType:
                    return ReadDateTime();
                case ObjectType.byteArrayType:
                    return ReadByteArray();
                case ObjectType.charArrayType:
                    return ReadCharArray();
                case ObjectType.otherType:
                    return DynamicDeserializer.Deserialize(BaseStream);
                default:
                    return null;
            }
        }
    }

}
