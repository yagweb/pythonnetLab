using Python.Runtime;
using System.Collections.Generic;

namespace Test
{
    public class TestPyConverter
    {
        public PyConverter NewMyListConverter()
        {
            using (Py.GIL())
            {
                var converter = new PyConverter();  //create a instance of PyConverter
                converter.AddListType();
                converter.Add(new StringType());
                converter.Add(new Int64Type());
                converter.Add(new Int32Type());
                converter.Add(new FloatType());
                converter.Add(new DoubleType());
                return converter;
            }
        }

        public PyConverter NewMyDictConverter()
        {
            using (Py.GIL())
            {
                var converter = new PyConverter();  //create a instance of PyConverter
                converter.AddListType();
                converter.Add(new StringType());
                converter.Add(new Int64Type());
                converter.Add(new Int32Type());
                converter.Add(new FloatType());
                converter.Add(new DoubleType());
                converter.AddDictType<string, object>();
                return converter;
            }
        }

        public void TestObject()
        {
            using (Py.GIL())
            {
                var scope = Py.CreateScope();
                scope.Exec(
                     "class TestObject(object): \n" +
                     "    def __init__(self): \n" +
                     "        self.id = 0 \n" +
                     "        self.Name = ''\n" +
                     "        self.addr = ''\n" +
                     "a = TestObject()\n" +
                     "a.id = 1 \n" +
                     "a.Name = 'create_in_python' \n" +
                     "a.addr = 'python' \n"
                );                
                var converter = NewMyDictConverter();
                converter.AddObjectType<ObjectType>(scope.Get("TestObject"));

                var a = scope.Get("a");
                var b = converter.ToClr(a);   //b is a List of CLR objects

                dynamic c = converter.ToPython(b);  //
                object c0 = c.id;
                object c1 = c.Name;

                var d = converter.ToClr(c);
                scope.Dispose();
            }
        }

        public void TestList()
        {
            using (Py.GIL())
            {
                var scope = Py.CreateScope();
                scope.Exec(
                     "a=[1, \"2\"]"
                );
                dynamic a = scope.Get("a");

                //now, I want to convert res into List<object>
                var converter = NewMyListConverter();  //create a instance of PyConverter

                var b = converter.ToClr(a);   //b is a List of CLR objects

                var c = converter.ToPython(b);  // 
                object c0 = c[0];
                object c1 = c[1];

                var d = converter.ToClr(c);
                scope.Dispose();
            }
        }

        public void TestDict()
        {
            using (Py.GIL())
            {  
                //create a instance of PyConverter                
                var converter = NewMyDictConverter();

                var scope = Py.CreateScope();
                scope.Exec(
                     "a={'0': 1, '1': [1, \"2\"]}"
                );
                dynamic a = scope.Get("a");

                var b = converter.ToClr(a);   //b is a List of CLR objects

                var c = converter.ToPython(b);  // 
                object c0 = c["0"];
                object c1 = c["1"];

                var d = converter.ToClr(c);
                object d0 = d["0"];
                object d1 = d["1"];

                scope.Dispose();
            }
        }

        public void TestHybird()
        {
            using (Py.GIL())
            {
                var scope = Py.CreateScope();
                scope.Exec(
                     "class TestObject(object): \n" +
                     "    def __init__(self): \n" +
                     "        self.id = 0 \n" +
                     "        self.Name = ''\n" +
                     "        self.addr = ''\n" +
                     "class ComplexObjectType(object): \n" +
                     "    def __init__(self): \n" +
                     "        self.id = 0 \n" +
                     "        self.ObjectField = ''\n" +
                     "        self.ListField = ''\n" +
                     "        self.DictField = ''\n"
                );
                var converter = NewMyDictConverter();
                converter.AddObjectType<ObjectType>(scope.Get("TestObject"));
                converter.AddObjectType<ComplexObjectType>(scope.Get("ComplexObjectType"));

                var b = new ComplexObjectType()
                {
                    ID = 12,
                    ObjectField = new ObjectType()
                    {
                        ID = 12,
                        Name = "create_in_clr",
                        Address = "clr"
                    },
                    ListField = new List<object>() { "a", "b"},
                    DictField = new Dictionary<string, object>() {
                        {"aa", 12 },
                        {"bb", 21 }
                    }
                };

                dynamic c = converter.ToPython(b);  //
                object c0 = c.id;
                object c1 = c.ObjectField;
                object c2 = c.ListField;
                object c3 = c.DictField;

                var d = converter.ToClr(c);
                scope.Dispose();
            }
        }
    }
    
    public class ObjectType
    {
        [PyPropetry("id")]
        public long ID;

        [PyPropetry]
        public string Name;

        [PyPropetry("addr")]
        public string Address
        {
            get;
            set;
        }
    }
    
    public class ComplexObjectType
    {
        [PyPropetry("id")]
        public long ID;
        
        [PyPropetry]
        public ObjectType ObjectField;

        [PyPropetry]
        public List<object> ListField;

        [PyPropetry]
        public Dictionary<string, object> DictField;
    }
}
