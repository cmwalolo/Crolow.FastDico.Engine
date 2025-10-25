using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Kalow.Apps.Common.Reflection
{
    public class ReflectionHelper
    {
        private static Dictionary<string, List<Type>> cache;
        private static Dictionary<string, Type> cacheSingle;
        private static readonly object syncRoot = new Object();

        private static Dictionary<string, List<Type>> Cache
        {
            get
            {
                if (cache == null)
                {
                    lock (syncRoot)
                    {
                        if (cache == null)
                        {
                            cache = new Dictionary<string, List<Type>>();
                        }
                    }
                }

                return cache;
            }
        }

        private static Dictionary<string, Type> CacheSingle
        {
            get
            {
                if (cacheSingle == null)
                {
                    lock (syncRoot)
                    {
                        if (cacheSingle == null)
                        {
                            cacheSingle = new Dictionary<string, Type>();
                        }
                    }
                }

                return cacheSingle;
            }
        }

        public static List<Type> GetClassesWithAttribute(Type type, bool excludeSystemTypes, Assembly onlyThisAssembly = null)
        {
            var list = new List<Type>();

            lock (syncRoot)
            {
                if (onlyThisAssembly == null && Cache.ContainsKey(type.AssemblyQualifiedName))
                {
                    return Cache[type.AssemblyQualifiedName];
                }

                IEnumerator enumerator = onlyThisAssembly != null ? (new[] { onlyThisAssembly }).GetEnumerator() : Thread.GetDomain().GetAssemblies().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    try
                    {
                        var a = ((Assembly)enumerator.Current);

                        Type[] types = a.GetTypes();


                        if (!excludeSystemTypes ||
                            (excludeSystemTypes && !((Assembly)enumerator.Current).FullName.StartsWith("Microsoft.") && !((Assembly)enumerator.Current).FullName.StartsWith("System.")))
                        {
                            IEnumerator enumerator2 = types.GetEnumerator();
                            while (enumerator2.MoveNext())
                            {
                                var current = (Type)enumerator2.Current;
                                object[] o = current.GetCustomAttributes(type, true);
                                if (o.Length > 0)
                                {
                                    list.Add(current);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                if (onlyThisAssembly == null) Cache.Add(type.AssemblyQualifiedName, list);
            }
            return list;
        }

        public static Type GetTypeByName(string findType, bool excludeSystemTypes)
        {
            if (CacheSingle.ContainsKey(findType))
            {
                return CacheSingle[findType];
            }

            var list = new List<Type>();
            IEnumerator enumerator = Thread.GetDomain().GetAssemblies().GetEnumerator();
            while (enumerator.MoveNext())
            {
                try
                {
                    var a = ((Assembly)enumerator.Current);

                    Type[] types = a.GetTypes();

                    if (!excludeSystemTypes ||
                        (excludeSystemTypes && !((Assembly)enumerator.Current).FullName.StartsWith("System.")))
                    {
                        IEnumerator enumerator2 = types.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            var current = (Type)enumerator2.Current;
                            if (current.FullName == findType)
                            {
                                CacheSingle.Add(findType, current);
                                return current;
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            return null;
        }

        public static Type GetTypeByAssemblyName(string findType, bool excludeSystemTypes)
        {
            if (CacheSingle.ContainsKey(findType))
            {
                return CacheSingle[findType];
            }

            var list = new List<Type>();
            IEnumerator enumerator = Thread.GetDomain().GetAssemblies().GetEnumerator();
            while (enumerator.MoveNext())
            {
                try
                {
                    var a = ((Assembly)enumerator.Current);

                    Type[] types = a.GetTypes();

                    if (!excludeSystemTypes ||
                        (excludeSystemTypes && !((Assembly)enumerator.Current).FullName.StartsWith("System.")))
                    {
                        IEnumerator enumerator2 = types.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            var current = (Type)enumerator2.Current;
                            if (current.AssemblyQualifiedName == findType)
                            {
                                lock (CacheSingle)
                                {
                                    CacheSingle.Add(findType, current);
                                }
                                return current;
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            return null;
        }

        public static List<Type> GetSubclassesOf(Type type, bool excludeSystemTypes)
        {
            var list = new List<Type>();
            lock (syncRoot)
            {
                if (Cache.ContainsKey(type.AssemblyQualifiedName))
                {
                    return Cache[type.AssemblyQualifiedName];
                }

                IEnumerator enumerator = Thread.GetDomain().GetAssemblies().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    try
                    {
                        var a = ((Assembly)enumerator.Current);

                        Type[] types = a.GetTypes();


                        if (!excludeSystemTypes ||
                            (excludeSystemTypes && !((Assembly)enumerator.Current).FullName.StartsWith("System.")))
                        {
                            IEnumerator enumerator2 = types.GetEnumerator();
                            while (enumerator2.MoveNext())
                            {
                                var current = (Type)enumerator2.Current;
                                if (type.IsInterface)
                                {
                                    if (current.GetInterface(type.FullName) != null)
                                    {
                                        list.Add(current);
                                    }
                                }
                                else if (current.IsSubclassOf(type))
                                {
                                    list.Add(current);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                Cache.Add(type.AssemblyQualifiedName, list);
            }
            return list;
        }


        public static List<Type> GetSubclassesOf(Assembly a, Type type)
        {
            var list = new List<Type>();
            lock (syncRoot)
            {
                try
                {

                    Type[] types = a.GetTypes();

                    IEnumerator enumerator2 = types.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        var current = (Type)enumerator2.Current;
                        if (type.IsInterface)
                        {
                            if (current.GetInterface(type.FullName) != null)
                            {
                                list.Add(current);
                            }
                        }
                        else if (current.IsSubclassOf(type))
                        {
                            list.Add(current);
                        }
                    }
                }
                catch
                {
                }

            }
            return list;
        }


        public static object GetObjectFromType(string type)
        {
            Type t = Type.GetType(type);

            if (t == null)
            {
                t = GetTypeByName(type, true);
            }

            if (t != null)
            {
                return t.GetConstructor(Type.EmptyTypes).Invoke(null);
            }
            return null;
        }

        public static Type GetFromType(string type)
        {
            if (CacheSingle.ContainsKey(type))
            {
                return CacheSingle[type];
            }

            Type t = Type.GetType(type);
            if (t == null)
            {
                t = GetTypeByName(type.Split(new char[] { ',' })[0].Trim(), true);
            }

            return t;

        }
    }
}