using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Python.Runtime
{
    public class NumpyException : System.ApplicationException
    {
        public NumpyException() { }
        public NumpyException(string message) { }
        public NumpyException(string message, System.Exception inner) { }
    }
    
    public class Numpy
    {
        public class NumpyArrayInterface
        {
            public NumpyArrayInterface(PyObject o)
            {
                if (o.GetPythonType().Handle != NumpyArrayType)
                {
                    throw new Exception("object is not a numpy array");
                }
                var meta = o.GetAttr("__array_interface__");
                IsCStyleContiguous = meta["strides"] == null;
                Address = new System.IntPtr((long)meta["data"][0].As<long>());

                var typestr = meta["typestr"].As<string>();
                var dtype = typestr.Substring(1);
                switch(dtype)
                {
                    case "b1":
                        DataType = typeof(bool);
                        break;
                    case "f4":
                        DataType = typeof(float);
                        break;
                    case "f8":
                        DataType = typeof(double);
                        break;
                    case "i2":
                        DataType = typeof(short);
                        break;
                    case "i4":
                        DataType = typeof(int);
                        break;
                    case "i8":
                        DataType = typeof(long);
                        break;
                    case "u1":
                        DataType = typeof(byte);
                        break;
                    case "u2":
                        DataType = typeof(ushort);
                        break;
                    case "u4":
                        DataType = typeof(uint);
                        break;
                    case "u8":
                        DataType = typeof(ulong);
                        break;
                    default:
                        throw new NumpyException($"type '{dtype}' not supported");
                } 
                Shape = o.GetAttr("shape").As<long[]>();
                NBytes = o.GetAttr("nbytes").As<int>();
            }

            public readonly IntPtr Address;

            public readonly System.Type DataType;

            public readonly long[] Shape;

            public readonly int NBytes;

            public readonly bool IsCStyleContiguous;
        }

        public static void Initialize()
        {
            np = Py.Import("numpy");
            NumpyArrayType = np.GetAttr("ndarray").Handle;
            np_dtypes.Clear();
            np_dtypes.Add(typeof(byte), np.GetAttr("uint8"));
            np_dtypes.Add(typeof(short), np.GetAttr("int16"));
            np_dtypes.Add(typeof(int), np.GetAttr("int32"));
            np_dtypes.Add(typeof(long), np.GetAttr("int64"));
            np_dtypes.Add(typeof(ushort), np.GetAttr("uint16"));
            np_dtypes.Add(typeof(uint), np.GetAttr("uint32"));
            np_dtypes.Add(typeof(ulong), np.GetAttr("uint64"));
            np_dtypes.Add(typeof(float), np.GetAttr("float"));
            np_dtypes.Add(typeof(double), np.GetAttr("float64"));
            var copy = Py.Import("copy");
            deepcopy = copy.GetAttr("deepcopy");
        }

        /// <summary>
        /// numpy Module
        /// </summary>
        internal static PyObject np;

        internal static Dictionary<Type, PyObject> np_dtypes = new Dictionary<Type, PyObject>();

        public static PyObject GetNumpyDataType(Type type)
        {
            PyObject dtype;
            np_dtypes.TryGetValue(type, out dtype);
            if(dtype == null)
            {
                throw new NumpyException($"type '{type}' not supported.");
            }
            return dtype;
        }

        internal static PyObject deepcopy;

        internal static IntPtr NumpyArrayType;

        public static PyObject NewArray(Array content)
        {
            // BlockCopy possibly multidimensional array of arbitrary type to onedimensional byte array
            System.Type ElementType = content.GetType().GetElementType();
            int nbytes = content.Length * Marshal.SizeOf(ElementType);
            byte[] data = new byte[nbytes];
            System.Buffer.BlockCopy(content, 0, data, 0, nbytes);

            // Create an python tuple with the dimensions of the input array
            PyObject[] lengths = new PyObject[content.Rank];
            for (int i = 0; i < content.Rank; i++)
                lengths[i] = new PyInt(content.GetLength(i));
            PyTuple shape = new PyTuple(lengths);

            // Create an empty numpy array in correct shape and datatype
            var dtype = GetNumpyDataType(ElementType);
            var arr = np.InvokeMethod("empty", shape, dtype);

            var meta = arr.GetAttr("__array_interface__");
            var address = new System.IntPtr((long)meta["data"][0].As<long>());

            // Copy the data to that array
            Marshal.Copy(data, 0, address, nbytes);
            return arr;
        }

        public static Array ToArray(PyObject array)
        {
            var info = new NumpyArrayInterface(array);
            // If the array is not contiguous in memory, copy it first.
            // This overwrites the array (but obviously the contents stay the same).
            PyObject arr = array;
            if(!info.IsCStyleContiguous)
            {
                arr = deepcopy.Invoke(array);
            }
            
            byte[] data = new byte[info.NBytes];
            Marshal.Copy(info.Address, data, 0, info.NBytes);
            if (info.DataType == typeof(byte) && info.Shape.Length == 1)
            {
                return data;
            }
            var result = System.Array.CreateInstance(info.DataType, info.Shape);
            System.Buffer.BlockCopy(data, 0, result, 0, info.NBytes);
            return result;
        }
    }
}
