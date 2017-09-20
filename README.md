# Serialization_Unity3D_2017_NET46

C# Expression Tree based serialization library that supports serializing all fundamental types, custom [Serializable] types that contain supported serializable types.

Reflection is used only at the Expression Tree generation time, and the emitted IL code is saved into assembly for use at runtime. There is no runtime relfection

The performance of the pre-generated expression tree over the runtime reflected version is 10 times faster.

This project relies on the experimental .NET 4.6 scripting runtime support provided in Unity3D 2017.1+ versions.
