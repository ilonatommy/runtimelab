﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Runtime.Serialization.Json;
using System.Text.Json.Serialization;

namespace System.Text.Json.SourceGeneration
{
    internal sealed partial class JsonSourceGeneratorHelper
    {
        private readonly Type _ienumerableType;
        private readonly Type _listOfTType;
        private readonly Type _ienumerableOfTType;
        private readonly Type _ilistOfTType;
        private readonly Type _dictionaryType;

        private readonly Type _booleanType;
        private readonly Type _byteArrayType;
        private readonly Type _charType;
        private readonly Type _dateTimeType;
        private readonly Type _dateTimeOffsetType;
        private readonly Type _guidType;
        private readonly Type _stringType;
        private readonly Type _uriType;
        private readonly Type _versionType;

        private readonly HashSet<Type> _numberTypes = new();

        private readonly HashSet<Type> _knownTypes = new();

        // Contains used JsonTypeInfo<T> identifiers.
        private readonly HashSet<string> _usedFriendlyTypeNames = new();

        /// <summary>
        /// Type information for member types in input object graphs.
        /// </summary>
        private readonly Dictionary<Type, TypeMetadata> _typeMetadataCache = new();

        /// </summary>
        private readonly Dictionary<string, (Type, bool)> _rootSerializableTypes;

        private readonly GeneratorExecutionContext _executionContext;

        private readonly MetadataLoadContext _metadataLoadContext;

        private const string JsonConverterAttributeFullName = "System.Text.Json.Serialization.JsonConverterAttribute";

        public JsonSourceGeneratorHelper(
            GeneratorExecutionContext executionContext,
            MetadataLoadContext metadataLoadContext,
            Dictionary<string, (Type, bool)> rootSerializableTypes)
        {
            _generationNamespace = $"{executionContext.Compilation.AssemblyName}.JsonSourceGeneration";
            _executionContext = executionContext;
            _metadataLoadContext = metadataLoadContext;
            _rootSerializableTypes = rootSerializableTypes;

            _ienumerableType = metadataLoadContext.Resolve(typeof(IEnumerable));
            _listOfTType = metadataLoadContext.Resolve(typeof(List<>));
            _ienumerableOfTType = metadataLoadContext.Resolve(typeof(IEnumerable<>));
            _ilistOfTType = metadataLoadContext.Resolve(typeof(IList<>));
            _dictionaryType = metadataLoadContext.Resolve(typeof(Dictionary<,>));

            _booleanType = metadataLoadContext.Resolve(typeof(bool));
            _byteArrayType = metadataLoadContext.Resolve(typeof(byte[]));
            _charType = metadataLoadContext.Resolve(typeof(char));
            _dateTimeType = metadataLoadContext.Resolve(typeof(DateTime));
            _dateTimeOffsetType = metadataLoadContext.Resolve(typeof(DateTimeOffset));
            _guidType = metadataLoadContext.Resolve(typeof(Guid));

            _stringType = metadataLoadContext.Resolve(typeof(string));
            // TODO: confirm that this is true.
            // System.Private.Uri may not be loaded in input compilation.
            _uriType = metadataLoadContext.Resolve(typeof(Uri));
            _versionType = metadataLoadContext.Resolve(typeof(Version));

            PopulateKnownTypes(metadataLoadContext);
            InitializeDiagnosticDescriptors();
        }

        public void GenerateSerializationMetadata()
        {
            if (_rootSerializableTypes == null || _rootSerializableTypes.Count == 0)
            {
                throw new InvalidOperationException("Serializable types must be provided to this helper via the constructor.");
            }

            foreach (KeyValuePair<string, (Type, bool)> pair in _rootSerializableTypes)
            {
                (Type type, bool canBeDynamic) = pair.Value;
                TypeMetadata typeMetadata = GetOrAddTypeMetadata(type, canBeDynamic);
                GenerateSerializationMetadataForType(typeMetadata);
            }

            // Add base default instance source.
            AddBaseJsonContextImplementation();

            // Add GetJsonTypeInfo override implementation.
            _executionContext.AddSource("JsonContext.GetJsonTypeInfo.g.cs", SourceText.From(GetGetTypeInfoImplementation(), Encoding.UTF8));
        }

