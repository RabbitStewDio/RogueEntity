using System;
using System.Reflection;

namespace RogueEntity.Core.Utils
{
    public static class ReflectionUtilities
    {
        public static bool IsSameFunction(this MethodInfo m, Type returnType, params Type[] parameter)
        {
            if (m.ReturnType != returnType)
            {
                return false;
            }

            var p = m.GetParameters();
            if (p.Length != parameter.Length)
            {
                return false;
            }

            for (var i = 0; i < p.Length; i++)
            {
                var pi = p[i];
                if (pi.ParameterType != parameter[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsSameAction(this MethodInfo m, params Type[] parameter)
        {
            return IsSameFunction(m, typeof(void), parameter);
        }
    }
}