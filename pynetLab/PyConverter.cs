using System;
using System.Collections.Generic;
using System.Reflection;

namespace Python.Runtime
{
    /// <summary>
    /// use PyClrType to convert between python object and clr object.
    /// </summary>
    public class PyConverter
    {
        public PyConverter()
        {
            this.Converters = new List<PyClrTypeBase>();
            this.PythonConverters = new Dictionary<IntPtr, PyClrTypeBase>();
            this.ClrConverters = new Dictionary<Type, PyClrTypeBase>();
        }

        private List<PyClrTypeBase> Converters;

        private Dictionary<IntPtr, PyClrTypeBase> PythonConverters;

        private Dictionary<Type, PyClrTypeBase> ClrConverters;

        public void Add(PyClrTypeBase converter)
        {
            this.Converters.Add(converter);
            if (!this.PythonConverters.ContainsKey(converter.PythonType.Handle))
            {
                this.PythonConverters.Add(converter.PythonType.Handle, converter);
            }
            if (!this.ClrConverters.ContainsKey(converter.ClrType))
            {
                this.ClrConverters.Add(converter.ClrType, converter);
            }
        }

        public void AddObjectType<T>(PyObject pyType, PyConverter converter = null)
        {
            if (converter == null)
            {
                converter = this;
            }
            this.Add(new ObjectType<T>(pyType, converter));
        }

        public void AddListType(PyConverter converter = null)
        {
            this.AddListType<object>(converter);
        }

        public void AddListType<T>(PyConverter converter = null)
        {
            if (converter == null)
            {
                converter = this;
            }
            this.Add(new PyListType<T>(converter));
        }

        public void AddDictType<K, V>(PyConverter converter = null)
        {
            if (converter == null)
            {
                converter = this;
            }
            this.Add(new PyDictType<K, V>(converter));
        }

        public T ToClr<T>(PyObject obj)
        {
            return (T)ToClr(obj);
        }

        public object ToClr(PyObject obj)
        {
            if (obj == null)
            {
                return null;
            }
            PyObject type = obj.GetPythonType();
            PyClrTypeBase converter;
            var state = PythonConverters.TryGetValue(type.Handle, out converter);
            if (!state)
            {
                throw new Exception($"Type {type.ToString()} not recognized");
            }
            return converter.ToClr(obj);
        }

        public PyObject ToPython(object clrObj)
        {
            if (clrObj == null)
            {
                return null;
            }
            Type type = clrObj.GetType();
            PyClrTypeBase converter;
            var state = ClrConverters.TryGetValue(type, out converter);
            if (!state)
            {
                throw new Exception($"Type {type.ToString()} not recognized");
            }
            return converter.ToPython(clrObj);
        }
    }

    public abstract class PyClrTypeBase
    {
        protected PyClrTypeBase(string pyType, Type clrType)
        {
            this.PythonType = PythonEngine.Eval(pyType);
            this.ClrType = clrType;
        }

        protected PyClrTypeBase(PyObject pyType, Type clrType)
        {
            this.PythonType = pyType;
            this.ClrType = clrType;
        }

        public PyObject PythonType
        {
            get;
            private set;
        }

        public Type ClrType
        {
            get;
            private set;
        }

        public abstract object ToClr(PyObject pyObj);

        public abstract PyObject ToPython(object clrObj);
    }

    public class PyClrType : PyClrTypeBase
    {
        public PyClrType(PyObject pyType, Type clrType, 
            Func<PyObject, object> py2clr, Func<object, PyObject> clr2py)
            :base(pyType, clrType)
        {
            this.Py2Clr = py2clr;
            this.Clr2Py = clr2py;
        }

        private Func<PyObject, object> Py2Clr;

        private Func<object, PyObject> Clr2Py;

        public override object ToClr(PyObject pyObj)
        {
            return this.Py2Clr(pyObj);
        }

        public override PyObject ToPython(object clrObj)
        {
            return this.Clr2Py(clrObj);
        }
    }

    public class StringType : PyClrTypeBase
    {
        public StringType()
            : base("str", typeof(string))
        {
        }

        public override object ToClr(PyObject pyObj)
        {
            return pyObj.As<string>();
        }

        public override PyObject ToPython(object clrObj)
        {
            return new PyString(Convert.ToString(clrObj));
        }
    }

    public class Int32Type : PyClrTypeBase
    {
        public Int32Type()
            : base("int", typeof(int))
        {
        }

        public override object ToClr(PyObject pyObj)
        {
            return pyObj.As<int>();
        }

        public override PyObject ToPython(object clrObj)
        {
            return new PyInt(Convert.ToInt32(clrObj));
        }
    }

    public class Int64Type : PyClrTypeBase
    {
        public Int64Type()
            : base("int", typeof(long))
        {
        }

        public override object ToClr(PyObject pyObj)
        {
            return pyObj.As<long>();
        }

        public override PyObject ToPython(object clrObj)
        {
            return new PyInt(Convert.ToInt64(clrObj));
        }
    }

    public class FloatType : PyClrTypeBase
    {
        public FloatType()
            : base("float", typeof(float))
        {
        }

        public override object ToClr(PyObject pyObj)
        {
            return pyObj.As<float>();
        }

        public override PyObject ToPython(object clrObj)
        {
            return new PyFloat(Convert.ToSingle(clrObj));
        }
    }

