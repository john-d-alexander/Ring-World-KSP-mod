using System;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Collections.Immutable;

using var file=File.OpenRead(args[0]);using var pe=new PEReader(file);var metadata=pe.GetMetadataReader();
foreach(var handle in metadata.TypeDefinitions)
{
    var type=metadata.GetTypeDefinition(handle);string name=metadata.GetString(type.Name);
    if(!name.Contains(args[1],StringComparison.OrdinalIgnoreCase))continue;
    Console.WriteLine(metadata.GetString(type.Namespace)+"."+name);
    foreach(var methodHandle in type.GetMethods())
    {
        var method=metadata.GetMethodDefinition(methodHandle);string methodName=metadata.GetString(method.Name);
        if(args.Length>2&&!methodName.Contains(args[2],StringComparison.OrdinalIgnoreCase))continue;
        var signature=method.DecodeSignature(new Names(),null);
        Console.WriteLine("  "+signature.ReturnType+" "+methodName+"("+string.Join(", ",signature.ParameterTypes)+")");
    }
}
class Names : ISignatureTypeProvider<string,object>
{
    public string GetArrayType(string t,ArrayShape s)=>t+"[]";
    public string GetByReferenceType(string t)=>"ref "+t;
    public string GetFunctionPointerType(MethodSignature<string> s)=>"fn";
    public string GetGenericInstantiation(string t,ImmutableArray<string> a)=>t+"<"+string.Join(",",a)+">";
    public string GetGenericMethodParameter(object c,int i)=>"M"+i;
    public string GetGenericTypeParameter(object c,int i)=>"T"+i;
    public string GetModifiedType(string m,string t,bool r)=>t;
    public string GetPinnedType(string t)=>t;
    public string GetPointerType(string t)=>t+"*";
    public string GetPrimitiveType(PrimitiveTypeCode c)=>c.ToString();
    public string GetSZArrayType(string t)=>t+"[]";
    public string GetTypeFromDefinition(MetadataReader r,TypeDefinitionHandle h,byte k)=>r.GetString(r.GetTypeDefinition(h).Name);
    public string GetTypeFromReference(MetadataReader r,TypeReferenceHandle h,byte k)=>r.GetString(r.GetTypeReference(h).Name);
    public string GetTypeFromSpecification(MetadataReader r,object c,TypeSpecificationHandle h,byte k)=>r.GetTypeSpecification(h).DecodeSignature(this,c);
}
