using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nitro
{
    /// <summary>
    /// Serves as a way of doing basic math operations on generic types.
    /// </summary>
    public static class GenericMath
    {
        private static object POD_op_Division(object a, object b)
        {
            if (a is sbyte sbyteA)
            {
                return sbyteA / (sbyte)b;
            }
            else if (a is byte byteA)
            {
                return byteA / (byte)b;
            }
            else if (a is char charA)
            {
                return charA / (char)b;
            }
            else if (a is short shortA)
            {
                return shortA / (short)b;
            }
            else if (a is ushort ushortA)
            {
                return ushortA / (ushort)b;
            }
            else if (a is int intA)
            {
                return intA / (int)b;
            }
            else if (a is uint uintA)
            {
                return uintA / (uint)b;
            }
            else if (a is long longA)
            {
                return longA / (long)b;
            }
            else if (a is ulong ulongA)
            {
                return ulongA / (ulong)b;
            }
            else if (a is float floatA)
            {
                return floatA / (float)b;
            }
            else if (a is double doubleA)
            {
                return doubleA / (double)b;
            }
            else if (a is decimal decimalA)
            {
                return decimalA / (decimal)b;
            }
            else
            {
                throw new Exception($"{a?.GetType()} is not a supported POD Type");
            }
        }

        private static object POD_op_Subtraction(object a, object b)
        {
            if (a is sbyte sbyteA)
            {
                return sbyteA - (sbyte)b;
            }
            else if (a is byte byteA)
            {
                return byteA - (byte)b;
            }
            else if (a is char charA)
            {
                return charA - (char)b;
            }
            else if (a is short shortA)
            {
                return shortA - (short)b;
            }
            else if (a is ushort ushortA)
            {
                return ushortA - (ushort)b;
            }
            else if (a is int intA)
            {
                return intA - (int)b;
            }
            else if (a is uint uintA)
            {
                return uintA - (uint)b;
            }
            else if (a is long longA)
            {
                return longA - (long)b;
            }
            else if (a is ulong ulongA)
            {
                return ulongA - (ulong)b;
            }
            else if (a is float floatA)
            {
                return floatA - (float)b;
            }
            else if (a is double doubleA)
            {
                return doubleA - (double)b;
            }
            else if (a is decimal decimalA)
            {
                return decimalA - (decimal)b;
            }
            else
            {
                throw new Exception($"{a?.GetType()} is not a supported POD Type");
            }
        }

        private static object POD_op_Multiply(object a, object b)
        {
            if (a is sbyte sbyteA)
            {
                return sbyteA * (sbyte)b;
            }
            else if (a is byte byteA)
            {
                return byteA * (byte)b;
            }
            else if (a is char charA)
            {
                return charA * (char)b;
            }
            else if (a is short shortA)
            {
                return shortA * (short)b;
            }
            else if (a is ushort ushortA)
            {
                return ushortA * (ushort)b;
            }
            else if (a is int intA)
            {
                return intA * (int)b;
            }
            else if (a is uint uintA)
            {
                return uintA * (uint)b;
            }
            else if (a is long longA)
            {
                return longA * (long)b;
            }
            else if (a is ulong ulongA)
            {
                return ulongA * (ulong)b;
            }
            else if (a is float floatA)
            {
                return floatA * (float)b;
            }
            else if (a is double doubleA)
            {
                return doubleA * (double)b;
            }
            else if (a is decimal decimalA)
            {
                return decimalA * (decimal)b;
            }
            else
            {
                throw new Exception($"{a?.GetType()} is not a supported POD Type");
            }
        }

        private static object POD_op_Addition(object a, object b)
        {
            if (a is sbyte sbyteA)
            {
                return sbyteA + (sbyte)b;
            }
            else if (a is byte byteA)
            {
                return byteA + (byte)b;
            }
            else if (a is char charA)
            {
                return charA + (char)b;
            }
            else if (a is short shortA)
            {
                return shortA + (short)b;
            }
            else if (a is ushort ushortA)
            {
                return ushortA + (ushort)b;
            }
            else if (a is int intA)
            {
                return intA + (int)b;
            }
            else if (a is uint uintA)
            {
                return uintA + (uint)b;
            }
            else if (a is long longA)
            {
                return longA + (long)b;
            }
            else if (a is ulong ulongA)
            {
                return ulongA + (ulong)b;
            }
            else if (a is float floatA)
            {
                return floatA + (float)b;
            }
            else if (a is double doubleA)
            {
                return doubleA + (double)b;
            }
            else if (a is decimal decimalA)
            {
                return decimalA + (decimal)b;
            }
            else
            {
                throw new Exception($"{a?.GetType()} is not a supported POD Type");
            }
        }

        static Dictionary<Type, MethodInfo> AddTypes = new Dictionary<Type, MethodInfo>();
        static Dictionary<Type, MethodInfo> MulTypes = new Dictionary<Type, MethodInfo>();
        static Dictionary<Type, MethodInfo> DivTypes = new Dictionary<Type, MethodInfo>();
        static Dictionary<Type, MethodInfo> SubTypes = new Dictionary<Type, MethodInfo>();

        private static Type[] podTypes = new Type[]
            {
                typeof(sbyte),
                typeof(byte),
                typeof(char),
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(float),
                typeof(double),
                typeof(decimal)
            };

        private static MethodInfo GetOpFunction(Type type, string opName)
        {
            return type.GetMethod(opName, BindingFlags.Public | BindingFlags.Static, null, new Type[] { type, type }, null);
        }

        private static bool IsPOD<T>()
        {
            for (int i = 0; i < podTypes.Length; i++)
            {
                if (podTypes[i] == typeof(T))
                {
                    return true;
                }
            }
            return false;
        }





        /// <summary>
        /// Checks if a type can be added to
        /// </summary>
        /// <typeparam name="T">The type to check</typeparam>
        /// <returns>Returns true if the type can be added to itself</returns>
        public static bool HasAdd<T>()
        {
            if (!IsPOD<T>())
            {
                if (!AddTypes.TryGetValue(typeof(T), out MethodInfo method))
                {
                    method = GetOpFunction(typeof(T), "op_Addition");
                    AddTypes.Add(typeof(T), method);
                }
                return method != null;
            }
            return true;
        }

        /// <summary>
        /// Checks if a type can be subtracted to
        /// </summary>
        /// <typeparam name="T">The type to check</typeparam>
        /// <returns>Returns true if the type can be subtracted from itself</returns>
        public static bool HasSub<T>()
        {
            if (!IsPOD<T>())
            {
                if (!SubTypes.TryGetValue(typeof(T), out MethodInfo method))
                {
                    method = GetOpFunction(typeof(T), "op_Subtraction");
                    SubTypes.Add(typeof(T), method);
                }
                return method != null;
            }
            return true;
        }

        /// <summary>
        /// Checks if a type can be multiplied with itself
        /// </summary>
        /// <typeparam name="T">The type to check</typeparam>
        /// <returns>Returns true if the type can be multiplied with itself</returns>
        public static bool HasMul<T>()
        {
            if (!IsPOD<T>())
            {
                if (!MulTypes.TryGetValue(typeof(T), out MethodInfo method))
                {
                    method = GetOpFunction(typeof(T), "op_Multiply");
                    MulTypes.Add(typeof(T), method);
                }
                return method != null;
            }
            return true;
        }

        /// <summary>
        /// Checks if a type can be divided by itself
        /// </summary>
        /// <typeparam name="T">The type to check</typeparam>
        /// <returns>Returns true if the type can be divided by itself</returns>
        public static bool HasDiv<T>()
        {
            if (!IsPOD<T>())
            {
                if (!DivTypes.TryGetValue(typeof(T), out MethodInfo method))
                {
                    method = GetOpFunction(typeof(T), "op_Division");
                    DivTypes.Add(typeof(T), method);
                }
                return method != null;
            }
            return true;
        }

        private static bool NonPodHasAdd<T>(out MethodInfo method)
        {
            if (!IsPOD<T>())
            {
                if (!AddTypes.TryGetValue(typeof(T), out method))
                {
                    method = GetOpFunction(typeof(T), "op_Addition");
                    AddTypes.Add(typeof(T), method);
                }
                return method != null;
            }
            method = null;
            return false;
        }

        private static bool NonPodHasSub<T>(out MethodInfo method)
        {
            if (!IsPOD<T>())
            {
                if (!SubTypes.TryGetValue(typeof(T), out method))
                {
                    method = GetOpFunction(typeof(T), "op_Subtraction");
                    SubTypes.Add(typeof(T), method);
                }
                return method != null;
            }
            method = null;
            return false;
        }

        private static bool NonPodHasMul<T>(out MethodInfo method)
        {
            if (!IsPOD<T>())
            {
                if (!MulTypes.TryGetValue(typeof(T), out method))
                {
                    method = GetOpFunction(typeof(T), "op_Multiply");
                    MulTypes.Add(typeof(T), method);
                }
                return method != null;
            }
            method = null;
            return false;
        }

        private static bool NonPodHasDiv<T>(out MethodInfo method)
        {
            if (!IsPOD<T>())
            {
                if (!DivTypes.TryGetValue(typeof(T), out method))
                {
                    method = GetOpFunction(typeof(T), "op_Division");
                    DivTypes.Add(typeof(T), method);
                }
                return method != null;
            }
            method = null;
            return false;
        }

        private static object[] paramCache = new object[2];

        /// <summary>
        /// Adds two variables together, if possible
        /// </summary>
        /// <typeparam name="T">The type of the variables to add</typeparam>
        /// <param name="a">The first variable</param>
        /// <param name="b">The second variable</param>
        /// <returns>Returns the sum of the variables</returns>
        /// <exception cref="Exception">Throws if the two variables can't be added together</exception>
        public static T Add<T>(T a, T b)
        {
            if (IsPOD<T>())
            {
                return (T)POD_op_Addition(a, b);
            }
            else if (NonPodHasAdd<T>(out MethodInfo method))
            {
                paramCache[0] = a;
                paramCache[1] = b;
                return (T)method.Invoke(null, paramCache);
            }
            else
            {
                throw new Exception($"{typeof(T).FullName} does not have an Addition operator");
            }
        }

        /// <summary>
        /// Subtracts two variables together, if possible
        /// </summary>
        /// <typeparam name="T">The type of the variables to subtract</typeparam>
        /// <param name="a">The first variable</param>
        /// <param name="b">The second variable</param>
        /// <returns>Returns the difference of the variables</returns>
        /// <exception cref="Exception">Throws if the two variables can't be subtracted together</exception>
        public static T Sub<T>(T a, T b)
        {
            if (IsPOD<T>())
            {
                return (T)POD_op_Subtraction(a, b);
            }
            else if (NonPodHasSub<T>(out MethodInfo method))
            {
                paramCache[0] = a;
                paramCache[1] = b;
                return (T)method.Invoke(null, paramCache);
            }
            else
            {
                throw new Exception($"{typeof(T).FullName} does not have a subtraction operator");
            }
        }

        /// <summary>
        /// Multiplies two variables together, if possible
        /// </summary>
        /// <typeparam name="T">The type of the variables to multiply</typeparam>
        /// <param name="a">The first variable</param>
        /// <param name="b">The second variable</param>
        /// <returns>Returns the product of the variables</returns>
        /// <exception cref="Exception">Throws if the two variables can't be multiplied together</exception>
        public static T Mul<T>(T a, T b)
        {
            if (IsPOD<T>())
            {
                return (T)POD_op_Multiply(a, b);
            }
            else if (NonPodHasMul<T>(out MethodInfo method))
            {
                paramCache[0] = a;
                paramCache[1] = b;
                return (T)method.Invoke(null, paramCache);
            }
            else
            {
                throw new Exception($"{typeof(T).FullName} does not have a multiplication operator");
            }
        }

        /// <summary>
        /// Divides two variables together, if possible
        /// </summary>
        /// <typeparam name="T">The type of the variables to divide</typeparam>
        /// <param name="a">The first variable</param>
        /// <param name="b">The second variable</param>
        /// <returns>Returns the quotient of the variables</returns>
        /// <exception cref="Exception">Throws if the two variables can't be divided together</exception>
        public static T Div<T>(T a, T b)
        {
            if (IsPOD<T>())
            {
                return (T)POD_op_Division(a, b);
            }
            else if (NonPodHasDiv<T>(out MethodInfo method))
            {
                paramCache[0] = a;
                paramCache[1] = b;
                return (T)method.Invoke(null, paramCache);
            }
            else
            {
                throw new Exception($"{typeof(T).FullName} does not have a division operator");
            }
        }
    }
}