    public class DoubleType : PyClrTypeBase
    {
        public DoubleType()
            :base("float", typeof(double))
        {
        }

        public override object ToClr(PyObject pyObj)
        {
            return pyObj.As<double>();
        }

        public override PyObject ToPython(object clrObj)
        {
            return new PyFloat(Convert.ToDouble(clrObj));
        }
    }

    public class PyPropetryAttribute : Attribute
    {
        public PyPropetryAttribute()
        {
            this.Name = null;
        }

        public PyPropetryAttribute(string name)
        {
            this.Name = name;
        }

        public string Name
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Convert between Python object and clr object
    /// </summary>
    public class ObjectType<T> : PyClrTypeBase
    {
        public ObjectType(PyObject pyType, PyConverter converter)
            : base(pyType, typeof(T))
        {
            this.Converter = converter;
            this.Properties = new Dictionary<string, PropertyInfo>();
            this.Fields = new Dictionary<string, FieldInfo>();

            // Get all attributes
            foreach (var property in this.ClrType.GetProperties())
            {
                var attr = property.GetCustomAttributes(typeof(PyPropetryAttribute), true);
                if(attr.Length == 0)
                {
                    continue;
                }
                string pyname = (attr[0] as PyPropetryAttribute).Name;
                if(string.IsNullOrEmpty(pyname))
                {
                    pyname = property.Name;
                }

                this.Properties.Add(pyname, property);
            }

            foreach (var field in this.ClrType.GetFields())
            {
                var attr = field.GetCustomAttributes(typeof(PyPropetryAttribute), true);
                if (attr.Length == 0)
                {
                    continue;
                }
                string pyname = (attr[0] as PyPropetryAttribute).Name;
                if (string.IsNullOrEmpty(pyname))
                {
                    pyname = field.Name;
                }

                this.Fields.Add(pyname, field);
            }
        }
        
        private PyConverter Converter;

        private Dictionary<string, PropertyInfo> Properties;

        private Dictionary<string, FieldInfo> Fields;

        public override object ToClr(PyObject pyObj)
        {
            var obj = Activator.CreateInstance(this.ClrType);
            foreach(var pair in this.Properties)
            {
                var value = pyObj.GetAttr(pair.Key);
                var _value = this.Converter.ToClr(value);
                pair.Value.SetValue(obj, _value, null);
            }
            foreach (var pair in this.Fields)
            {
                var value = pyObj.GetAttr(pair.Key);
                var _value = this.Converter.ToClr(value);
                pair.Value.SetValue(obj, _value);
            }
            return obj;
        }

        public override PyObject ToPython(object clrObj)
        {
            var pyObj = this.PythonType.Invoke();
            foreach (var pair in this.Properties)
            {
                var value = pair.Value.GetValue(clrObj, null);
                var _value = this.Converter.ToPython(value);
                pyObj.SetAttr(pair.Key, _value);
            }
            foreach (var pair in this.Fields)
            {
                var value = pair.Value.GetValue(clrObj);
                var _value = this.Converter.ToPython(value);
                pyObj.SetAttr(pair.Key, _value);
            }
            return pyObj;
        }
    }

    public class PyListType<T> : PyClrTypeBase
    {
        public PyListType(PyConverter converter)
            :base("list", typeof(List<T>))
        {
            this.Converter = converter;
        }

        private PyConverter Converter;

        public override object ToClr(PyObject pyObj)
        {
            var dict = this._ToClr(new PyList(pyObj));
            return dict;
        }

        private object _ToClr(PyList pyList)
        {
            var list = new List<T>();
            foreach (PyObject item in pyList)
            {
                var _item = this.Converter.ToClr<T>(item);
                list.Add(_item);
            }
            return list;
        }

        public override PyObject ToPython(object clrObj)
        {
            return this._ToPython(clrObj as List<T>);
        }

        public PyObject _ToPython(List<T> clrObj)
        {
            var pyList = new PyList();
            foreach (var item in clrObj)
            {
                PyObject _item = this.Converter.ToPython(item);
                pyList.Append(_item);
            }
            return pyList;
        }
    }

    public class PyDictType<K, V> : PyClrTypeBase
    {
        public PyDictType(PyConverter converter)
            :base("dict", typeof(Dictionary<K, V>))
        {
            this.Converter = converter;
        }

        private PyConverter Converter;

        public override object ToClr(PyObject pyObj)
        {
            var dict = this._ToClr(new PyDict(pyObj));
            return dict;
        }

        private object _ToClr(PyDict pyDict)
        {
            var dict = new Dictionary<K, V>();
            foreach (PyObject key in pyDict.Keys())
            {
                var _key = this.Converter.ToClr<K>(key);
                var _value = this.Converter.ToClr<V>(pyDict[key]);
                dict.Add(_key, _value);
            }
            return dict;
        }

        public override PyObject ToPython(object clrObj)
        {
            return this._ToPython(clrObj as Dictionary<K, V>);
        }

        public PyObject _ToPython(Dictionary<K, V> clrObj)
        {
            var pyDict = new PyDict();
            foreach(var item in clrObj)
            {
                PyObject _key = this.Converter.ToPython(item.Key);
                PyObject _value = this.Converter.ToPython(item.Value);
                pyDict[_key] = _value;
            }
            return pyDict;
        }
    }
}
