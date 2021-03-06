﻿using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Aoite.Reflection.Emitter
{
    internal class ArraySetEmitter : BaseEmitter
    {
        public ArraySetEmitter( Type targetType )
            : base(new CallInfo(targetType, null, Flags.InstanceAnyVisibility, MemberTypes.Method, Constants.ArraySetterName,
                                     new[] { typeof(int), targetType.GetElementType() }, null, false))
        {
        }

        protected internal override DynamicMethod CreateDynamicMethod()
        {
            return CreateDynamicMethod( Constants.ArraySetterName, CallInfo.TargetType, null,
                                        new[] { Constants.ObjectType, Constants.IntType, Constants.ObjectType } );
        }

        protected internal override Delegate CreateDelegate()
        {
            Type elementType = CallInfo.TargetType.GetElementType();
            Generator.ldarg_0 // load array
                .castclass( CallInfo.TargetType ) // (T[])array
                .ldarg_1 // load index
                .ldarg_2 // load value
                .CastFromObject( elementType ) // (unbox | cast) value
                .stelem( elementType ) // array[index] = value
                .ret();
            return Method.CreateDelegate( typeof(ArrayElementSetter) );
        }
    }
}