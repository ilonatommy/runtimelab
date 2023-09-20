// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection.Emit;
using Internal.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Internal.Mint.Abstraction;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoMethodInstanceAbstractionVTable
{
    public delegate* unmanaged<MonoMethodInstanceAbstractionNativeAot*, MonoMethodSignatureInstanceAbstractionNativeAot*> get_signature; // MonoMethodSignature* (* get_signature) (MonoMethod* self);
    public delegate* unmanaged<MonoMethodInstanceAbstractionNativeAot*, MonoMethodHeaderInstanceAbstractionNativeAot*> get_header; // MonoMethodHeader* (* get_header) (MonoMethod* self);
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoMethodInstanceAbstractionNativeAot
{
    public MonoMethodInstanceAbstractionVTable* vtable;
    public byte* name;
    public IntPtr /*MonoClass* */ klass;

    public byte is_dynamic; // this is a DynamicMethod

    public IntPtr gcHandle; // FIXME
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoMethodSignatureInstanceAbstractionVTable
{
    public delegate* unmanaged<MonoMethodSignatureInstanceAbstractionNativeAot*, MonoTypeInstanceAbstractionNativeAot**> method_params; // MonoType ** (*method_params)(MonoMethodSignature *self);
    public delegate* unmanaged<MonoMethodSignatureInstanceAbstractionNativeAot*, MonoTypeInstanceAbstractionNativeAot*> ret_ult; // MonoType * (*ret_ult)(MonoMethodSignature *self);
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoMethodSignatureInstanceAbstractionNativeAot
{
    public MonoMethodSignatureInstanceAbstractionVTable* vtable;
    public int param_count;
    public byte hasthis;

    public IntPtr gcHandle;
    public MonoTypeInstanceAbstractionNativeAot** MethodParamsTypes;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoMethodHeaderInstanceAbstractionVTable
{
    public IntPtr get_local_sig; // MonoType * (*get_local_sig)(MonoMethodHeader *self, int32_t i);
    public delegate* unmanaged<MonoMethodHeaderInstanceAbstractionNativeAot*, byte*> get_code; // const uint8_t * (*get_code)(MonoMethodHeader *self);
    public delegate* unmanaged<MonoMethodHeaderInstanceAbstractionNativeAot*, byte*, int> get_ip_offset; // int32_t (*get_ip_offset)(MonoMethodHeader *self, const uint8_t *ip);
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoMethodHeaderInstanceAbstractionNativeAot
{
    public MonoMethodHeaderInstanceAbstractionVTable* vtable;
    public int code_size;
    public int max_stack;
    public int num_locals;
    public int num_clauses;
    public byte init_locals;

    public IntPtr gcHandle;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoTypeInstanceAbstractionNativeAot
{
    public int type_code; // FIXME this could be a byte. it's just CorElementType
    public byte is_byref;

    public IntPtr gcHandle;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoMemPoolInstanceAbstractionVTable
{
    public delegate* unmanaged<MonoMemPoolInstanceAbstraction*, void> destroy; // void (*destroy)(MonoMemPool *self);
    public delegate* unmanaged<MonoMemPoolInstanceAbstraction*, uint, IntPtr> alloc0; // void* (*alloc0)(MonoMemPool *self, uint32_t size);
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoMemPoolInstanceAbstraction
{
    public MonoMemPoolInstanceAbstractionVTable* vtable;
    public IntPtr gcHandle;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MonoMemManagerInstanceAbstraction
{
    // for nativeaot, a mem manager _is_ just a mempool. don't make a distinction here
    public MonoMemPoolInstanceAbstraction mempool;
}