        private TypeMetadata GetOrAddTypeMetadata(Type type, bool canBeDynamic)
        {
            if (_typeMetadataCache.TryGetValue(type, out TypeMetadata? typeMetadata))
            {
                return typeMetadata!;
            }

            return GetTypeMetadata(type, canBeDynamic);
        }

        private TypeMetadata GetOrAddTypeMetadata(Type type)
        {
            if (_typeMetadataCache.TryGetValue(type, out TypeMetadata? typeMetadata))
            {
                return typeMetadata!;
            }

            bool found = _rootSerializableTypes.TryGetValue(type.FullName, out (Type, bool) typeInfo);

            if (!found && type.IsValueType)
            {
                // To help callers, check if `CanBeDynamic` was specified for the nullable equivalent.
                string nullableTypeFullName = $"System.Nullable`1[[{type.AssemblyQualifiedName}]]";
                _rootSerializableTypes.TryGetValue(nullableTypeFullName, out typeInfo);
            }

            bool canBeDynamic = typeInfo.Item2;
            return GetTypeMetadata(type, canBeDynamic);
        }

        private TypeMetadata GetTypeMetadata(Type type, bool canBeDynamic)
        {
            // Add metadata to cache now to prevent stack overflow when the same type is found somewhere else in the object graph.
            TypeMetadata typeMetadata = new();
            _typeMetadataCache[type] = typeMetadata;

            ClassType classType;
            Type? collectionKeyType = null;
            Type? collectionValueType = null;
            Type? nullableUnderlyingType = null;
            List<PropertyMetadata>? propertiesMetadata = null;
            CollectionType collectionType = CollectionType.NotApplicable;
            ObjectConstructionStrategy constructionStrategy = default;
            JsonNumberHandling? numberHandling = null;
            bool containsOnlyPrimitives = true;

            bool foundDesignTimeCustomConverter = false;
            string? converterInstatiationLogic = null;

            IList<CustomAttributeData> attributeDataList = CustomAttributeData.GetCustomAttributes(type);
            foreach (CustomAttributeData attributeData in attributeDataList)
            {
                Type attributeType = attributeData.AttributeType;
                if (attributeType.FullName == "System.Text.Json.Serialization.JsonNumberHandlingAttribute")
                {
                    IList<CustomAttributeTypedArgument> ctorArgs = attributeData.ConstructorArguments;
                    if (ctorArgs.Count != 1)
                    {
                        throw new InvalidOperationException($"Invalid use of 'JsonNumberHandlingAttribute' detected on '{type}'.");
                    }

                    numberHandling = (JsonNumberHandling)ctorArgs[0].Value;
                    continue;
                }
                else if (!foundDesignTimeCustomConverter && attributeType.GetCompatibleBaseClass(JsonConverterAttributeFullName) != null)
                {
                    foundDesignTimeCustomConverter = true;
                    converterInstatiationLogic = GetConverterInstantiationLogic(attributeData);
                }
            }

            if (foundDesignTimeCustomConverter)
            {
                classType = converterInstatiationLogic != null
                    ? ClassType.TypeWithDesignTimeProvidedCustomConverter
                    : ClassType.TypeUnsupportedBySourceGen; // TODO: provide diagnostic with reason.
            }
            else if (_knownTypes.Contains(type))
            {
                classType = ClassType.KnownType;
            }
            else if (type.IsNullableValueType(out nullableUnderlyingType))
            {
                Debug.Assert(nullableUnderlyingType != null);
                classType = ClassType.Nullable;
            }
            else if (type.IsEnum)
            {
                classType = ClassType.Enum;
            }
            else if (_ienumerableType.IsAssignableFrom(type))
            {
                // Only T[], List<T>, Dictionary<Tkey, TValue>, and IEnumerable<T> are supported.

                if (type.IsArray)
                {
                    classType = ClassType.Enumerable;
                    collectionType = CollectionType.Array;
                    collectionValueType = type.GetElementType();
                }
                else if (!type.IsGenericType)
                {
                    classType = ClassType.TypeUnsupportedBySourceGen;
                }
                else
                {
                    Type genericTypeDef = type.GetGenericTypeDefinition();
                    Type[] genericTypeArgs = type.GetGenericArguments();

                    if (genericTypeDef == _listOfTType)
                    {
                        classType = ClassType.Enumerable;
                        collectionType = CollectionType.List;
                        collectionValueType = genericTypeArgs[0];
                    }
                    else if (genericTypeDef == _ienumerableOfTType)
                    {
                        classType = ClassType.Enumerable;
                        collectionType = CollectionType.IEnumerable;
                        collectionValueType = genericTypeArgs[0];
                    }
                    else if (genericTypeDef == _ilistOfTType)
                    {
                        classType = ClassType.Enumerable;
                        collectionType = CollectionType.IList;
                        collectionValueType = genericTypeArgs[0];
                    }
                    else if (genericTypeDef == _dictionaryType)
                    {
                        classType = ClassType.Dictionary;
                        collectionType = CollectionType.Dictionary;
                        collectionKeyType = genericTypeArgs[0];
                        collectionValueType = genericTypeArgs[1];
                    }
                    else
                    {
                        classType = ClassType.TypeUnsupportedBySourceGen;
                    }
                }
            }
            else
            {
                classType = ClassType.Object;

                if (!type.IsAbstract && !type.IsInterface)
                {
                    // TODO: account for parameterized ctors.
                    constructionStrategy = ObjectConstructionStrategy.ParameterlessConstructor;
                }

                Dictionary<string, PropertyMetadata>? ignoredMembers = null;

                for (Type? currentType = type; currentType != null; currentType = currentType.BaseType)
                {
                    const BindingFlags bindingFlags =
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly;

                    foreach (Reflection.PropertyInfo propertyInfo in currentType.GetProperties(bindingFlags))
                    {
                        PropertyMetadata metadata = GetPropertyMetadata(propertyInfo);

                        // Ignore indexers and virtual properties that have overrides that were [JsonIgnore]d.
                        if (propertyInfo.GetIndexParameters().Length > 0 || PropertyIsOverridenAndIgnored(metadata, ignoredMembers))
                        {
                            continue;
                        }

                        string key = metadata.JsonPropertyName ?? metadata.ClrName;

                        if (metadata.HasGetter || metadata.HasSetter) // Don't have to check for JsonInclude since that is used to determine these values.
                        {
                            (propertiesMetadata ??= new()).Add(metadata);
                        }

                        if (containsOnlyPrimitives && !IsPrimitive(propertyInfo.PropertyType))
                        {
                            containsOnlyPrimitives = false;
                        }
                    }

                    foreach (FieldInfo fieldInfo in currentType.GetFields(bindingFlags))
                    {
                        PropertyMetadata metadata = GetPropertyMetadata(fieldInfo);

                        if (PropertyIsOverridenAndIgnored(metadata, ignoredMembers))
                        {
                            continue;
                        }

                        if (metadata.HasGetter || metadata.HasSetter) // TODO: don't have to check for JsonInclude since that is used to determine these values?
                        {
                            (propertiesMetadata ??= new()).Add(metadata);
                        }
                    }
                }
            }

            string compilableName = type.GetUniqueCompilableTypeName();
            string friendlyName = type.GetFriendlyTypeName();

            if (_usedFriendlyTypeNames.Contains(friendlyName))
            {
                friendlyName = type.GetUniqueFriendlyTypeName();
            }
            else
            {
                _usedFriendlyTypeNames.Add(friendlyName);
            }

            typeMetadata.Initialize(
                compilableName,
                friendlyName,
                type,
                classType,
                isValueType: type.IsValueType,
                canBeDynamic,
                numberHandling,
                propertiesMetadata,
                collectionType,
                collectionKeyTypeMetadata: collectionKeyType != null ? GetOrAddTypeMetadata(collectionKeyType) : null,
                collectionValueTypeMetadata: collectionValueType != null ? GetOrAddTypeMetadata(collectionValueType) : null,
                constructionStrategy,
                nullableUnderlyingTypeMetadata: nullableUnderlyingType != null ? GetOrAddTypeMetadata(nullableUnderlyingType) : null,
                converterInstatiationLogic,
                containsOnlyPrimitives);

            return typeMetadata;
        }

