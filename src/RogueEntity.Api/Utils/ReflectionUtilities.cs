using System;
using System.Reflection;
using System.Text;

namespace RogueEntity.Api.Utils
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

        public static bool IsSameGenericAction(this MethodInfo m,
                                               Type[] generics,
                                               out MethodInfo genericMethod,
                                               out string errorHint,
                                               params Type[] parameter)
        {
            errorHint = default;
            genericMethod = default;
            if (!m.IsGenericMethod) return false;
            if (m.GetGenericArguments().Length != generics.Length) return false;

            try
            {
                genericMethod = m.MakeGenericMethod(generics);
                return genericMethod.IsSameAction(parameter);
            }
            catch (ArgumentException e)
            {
                errorHint = BuildErrorMessage(m, generics, e);
                return false;
            }
        }

        public static bool IsSameGenericFunction(this MethodInfo m,
                                                 Type[] generics,
                                                 out MethodInfo genericMethod,
                                                 out string errorHint,
                                                 Type returnValue,
                                                 params Type[] parameter)
        {
            errorHint = default;
            genericMethod = default;
            if (!m.IsGenericMethod) return false;
            if (m.GetGenericArguments().Length != generics.Length) return false;

            try
            {
                genericMethod = m.MakeGenericMethod(generics);
                return genericMethod.IsSameFunction(returnValue, parameter);
            }
            catch (ArgumentException e)
            {
                errorHint = BuildErrorMessage(m, generics, e);
                return false;
            }
        }

        static string BuildErrorMessage(MethodInfo m, Type[] generics, ArgumentException e)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(e.Message);

            var args = m.GetGenericArguments();
            for (var index = 0; index < args.Length; index++)
            {
                var arg = args[index];
                var genericArgs = generics[index];
                var attrs = arg.GenericParameterAttributes;

                if ((attrs & GenericParameterAttributes.ReferenceTypeConstraint) != 0 && genericArgs.IsValueType)
                {
                    b.AppendFormat("Type parameter {0} requires a reference type, but {1} is a value type", arg.Name,
                                   genericArgs);
                    b.AppendLine();
                }

                if ((attrs & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0 && !genericArgs.IsValueType)
                {
                    b.AppendFormat("Type parameter {0} requires a value type, but {1} is not a value type", arg.Name,
                                   genericArgs);
                    b.AppendLine();
                }

                foreach (var tp in arg.GetGenericParameterConstraints())
                {
                    if (!tp.IsAssignableFrom(genericArgs))
                    {
                        b.AppendFormat("Type parameter {0} requires {1} to be an instance of {2}.", arg, genericArgs, tp);
                    }
                }
            }

            return b.ToString();
        }
    }
}