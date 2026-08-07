using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace KSeF.Client.XmlSerialization
{
    public sealed class XmlModelSerializer : IXmlModelSerializer
    {
        private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesByType = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private readonly XmlSerializationOptions options;

        public XmlModelSerializer()
            : this(null)
        {
        }

        public XmlModelSerializer(XmlSerializationOptions options)
        {
            this.options = options ?? new XmlSerializationOptions();
        }

        public string Serialize<T>(T value)
        {
            using (Utf8StringWriter stringWriter = new Utf8StringWriter(options.Encoding))
            {
                XmlWriterSettings settings = CreateWriterSettings();
                using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
                {
                    WriteRoot(writer, typeof(T), value);
                }

                return stringWriter.ToString();
            }
        }

        public void Serialize<T>(Stream stream, T value)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            XmlWriterSettings settings = CreateWriterSettings();
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                WriteRoot(writer, typeof(T), value);
            }
        }

        public T Deserialize<T>(string xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            using (StringReader reader = new StringReader(xml))
            {
                XDocument document = XDocument.Load(reader);
                return (T)ReadValue(document.Root, typeof(T), typeof(T).Name);
            }
        }

        public T Deserialize<T>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            XDocument document = XDocument.Load(stream);
            return (T)ReadValue(document.Root, typeof(T), typeof(T).Name);
        }

        private XmlWriterSettings CreateWriterSettings()
        {
            return new XmlWriterSettings
            {
                Indent = options.Indent,
                Encoding = options.Encoding,
                OmitXmlDeclaration = false
            };
        }

        private void WriteRoot(XmlWriter writer, Type declaredType, object value)
        {
            string rootName = string.IsNullOrWhiteSpace(options.RootElementName) ? declaredType.Name : options.RootElementName;
            WriteElement(writer, rootName, declaredType, value, declaredType.Name);
        }

        private void WriteElement(XmlWriter writer, string elementName, Type declaredType, object value, string path)
        {
            writer.WriteStartElement(elementName, options.Namespace);

            if (value == null)
            {
                if (options.EmitNullValues)
                {
                    writer.WriteAttributeString("xsi", "nil", XsiNamespace, "true");
                }

                writer.WriteEndElement();
                return;
            }

            Type valueType = value.GetType();
            if (TryFormatScalar(value, valueType, out string text))
            {
                writer.WriteString(text);
                writer.WriteEndElement();
                return;
            }

            if (TryGetDictionaryTypes(valueType, declaredType, out Type keyType, out Type valueItemType))
            {
                WriteDictionary(writer, (IEnumerable)value, keyType, valueItemType, path);
                writer.WriteEndElement();
                return;
            }

            if (TryGetEnumerableItemType(valueType, declaredType, out Type itemType))
            {
                WriteCollection(writer, (IEnumerable)value, itemType, path);
                writer.WriteEndElement();
                return;
            }

            foreach (PropertyInfo property in GetSerializableProperties(valueType))
            {
                object propertyValue = property.GetValue(value, null);
                if (propertyValue == null && !options.EmitNullValues)
                {
                    continue;
                }

                WriteElement(writer, property.Name, property.PropertyType, propertyValue, path + "." + property.Name);
            }

            writer.WriteEndElement();
        }

        private void WriteCollection(XmlWriter writer, IEnumerable value, Type itemType, string path)
        {
            foreach (object item in value)
            {
                WriteElement(writer, options.CollectionItemElementName, itemType, item, path + "[]");
            }
        }

        private void WriteDictionary(XmlWriter writer, IEnumerable value, Type keyType, Type valueType, string path)
        {
            foreach (object item in value)
            {
                object key = GetDictionaryEntryMember(item, "Key");
                object dictionaryValue = GetDictionaryEntryMember(item, "Value");

                writer.WriteStartElement(options.DictionaryItemElementName, options.Namespace);
                WriteElement(writer, options.DictionaryKeyElementName, keyType, key, path + ".Key");
                WriteElement(writer, options.DictionaryValueElementName, valueType, dictionaryValue, path + ".Value");
                writer.WriteEndElement();
            }
        }

        private object ReadValue(XElement element, Type targetType, string path)
        {
            if (element == null)
            {
                return null;
            }

            if (IsNil(element))
            {
                return null;
            }

            Type nullableType = Nullable.GetUnderlyingType(targetType);
            Type effectiveType = nullableType ?? targetType;

            if (nullableType != null && string.IsNullOrWhiteSpace(element.Value) && !element.HasElements)
            {
                return null;
            }

            if (TryParseScalar(element.Value, effectiveType, path, out object scalar))
            {
                return scalar;
            }

            if (TryGetDictionaryTypes(targetType, targetType, out Type keyType, out Type valueType))
            {
                return ReadDictionary(element, targetType, keyType, valueType, path);
            }

            if (TryGetEnumerableItemType(targetType, targetType, out Type itemType))
            {
                return ReadCollection(element, targetType, itemType, path);
            }

            object instance = CreateObject(targetType, path);
            foreach (PropertyInfo property in GetSerializableProperties(targetType))
            {
                XElement child = element.Elements().FirstOrDefault(x => x.Name.LocalName == property.Name);
                if (child == null)
                {
                    continue;
                }

                object propertyValue = ReadValue(child, property.PropertyType, path + "." + property.Name);
                property.SetValue(instance, propertyValue, null);
            }

            return instance;
        }

        private object ReadCollection(XElement element, Type targetType, Type itemType, string path)
        {
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
            foreach (XElement itemElement in element.Elements().Where(x => x.Name.LocalName == options.CollectionItemElementName))
            {
                list.Add(ReadValue(itemElement, itemType, path + "[]"));
            }

            if (targetType.IsArray)
            {
                Array array = Array.CreateInstance(itemType, list.Count);
                list.CopyTo(array, 0);
                return array;
            }

            if (targetType.IsAssignableFrom(list.GetType()))
            {
                return list;
            }

            object collection = CreateObject(targetType, path);
            if (collection is IList targetList)
            {
                foreach (object item in list)
                {
                    targetList.Add(item);
                }

                return targetList;
            }

            throw new XmlModelSerializationException("Cannot create collection for '" + path + "' of type '" + targetType.FullName + "'.");
        }

        private object ReadDictionary(XElement element, Type targetType, Type keyType, Type valueType, string path)
        {
            IDictionary dictionary = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
            foreach (XElement itemElement in element.Elements().Where(x => x.Name.LocalName == options.DictionaryItemElementName))
            {
                XElement keyElement = itemElement.Elements().FirstOrDefault(x => x.Name.LocalName == options.DictionaryKeyElementName);
                XElement valueElement = itemElement.Elements().FirstOrDefault(x => x.Name.LocalName == options.DictionaryValueElementName);
                object key = ReadValue(keyElement, keyType, path + ".Key");
                object value = ReadValue(valueElement, valueType, path + ".Value");

                if (dictionary.Contains(key))
                {
                    throw new XmlModelSerializationException("Duplicate dictionary key at '" + path + "': '" + Convert.ToString(key, options.FormatCulture) + "'.");
                }

                dictionary.Add(key, value);
            }

            if (targetType.IsAssignableFrom(dictionary.GetType()))
            {
                return dictionary;
            }

            object targetDictionary = CreateObject(targetType, path);
            if (targetDictionary is IDictionary assignableDictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    assignableDictionary.Add(entry.Key, entry.Value);
                }

                return assignableDictionary;
            }

            throw new XmlModelSerializationException("Cannot create dictionary for '" + path + "' of type '" + targetType.FullName + "'.");
        }

        private bool TryFormatScalar(object value, Type valueType, out string text)
        {
            Type nullableType = Nullable.GetUnderlyingType(valueType);
            Type effectiveType = nullableType ?? valueType;

            if (effectiveType == typeof(string))
            {
                text = (string)value;
                return true;
            }

            if (effectiveType == typeof(byte[]))
            {
                text = Convert.ToBase64String((byte[])value);
                return true;
            }

            if (effectiveType == typeof(Uri))
            {
                text = value.ToString();
                return true;
            }

            if (effectiveType == typeof(DateTimeOffset))
            {
                text = ((DateTimeOffset)value).ToString("O", options.FormatCulture);
                return true;
            }

            if (effectiveType == typeof(DateTime))
            {
                text = ((DateTime)value).ToString("O", options.FormatCulture);
                return true;
            }

            if (effectiveType == typeof(bool))
            {
                text = XmlConvert.ToString((bool)value);
                return true;
            }

            if (effectiveType == typeof(Guid))
            {
                text = ((Guid)value).ToString("D");
                return true;
            }

            if (effectiveType.IsEnum)
            {
                text = value.ToString();
                return true;
            }

            if (IsNumericType(effectiveType))
            {
                text = Convert.ToString(value, options.FormatCulture);
                return true;
            }

            text = null;
            return false;
        }

        private bool TryParseScalar(string text, Type targetType, string path, out object value)
        {
            if (targetType == typeof(string))
            {
                value = text;
                return true;
            }

            if (targetType == typeof(byte[]))
            {
                value = string.IsNullOrWhiteSpace(text) ? Array.Empty<byte>() : Convert.FromBase64String(text);
                return true;
            }

            if (targetType == typeof(Uri))
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    value = null;
                    return true;
                }

                if (Uri.TryCreate(text, UriKind.RelativeOrAbsolute, out Uri uri))
                {
                    value = uri;
                    return true;
                }

                throw new XmlModelSerializationException("Invalid URI at '" + path + "': '" + text + "'.");
            }

            if (targetType == typeof(DateTimeOffset))
            {
                value = DateTimeOffset.Parse(text, options.FormatCulture, DateTimeStyles.RoundtripKind);
                return true;
            }

            if (targetType == typeof(DateTime))
            {
                value = DateTime.Parse(text, options.FormatCulture, DateTimeStyles.RoundtripKind);
                return true;
            }

            if (targetType == typeof(bool))
            {
                value = XmlConvert.ToBoolean(text);
                return true;
            }

            if (targetType == typeof(Guid))
            {
                value = Guid.Parse(text);
                return true;
            }

            if (targetType.IsEnum)
            {
                value = Enum.Parse(targetType, text);
                return true;
            }

            if (IsNumericType(targetType))
            {
                value = Convert.ChangeType(text, targetType, options.FormatCulture);
                return true;
            }

            value = null;
            return false;
        }

        private static object CreateObject(Type targetType, string path)
        {
            try
            {
                return Activator.CreateInstance(targetType);
            }
            catch (MissingMethodException)
            {
                if (!targetType.IsClass)
                {
                    throw;
                }

                return FormatterServices.GetUninitializedObject(targetType);
            }
            catch (MemberAccessException)
            {
                if (!targetType.IsClass)
                {
                    throw;
                }

                return FormatterServices.GetUninitializedObject(targetType);
            }
            catch (Exception exception)
            {
                throw new XmlModelSerializationException("Cannot create object for '" + path + "' of type '" + targetType.FullName + "'.", exception);
            }
        }

        private static PropertyInfo[] GetSerializableProperties(Type type)
        {
            return PropertiesByType.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.MetadataToken)
                .ThenBy(p => p.Name, StringComparer.Ordinal)
                .ToArray());
        }

        private static bool TryGetEnumerableItemType(Type valueType, Type declaredType, out Type itemType)
        {
            itemType = null;
            Type type = declaredType;
            if (type == typeof(string) || type == typeof(byte[]))
            {
                return false;
            }

            if (type.IsArray)
            {
                itemType = type.GetElementType();
                return true;
            }

            Type enumerableType = type.GetInterfaces()
                .Concat(new[] { type })
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableType == null)
            {
                Type runtimeEnumerableType = valueType.GetInterfaces()
                    .Concat(new[] { valueType })
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                enumerableType = runtimeEnumerableType;
            }

            if (enumerableType == null)
            {
                return false;
            }

            itemType = enumerableType.GetGenericArguments()[0];
            return true;
        }

        private static bool TryGetDictionaryTypes(Type valueType, Type declaredType, out Type keyType, out Type valueItemType)
        {
            keyType = null;
            valueItemType = null;

            Type dictionaryType = declaredType.GetInterfaces()
                .Concat(new[] { declaredType })
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));

            if (dictionaryType == null)
            {
                dictionaryType = valueType.GetInterfaces()
                    .Concat(new[] { valueType })
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            }

            if (dictionaryType == null)
            {
                return false;
            }

            Type[] arguments = dictionaryType.GetGenericArguments();
            keyType = arguments[0];
            valueItemType = arguments[1];
            return true;
        }

        private static object GetDictionaryEntryMember(object item, string propertyName)
        {
            if (item is DictionaryEntry dictionaryEntry)
            {
                return propertyName == "Key" ? dictionaryEntry.Key : dictionaryEntry.Value;
            }

            PropertyInfo property = item.GetType().GetProperty(propertyName);
            return property.GetValue(item, null);
        }

        private static bool IsNil(XElement element)
        {
            XAttribute attribute = element.Attribute(XName.Get("nil", XsiNamespace));
            return attribute != null && string.Equals(attribute.Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }
    }
}