        private static bool PropertyIsOverridenAndIgnored(PropertyMetadata currentMemberMetadata, Dictionary<string, PropertyMetadata>? ignoredMembers)
        {
            if (ignoredMembers == null || !ignoredMembers.TryGetValue(currentMemberMetadata.ClrName, out PropertyMetadata? ignoredMemberMetadata))
            {
                return false;
            }

            return currentMemberMetadata.TypeMetadata.Type == ignoredMemberMetadata.TypeMetadata.Type &&
                PropertyIsVirtual(currentMemberMetadata) &&
                PropertyIsVirtual(ignoredMemberMetadata);
        }

        private static bool PropertyIsVirtual(PropertyMetadata? propertyMetadata)
        {
            return propertyMetadata != null && (propertyMetadata.GetterIsVirtual == true || propertyMetadata.SetterIsVirtual == true);
        }

        private PropertyMetadata GetPropertyMetadata(MemberInfo memberInfo)
        {
            IList<CustomAttributeData> attributeDataList = CustomAttributeData.GetCustomAttributes(memberInfo);

            bool hasJsonInclude = false;
            JsonIgnoreCondition? ignoreCondition = null;
            JsonNumberHandling? numberHandling = null;
            string? jsonPropertyName = null;

            bool foundDesignTimeCustomConverter = false;
            string? converterInstantiationLogic = null;

            foreach (CustomAttributeData attributeData in attributeDataList)
            {
                Type attributeType = attributeData.AttributeType;

                if (!foundDesignTimeCustomConverter && attributeType.GetCompatibleBaseClass(JsonConverterAttributeFullName) != null)
                {
                    foundDesignTimeCustomConverter = true;
                    converterInstantiationLogic = GetConverterInstantiationLogic(attributeData);
                }
                else if (attributeType.Assembly.FullName == "System.Text.Json")
                {
                    switch (attributeData.AttributeType.FullName)
                    {
                        case "System.Text.Json.Serialization.JsonIgnoreAttribute":
                            {
                                IList<CustomAttributeNamedArgument> namedArgs = attributeData.NamedArguments;

                                if (namedArgs.Count == 0)
                                {
                                    ignoreCondition = JsonIgnoreCondition.Always;
                                }
                                else if (namedArgs.Count == 1 &&
                                    namedArgs[0].MemberInfo.MemberType == MemberTypes.Property &&
                                    ((Reflection.PropertyInfo)namedArgs[0].MemberInfo).PropertyType.FullName == "System.Text.Json.Serialization.JsonIgnoreCondition")
                                {
                                    ignoreCondition = (JsonIgnoreCondition)namedArgs[0].TypedValue.Value;
                                }
                            }
                            break;
                        case "System.Text.Json.Serialization.JsonIncludeAttribute":
                            {
                                hasJsonInclude = true;
                            }
                            break;
                        case "System.Text.Json.Serialization.JsonNumberHandlingAttribute":
                            {
                                IList<CustomAttributeTypedArgument> ctorArgs = attributeData.ConstructorArguments;
                                if (ctorArgs.Count != 1)
                                {
                                    throw new InvalidOperationException($"Invalid use of 'JsonNumberHandlingAttribute' detected on '{memberInfo.DeclaringType}.{memberInfo.Name}'.");
                                }

                                numberHandling = (JsonNumberHandling)ctorArgs[0].Value;
                            }
                            break;
                        case "System.Text.Json.Serialization.JsonPropertyNameAttribute":
                            {
                                IList<CustomAttributeTypedArgument> ctorArgs = attributeData.ConstructorArguments;
                                if (ctorArgs.Count != 1 || ctorArgs[0].ArgumentType != _stringType)
                                {
                                    throw new InvalidOperationException($"Invalid use of 'JsonPropertyNameAttribute' detected on '{memberInfo.DeclaringType}.{memberInfo.Name}'.");
                                }

                                jsonPropertyName = (string)ctorArgs[0].Value;
                                // Null check here is done at runtime within JsonSerializer.
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            Type memberCLRType;
            bool hasGetter = false;
            bool hasSetter = false;
            bool getterIsVirtual = false;
            bool setterIsVirtual = false;

            switch (memberInfo)
            {
                case Reflection.PropertyInfo propertyInfo:
                    {
                        MethodInfo setMethod = propertyInfo.SetMethod;

                        memberCLRType = propertyInfo.PropertyType;
                        hasGetter = PropertyAccessorCanBeReferenced(propertyInfo.GetMethod, hasJsonInclude);
                        hasSetter = PropertyAccessorCanBeReferenced(setMethod, hasJsonInclude) && !setMethod.IsInitOnly();
                        getterIsVirtual = propertyInfo.GetMethod?.IsVirtual == true;
                        setterIsVirtual = propertyInfo.SetMethod?.IsVirtual == true;
                    }
                    break;
                case FieldInfo fieldInfo:
                    {
                        Debug.Assert(fieldInfo.IsPublic);

                        memberCLRType = fieldInfo.FieldType;
                        hasGetter = true;
                        hasSetter = !fieldInfo.IsInitOnly;
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return new PropertyMetadata
            {
                ClrName = memberInfo.Name,
                MemberType = memberInfo.MemberType,
                JsonPropertyName = jsonPropertyName,
                HasGetter = hasGetter,
                HasSetter = hasSetter,
                GetterIsVirtual = getterIsVirtual,
                SetterIsVirtual = setterIsVirtual,
                IgnoreCondition = ignoreCondition,
                NumberHandling = numberHandling,
                HasJsonInclude = hasJsonInclude,
                TypeMetadata = GetOrAddTypeMetadata(memberCLRType),
                DeclaringTypeCompilableName = memberInfo.DeclaringType.GetUniqueCompilableTypeName(),
                ConverterInstantiationLogic = converterInstantiationLogic
            };
        }

        private static bool PropertyAccessorCanBeReferenced(MethodInfo? memberAccessor, bool hasJsonInclude) =>
            (memberAccessor != null && !memberAccessor.IsPrivate) && (memberAccessor.IsPublic || hasJsonInclude);

        private string? GetConverterInstantiationLogic(CustomAttributeData attributeData)
        {
            if (attributeData.AttributeType.FullName != JsonConverterAttributeFullName)
            {
                // TODO: need diagnostic here telling user that converter was ignored (derived JsonConverterAttribute not supported in codegen mode.)
                return null;
            }

            Type converterType = new TypeWrapper((ITypeSymbol)attributeData.ConstructorArguments[0].Value, _metadataLoadContext);

            if (converterType == null || converterType.GetConstructor(Type.EmptyTypes) == null || converterType.IsNestedPrivate)
            {
                // TODO: need diagnostic here telling user that converter was ignored.
                return null;
            }

            return $"new {converterType.GetUniqueCompilableTypeName()}()";
        }

        private void PopulateNumberTypes(MetadataLoadContext metadataLoadContext)
        {
            Debug.Assert(_numberTypes != null);
            _numberTypes.Add(metadataLoadContext.Resolve(typeof(byte)));
            _numberTypes.Add(metadataLoadContext.Resolve(typeof(Decimal)));
            _numberTypes.Add(metadataLoadContext.Resolve(typeof(double)));
            _numberTypes.Add(metadataLoadContext.Resolve(typeof(short)));
            _numberTypes.Add(metadataLoadContext.Resolve(typeof(sbyte)));
            _numberTypes.Add(metadataLoadContext.Resolve(typeof(int)));
            _numberTypes.Add(metadataLoadContext.Resolve(typeof(long)));
            _numberTypes.Add(metadataLoadContext.Resolve(typeof(float)));
            _numberTypes.Add(metadataLoadContext.Resolve(typeof(ushort)));
            _numberTypes.Add(metadataLoadContext.Resolve(typeof(uint)));
            _numberTypes.Add(metadataLoadContext.Resolve(typeof(ulong)));
        }

        private void PopulateKnownTypes(MetadataLoadContext metadataLoadContext)
        {
            PopulateNumberTypes(metadataLoadContext);

            Debug.Assert(_knownTypes != null);
            Debug.Assert(_numberTypes != null);

            _knownTypes.UnionWith(_numberTypes);
            _knownTypes.Add(_booleanType);
            _knownTypes.Add(_byteArrayType);
            _knownTypes.Add(_charType);
            _knownTypes.Add(_dateTimeType);
            _knownTypes.Add(_dateTimeOffsetType);
            _knownTypes.Add(_guidType);
            _knownTypes.Add(metadataLoadContext.Resolve(typeof(object)));
            _knownTypes.Add(_stringType);

            // TODO: confirm that this is true.
            // System.Private.Uri may not be loaded in input compilation.
            if (_uriType != null)
            {
                _knownTypes.Add(_uriType);
            }

            _knownTypes.Add(metadataLoadContext.Resolve(typeof(Version)));
        }

        private bool IsPrimitive(Type type)
            => _knownTypes.Contains(type) && type != _uriType && type != _versionType;

        private bool IsStringBasedType(Type type)
            => type == _stringType || type == _dateTimeType || type == _dateTimeOffsetType || type == _guidType;
    }
}
