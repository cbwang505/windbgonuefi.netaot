﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Changes made by Joshua Wierenga.

using System.Runtime.CompilerServices;
using Internal.Runtime.CompilerServices;

namespace System
{
    // CONTRACT with Runtime
    // The Array type is one of the primitives understood by the compilers and runtime
    // Data Contract: Single field of type int

    public partial class Array: Object
    {
        // This field should be the first field in Array as the runtime/compilers depend on it
        internal int _numComponents;

        public int Length => (int)Unsafe.As<RawArrayData>(this).Length;
    }

    // To accommodate class libraries that wish to implement generic interfaces on arrays, all class libraries
    // are now required to provide an Array<T> class that derives from Array.
    internal class Array<T> : Array
    {
    }
}