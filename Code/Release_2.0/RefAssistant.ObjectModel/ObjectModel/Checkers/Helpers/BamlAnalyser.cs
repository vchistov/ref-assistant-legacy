//
// Author: lutzroeder
// 
// http://reflectoraddins.codeplex.com/
//
// Original class name is "Reflector.BamlViewer.BamlTranslator"
//

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Lardite.RefAssistant.ObjectModel.Checkers.Helpers
{
    /// <summary>
    /// Reader of BAML stream.
    /// </summary>
    internal sealed class BamlAnalyser : IXamlAnalyser
    {
        #region Fields

        private Dictionary<short, string> _assemblyTable;
        private Dictionary<short, string> _stringTable;
        private Dictionary<short, TypeDeclaration> _typeTable;
        private Dictionary<short, PropertyDeclaration> _propertyTable;
        private IList _staticResourceTable;

        private NamespaceManager _namespaceManager;

        private Element rootElement;

        private Stack _elementStack;
        private IList<Element> _constructorParameterTable;

        private TypeDeclaration[] _knownTypeTable;
        private PropertyDeclaration[] _knownPropertyTable;
        private Dictionary<int, ResourceName> _knownResourceTable;

        private Dictionary<Element, int> _dictionaryKeyStartTable;
        private Dictionary<Element, IDictionary> _dictionaryKeyPositionTable;

        private int _lineNumber;
        private int _linePosition;

        #endregion // Fields

        #region .ctor

        public BamlAnalyser(Stream bamlStream)
        {
            _assemblyTable = new Dictionary<short, string>();
            _stringTable = new Dictionary<short, string>();
            _typeTable = new Dictionary<short, TypeDeclaration>();
            _propertyTable = new Dictionary<short, PropertyDeclaration>();
            _staticResourceTable = new ArrayList();

            _namespaceManager = new NamespaceManager();

            _elementStack = new Stack();
            _constructorParameterTable = new List<Element>();

            _knownResourceTable = new Dictionary<int, ResourceName>();

            _dictionaryKeyStartTable = new Dictionary<Element, int>();
            _dictionaryKeyPositionTable = new Dictionary<Element, IDictionary>();

            ReadBamlStream(bamlStream);
        }

        #endregion // .ctor

        #region IXamlAnalyser implementation

        /// <summary>
        /// Gets types list which declared into XAML markup.
        /// </summary>
        /// <returns>Returns <see cref="Lardite.RefAssistant.ObjectModel.Checkers.XamlTypeDeclaration"/> collection.</returns>
        public IEnumerable<XamlTypeDeclaration> GetDeclaredTypes()
        {            
            return _typeTable.Select(t => new XamlTypeDeclaration(t.Value.Assembly, t.Value.Namespace, t.Value.Name));
        }

        #endregion //IXamlAnalyser implementation

        #region Public methods

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public override string ToString()
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                using (var indentationTextWriter = new IndentedTextWriter(stringWriter))
                {
                    WriteElement(this.rootElement, indentationTextWriter);
                    return stringWriter.ToString();
                }                
            }
        }

        #endregion // Public methods

        #region Private methods

        private void ReadBamlStream(Stream stream)
        {
            BamlBinaryReader reader = new BamlBinaryReader(stream);

            int length = reader.ReadInt32();
            string format = new string(new BinaryReader(stream, Encoding.Unicode).ReadChars(length >> 1));
            if (format != "MSBAML")
            {
                throw new NotSupportedException();
            }

            int readerVersion = reader.ReadInt32();
            int updateVersion = reader.ReadInt32();
            int writerVersion = reader.ReadInt32();
            if ((readerVersion != 0x00600000) || (updateVersion != 0x00600000) || (writerVersion != 0x00600000))
            {
                throw new NotSupportedException();
            }

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                BamlRecordType recordType = (BamlRecordType)reader.ReadByte();

                long position = reader.BaseStream.Position;
                int size = 0;

                switch (recordType)
                {
                    case BamlRecordType.XmlnsProperty:
                    case BamlRecordType.PresentationOptionsAttribute:
                    case BamlRecordType.PIMapping:
                    case BamlRecordType.AssemblyInfo:
                    case BamlRecordType.Property:
                    case BamlRecordType.PropertyWithConverter:
                    case BamlRecordType.PropertyCustom:
                    case BamlRecordType.DefAttribute:
                    case BamlRecordType.DefAttributeKeyString:
                    case BamlRecordType.TypeInfo:
                    case BamlRecordType.AttributeInfo:
                    case BamlRecordType.StringInfo:
                    case BamlRecordType.Text:
                    case BamlRecordType.TextWithConverter:
                    case BamlRecordType.TextWithId:
                        size = reader.ReadCompressedInt32();
                        break;
                }

                // Console.WriteLine(recordType.ToString());

                switch (recordType)
                {
                    case BamlRecordType.DocumentStart:
                        bool loadAsync = reader.ReadBoolean();
                        int maxAsyncRecords = reader.ReadInt32();
                        bool debugBaml = reader.ReadBoolean();
                        break;

                    case BamlRecordType.DocumentEnd:
                        break;

                    case BamlRecordType.ElementStart:
                        _namespaceManager.OnElementStart();
                        ReadElementStart(reader);
                        break;

                    case BamlRecordType.ElementEnd:
                        ReadElementEnd();
                        _namespaceManager.OnElementEnd();
                        break;

                    case BamlRecordType.KeyElementStart:
                        ReadKeyElementStart(reader);
                        break;

                    case BamlRecordType.KeyElementEnd:
                        ReadKeyElementEnd();
                        break;

                    case BamlRecordType.XmlnsProperty:
                        ReadXmlnsProperty(reader);
                        break;

                    case BamlRecordType.PIMapping:
                        ReadNamespaceMapping(reader);
                        break;

                    case BamlRecordType.PresentationOptionsAttribute:
                        ReadPresentationOptionsAttribute(reader);
                        break;

                    case BamlRecordType.AssemblyInfo:
                        ReadAssemblyInfo(reader);
                        break;

                    case BamlRecordType.StringInfo:
                        ReadStringInfo(reader);
                        break;

                    case BamlRecordType.ConnectionId:
                        reader.ReadInt32(); // ConnectionId
                        break;

                    case BamlRecordType.Property:
                        ReadPropertyRecord(reader);
                        break;

                    case BamlRecordType.PropertyWithConverter:
                        ReadPropertyWithConverter(reader);
                        break;

                    case BamlRecordType.PropertyWithExtension:
                        ReadPropertyWithExtension(reader);
                        break;

                    case BamlRecordType.PropertyTypeReference:
                        ReadPropertyTypeReference(reader);
                        break;

                    case BamlRecordType.PropertyWithStaticResourceId:
                        ReadPropertyWithStaticResourceIdentifier(reader);
                        break;

                    case BamlRecordType.ContentProperty:
                        ReadContentProperty(reader);
                        break;

                    case BamlRecordType.TypeInfo:
                        ReadTypeInfo(reader);
                        break;

                    case BamlRecordType.AttributeInfo:
                        ReadAttributeInfo(reader);
                        break;

                    case BamlRecordType.DefAttribute:
                        ReadDefAttribute(reader);
                        break;

                    case BamlRecordType.DefAttributeKeyString:
                        ReadDefAttributeKeyString(reader);
                        break;

                    case BamlRecordType.DefAttributeKeyType:
                        ReadDefAttributeKeyType(reader);
                        break;

                    case BamlRecordType.Text:
                        ReadText(reader);
                        break;

                    case BamlRecordType.TextWithConverter:
                        ReadTextWithConverter(reader);
                        break;

                    case BamlRecordType.PropertyCustom:
                        ReadPropertyCustom(reader);
                        break;

                    case BamlRecordType.PropertyListStart:
                        ReadPropertyListStart(reader);
                        break;

                    case BamlRecordType.PropertyListEnd:
                        ReadPropertyListEnd();
                        break;

                    case BamlRecordType.PropertyDictionaryStart:
                        ReadPropertyDictionaryStart(reader);
                        break;

                    case BamlRecordType.PropertyDictionaryEnd:
                        ReadPropertyDictionaryEnd();
                        break;

                    case BamlRecordType.PropertyComplexStart:
                        ReadPropertyComplexStart(reader);
                        break;

                    case BamlRecordType.PropertyComplexEnd:
                        ReadPropertyComplexEnd();
                        break;

                    case BamlRecordType.ConstructorParametersStart:
                        ReadConstructorParametersStart();
                        break;

                    case BamlRecordType.ConstructorParametersEnd:
                        ReadConstructorParametersEnd();
                        break;

                    case BamlRecordType.ConstructorParameterType:
                        ReadConstructorParameterType(reader);
                        break;

                    case BamlRecordType.DeferableContentStart:
                        int contentSize = reader.ReadInt32();
                        break;

                    case BamlRecordType.StaticResourceStart:
                        ReadStaticResourceStart(reader);
                        break;

                    case BamlRecordType.StaticResourceEnd:
                        ReadStaticResourceEnd(reader);
                        break;

                    case BamlRecordType.StaticResourceId:
                        ReadStaticResourceIdentifier(reader);
                        break;

                    case BamlRecordType.OptimizedStaticResource:
                        ReadOptimizedStaticResource(reader);
                        break;

                    case BamlRecordType.LineNumberAndPosition:
                        _lineNumber = reader.ReadInt32(); // LineNumber
                        _linePosition = reader.ReadInt32(); // Position
                        // Console.WriteLine(_lineNumber.ToString() + "," + _linePosition.ToString());
                        break;

                    case BamlRecordType.LinePosition:
                        _linePosition = reader.ReadInt32(); // Position
                        break;

                    case BamlRecordType.TextWithId:
                        ReadTextWithId(reader);
                        break;

                    default:
                        throw new NotSupportedException(recordType.ToString());
                }

                if (size > 0)
                {
                    reader.BaseStream.Position = position + size;
                }
            }
        }

        private static void WriteElement(Element element, IndentedTextWriter writer)
        {
            writer.Write("<");
            WriteTypeDeclaration(element.TypeDeclaration, writer);

            ArrayList attributes = new ArrayList();
            ArrayList properties = new ArrayList();
            Property contentProperty = null;
            foreach (Property property in element.Properties)
            {
                switch (property.PropertyType)
                {
                    case PropertyType.List:
                    case PropertyType.Dictionary:
                        properties.Add(property);
                        break;

                    case PropertyType.Namespace:
                    case PropertyType.Value:
                    case PropertyType.Declaration:
                        attributes.Add(property);
                        break;

                    case PropertyType.Complex:
                        if (IsExtension(property.Value))
                        {
                            attributes.Add(property);
                        }
                        else
                        {
                            properties.Add(property);
                        }
                        break;

                    case PropertyType.Content:
                        contentProperty = property;
                        break;
                }
            }

            foreach (Property property in attributes)
            {
                writer.Write(" ");
                WritePropertyDeclaration(property.PropertyDeclaration, element.TypeDeclaration, writer);
                writer.Write("=");
                writer.Write("\"");

                switch (property.PropertyType)
                {
                    case PropertyType.Complex:
                        WriteComplexElement((Element)property.Value, writer);
                        break;

                    case PropertyType.Namespace:
                    case PropertyType.Declaration:
                    case PropertyType.Value:
                        writer.Write(property.Value.ToString());
                        break;

                    default:
                        throw new NotSupportedException();
                }


                writer.Write("\"");
            }

            if ((contentProperty != null) || (properties.Count > 0))
            {
                writer.Write(">");

                if ((properties.Count > 0) || (contentProperty.Value is IList))
                {
                    writer.WriteLine();
                }

                writer.Indent++;

                foreach (Property property in properties)
                {
                    writer.Write("<");
                    WriteTypeDeclaration(element.TypeDeclaration, writer);
                    writer.Write(".");
                    WritePropertyDeclaration(property.PropertyDeclaration, element.TypeDeclaration, writer);
                    writer.Write(">");
                    writer.WriteLine();
                    writer.Indent++;
                    WritePropertyValue(property, writer);
                    writer.Indent--;
                    writer.Write("</");
                    WriteTypeDeclaration(element.TypeDeclaration, writer);
                    writer.Write(".");
                    WritePropertyDeclaration(property.PropertyDeclaration, element.TypeDeclaration, writer);
                    writer.Write(">");
                    writer.WriteLine();
                }

                if (contentProperty != null)
                {
                    WritePropertyValue(contentProperty, writer);
                }

                writer.Indent--;

                writer.Write("</");
                WriteTypeDeclaration(element.TypeDeclaration, writer);
                writer.Write(">");
            }
            else
            {
                writer.Write(" />");
            }

            writer.WriteLine();
        }

        private static void WriteTypeDeclaration(TypeDeclaration value, TextWriter writer)
        {
            writer.Write(value.ToString());
        }

        private static void WritePropertyDeclaration(PropertyDeclaration value, TypeDeclaration context, TextWriter writer)
        {
            writer.Write(value.ToString());
        }

        private static void WritePropertyValue(Property property, IndentedTextWriter writer)
        {
            object value = property.Value;

            if (value is IList)
            {
                IList elements = (IList)value;

                if ((property.PropertyDeclaration != null) && (property.PropertyDeclaration.Name == "Resources") && (elements.Count == 1) && (elements[0] is Element))
                {
                    Element element = (Element)elements[0];
                    if ((element.TypeDeclaration.Name == "ResourceDictionary") && (element.TypeDeclaration.Namespace == "System.Windows") && (element.TypeDeclaration.Assembly == "PresentationFramework") && (element.Arguments.Count == 0) && (element.Properties.Count == 1) && (element.Properties[0].PropertyType == PropertyType.Content))
                    {
                        WritePropertyValue(element.Properties[0], writer);
                        return;
                    }
                }

                foreach (object child in elements)
                {
                    if (child is string)
                    {
                        writer.Write((string)child);
                    }
                    else if (child is Element)
                    {
                        WriteElement((Element)child, writer);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
            else if (value is string)
            {
                string text = (string)value;
                writer.Write(text);
            }
            else if (value is Element)
            {
                Element element = (Element)value;
                WriteElement(element, writer);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private static void WriteComplexElement(Element element, TextWriter writer)
        {
            if (element.TypeDeclaration.Name == "Binding")
            {
                Console.WriteLine();
            }

            writer.Write("{");

            string name = element.TypeDeclaration.ToString();

            if (name.EndsWith("Extension"))
            {
                name = name.Substring(0, name.Length - 9);
            }

            writer.Write(name);

            if ((element.Arguments.Count > 0) || (element.Properties.Count > 0))
            {
                if (element.Arguments.Count > 0)
                {
                    writer.Write(" ");

                    for (int i = 0; i < element.Arguments.Count; i++)
                    {
                        if (i != 0)
                        {
                            writer.Write(", ");
                        }

                        if (element.Arguments[i] is string)
                        {
                            writer.Write((string)element.Arguments[i]);
                        }
                        else if (element.Arguments[i] is TypeDeclaration)
                        {
                            WriteTypeDeclaration((TypeDeclaration)element.Arguments[i], writer);
                        }
                        else if (element.Arguments[i] is PropertyDeclaration)
                        {
                            PropertyDeclaration propertyName = (PropertyDeclaration)element.Arguments[i];
                            writer.Write(propertyName.Name);
                        }
                        else if (element.Arguments[i] is ResourceName)
                        {
                            ResourceName resourceName = (ResourceName)element.Arguments[i];
                            writer.Write(resourceName.Name);
                        }
                        else if (element.Arguments[i] is Element)
                        {
                            WriteComplexElement((Element)element.Arguments[i], writer);
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }

                if (element.Properties.Count > 0)
                {
                    writer.Write(" ");

                    for (int i = 0; i < element.Properties.Count; i++)
                    {
                        if ((i != 0) || (element.Arguments.Count > 0))
                        {
                            writer.Write(", ");
                        }

                        WritePropertyDeclaration(element.Properties[i].PropertyDeclaration, element.TypeDeclaration, writer);
                        writer.Write("=");

                        if (element.Properties[i].Value is string)
                        {
                            writer.Write((string)element.Properties[i].Value);
                        }
                        else if (element.Properties[i].Value is Element)
                        {
                            WriteComplexElement((Element)element.Properties[i].Value, writer);
                        }
                        else if (element.Properties[i].Value is PropertyDeclaration)
                        {
                            // We had a templatebinding with a converter=,property= syntax. The value
                            // of the property was a Property instance in which case we should just
                            // write out the value.
                            Property prop = (Property)element.Properties[i];
                            writer.Write(prop.Value.ToString());
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }

            writer.Write("}");
        }

        private static bool IsExtension(object value)
        {
            Element element = value as Element;
            if (element != null)
            {
                if (element.Arguments.Count == 0)
                {
                    foreach (Property property in element.Properties)
                    {
                        if (property.PropertyType == PropertyType.Declaration)
                        {
                            return false;
                        }

                        if (!IsExtension(property.Value))
                        {
                            return false;
                        }

                        // An element with property content such as the following should
                        // not be considered an extension.
                        //
                        // e.g. <Button><Button.Content><sys:DateTime>12/25/2008</sys:DateTime></Button.Content></Button>
                        //
                        if (property.PropertyType == PropertyType.Content)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            IList list = value as IList;
            if (list != null)
            {
                return false;
            }

            return true;
        }

        private void AddElementToTree(Element element, BamlBinaryReader reader)
        {
            if (this.rootElement == null)
            {
                this.rootElement = element;
            }
            else
            {
                Property property = _elementStack.Peek() as Property;
                if (property != null)
                {
                    switch (property.PropertyType)
                    {
                        case PropertyType.List:
                        case PropertyType.Dictionary:
                            IList elements = (IList)property.Value;
                            elements.Add(element);
                            break;

                        case PropertyType.Complex:
                            property.Value = element;
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                }
                else
                {
                    Element parent = _elementStack.Peek() as Element;

                    if (_dictionaryKeyPositionTable.ContainsKey(parent))
                    {
                        int currentPosition = (int)(reader.BaseStream.Position - 1);

                        if (!_dictionaryKeyStartTable.ContainsKey(parent))
                        {
                            _dictionaryKeyStartTable.Add(parent, currentPosition);
                        }

                        int startPosition = _dictionaryKeyStartTable[parent];

                        int position = currentPosition - startPosition;

                        IDictionary keyPositionTable;
                        _dictionaryKeyPositionTable.TryGetValue(parent, out keyPositionTable);
                        if ((keyPositionTable != null) && (keyPositionTable.Contains(position)))
                        {
                            IList list = (IList)keyPositionTable[position];
                            foreach (Property keyProperty in list)
                            {
                                element.Properties.Add(keyProperty);
                            }
                        }
                    }

                    if (parent != null)
                    {
                        // The element could be a parameter to a constructor - e.g. the Type
                        // for a ComponentResourceKey in which case it should be an argument
                        // of that element.
                        //
                        if (_constructorParameterTable.Count > 0 &&
                            _constructorParameterTable[_constructorParameterTable.Count - 1] == parent)
                        {
                            parent.Arguments.Add(element);
                        }
                        else
                        {
                            AddContent(parent, element);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
        }

        private void ReadAssemblyInfo(BamlBinaryReader reader)
        {
            short assemblyIdentifier = reader.ReadInt16();
            string assemblyName = reader.ReadString();
            _assemblyTable.Add(assemblyIdentifier, assemblyName);
        }

        private void ReadPresentationOptionsAttribute(BamlBinaryReader reader)
        {
            string value = reader.ReadString();
            short nameIdentifier = reader.ReadInt16();

            Property property = new Property(PropertyType.Value);
            property.PropertyDeclaration = new PropertyDeclaration("PresentationOptions:" + _stringTable[nameIdentifier]);
            property.Value = value;

            Element element = (Element)_elementStack.Peek();
            element.Properties.Add(property);
        }

        private void ReadStringInfo(BamlBinaryReader reader)
        {
            short stringIdentifier = reader.ReadInt16();
            string value = reader.ReadString();

            // This isn't a bug but more of a usability issue. MS tends to use 
            // single character identifiers which makes it difficult to find 
            // the associated resource.
            //
            if (null != value && value.Length == 1)
                value = string.Format("[{0}] {1}", stringIdentifier, value);

            _stringTable.Add(stringIdentifier, value);
        }

        private void ReadTypeInfo(BamlBinaryReader reader)
        {
            short typeIdentifier = reader.ReadInt16();
            short assemblyIdentifier = reader.ReadInt16();
            string typeFullName = reader.ReadString();

            assemblyIdentifier = (short)(assemblyIdentifier & 0x0fff);
            string assembly = _assemblyTable[assemblyIdentifier];

            TypeDeclaration typeDeclaration = null;

            int index = typeFullName.LastIndexOf('.');
            if (index != -1)
            {
                string name = typeFullName.Substring(index + 1);
                string namespaceName = typeFullName.Substring(0, index);
                typeDeclaration = new TypeDeclaration(name, namespaceName, assembly);
            }
            else
            {
                typeDeclaration = new TypeDeclaration(typeFullName, string.Empty, assembly);
            }

            _typeTable.Add(typeIdentifier, typeDeclaration);
        }

        private void ReadAttributeInfo(BamlBinaryReader reader)
        {
            short attributeIdentifier = reader.ReadInt16();
            short ownerTypeIdentifier = reader.ReadInt16();
            BamlAttributeUsage attributeUsage = (BamlAttributeUsage)reader.ReadByte();
            string attributeName = reader.ReadString();

            TypeDeclaration declaringType = this.GetTypeDeclaration(ownerTypeIdentifier);
            PropertyDeclaration propertyName = new PropertyDeclaration(attributeName, declaringType);
            _propertyTable.Add(attributeIdentifier, propertyName);
        }

        private void ReadElementStart(BamlBinaryReader reader)
        {
            short typeIdentifier = reader.ReadInt16();
            byte flags = reader.ReadByte(); // 1 = CreateUsingTypeConverter, 2 = Injected

            Element element = new Element();
            element.TypeDeclaration = this.GetTypeDeclaration(typeIdentifier);

            AddElementToTree(element, reader);

            _elementStack.Push(element);
        }

        private void ReadElementEnd()
        {
            Property property = _elementStack.Peek() as Property;
            if ((property != null) && (property.PropertyType == PropertyType.Dictionary))
            {
                property = null;
            }
            else
            {
                Element element = (Element)_elementStack.Pop();

                Property contentProperty = GetContentProperty(element);
                if ((contentProperty != null) && (contentProperty.Value == null))
                {
                    element.Properties.Remove(contentProperty);
                }

                if (element.TypeDeclaration == this.GetTypeDeclaration(-0x0078))
                {
                    bool removeKey = false;

                    for (int i = element.Properties.Count - 1; i >= 0; i--)
                    {
                        if ((element.Properties[i].PropertyDeclaration != null) && (element.Properties[i].PropertyDeclaration.Name == "DataType"))
                        {
                            removeKey = true;
                            break;
                        }
                    }

                    if (removeKey)
                    {
                        for (int i = element.Properties.Count - 1; i >= 0; i--)
                        {
                            if ((element.Properties[i].PropertyDeclaration != null) && (element.Properties[i].PropertyDeclaration.Name == "x:Key"))
                            {
                                element.Properties.Remove(element.Properties[i]);
                            }
                        }
                    }
                }
            }
        }

        private void ReadStaticResourceStart(BamlBinaryReader reader)
        {
            short typeIdentifier = reader.ReadInt16();
            byte flags = reader.ReadByte(); // 1 = CreateUsingTypeConverter, 2 = Injected

            Element element = new Element();
            element.TypeDeclaration = GetTypeDeclaration(typeIdentifier);

            _elementStack.Push(element);

            _staticResourceTable.Add(element);
        }

        private void ReadStaticResourceEnd(BamlBinaryReader reader)
        {
            _elementStack.Pop();
        }

        private void ReadKeyElementStart(BamlBinaryReader reader)
        {
            short typeIdentifier = reader.ReadInt16();

            byte flags = reader.ReadByte();
            bool createUsingTypeConverter = ((flags & 1) != 0);
            bool injected = ((flags & 2) != 0);

            int position = reader.ReadInt32();
            bool shared = reader.ReadBoolean();
            bool sharedSet = reader.ReadBoolean();

            Property keyProperty = new Property(PropertyType.Complex);
            keyProperty.PropertyDeclaration = new PropertyDeclaration("x:Key");
            // At least for the case where we are processing the key of a dictionary,
            // we should not assume that the key is a type. This is particularly true
            // when the type is a ComponentResourceKey.
            //
            //keyProperty.Value = this.CreateTypeExtension(typeIdentifier);
            keyProperty.Value = this.CreateTypeExtension(typeIdentifier, false);

            Element dictionary = (Element)_elementStack.Peek();
            AddDictionaryEntry(dictionary, position, keyProperty);
            _elementStack.Push(keyProperty.Value);
        }

        private void ReadKeyElementEnd()
        {
            Element parent = (Element)_elementStack.Pop();
        }

        private void ReadConstructorParametersStart()
        {
            Element element = (Element)_elementStack.Peek();
            _constructorParameterTable.Add(element);
        }

        private void ReadConstructorParametersEnd()
        {
            Element element = (Element)_elementStack.Peek();
            _constructorParameterTable.Remove(element);
        }

        private void ReadConstructorParameterType(BamlBinaryReader reader)
        {
            short typeIdentifier = reader.ReadInt16();

            TypeDeclaration elementName = this.GetTypeDeclaration(typeIdentifier);

            Element element = (Element)_elementStack.Peek();
            element.Arguments.Add(elementName);
        }

        private void ReadOptimizedStaticResource(BamlBinaryReader reader)
        {
            byte extension = reader.ReadByte(); // num1
            short valueIdentifier = reader.ReadInt16();

            bool typeExtension = ((extension & 1) == 1);
            bool staticExtension = ((extension & 2) == 2);

            object element = null;

            if (typeExtension)
            {
                Element innerElement = this.CreateTypeExtension(valueIdentifier);
                element = innerElement;
            }
            else if (staticExtension)
            {
                Element innerElement = new Element();
                innerElement.TypeDeclaration = new TypeDeclaration("x:Static");
                // innerElement.TypeDeclaration = new TypeDeclaration("StaticExtension", "System.Windows.Markup);

                ResourceName resourceName = GetResourceName(valueIdentifier);
                innerElement.Arguments.Add(resourceName);

                element = innerElement;
            }
            else
            {
                string value;
                _stringTable.TryGetValue(valueIdentifier, out value);
                element = value;
            }

            _staticResourceTable.Add(element);
        }

        private void ReadPropertyWithExtension(BamlBinaryReader reader)
        {
            short attributeIdentifier = reader.ReadInt16();

            // 0x025b StaticResource
            // 0x027a TemplateBinding
            // 0x00bd DynamicResource
            short extension = reader.ReadInt16();
            short valueIdentifier = reader.ReadInt16();

            bool typeExtension = ((extension & 0x4000) == 0x4000);
            bool staticExtension = ((extension & 0x2000) == 0x2000);

            extension = (short)(extension & 0x0fff);

            Property property = new Property(PropertyType.Complex);
            property.PropertyDeclaration = this.GetPropertyDeclaration(attributeIdentifier);

            short typeIdentifier = (short)-(extension & 0x0fff);

            Element element = new Element();
            element.TypeDeclaration = this.GetTypeDeclaration(typeIdentifier);

            switch (extension)
            {
                case 0x00bd: // DynamicResource
                case 0x025b: // StaticResource
                    {
                        if (typeExtension)
                        {
                            Element innerElement = this.CreateTypeExtension(valueIdentifier);
                            element.Arguments.Add(innerElement);
                        }
                        else if (staticExtension)
                        {
                            Element innerElement = new Element();
                            innerElement.TypeDeclaration = new TypeDeclaration("x:Static");
                            // innerElement.TypeDeclaration = new TypeDeclaration("StaticExtension", "System.Windows.Markup);

                            ResourceName resourceName = GetResourceName(valueIdentifier);
                            innerElement.Arguments.Add(resourceName);

                            element.Arguments.Add(innerElement);
                        }
                        else
                        {
                            string value = _stringTable[valueIdentifier];
                            element.Arguments.Add(value);
                        }
                    }
                    break;

                case 0x25a: // Static
                    {
                        ResourceName resourceName = GetResourceName(valueIdentifier);
                        element.Arguments.Add(resourceName);
                    }
                    break;

                case 0x027a: // TemplateBinding
                    {
                        PropertyDeclaration propertyName = this.GetPropertyDeclaration(valueIdentifier);
                        element.Arguments.Add(propertyName);
                    }
                    break;

                default:
                    throw new NotSupportedException("Unknown property with extension");
            }

            property.Value = element;

            Element parent = (Element)_elementStack.Peek();
            parent.Properties.Add(property);
        }

        private void ReadStaticResourceIdentifier(BamlBinaryReader reader)
        {
            short staticResourceIdentifier = reader.ReadInt16();
            object staticResource = this.GetStaticResource(staticResourceIdentifier);
            Element staticResourceElement = new Element();
            staticResourceElement.TypeDeclaration = this.GetTypeDeclaration(-0x25b);
            staticResourceElement.Arguments.Add(staticResource);

            this.AddElementToTree(staticResourceElement, reader);
        }

        private void ReadPropertyWithStaticResourceIdentifier(BamlBinaryReader reader)
        {
            short attributeIdentifier = reader.ReadInt16();

            short staticResourceIdentifier = reader.ReadInt16();
            object staticResource = this.GetStaticResource(staticResourceIdentifier);

            Element staticResourcEelement = new Element();
            staticResourcEelement.TypeDeclaration = this.GetTypeDeclaration(-0x25b);
            staticResourcEelement.Arguments.Add(staticResource);

            Property property = new Property(PropertyType.Complex);
            property.PropertyDeclaration = this.GetPropertyDeclaration(attributeIdentifier);
            property.Value = staticResourcEelement;

            Element parent = (Element)_elementStack.Peek();
            parent.Properties.Add(property);
        }

        private void ReadPropertyTypeReference(BamlBinaryReader reader)
        {
            short attributeIdentifier = reader.ReadInt16();
            short typeIdentifier = reader.ReadInt16();

            Property property = new Property(PropertyType.Complex);
            property.PropertyDeclaration = this.GetPropertyDeclaration(attributeIdentifier);
            property.Value = this.CreateTypeExtension(typeIdentifier);

            Element parent = (Element)_elementStack.Peek();
            parent.Properties.Add(property);
        }

        private void ReadPropertyRecord(BamlBinaryReader reader)
        {
            short attributeIdentifier = reader.ReadInt16();
            string value = reader.ReadString();

            Property property = new Property(PropertyType.Value);
            property.PropertyDeclaration = this.GetPropertyDeclaration(attributeIdentifier);
            property.Value = value;

            Element element = (Element)_elementStack.Peek();
            element.Properties.Add(property);
        }

        private void ReadPropertyWithConverter(BamlBinaryReader reader)
        {
            short attributeIdentifier = reader.ReadInt16();
            string value = reader.ReadString();
            short converterTypeIdentifier = reader.ReadInt16();

            Property property = new Property(PropertyType.Value);
            property.PropertyDeclaration = this.GetPropertyDeclaration(attributeIdentifier);
            property.Value = value;

            Element element = (Element)_elementStack.Peek();
            element.Properties.Add(property);
        }

        private void ReadPropertyCustom(BamlBinaryReader reader)
        {
            short attributeIdentifier = reader.ReadInt16();
            short serializerTypeIdentifier = reader.ReadInt16();
            bool typeIdentifier = (serializerTypeIdentifier & 0x4000) == 0x4000;
            serializerTypeIdentifier = (short)(serializerTypeIdentifier & ~0x4000);

            Property property = new Property(PropertyType.Value);
            property.PropertyDeclaration = this.GetPropertyDeclaration(attributeIdentifier);

            switch (serializerTypeIdentifier)
            {
                // PropertyReference
                case 0x0089:
                    short identifier = reader.ReadInt16();
                    if (typeIdentifier)
                    {
                        TypeDeclaration declaringType = this.GetTypeDeclaration(identifier);
                        string propertyName = reader.ReadString();
                        property.Value = new PropertyDeclaration(propertyName, declaringType);
                    }
                    else
                    {
                        property.Value = this.GetPropertyDeclaration(identifier);
                    }
                    break;

                case 0x002e: // Boolean
                    int value = reader.ReadByte();
                    property.Value = (value == 0x01) ? "true" : "false";
                    break;

                case 0x02e8: // SolidColorBrush
                    switch (reader.ReadByte())
                    {
                        case 0x01: // KnownSolidColor
                            uint color = reader.ReadUInt32();
                            switch (color)
                            {
                                case 0x00ffffff:
                                    property.Value = "Transparent";
                                    break;

                                default:
                                    property.Value = KnownColors.KnownColorFromUInt(color);
                                    break;
                            }
                            break;

                        case 0x02: // OtherColor
                            property.Value = reader.ReadString();
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                    break;

                case 0x02e9: // IntCollection
                    using (StringWriter writer = new StringWriter())
                    {
                        byte format = reader.ReadByte();
                        int count = reader.ReadInt32();

                        switch (format)
                        {
                            case 0x01: // Consecutive
                                for (int i = 0; i < count; i++)
                                {
                                    if (i != 0)
                                    {
                                        writer.Write(",");
                                    }

                                    int number = reader.ReadInt32();
                                    writer.Write(number.ToString());
                                    if (number > count)
                                    {
                                        break;
                                    }
                                }
                                break;

                            case 0x02: // Byte
                                for (int i = 0; i < count; i++)
                                {
                                    if (i != 0)
                                    {
                                        writer.Write(",");
                                    }

                                    int number = reader.ReadByte();
                                    writer.Write(number.ToString());
                                }
                                break;

                            case 0x03: // UInt16
                                for (int i = 0; i < count; i++)
                                {
                                    if (i != 0)
                                    {
                                        writer.Write(",");
                                    }

                                    int number = reader.ReadUInt16();
                                    writer.Write(number.ToString());
                                }
                                break;

                            case 0x04: // UInt32
                                throw new NotSupportedException();

                            default:
                                throw new NotSupportedException();
                        }

                        property.Value = writer.ToString();
                    }
                    break;

                case 0x02ea: // PathData
                    property.Value = PathDataParser.ParseStreamGeometry(reader);
                    break;

                case 0x02ec: // Point
                    using (StringWriter writer = new StringWriter())
                    {
                        int count = reader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            if (i != 0)
                            {
                                writer.Write(" ");
                            }

                            for (int j = 0; j < 2; j++)
                            {
                                if (j != 0)
                                {
                                    writer.Write(",");
                                }

                                double number = reader.ReadCompressedDouble();
                                writer.Write(number.ToString());
                            }
                        }

                        property.Value = writer.ToString();
                    }
                    break;

                case 0x02eb: // Point3D
                case 0x02f0: // Vector3D
                    using (StringWriter writer = new StringWriter())
                    {
                        int count = reader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            if (i != 0)
                            {
                                writer.Write(" ");
                            }

                            for (int j = 0; j < 3; j++)
                            {
                                if (j != 0)
                                {
                                    writer.Write(",");
                                }

                                double number = reader.ReadCompressedDouble();
                                writer.Write(number.ToString());
                            }
                        }

                        property.Value = writer.ToString();
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }

            Element element = (Element)_elementStack.Peek();
            element.Properties.Add(property);
        }

        private void ReadContentProperty(BamlBinaryReader reader)
        {
            short attributeIdentifier = reader.ReadInt16();

            Element element = (Element)_elementStack.Peek();

            Property contentProperty = this.GetContentProperty(element);
            if (contentProperty == null)
            {
                contentProperty = new Property(PropertyType.Content);
                element.Properties.Add(contentProperty);
            }

            PropertyDeclaration propertyName = this.GetPropertyDeclaration(attributeIdentifier);
            if ((contentProperty.PropertyDeclaration != null) && (contentProperty.PropertyDeclaration != propertyName))
            {
                throw new NotSupportedException();
            }

            contentProperty.PropertyDeclaration = propertyName;
        }

        private void ReadPropertyListStart(BamlBinaryReader reader)
        {
            short attributeIdentifier = reader.ReadInt16();

            Property property = new Property(PropertyType.List);
            property.Value = new ArrayList();
            property.PropertyDeclaration = this.GetPropertyDeclaration(attributeIdentifier);

            Element element = (Element)_elementStack.Peek();
            element.Properties.Add(property);

            _elementStack.Push(property);
        }

        private void ReadPropertyListEnd()
        {
            Property property = (Property)_elementStack.Pop();
            if (property.PropertyType != PropertyType.List)
            {
                throw new NotSupportedException();
            }
        }

        private void ReadPropertyDictionaryStart(BamlBinaryReader reader)
        {
            short attributeIdentifier = reader.ReadInt16();

            Property property = new Property(PropertyType.Dictionary);
            property.Value = new ArrayList();
            property.PropertyDeclaration = this.GetPropertyDeclaration(attributeIdentifier);

            Element element = (Element)_elementStack.Peek();
            element.Properties.Add(property);

            _elementStack.Push(property);
        }

        private void ReadPropertyDictionaryEnd()
        {
            Property property = (Property)_elementStack.Pop();
            if (property.PropertyType != PropertyType.Dictionary)
            {
                throw new NotSupportedException();
            }
        }

        private void ReadPropertyComplexStart(BamlBinaryReader reader)
        {
            short attributeIdentifier = reader.ReadInt16();

            Property property = new Property(PropertyType.Complex);
            property.PropertyDeclaration = this.GetPropertyDeclaration(attributeIdentifier);

            if (property.PropertyDeclaration.Name == "RelativeTransform")
            {
                Console.WriteLine();
            }

            Element element = (Element)_elementStack.Peek();
            element.Properties.Add(property);

            _elementStack.Push(property);
        }

        private void ReadPropertyComplexEnd()
        {
            Property property = (Property)_elementStack.Pop();
            if (property.PropertyType != PropertyType.Complex)
            {
                throw new NotSupportedException();
            }
        }

        private void ReadXmlnsProperty(BamlBinaryReader reader)
        {
            string prefix = reader.ReadString();
            string xmlNamespace = reader.ReadString();

            string[] assemblies = new string[reader.ReadInt16()];
            for (int i = 0; i < assemblies.Length; i++)
            {
                assemblies[i] = _assemblyTable[reader.ReadInt16()];
            }

            Property property = new Property(PropertyType.Namespace);
            property.PropertyDeclaration = new PropertyDeclaration(prefix, new TypeDeclaration("XmlNamespace", null, null));
            property.Value = xmlNamespace;

            _namespaceManager.AddMapping(prefix, xmlNamespace);

            Element element = (Element)_elementStack.Peek();
            element.Properties.Add(property);
        }

        private void ReadNamespaceMapping(BamlBinaryReader reader)
        {
            string xmlNamespace = reader.ReadString();
            string clrNamespace = reader.ReadString();
            short assemblyIdentifier = reader.ReadInt16();
            string assembly = _assemblyTable[assemblyIdentifier];

            _namespaceManager.AddNamespaceMapping(xmlNamespace, clrNamespace, assembly);
        }

        private void ReadDefAttribute(BamlBinaryReader reader)
        {
            string value = reader.ReadString();
            short attributeIdentifier = reader.ReadInt16();

            Property property = new Property(PropertyType.Declaration);

            switch (attributeIdentifier)
            {
                case -1:
                    property.PropertyDeclaration = new PropertyDeclaration("x:Name");
                    break;

                case -2:
                    property.PropertyDeclaration = new PropertyDeclaration("x:Uid");
                    break;

                default:
                    property.PropertyDeclaration = this.GetPropertyDeclaration(attributeIdentifier);
                    break;
            }

            property.Value = value;

            Element element = (Element)_elementStack.Peek();
            element.Properties.Add(property);
        }

        private void ReadDefAttributeKeyString(BamlBinaryReader reader)
        {
            short valueIdentifier = reader.ReadInt16();
            int position = reader.ReadInt32();
            bool shared = reader.ReadBoolean();
            bool sharedSet = reader.ReadBoolean();

            if (!_stringTable.ContainsKey(valueIdentifier))
            {
                throw new NotSupportedException();
            }

            Property keyProperty = new Property(PropertyType.Value);
            keyProperty.PropertyDeclaration = new PropertyDeclaration("x:Key");
            keyProperty.Value = _stringTable[valueIdentifier];

            Element dictionary = (Element)_elementStack.Peek();

            this.AddDictionaryEntry(dictionary, position, keyProperty);
        }

        private void ReadDefAttributeKeyType(BamlBinaryReader reader)
        {
            short typeIdentifier = reader.ReadInt16();
            reader.ReadByte();
            int position = reader.ReadInt32();
            bool shared = reader.ReadBoolean();
            bool sharedSet = reader.ReadBoolean();

            Property keyProperty = new Property(PropertyType.Complex);
            keyProperty.PropertyDeclaration = new PropertyDeclaration("x:Key");
            keyProperty.Value = this.CreateTypeExtension(typeIdentifier);

            Element dictionary = (Element)_elementStack.Peek();

            this.AddDictionaryEntry(dictionary, position, keyProperty);
        }

        private void ReadText(BamlBinaryReader reader)
        {
            string value = reader.ReadString();
            ReadText(value);
        }

        private void ReadText(string value)
        {
            Element parent = (Element)_elementStack.Peek();
            if (_constructorParameterTable.Contains(parent))
            {
                parent.Arguments.Add(value);
            }
            else
            {
                AddContent(parent, value);
            }
        }

        private void ReadTextWithId(BamlBinaryReader reader)
        {
            short id = reader.ReadInt16();
            string value;
            _stringTable.TryGetValue(id, out value);
            ReadText(value);
        }

        private void ReadTextWithConverter(BamlBinaryReader reader)
        {
            string value = reader.ReadString();
            short converterTypeIdentifier = reader.ReadInt16();
            ReadText(value);
        }

        private void AddContent(Element parent, object content)
        {
            if (content == null)
            {
                throw new ArgumentNullException();
            }

            Property contentProperty = this.GetContentProperty(parent);
            if (contentProperty == null)
            {
                contentProperty = new Property(PropertyType.Content);
                parent.Properties.Add(contentProperty);
            }

            if (contentProperty.Value != null)
            {
                if (contentProperty.Value is string)
                {
                    IList value = new ArrayList();
                    value.Add(contentProperty.Value);
                    contentProperty.Value = value;
                }

                if (contentProperty.Value is IList)
                {
                    IList value = (IList)contentProperty.Value;
                    value.Add(content);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                if (content is string)
                {
                    contentProperty.Value = content;
                }
                else
                {
                    IList value = new ArrayList();
                    value.Add(content);
                    contentProperty.Value = value;
                }
            }
        }

        private void AddDictionaryEntry(Element dictionary, int position, Property keyProperty)
        {
            IDictionary table;
            _dictionaryKeyPositionTable.TryGetValue(dictionary, out table);
            if (table == null)
            {
                table = new Hashtable();
                _dictionaryKeyPositionTable.Add(dictionary, table);
            }

            IList list = (IList)table[position];
            if (list == null)
            {
                list = new ArrayList();
                table.Add(position, list);
            }

            list.Add(keyProperty);
        }

        private Element CreateTypeExtension(short typeIdentifier)
        {
            return CreateTypeExtension(typeIdentifier, true);
        }

        private Element CreateTypeExtension(short typeIdentifier, bool wrapInType)
        {
            Element element = new Element();
            element.TypeDeclaration = new TypeDeclaration("x:Type");
            // element.TypeDeclaration = new TypeDeclaration("TypeExtension", "System.Windows.Markup");

            TypeDeclaration typeDeclaration = this.GetTypeDeclaration(typeIdentifier);
            if (typeDeclaration == null)
            {
                throw new NotSupportedException();
            }

            if (false == wrapInType)
                element.TypeDeclaration = typeDeclaration;
            else
                element.Arguments.Add(typeDeclaration);

            return element;
        }

        private Property GetContentProperty(Element parent)
        {
            foreach (Property property in parent.Properties)
            {
                if (property.PropertyType == PropertyType.Content)
                {
                    return property;
                }
            }

            return null;
        }

        private TypeDeclaration GetTypeDeclaration(short identifier)
        {
            TypeDeclaration typeDeclaration = null;

            if (identifier >= 0)
            {
                typeDeclaration = _typeTable[identifier];
            }
            else
            {
                if (_knownTypeTable == null)
                {
                    Initialize();
                }

                typeDeclaration = _knownTypeTable[-identifier];
            }

            // if an xml namespace prefix has been mapped for the specified assembly/clrnamespace
            // use its prefix in the returned type declaration. we have to do this here because
            // later on we may not have access to the mapping information
            string xmlNs = _namespaceManager.GetXmlNamespace(typeDeclaration);

            if (null != xmlNs)
            {
                string prefix = _namespaceManager.GetPrefix(xmlNs);

                if (null != prefix)
                    typeDeclaration = typeDeclaration.Copy(prefix);
            }

            if (typeDeclaration == null)
            {
                throw new NotSupportedException();
            }

            return typeDeclaration;
        }

        private PropertyDeclaration GetPropertyDeclaration(short identifier)
        {
            PropertyDeclaration propertyDeclaration = null;

            if (identifier >= 0)
            {
                propertyDeclaration = _propertyTable[identifier];
            }
            else
            {
                if (this._knownPropertyTable == null)
                {
                    this.Initialize();
                }

                propertyDeclaration = this._knownPropertyTable[-identifier];
            }

            if (propertyDeclaration == null)
            {
                throw new NotSupportedException();
            }

            return propertyDeclaration;
        }

        private object GetStaticResource(short identifier)
        {
            object resource = _staticResourceTable[identifier];
            return resource;
        }

        private ResourceName GetResourceName(short identifier)
        {
            if (identifier >= 0)
            {
                PropertyDeclaration propertyName = _propertyTable[identifier];
                return new ResourceName(propertyName.Name);
            }
            else
            {
                if (_knownResourceTable.Count == 0)
                {
                    Initialize();
                }

                identifier = (short)-identifier;
                if (identifier > 0x00e8)
                {
                    identifier -= 0x00e8;
                }

                ResourceName resourceName;
                _knownResourceTable.TryGetValue((int)identifier, out resourceName);
                return resourceName;
            }
        }

        private string GetAssembly(string assemblyName)
        {
            return assemblyName;
        }

        private void Initialize()
        {
            _knownTypeTable = new TypeDeclaration[0x02f8];
            _knownTypeTable[0x0000] = new TypeDeclaration(string.Empty, string.Empty, string.Empty); // Unknown
            _knownTypeTable[0x0001] = new TypeDeclaration("AccessText", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0002] = new TypeDeclaration("AdornedElementPlaceholder", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0003] = new TypeDeclaration("Adorner", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0004] = new TypeDeclaration("AdornerDecorator", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0005] = new TypeDeclaration("AdornerLayer", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0006] = new TypeDeclaration("AffineTransform3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0007] = new TypeDeclaration("AmbientLight", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0008] = new TypeDeclaration("AnchoredBlock", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0009] = new TypeDeclaration("Animatable", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x000a] = new TypeDeclaration("AnimationClock", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x000b] = new TypeDeclaration("AnimationTimeline", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x000c] = new TypeDeclaration("Application", "System.Net.Mime", this.GetAssembly("System"));
            _knownTypeTable[0x000d] = new TypeDeclaration("ArcSegment", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x000e] = new TypeDeclaration("ArrayExtension", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x000f] = new TypeDeclaration("AxisAngleRotation3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0010] = new TypeDeclaration("BaseIListConverter", "System.Windows.Media.Converters", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0011] = new TypeDeclaration("BeginStoryboard", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0012] = new TypeDeclaration("BevelBitmapEffect", "System.Windows.Media.Effects", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0013] = new TypeDeclaration("BezierSegment", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0014] = new TypeDeclaration("Binding", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0015] = new TypeDeclaration("BindingBase", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0016] = new TypeDeclaration("BindingExpression", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0017] = new TypeDeclaration("BindingExpressionBase", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0018] = new TypeDeclaration("BindingListCollectionView", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0019] = new TypeDeclaration("BitmapDecoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x001a] = new TypeDeclaration("BitmapEffect", "System.Windows.Media.Effects", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x001b] = new TypeDeclaration("BitmapEffectCollection", "System.Windows.Media.Effects", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x001c] = new TypeDeclaration("BitmapEffectGroup", "System.Windows.Media.Effects", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x001d] = new TypeDeclaration("BitmapEffectInput", "System.Windows.Media.Effects", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x001e] = new TypeDeclaration("BitmapEncoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x001f] = new TypeDeclaration("BitmapFrame", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0020] = new TypeDeclaration("BitmapImage", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0021] = new TypeDeclaration("BitmapMetadata", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0022] = new TypeDeclaration("BitmapPalette", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0023] = new TypeDeclaration("BitmapSource", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0024] = new TypeDeclaration("Block", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0025] = new TypeDeclaration("BlockUIContainer", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0026] = new TypeDeclaration("BlurBitmapEffect", "System.Windows.Media.Effects", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0027] = new TypeDeclaration("BmpBitmapDecoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0028] = new TypeDeclaration("BmpBitmapEncoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0029] = new TypeDeclaration("Bold", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x002b] = new TypeDeclaration("Boolean", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x002c] = new TypeDeclaration("BooleanAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x002d] = new TypeDeclaration("BooleanAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x002e] = new TypeDeclaration("BooleanConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x002f] = new TypeDeclaration("BooleanKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0030] = new TypeDeclaration("BooleanKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0031] = new TypeDeclaration("BooleanToVisibilityConverter", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x002a] = new TypeDeclaration("BoolIListConverter", "System.Windows.Media.Converters", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0032] = new TypeDeclaration("Border", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0033] = new TypeDeclaration("BorderGapMaskConverter", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0034] = new TypeDeclaration("Brush", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0035] = new TypeDeclaration("BrushConverter", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0036] = new TypeDeclaration("BulletDecorator", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0037] = new TypeDeclaration("Button", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0038] = new TypeDeclaration("ButtonBase", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0039] = new TypeDeclaration("Byte", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x003a] = new TypeDeclaration("ByteAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x003b] = new TypeDeclaration("ByteAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x003c] = new TypeDeclaration("ByteAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x003d] = new TypeDeclaration("ByteConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x003e] = new TypeDeclaration("ByteKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x003f] = new TypeDeclaration("ByteKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0040] = new TypeDeclaration("CachedBitmap", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0041] = new TypeDeclaration("Camera", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0042] = new TypeDeclaration("Canvas", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0043] = new TypeDeclaration("Char", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x0044] = new TypeDeclaration("CharAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0045] = new TypeDeclaration("CharAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0046] = new TypeDeclaration("CharConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x0047] = new TypeDeclaration("CharIListConverter", "System.Windows.Media.Converters", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0048] = new TypeDeclaration("CharKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0049] = new TypeDeclaration("CharKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x004a] = new TypeDeclaration("CheckBox", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x004b] = new TypeDeclaration("Clock", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x004c] = new TypeDeclaration("ClockController", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x004d] = new TypeDeclaration("ClockGroup", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x004e] = new TypeDeclaration("CollectionContainer", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x004f] = new TypeDeclaration("CollectionView", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0050] = new TypeDeclaration("CollectionViewSource", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0051] = new TypeDeclaration("Color", "Microsoft.Win32", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x0052] = new TypeDeclaration("ColorAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0053] = new TypeDeclaration("ColorAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0054] = new TypeDeclaration("ColorAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0055] = new TypeDeclaration("ColorConvertedBitmap", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0056] = new TypeDeclaration("ColorConvertedBitmapExtension", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0057] = new TypeDeclaration("ColorConverter", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0058] = new TypeDeclaration("ColorKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0059] = new TypeDeclaration("ColorKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x005a] = new TypeDeclaration("ColumnDefinition", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x005b] = new TypeDeclaration("CombinedGeometry", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x005c] = new TypeDeclaration("ComboBox", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x005d] = new TypeDeclaration("ComboBoxItem", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x005e] = new TypeDeclaration("CommandConverter", "System.Windows.Input", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x005f] = new TypeDeclaration("ComponentResourceKey", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0060] = new TypeDeclaration("ComponentResourceKeyConverter", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0061] = new TypeDeclaration("CompositionTarget", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0062] = new TypeDeclaration("Condition", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0063] = new TypeDeclaration("ContainerVisual", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0064] = new TypeDeclaration("ContentControl", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0065] = new TypeDeclaration("ContentElement", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0066] = new TypeDeclaration("ContentPresenter", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0067] = new TypeDeclaration("ContentPropertyAttribute", "System.Windows.Markup", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0068] = new TypeDeclaration("ContentWrapperAttribute", "System.Windows.Markup", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0069] = new TypeDeclaration("ContextMenu", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x006a] = new TypeDeclaration("ContextMenuService", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x006b] = new TypeDeclaration("Control", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x006d] = new TypeDeclaration("ControllableStoryboardAction", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x006c] = new TypeDeclaration("ControlTemplate", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x006e] = new TypeDeclaration("CornerRadius", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x006f] = new TypeDeclaration("CornerRadiusConverter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0070] = new TypeDeclaration("CroppedBitmap", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0071] = new TypeDeclaration("CultureInfo", "System.Globalization", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x0072] = new TypeDeclaration("CultureInfoConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x0073] = new TypeDeclaration("CultureInfoIetfLanguageTagConverter", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0074] = new TypeDeclaration("Cursor", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0075] = new TypeDeclaration("CursorConverter", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0076] = new TypeDeclaration("DashStyle", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0077] = new TypeDeclaration("DataChangedEventManager", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0078] = new TypeDeclaration("DataTemplate", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0079] = new TypeDeclaration("DataTemplateKey", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x007a] = new TypeDeclaration("DataTrigger", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x007b] = new TypeDeclaration("DateTime", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x007c] = new TypeDeclaration("DateTimeConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x007d] = new TypeDeclaration("DateTimeConverter2", "System.Windows.Markup", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x007e] = new TypeDeclaration("Decimal", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x007f] = new TypeDeclaration("DecimalAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0080] = new TypeDeclaration("DecimalAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0081] = new TypeDeclaration("DecimalAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0082] = new TypeDeclaration("DecimalConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x0083] = new TypeDeclaration("DecimalKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0084] = new TypeDeclaration("DecimalKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0085] = new TypeDeclaration("Decorator", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0086] = new TypeDeclaration("DefinitionBase", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0087] = new TypeDeclaration("DependencyObject", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0088] = new TypeDeclaration("DependencyProperty", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0089] = new TypeDeclaration("DependencyPropertyConverter", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x008a] = new TypeDeclaration("DialogResultConverter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x008b] = new TypeDeclaration("DiffuseMaterial", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x008c] = new TypeDeclaration("DirectionalLight", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x008d] = new TypeDeclaration("DiscreteBooleanKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x008e] = new TypeDeclaration("DiscreteByteKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x008f] = new TypeDeclaration("DiscreteCharKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0090] = new TypeDeclaration("DiscreteColorKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0091] = new TypeDeclaration("DiscreteDecimalKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0092] = new TypeDeclaration("DiscreteDoubleKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0093] = new TypeDeclaration("DiscreteInt16KeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0094] = new TypeDeclaration("DiscreteInt32KeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0095] = new TypeDeclaration("DiscreteInt64KeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0096] = new TypeDeclaration("DiscreteMatrixKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0097] = new TypeDeclaration("DiscreteObjectKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0098] = new TypeDeclaration("DiscretePoint3DKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0099] = new TypeDeclaration("DiscretePointKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x009a] = new TypeDeclaration("DiscreteQuaternionKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x009b] = new TypeDeclaration("DiscreteRectKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x009c] = new TypeDeclaration("DiscreteRotation3DKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x009d] = new TypeDeclaration("DiscreteSingleKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x009e] = new TypeDeclaration("DiscreteSizeKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x009f] = new TypeDeclaration("DiscreteStringKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00a0] = new TypeDeclaration("DiscreteThicknessKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00a1] = new TypeDeclaration("DiscreteVector3DKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00a2] = new TypeDeclaration("DiscreteVectorKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00a3] = new TypeDeclaration("DockPanel", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00a4] = new TypeDeclaration("DocumentPageView", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00a5] = new TypeDeclaration("DocumentReference", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00a6] = new TypeDeclaration("DocumentViewer", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00a7] = new TypeDeclaration("DocumentViewerBase", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00a8] = new TypeDeclaration("Double", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x00a9] = new TypeDeclaration("DoubleAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00aa] = new TypeDeclaration("DoubleAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00ab] = new TypeDeclaration("DoubleAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00ac] = new TypeDeclaration("DoubleAnimationUsingPath", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00ad] = new TypeDeclaration("DoubleCollection", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00ae] = new TypeDeclaration("DoubleCollectionConverter", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00af] = new TypeDeclaration("DoubleConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x00b0] = new TypeDeclaration("DoubleIListConverter", "System.Windows.Media.Converters", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00b1] = new TypeDeclaration("DoubleKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00b2] = new TypeDeclaration("DoubleKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00b3] = new TypeDeclaration("Drawing", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00b4] = new TypeDeclaration("DrawingBrush", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00b5] = new TypeDeclaration("DrawingCollection", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00b6] = new TypeDeclaration("DrawingContext", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00b7] = new TypeDeclaration("DrawingGroup", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00b8] = new TypeDeclaration("DrawingImage", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00b9] = new TypeDeclaration("DrawingVisual", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00ba] = new TypeDeclaration("DropShadowBitmapEffect", "System.Windows.Media.Effects", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00bb] = new TypeDeclaration("Duration", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00bc] = new TypeDeclaration("DurationConverter", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00bd] = new TypeDeclaration("DynamicResourceExtension", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00be] = new TypeDeclaration("DynamicResourceExtensionConverter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00bf] = new TypeDeclaration("Ellipse", "System.Windows.Shapes", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00c0] = new TypeDeclaration("EllipseGeometry", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00c1] = new TypeDeclaration("EmbossBitmapEffect", "System.Windows.Media.Effects", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00c2] = new TypeDeclaration("EmissiveMaterial", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00c3] = new TypeDeclaration("EnumConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x00c4] = new TypeDeclaration("EventManager", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00c5] = new TypeDeclaration("EventSetter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00c6] = new TypeDeclaration("EventTrigger", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00c7] = new TypeDeclaration("Expander", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00c8] = new TypeDeclaration("Expression", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x00c9] = new TypeDeclaration("ExpressionConverter", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x00ca] = new TypeDeclaration("Figure", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00cb] = new TypeDeclaration("FigureLength", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00cc] = new TypeDeclaration("FigureLengthConverter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00cd] = new TypeDeclaration("FixedDocument", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00ce] = new TypeDeclaration("FixedDocumentSequence", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00cf] = new TypeDeclaration("FixedPage", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00d0] = new TypeDeclaration("Floater", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00d1] = new TypeDeclaration("FlowDocument", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00d2] = new TypeDeclaration("FlowDocumentPageViewer", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00d3] = new TypeDeclaration("FlowDocumentReader", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00d4] = new TypeDeclaration("FlowDocumentScrollViewer", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00d5] = new TypeDeclaration("FocusManager", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00d6] = new TypeDeclaration("FontFamily", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00d7] = new TypeDeclaration("FontFamilyConverter", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00d8] = new TypeDeclaration("FontSizeConverter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00d9] = new TypeDeclaration("FontStretch", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00da] = new TypeDeclaration("FontStretchConverter", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00db] = new TypeDeclaration("FontStyle", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00dc] = new TypeDeclaration("FontStyleConverter", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00dd] = new TypeDeclaration("FontWeight", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00de] = new TypeDeclaration("FontWeightConverter", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00df] = new TypeDeclaration("FormatConvertedBitmap", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00e0] = new TypeDeclaration("Frame", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00e1] = new TypeDeclaration("FrameworkContentElement", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00e2] = new TypeDeclaration("FrameworkElement", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00e3] = new TypeDeclaration("FrameworkElementFactory", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00e4] = new TypeDeclaration("FrameworkPropertyMetadata", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00e5] = new TypeDeclaration("FrameworkPropertyMetadataOptions", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00e6] = new TypeDeclaration("FrameworkRichTextComposition", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00e7] = new TypeDeclaration("FrameworkTemplate", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00e8] = new TypeDeclaration("FrameworkTextComposition", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00e9] = new TypeDeclaration("Freezable", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x00ea] = new TypeDeclaration("GeneralTransform", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00eb] = new TypeDeclaration("GeneralTransformCollection", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00ec] = new TypeDeclaration("GeneralTransformGroup", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00ed] = new TypeDeclaration("Geometry", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00ee] = new TypeDeclaration("Geometry3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00ef] = new TypeDeclaration("GeometryCollection", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00f0] = new TypeDeclaration("GeometryConverter", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00f1] = new TypeDeclaration("GeometryDrawing", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00f2] = new TypeDeclaration("GeometryGroup", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00f3] = new TypeDeclaration("GeometryModel3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00f4] = new TypeDeclaration("GestureRecognizer", "System.Windows.Ink", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00f5] = new TypeDeclaration("GifBitmapDecoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00f6] = new TypeDeclaration("GifBitmapEncoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00f7] = new TypeDeclaration("GlyphRun", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00f8] = new TypeDeclaration("GlyphRunDrawing", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00fa] = new TypeDeclaration("Glyphs", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00f9] = new TypeDeclaration("GlyphTypeface", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00fb] = new TypeDeclaration("GradientBrush", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00fc] = new TypeDeclaration("GradientStop", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00fd] = new TypeDeclaration("GradientStopCollection", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x00fe] = new TypeDeclaration("Grid", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x00ff] = new TypeDeclaration("GridLength", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0100] = new TypeDeclaration("GridLengthConverter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0101] = new TypeDeclaration("GridSplitter", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0102] = new TypeDeclaration("GridView", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0103] = new TypeDeclaration("GridViewColumn", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0104] = new TypeDeclaration("GridViewColumnHeader", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0105] = new TypeDeclaration("GridViewHeaderRowPresenter", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0106] = new TypeDeclaration("GridViewRowPresenter", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0107] = new TypeDeclaration("GridViewRowPresenterBase", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0108] = new TypeDeclaration("GroupBox", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0109] = new TypeDeclaration("GroupItem", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x010a] = new TypeDeclaration("Guid", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x010b] = new TypeDeclaration("GuidConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x010c] = new TypeDeclaration("GuidelineSet", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x010d] = new TypeDeclaration("HeaderedContentControl", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x010e] = new TypeDeclaration("HeaderedItemsControl", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x010f] = new TypeDeclaration("HierarchicalDataTemplate", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0110] = new TypeDeclaration("HostVisual", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0111] = new TypeDeclaration("Hyperlink", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0112] = new TypeDeclaration("IAddChild", "System.Windows.Markup", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0113] = new TypeDeclaration("IAddChildInternal", "System.Windows.Markup", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0114] = new TypeDeclaration("ICommand", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0115] = new TypeDeclaration("IComponentConnector", "System.Windows.Markup", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0118] = new TypeDeclaration("IconBitmapDecoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0119] = new TypeDeclaration("Image", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x011a] = new TypeDeclaration("ImageBrush", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x011b] = new TypeDeclaration("ImageDrawing", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x011c] = new TypeDeclaration("ImageMetadata", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x011d] = new TypeDeclaration("ImageSource", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x011e] = new TypeDeclaration("ImageSourceConverter", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0116] = new TypeDeclaration("INameScope", "System.Windows.Markup", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0120] = new TypeDeclaration("InkCanvas", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0121] = new TypeDeclaration("InkPresenter", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0122] = new TypeDeclaration("Inline", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0123] = new TypeDeclaration("InlineCollection", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0124] = new TypeDeclaration("InlineUIContainer", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x011f] = new TypeDeclaration("InPlaceBitmapMetadataWriter", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0125] = new TypeDeclaration("InputBinding", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0126] = new TypeDeclaration("InputDevice", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0127] = new TypeDeclaration("InputLanguageManager", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0128] = new TypeDeclaration("InputManager", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0129] = new TypeDeclaration("InputMethod", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x012a] = new TypeDeclaration("InputScope", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x012b] = new TypeDeclaration("InputScopeConverter", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x012c] = new TypeDeclaration("InputScopeName", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x012d] = new TypeDeclaration("InputScopeNameConverter", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x012e] = new TypeDeclaration("Int16", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x012f] = new TypeDeclaration("Int16Animation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0130] = new TypeDeclaration("Int16AnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0131] = new TypeDeclaration("Int16AnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0132] = new TypeDeclaration("Int16Converter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x0133] = new TypeDeclaration("Int16KeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0134] = new TypeDeclaration("Int16KeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0135] = new TypeDeclaration("Int32", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x0136] = new TypeDeclaration("Int32Animation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0137] = new TypeDeclaration("Int32AnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0138] = new TypeDeclaration("Int32AnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0139] = new TypeDeclaration("Int32Collection", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x013a] = new TypeDeclaration("Int32CollectionConverter", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x013b] = new TypeDeclaration("Int32Converter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x013c] = new TypeDeclaration("Int32KeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x013d] = new TypeDeclaration("Int32KeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x013e] = new TypeDeclaration("Int32Rect", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x013f] = new TypeDeclaration("Int32RectConverter", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0140] = new TypeDeclaration("Int64", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x0141] = new TypeDeclaration("Int64Animation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0142] = new TypeDeclaration("Int64AnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0143] = new TypeDeclaration("Int64AnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0144] = new TypeDeclaration("Int64Converter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x0145] = new TypeDeclaration("Int64KeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0146] = new TypeDeclaration("Int64KeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0117] = new TypeDeclaration("IStyleConnector", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0147] = new TypeDeclaration("Italic", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0148] = new TypeDeclaration("ItemCollection", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0149] = new TypeDeclaration("ItemsControl", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x014a] = new TypeDeclaration("ItemsPanelTemplate", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x014b] = new TypeDeclaration("ItemsPresenter", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x014c] = new TypeDeclaration("JournalEntry", "System.Windows.Navigation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x014d] = new TypeDeclaration("JournalEntryListConverter", "System.Windows.Navigation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x014e] = new TypeDeclaration("JournalEntryUnifiedViewConverter", "System.Windows.Navigation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x014f] = new TypeDeclaration("JpegBitmapDecoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0150] = new TypeDeclaration("JpegBitmapEncoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0151] = new TypeDeclaration("KeyBinding", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0159] = new TypeDeclaration("KeyboardDevice", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0152] = new TypeDeclaration("KeyConverter", "System.Windows.Input", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0153] = new TypeDeclaration("KeyGesture", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0154] = new TypeDeclaration("KeyGestureConverter", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0155] = new TypeDeclaration("KeySpline", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0156] = new TypeDeclaration("KeySplineConverter", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0157] = new TypeDeclaration("KeyTime", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0158] = new TypeDeclaration("KeyTimeConverter", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x015a] = new TypeDeclaration("Label", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x015b] = new TypeDeclaration("LateBoundBitmapDecoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x015c] = new TypeDeclaration("LengthConverter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x015d] = new TypeDeclaration("Light", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x015e] = new TypeDeclaration("Line", "System.Windows.Shapes", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0162] = new TypeDeclaration("LinearByteKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0163] = new TypeDeclaration("LinearColorKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0164] = new TypeDeclaration("LinearDecimalKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0165] = new TypeDeclaration("LinearDoubleKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0166] = new TypeDeclaration("LinearGradientBrush", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0167] = new TypeDeclaration("LinearInt16KeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0168] = new TypeDeclaration("LinearInt32KeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0169] = new TypeDeclaration("LinearInt64KeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x016a] = new TypeDeclaration("LinearPoint3DKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x016b] = new TypeDeclaration("LinearPointKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x016c] = new TypeDeclaration("LinearQuaternionKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x016d] = new TypeDeclaration("LinearRectKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x016e] = new TypeDeclaration("LinearRotation3DKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x016f] = new TypeDeclaration("LinearSingleKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0170] = new TypeDeclaration("LinearSizeKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0171] = new TypeDeclaration("LinearThicknessKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0172] = new TypeDeclaration("LinearVector3DKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0173] = new TypeDeclaration("LinearVectorKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x015f] = new TypeDeclaration("LineBreak", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0160] = new TypeDeclaration("LineGeometry", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0161] = new TypeDeclaration("LineSegment", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0174] = new TypeDeclaration("List", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0175] = new TypeDeclaration("ListBox", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0176] = new TypeDeclaration("ListBoxItem", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0177] = new TypeDeclaration("ListCollectionView", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0178] = new TypeDeclaration("ListItem", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0179] = new TypeDeclaration("ListView", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x017a] = new TypeDeclaration("ListViewItem", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x017b] = new TypeDeclaration("Localization", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x017c] = new TypeDeclaration("LostFocusEventManager", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x017d] = new TypeDeclaration("MarkupExtension", "System.Windows.Markup", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x017e] = new TypeDeclaration("Material", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x017f] = new TypeDeclaration("MaterialCollection", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0180] = new TypeDeclaration("MaterialGroup", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0181] = new TypeDeclaration("Matrix", "System.Windows.Media", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0182] = new TypeDeclaration("Matrix3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0183] = new TypeDeclaration("Matrix3DConverter", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0184] = new TypeDeclaration("MatrixAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0185] = new TypeDeclaration("MatrixAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0186] = new TypeDeclaration("MatrixAnimationUsingPath", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0187] = new TypeDeclaration("MatrixCamera", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0188] = new TypeDeclaration("MatrixConverter", "System.Windows.Media", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0189] = new TypeDeclaration("MatrixKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x018a] = new TypeDeclaration("MatrixKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x018b] = new TypeDeclaration("MatrixTransform", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x018c] = new TypeDeclaration("MatrixTransform3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x018d] = new TypeDeclaration("MediaClock", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x018e] = new TypeDeclaration("MediaElement", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x018f] = new TypeDeclaration("MediaPlayer", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0190] = new TypeDeclaration("MediaTimeline", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0191] = new TypeDeclaration("Menu", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0192] = new TypeDeclaration("MenuBase", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0193] = new TypeDeclaration("MenuItem", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0194] = new TypeDeclaration("MenuScrollingVisibilityConverter", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0195] = new TypeDeclaration("MeshGeometry3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0196] = new TypeDeclaration("Model3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0197] = new TypeDeclaration("Model3DCollection", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0198] = new TypeDeclaration("Model3DGroup", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0199] = new TypeDeclaration("ModelVisual3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x019a] = new TypeDeclaration("ModifierKeysConverter", "System.Windows.Input", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x019b] = new TypeDeclaration("MouseActionConverter", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x019c] = new TypeDeclaration("MouseBinding", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x019d] = new TypeDeclaration("MouseDevice", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x019e] = new TypeDeclaration("MouseGesture", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x019f] = new TypeDeclaration("MouseGestureConverter", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01a0] = new TypeDeclaration("MultiBinding", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01a1] = new TypeDeclaration("MultiBindingExpression", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01a2] = new TypeDeclaration("MultiDataTrigger", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01a3] = new TypeDeclaration("MultiTrigger", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01a4] = new TypeDeclaration("NameScope", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01a5] = new TypeDeclaration("NavigationWindow", "System.Windows.Navigation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01a7] = new TypeDeclaration("NullableBoolConverter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01a8] = new TypeDeclaration("NullableConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x01a6] = new TypeDeclaration("NullExtension", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01a9] = new TypeDeclaration("NumberSubstitution", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01aa] = new TypeDeclaration("Object", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x01ab] = new TypeDeclaration("ObjectAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01ac] = new TypeDeclaration("ObjectAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01ad] = new TypeDeclaration("ObjectDataProvider", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01ae] = new TypeDeclaration("ObjectKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01af] = new TypeDeclaration("ObjectKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01b0] = new TypeDeclaration("OrthographicCamera", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01b1] = new TypeDeclaration("OuterGlowBitmapEffect", "System.Windows.Media.Effects", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01b2] = new TypeDeclaration("Page", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01b3] = new TypeDeclaration("PageContent", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01b4] = new TypeDeclaration("PageFunctionBase", "System.Windows.Navigation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01b5] = new TypeDeclaration("Panel", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01b6] = new TypeDeclaration("Paragraph", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01b7] = new TypeDeclaration("ParallelTimeline", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01b8] = new TypeDeclaration("ParserContext", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01b9] = new TypeDeclaration("PasswordBox", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01ba] = new TypeDeclaration("Path", "System.Windows.Shapes", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01bb] = new TypeDeclaration("PathFigure", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01bc] = new TypeDeclaration("PathFigureCollection", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01bd] = new TypeDeclaration("PathFigureCollectionConverter", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01be] = new TypeDeclaration("PathGeometry", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01bf] = new TypeDeclaration("PathSegment", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01c0] = new TypeDeclaration("PathSegmentCollection", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01c1] = new TypeDeclaration("PauseStoryboard", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01c2] = new TypeDeclaration("Pen", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01c3] = new TypeDeclaration("PerspectiveCamera", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01c4] = new TypeDeclaration("PixelFormat", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01c5] = new TypeDeclaration("PixelFormatConverter", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01c6] = new TypeDeclaration("PngBitmapDecoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01c7] = new TypeDeclaration("PngBitmapEncoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01c8] = new TypeDeclaration("Point", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x01c9] = new TypeDeclaration("Point3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01ca] = new TypeDeclaration("Point3DAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01cb] = new TypeDeclaration("Point3DAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01cc] = new TypeDeclaration("Point3DAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01cd] = new TypeDeclaration("Point3DCollection", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01ce] = new TypeDeclaration("Point3DCollectionConverter", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01cf] = new TypeDeclaration("Point3DConverter", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01d0] = new TypeDeclaration("Point3DKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01d1] = new TypeDeclaration("Point3DKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01d2] = new TypeDeclaration("Point4D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01d3] = new TypeDeclaration("Point4DConverter", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01d4] = new TypeDeclaration("PointAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01d5] = new TypeDeclaration("PointAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01d6] = new TypeDeclaration("PointAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01d7] = new TypeDeclaration("PointAnimationUsingPath", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01d8] = new TypeDeclaration("PointCollection", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01d9] = new TypeDeclaration("PointCollectionConverter", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01da] = new TypeDeclaration("PointConverter", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x01db] = new TypeDeclaration("PointIListConverter", "System.Windows.Media.Converters", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01dc] = new TypeDeclaration("PointKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01dd] = new TypeDeclaration("PointKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01de] = new TypeDeclaration("PointLight", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01df] = new TypeDeclaration("PointLightBase", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01e0] = new TypeDeclaration("PolyBezierSegment", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01e3] = new TypeDeclaration("Polygon", "System.Windows.Shapes", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01e4] = new TypeDeclaration("Polyline", "System.Windows.Shapes", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01e1] = new TypeDeclaration("PolyLineSegment", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01e2] = new TypeDeclaration("PolyQuadraticBezierSegment", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01e5] = new TypeDeclaration("Popup", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01e6] = new TypeDeclaration("PresentationSource", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01e7] = new TypeDeclaration("PriorityBinding", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01e8] = new TypeDeclaration("PriorityBindingExpression", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01e9] = new TypeDeclaration("ProgressBar", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01ea] = new TypeDeclaration("ProjectionCamera", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01eb] = new TypeDeclaration("PropertyPath", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01ec] = new TypeDeclaration("PropertyPathConverter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01ed] = new TypeDeclaration("QuadraticBezierSegment", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01ee] = new TypeDeclaration("Quaternion", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01ef] = new TypeDeclaration("QuaternionAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01f0] = new TypeDeclaration("QuaternionAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01f1] = new TypeDeclaration("QuaternionAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01f2] = new TypeDeclaration("QuaternionConverter", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01f3] = new TypeDeclaration("QuaternionKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01f4] = new TypeDeclaration("QuaternionKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01f5] = new TypeDeclaration("QuaternionRotation3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01f6] = new TypeDeclaration("RadialGradientBrush", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01f7] = new TypeDeclaration("RadioButton", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01f8] = new TypeDeclaration("RangeBase", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x01f9] = new TypeDeclaration("Rect", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01fa] = new TypeDeclaration("Rect3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01fb] = new TypeDeclaration("Rect3DConverter", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0202] = new TypeDeclaration("Rectangle", "System.Windows.Shapes", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0203] = new TypeDeclaration("RectangleGeometry", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01fc] = new TypeDeclaration("RectAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01fd] = new TypeDeclaration("RectAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01fe] = new TypeDeclaration("RectAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x01ff] = new TypeDeclaration("RectConverter", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0200] = new TypeDeclaration("RectKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0201] = new TypeDeclaration("RectKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0204] = new TypeDeclaration("RelativeSource", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0205] = new TypeDeclaration("RemoveStoryboard", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0206] = new TypeDeclaration("RenderOptions", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0207] = new TypeDeclaration("RenderTargetBitmap", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0208] = new TypeDeclaration("RepeatBehavior", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0209] = new TypeDeclaration("RepeatBehaviorConverter", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x020a] = new TypeDeclaration("RepeatButton", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x020b] = new TypeDeclaration("ResizeGrip", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x020c] = new TypeDeclaration("ResourceDictionary", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x020d] = new TypeDeclaration("ResourceKey", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x020e] = new TypeDeclaration("ResumeStoryboard", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x020f] = new TypeDeclaration("RichTextBox", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0210] = new TypeDeclaration("RotateTransform", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0211] = new TypeDeclaration("RotateTransform3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0212] = new TypeDeclaration("Rotation3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0213] = new TypeDeclaration("Rotation3DAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0214] = new TypeDeclaration("Rotation3DAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0215] = new TypeDeclaration("Rotation3DAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0216] = new TypeDeclaration("Rotation3DKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0217] = new TypeDeclaration("Rotation3DKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0218] = new TypeDeclaration("RoutedCommand", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0219] = new TypeDeclaration("RoutedEvent", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x021a] = new TypeDeclaration("RoutedEventConverter", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x021b] = new TypeDeclaration("RoutedUICommand", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x021c] = new TypeDeclaration("RoutingStrategy", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x021d] = new TypeDeclaration("RowDefinition", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x021e] = new TypeDeclaration("Run", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x021f] = new TypeDeclaration("RuntimeNamePropertyAttribute", "System.Windows.Markup", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0220] = new TypeDeclaration("SByte", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x0221] = new TypeDeclaration("SByteConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x0222] = new TypeDeclaration("ScaleTransform", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0223] = new TypeDeclaration("ScaleTransform3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0224] = new TypeDeclaration("ScrollBar", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0225] = new TypeDeclaration("ScrollContentPresenter", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0226] = new TypeDeclaration("ScrollViewer", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0227] = new TypeDeclaration("Section", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0228] = new TypeDeclaration("SeekStoryboard", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0229] = new TypeDeclaration("Selector", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x022a] = new TypeDeclaration("Separator", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x022b] = new TypeDeclaration("SetStoryboardSpeedRatio", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x022c] = new TypeDeclaration("Setter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x022d] = new TypeDeclaration("SetterBase", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x022e] = new TypeDeclaration("Shape", "System.Windows.Shapes", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x022f] = new TypeDeclaration("Single", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x0230] = new TypeDeclaration("SingleAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0231] = new TypeDeclaration("SingleAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0232] = new TypeDeclaration("SingleAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0233] = new TypeDeclaration("SingleConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x0234] = new TypeDeclaration("SingleKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0235] = new TypeDeclaration("SingleKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0236] = new TypeDeclaration("Size", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x0237] = new TypeDeclaration("Size3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0238] = new TypeDeclaration("Size3DConverter", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0239] = new TypeDeclaration("SizeAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x023a] = new TypeDeclaration("SizeAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x023b] = new TypeDeclaration("SizeAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x023c] = new TypeDeclaration("SizeConverter", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x023d] = new TypeDeclaration("SizeKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x023e] = new TypeDeclaration("SizeKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x023f] = new TypeDeclaration("SkewTransform", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0240] = new TypeDeclaration("SkipStoryboardToFill", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0241] = new TypeDeclaration("Slider", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0242] = new TypeDeclaration("SolidColorBrush", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0243] = new TypeDeclaration("SoundPlayerAction", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0244] = new TypeDeclaration("Span", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0245] = new TypeDeclaration("SpecularMaterial", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0246] = new TypeDeclaration("SpellCheck", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0247] = new TypeDeclaration("SplineByteKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0248] = new TypeDeclaration("SplineColorKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0249] = new TypeDeclaration("SplineDecimalKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x024a] = new TypeDeclaration("SplineDoubleKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x024b] = new TypeDeclaration("SplineInt16KeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x024c] = new TypeDeclaration("SplineInt32KeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x024d] = new TypeDeclaration("SplineInt64KeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x024e] = new TypeDeclaration("SplinePoint3DKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x024f] = new TypeDeclaration("SplinePointKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0250] = new TypeDeclaration("SplineQuaternionKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0251] = new TypeDeclaration("SplineRectKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0252] = new TypeDeclaration("SplineRotation3DKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0253] = new TypeDeclaration("SplineSingleKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0254] = new TypeDeclaration("SplineSizeKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0255] = new TypeDeclaration("SplineThicknessKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0256] = new TypeDeclaration("SplineVector3DKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0257] = new TypeDeclaration("SplineVectorKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0258] = new TypeDeclaration("SpotLight", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0259] = new TypeDeclaration("StackPanel", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x025a] = new TypeDeclaration("StaticExtension", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x025b] = new TypeDeclaration("StaticResourceExtension", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x025c] = new TypeDeclaration("StatusBar", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x025d] = new TypeDeclaration("StatusBarItem", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x025e] = new TypeDeclaration("StickyNoteControl", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x025f] = new TypeDeclaration("StopStoryboard", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0260] = new TypeDeclaration("Storyboard", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0261] = new TypeDeclaration("StreamGeometry", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0262] = new TypeDeclaration("StreamGeometryContext", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0263] = new TypeDeclaration("StreamResourceInfo", "System.Windows.Resources", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0264] = new TypeDeclaration("String", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x0265] = new TypeDeclaration("StringAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0266] = new TypeDeclaration("StringAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0267] = new TypeDeclaration("StringConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x0268] = new TypeDeclaration("StringKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0269] = new TypeDeclaration("StringKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x026a] = new TypeDeclaration("StrokeCollection", "System.Windows.Ink", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x026b] = new TypeDeclaration("StrokeCollectionConverter", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x026c] = new TypeDeclaration("Style", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x026d] = new TypeDeclaration("Stylus", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x026e] = new TypeDeclaration("StylusDevice", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x026f] = new TypeDeclaration("TabControl", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0270] = new TypeDeclaration("TabItem", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0272] = new TypeDeclaration("Table", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0273] = new TypeDeclaration("TableCell", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0274] = new TypeDeclaration("TableColumn", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0275] = new TypeDeclaration("TableRow", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0276] = new TypeDeclaration("TableRowGroup", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0277] = new TypeDeclaration("TabletDevice", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0271] = new TypeDeclaration("TabPanel", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0278] = new TypeDeclaration("TemplateBindingExpression", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0279] = new TypeDeclaration("TemplateBindingExpressionConverter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x027a] = new TypeDeclaration("TemplateBindingExtension", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x027b] = new TypeDeclaration("TemplateBindingExtensionConverter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x027c] = new TypeDeclaration("TemplateKey", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x027d] = new TypeDeclaration("TemplateKeyConverter", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x027e] = new TypeDeclaration("TextBlock", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x027f] = new TypeDeclaration("TextBox", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0280] = new TypeDeclaration("TextBoxBase", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0281] = new TypeDeclaration("TextComposition", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0282] = new TypeDeclaration("TextCompositionManager", "System.Windows.Input", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0283] = new TypeDeclaration("TextDecoration", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0284] = new TypeDeclaration("TextDecorationCollection", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0285] = new TypeDeclaration("TextDecorationCollectionConverter", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0286] = new TypeDeclaration("TextEffect", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0287] = new TypeDeclaration("TextEffectCollection", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0288] = new TypeDeclaration("TextElement", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0289] = new TypeDeclaration("TextSearch", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x028a] = new TypeDeclaration("ThemeDictionaryExtension", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x028b] = new TypeDeclaration("Thickness", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x028c] = new TypeDeclaration("ThicknessAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x028d] = new TypeDeclaration("ThicknessAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x028e] = new TypeDeclaration("ThicknessAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x028f] = new TypeDeclaration("ThicknessConverter", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0290] = new TypeDeclaration("ThicknessKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0291] = new TypeDeclaration("ThicknessKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0292] = new TypeDeclaration("Thumb", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0293] = new TypeDeclaration("TickBar", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x0294] = new TypeDeclaration("TiffBitmapDecoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0295] = new TypeDeclaration("TiffBitmapEncoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0296] = new TypeDeclaration("TileBrush", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0299] = new TypeDeclaration("Timeline", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x029a] = new TypeDeclaration("TimelineCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x029b] = new TypeDeclaration("TimelineGroup", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x0297] = new TypeDeclaration("TimeSpan", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x0298] = new TypeDeclaration("TimeSpanConverter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x029c] = new TypeDeclaration("ToggleButton", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x029d] = new TypeDeclaration("ToolBar", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x029e] = new TypeDeclaration("ToolBarOverflowPanel", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x029f] = new TypeDeclaration("ToolBarPanel", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02a0] = new TypeDeclaration("ToolBarTray", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02a1] = new TypeDeclaration("ToolTip", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02a2] = new TypeDeclaration("ToolTipService", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02a3] = new TypeDeclaration("Track", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02a4] = new TypeDeclaration("Transform", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02a5] = new TypeDeclaration("Transform3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02a6] = new TypeDeclaration("Transform3DCollection", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02a7] = new TypeDeclaration("Transform3DGroup", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02a8] = new TypeDeclaration("TransformCollection", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02a9] = new TypeDeclaration("TransformConverter", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02ab] = new TypeDeclaration("TransformedBitmap", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02aa] = new TypeDeclaration("TransformGroup", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02ac] = new TypeDeclaration("TranslateTransform", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02ad] = new TypeDeclaration("TranslateTransform3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02ae] = new TypeDeclaration("TreeView", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02af] = new TypeDeclaration("TreeViewItem", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02b0] = new TypeDeclaration("Trigger", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02b1] = new TypeDeclaration("TriggerAction", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02b2] = new TypeDeclaration("TriggerBase", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02b3] = new TypeDeclaration("TypeExtension", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02b4] = new TypeDeclaration("TypeTypeConverter", "System.Net.Configuration", this.GetAssembly("System"));
            _knownTypeTable[0x02b5] = new TypeDeclaration("Typography", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02b6] = new TypeDeclaration("UIElement", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02b7] = new TypeDeclaration("UInt16", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x02b8] = new TypeDeclaration("UInt16Converter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x02b9] = new TypeDeclaration("UInt32", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x02ba] = new TypeDeclaration("UInt32Converter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x02bb] = new TypeDeclaration("UInt64", "System", this.GetAssembly("mscorlib"));
            _knownTypeTable[0x02bc] = new TypeDeclaration("UInt64Converter", "System.ComponentModel", this.GetAssembly("System"));
            _knownTypeTable[0x02be] = new TypeDeclaration("Underline", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02bf] = new TypeDeclaration("UniformGrid", "System.Windows.Controls.Primitives", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02c0] = new TypeDeclaration("Uri", "System", this.GetAssembly("System"));
            _knownTypeTable[0x02c1] = new TypeDeclaration("UriTypeConverter", "System", this.GetAssembly("System"));
            _knownTypeTable[0x02c2] = new TypeDeclaration("UserControl", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02bd] = new TypeDeclaration("UShortIListConverter", "System.Windows.Media.Converters", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02c3] = new TypeDeclaration("Validation", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02c4] = new TypeDeclaration("Vector", "System.Windows", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02c5] = new TypeDeclaration("Vector3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02c6] = new TypeDeclaration("Vector3DAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02c7] = new TypeDeclaration("Vector3DAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02c8] = new TypeDeclaration("Vector3DAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02c9] = new TypeDeclaration("Vector3DCollection", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02ca] = new TypeDeclaration("Vector3DCollectionConverter", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02cb] = new TypeDeclaration("Vector3DConverter", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02cc] = new TypeDeclaration("Vector3DKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02cd] = new TypeDeclaration("Vector3DKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02ce] = new TypeDeclaration("VectorAnimation", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02cf] = new TypeDeclaration("VectorAnimationBase", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02d0] = new TypeDeclaration("VectorAnimationUsingKeyFrames", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02d1] = new TypeDeclaration("VectorCollection", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02d2] = new TypeDeclaration("VectorCollectionConverter", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02d3] = new TypeDeclaration("VectorConverter", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x02d4] = new TypeDeclaration("VectorKeyFrame", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02d5] = new TypeDeclaration("VectorKeyFrameCollection", "System.Windows.Media.Animation", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02d6] = new TypeDeclaration("VideoDrawing", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02d7] = new TypeDeclaration("ViewBase", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02d8] = new TypeDeclaration("Viewbox", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02d9] = new TypeDeclaration("Viewport3D", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02da] = new TypeDeclaration("Viewport3DVisual", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02db] = new TypeDeclaration("VirtualizingPanel", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02dc] = new TypeDeclaration("VirtualizingStackPanel", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02dd] = new TypeDeclaration("Visual", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02de] = new TypeDeclaration("Visual3D", "System.Windows.Media.Media3D", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02df] = new TypeDeclaration("VisualBrush", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02e0] = new TypeDeclaration("VisualTarget", "System.Windows.Media", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02e1] = new TypeDeclaration("WeakEventManager", "System.Windows", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x02e2] = new TypeDeclaration("WhitespaceSignificantCollectionAttribute", "System.Windows.Markup", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x02e3] = new TypeDeclaration("Window", "System.Windows", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02e4] = new TypeDeclaration("WmpBitmapDecoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02e5] = new TypeDeclaration("WmpBitmapEncoder", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02e6] = new TypeDeclaration("WrapPanel", "System.Windows.Controls", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02e7] = new TypeDeclaration("WriteableBitmap", "System.Windows.Media.Imaging", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02e8] = new TypeDeclaration("XamlBrushSerializer", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02e9] = new TypeDeclaration("XamlInt32CollectionSerializer", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02ea] = new TypeDeclaration("XamlPathDataSerializer", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02eb] = new TypeDeclaration("XamlPoint3DCollectionSerializer", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02ec] = new TypeDeclaration("XamlPointCollectionSerializer", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02ed] = new TypeDeclaration("XamlReader", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02ee] = new TypeDeclaration("XamlStyleSerializer", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02ef] = new TypeDeclaration("XamlTemplateSerializer", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02f0] = new TypeDeclaration("XamlVector3DCollectionSerializer", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02f1] = new TypeDeclaration("XamlWriter", "System.Windows.Markup", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02f2] = new TypeDeclaration("XmlDataProvider", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02f3] = new TypeDeclaration("XmlLangPropertyAttribute", "System.Windows.Markup", this.GetAssembly("WindowsBase"));
            _knownTypeTable[0x02f4] = new TypeDeclaration("XmlLanguage", "System.Windows.Markup", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02f5] = new TypeDeclaration("XmlLanguageConverter", "System.Windows.Markup", this.GetAssembly("PresentationCore"));
            _knownTypeTable[0x02f6] = new TypeDeclaration("XmlNamespaceMapping", "System.Windows.Data", this.GetAssembly("PresentationFramework"));
            _knownTypeTable[0x02f7] = new TypeDeclaration("ZoomPercentageConverter", "System.Windows.Documents", this.GetAssembly("PresentationFramework"));

            _knownPropertyTable = new PropertyDeclaration[0x010d];
            _knownPropertyTable[0x0001] = new PropertyDeclaration("Text", _knownTypeTable[0x0001]); // AccessText
            _knownPropertyTable[0x0002] = new PropertyDeclaration("Storyboard", _knownTypeTable[0x0011]); // BeginStoryboard
            _knownPropertyTable[0x0003] = new PropertyDeclaration("Children", _knownTypeTable[0x001c]); // BitmapEffectGroup
            _knownPropertyTable[0x0004] = new PropertyDeclaration("Background", _knownTypeTable[0x0032]); // Border
            _knownPropertyTable[0x0005] = new PropertyDeclaration("BorderBrush", _knownTypeTable[0x0032]); // Border
            _knownPropertyTable[0x0006] = new PropertyDeclaration("BorderThickness", _knownTypeTable[0x0032]); // Border
            _knownPropertyTable[0x0007] = new PropertyDeclaration("Command", _knownTypeTable[0x0038]); // ButtonBase
            _knownPropertyTable[0x0008] = new PropertyDeclaration("CommandParameter", _knownTypeTable[0x0038]); // ButtonBase
            _knownPropertyTable[0x0009] = new PropertyDeclaration("CommandTarget", _knownTypeTable[0x0038]); // ButtonBase
            _knownPropertyTable[0x000a] = new PropertyDeclaration("IsPressed", _knownTypeTable[0x0038]); // ButtonBase
            _knownPropertyTable[0x000b] = new PropertyDeclaration("MaxWidth", _knownTypeTable[0x005a]); // ColumnDefinition
            _knownPropertyTable[0x000c] = new PropertyDeclaration("MinWidth", _knownTypeTable[0x005a]); // ColumnDefinition
            _knownPropertyTable[0x000d] = new PropertyDeclaration("Width", _knownTypeTable[0x005a]); // ColumnDefinition
            _knownPropertyTable[0x000e] = new PropertyDeclaration("Content", _knownTypeTable[0x0064]); // ContentControl
            _knownPropertyTable[0x000f] = new PropertyDeclaration("ContentTemplate", _knownTypeTable[0x0064]); // ContentControl
            _knownPropertyTable[0x0010] = new PropertyDeclaration("ContentTemplateSelector", _knownTypeTable[0x0064]); // ContentControl
            _knownPropertyTable[0x0011] = new PropertyDeclaration("HasContent", _knownTypeTable[0x0064]); // ContentControl
            _knownPropertyTable[0x0012] = new PropertyDeclaration("Focusable", _knownTypeTable[0x0065]); // ContentElement
            _knownPropertyTable[0x0013] = new PropertyDeclaration("Content", _knownTypeTable[0x0066]); // ContentPresenter
            _knownPropertyTable[0x0014] = new PropertyDeclaration("ContentSource", _knownTypeTable[0x0066]); // ContentPresenter
            _knownPropertyTable[0x0015] = new PropertyDeclaration("ContentTemplate", _knownTypeTable[0x0066]); // ContentPresenter
            _knownPropertyTable[0x0016] = new PropertyDeclaration("ContentTemplateSelector", _knownTypeTable[0x0066]); // ContentPresenter
            _knownPropertyTable[0x0017] = new PropertyDeclaration("RecognizesAccessKey", _knownTypeTable[0x0066]); // ContentPresenter
            _knownPropertyTable[0x0018] = new PropertyDeclaration("Background", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x0019] = new PropertyDeclaration("BorderBrush", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x001a] = new PropertyDeclaration("BorderThickness", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x001b] = new PropertyDeclaration("FontFamily", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x001c] = new PropertyDeclaration("FontSize", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x001d] = new PropertyDeclaration("FontStretch", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x001e] = new PropertyDeclaration("FontStyle", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x001f] = new PropertyDeclaration("FontWeight", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x0020] = new PropertyDeclaration("Foreground", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x0021] = new PropertyDeclaration("HorizontalContentAlignment", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x0022] = new PropertyDeclaration("IsTabStop", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x0023] = new PropertyDeclaration("Padding", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x0024] = new PropertyDeclaration("TabIndex", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x0025] = new PropertyDeclaration("Template", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x0026] = new PropertyDeclaration("VerticalContentAlignment", _knownTypeTable[0x006b]); // Control
            _knownPropertyTable[0x0027] = new PropertyDeclaration("Dock", _knownTypeTable[0x00a3]); // DockPanel
            _knownPropertyTable[0x0028] = new PropertyDeclaration("LastChildFill", _knownTypeTable[0x00a3]); // DockPanel
            _knownPropertyTable[0x0029] = new PropertyDeclaration("Document", _knownTypeTable[0x00a7]); // DocumentViewerBase
            _knownPropertyTable[0x002a] = new PropertyDeclaration("Children", _knownTypeTable[0x00b7]); // DrawingGroup
            _knownPropertyTable[0x002b] = new PropertyDeclaration("Document", _knownTypeTable[0x00d3]); // FlowDocumentReader
            _knownPropertyTable[0x002c] = new PropertyDeclaration("Document", _knownTypeTable[0x00d4]); // FlowDocumentScrollViewer
            _knownPropertyTable[0x002d] = new PropertyDeclaration("Style", _knownTypeTable[0x00e1]); // FrameworkContentElement
            _knownPropertyTable[0x002e] = new PropertyDeclaration("FlowDirection", _knownTypeTable[0x00e2]); // FrameworkElement
            _knownPropertyTable[0x002f] = new PropertyDeclaration("Height", _knownTypeTable[0x00e2]); // FrameworkElement
            _knownPropertyTable[0x0030] = new PropertyDeclaration("HorizontalAlignment", _knownTypeTable[0x00e2]); // FrameworkElement
            _knownPropertyTable[0x0031] = new PropertyDeclaration("Margin", _knownTypeTable[0x00e2]); // FrameworkElement
            _knownPropertyTable[0x0032] = new PropertyDeclaration("MaxHeight", _knownTypeTable[0x00e2]); // FrameworkElement
            _knownPropertyTable[0x0033] = new PropertyDeclaration("MaxWidth", _knownTypeTable[0x00e2]); // FrameworkElement
            _knownPropertyTable[0x0034] = new PropertyDeclaration("MinHeight", _knownTypeTable[0x00e2]); // FrameworkElement
            _knownPropertyTable[0x0035] = new PropertyDeclaration("MinWidth", _knownTypeTable[0x00e2]); // FrameworkElement
            _knownPropertyTable[0x0036] = new PropertyDeclaration("Name", _knownTypeTable[0x00e2]); // FrameworkElement
            _knownPropertyTable[0x0037] = new PropertyDeclaration("Style", _knownTypeTable[0x00e2]); // FrameworkElement
            _knownPropertyTable[0x0038] = new PropertyDeclaration("VerticalAlignment", _knownTypeTable[0x00e2]); // FrameworkElement
            _knownPropertyTable[0x0039] = new PropertyDeclaration("Width", _knownTypeTable[0x00e2]); // FrameworkElement
            _knownPropertyTable[0x003a] = new PropertyDeclaration("Children", _knownTypeTable[0x00ec]); // GeneralTransformGroup
            _knownPropertyTable[0x003b] = new PropertyDeclaration("Children", _knownTypeTable[0x00f2]); // GeometryGroup
            _knownPropertyTable[0x003c] = new PropertyDeclaration("GradientStops", _knownTypeTable[0x00fb]); // GradientBrush
            _knownPropertyTable[0x003d] = new PropertyDeclaration("Column", _knownTypeTable[0x00fe]); // Grid
            _knownPropertyTable[0x003e] = new PropertyDeclaration("ColumnSpan", _knownTypeTable[0x00fe]); // Grid
            _knownPropertyTable[0x003f] = new PropertyDeclaration("Row", _knownTypeTable[0x00fe]); // Grid
            _knownPropertyTable[0x0040] = new PropertyDeclaration("RowSpan", _knownTypeTable[0x00fe]); // Grid
            _knownPropertyTable[0x0041] = new PropertyDeclaration("Header", _knownTypeTable[0x0103]); // GridViewColumn
            _knownPropertyTable[0x0042] = new PropertyDeclaration("HasHeader", _knownTypeTable[0x010d]); // HeaderedContentControl
            _knownPropertyTable[0x0043] = new PropertyDeclaration("Header", _knownTypeTable[0x010d]); // HeaderedContentControl
            _knownPropertyTable[0x0044] = new PropertyDeclaration("HeaderTemplate", _knownTypeTable[0x010d]); // HeaderedContentControl
            _knownPropertyTable[0x0045] = new PropertyDeclaration("HeaderTemplateSelector", _knownTypeTable[0x010d]); // HeaderedContentControl
            _knownPropertyTable[0x0046] = new PropertyDeclaration("HasHeader", _knownTypeTable[0x010e]); // HeaderedItemsControl
            _knownPropertyTable[0x0047] = new PropertyDeclaration("Header", _knownTypeTable[0x010e]); // HeaderedItemsControl
            _knownPropertyTable[0x0048] = new PropertyDeclaration("HeaderTemplate", _knownTypeTable[0x010e]); // HeaderedItemsControl
            _knownPropertyTable[0x0049] = new PropertyDeclaration("HeaderTemplateSelector", _knownTypeTable[0x010e]); // HeaderedItemsControl
            _knownPropertyTable[0x004a] = new PropertyDeclaration("NavigateUri", _knownTypeTable[0x0111]); // Hyperlink
            _knownPropertyTable[0x004b] = new PropertyDeclaration("Source", _knownTypeTable[0x0119]); // Image
            _knownPropertyTable[0x004c] = new PropertyDeclaration("Stretch", _knownTypeTable[0x0119]); // Image
            _knownPropertyTable[0x004d] = new PropertyDeclaration("ItemContainerStyle", _knownTypeTable[0x0149]); // ItemsControl
            _knownPropertyTable[0x004e] = new PropertyDeclaration("ItemContainerStyleSelector", _knownTypeTable[0x0149]); // ItemsControl
            _knownPropertyTable[0x004f] = new PropertyDeclaration("ItemTemplate", _knownTypeTable[0x0149]); // ItemsControl
            _knownPropertyTable[0x0050] = new PropertyDeclaration("ItemTemplateSelector", _knownTypeTable[0x0149]); // ItemsControl
            _knownPropertyTable[0x0051] = new PropertyDeclaration("ItemsPanel", _knownTypeTable[0x0149]); // ItemsControl
            _knownPropertyTable[0x0052] = new PropertyDeclaration("ItemsSource", _knownTypeTable[0x0149]); // ItemsControl
            _knownPropertyTable[0x0053] = new PropertyDeclaration("Children", _knownTypeTable[0x0180]); // MaterialGroup
            _knownPropertyTable[0x0054] = new PropertyDeclaration("Children", _knownTypeTable[0x0198]); // Model3DGroup
            _knownPropertyTable[0x0055] = new PropertyDeclaration("Content", _knownTypeTable[0x01b2]); // Page
            _knownPropertyTable[0x0056] = new PropertyDeclaration("Background", _knownTypeTable[0x01b5]); // Panel
            _knownPropertyTable[0x0057] = new PropertyDeclaration("Data", _knownTypeTable[0x01ba]); // Path
            _knownPropertyTable[0x0058] = new PropertyDeclaration("Segments", _knownTypeTable[0x01bb]); // PathFigure
            _knownPropertyTable[0x0059] = new PropertyDeclaration("Figures", _knownTypeTable[0x01be]); // PathGeometry
            _knownPropertyTable[0x005a] = new PropertyDeclaration("Child", _knownTypeTable[0x01e5]); // Popup
            _knownPropertyTable[0x005b] = new PropertyDeclaration("IsOpen", _knownTypeTable[0x01e5]); // Popup
            _knownPropertyTable[0x005c] = new PropertyDeclaration("Placement", _knownTypeTable[0x01e5]); // Popup
            _knownPropertyTable[0x005d] = new PropertyDeclaration("PopupAnimation", _knownTypeTable[0x01e5]); // Popup
            _knownPropertyTable[0x005e] = new PropertyDeclaration("Height", _knownTypeTable[0x021d]); // RowDefinition
            _knownPropertyTable[0x005f] = new PropertyDeclaration("MaxHeight", _knownTypeTable[0x021d]); // RowDefinition
            _knownPropertyTable[0x0060] = new PropertyDeclaration("MinHeight", _knownTypeTable[0x021d]); // RowDefinition
            _knownPropertyTable[0x0061] = new PropertyDeclaration("CanContentScroll", _knownTypeTable[0x0226]); // ScrollViewer
            _knownPropertyTable[0x0062] = new PropertyDeclaration("HorizontalScrollBarVisibility", _knownTypeTable[0x0226]); // ScrollViewer
            _knownPropertyTable[0x0063] = new PropertyDeclaration("VerticalScrollBarVisibility", _knownTypeTable[0x0226]); // ScrollViewer
            _knownPropertyTable[0x0064] = new PropertyDeclaration("Fill", _knownTypeTable[0x022e]); // Shape
            _knownPropertyTable[0x0065] = new PropertyDeclaration("Stroke", _knownTypeTable[0x022e]); // Shape
            _knownPropertyTable[0x0066] = new PropertyDeclaration("StrokeThickness", _knownTypeTable[0x022e]); // Shape
            _knownPropertyTable[0x0067] = new PropertyDeclaration("Background", _knownTypeTable[0x027e]); // TextBlock
            _knownPropertyTable[0x0068] = new PropertyDeclaration("FontFamily", _knownTypeTable[0x027e]); // TextBlock
            _knownPropertyTable[0x0069] = new PropertyDeclaration("FontSize", _knownTypeTable[0x027e]); // TextBlock
            _knownPropertyTable[0x006a] = new PropertyDeclaration("FontStretch", _knownTypeTable[0x027e]); // TextBlock
            _knownPropertyTable[0x006b] = new PropertyDeclaration("FontStyle", _knownTypeTable[0x027e]); // TextBlock
            _knownPropertyTable[0x006c] = new PropertyDeclaration("FontWeight", _knownTypeTable[0x027e]); // TextBlock
            _knownPropertyTable[0x006d] = new PropertyDeclaration("Foreground", _knownTypeTable[0x027e]); // TextBlock
            _knownPropertyTable[0x006e] = new PropertyDeclaration("Text", _knownTypeTable[0x027e]); // TextBlock
            _knownPropertyTable[0x006f] = new PropertyDeclaration("TextDecorations", _knownTypeTable[0x027e]); // TextBlock
            _knownPropertyTable[0x0070] = new PropertyDeclaration("TextTrimming", _knownTypeTable[0x027e]); // TextBlock
            _knownPropertyTable[0x0071] = new PropertyDeclaration("TextWrapping", _knownTypeTable[0x027e]); // TextBlock
            _knownPropertyTable[0x0072] = new PropertyDeclaration("Text", _knownTypeTable[0x027f]); // TextBox
            _knownPropertyTable[0x0073] = new PropertyDeclaration("Background", _knownTypeTable[0x0288]); // TextElement
            _knownPropertyTable[0x0074] = new PropertyDeclaration("FontFamily", _knownTypeTable[0x0288]); // TextElement
            _knownPropertyTable[0x0075] = new PropertyDeclaration("FontSize", _knownTypeTable[0x0288]); // TextElement
            _knownPropertyTable[0x0076] = new PropertyDeclaration("FontStretch", _knownTypeTable[0x0288]); // TextElement
            _knownPropertyTable[0x0077] = new PropertyDeclaration("FontStyle", _knownTypeTable[0x0288]); // TextElement
            _knownPropertyTable[0x0078] = new PropertyDeclaration("FontWeight", _knownTypeTable[0x0288]); // TextElement
            _knownPropertyTable[0x0079] = new PropertyDeclaration("Foreground", _knownTypeTable[0x0288]); // TextElement
            _knownPropertyTable[0x007a] = new PropertyDeclaration("Children", _knownTypeTable[0x029b]); // TimelineGroup
            _knownPropertyTable[0x007b] = new PropertyDeclaration("IsDirectionReversed", _knownTypeTable[0x02a3]); // Track
            _knownPropertyTable[0x007c] = new PropertyDeclaration("Maximum", _knownTypeTable[0x02a3]); // Track
            _knownPropertyTable[0x007d] = new PropertyDeclaration("Minimum", _knownTypeTable[0x02a3]); // Track
            _knownPropertyTable[0x007e] = new PropertyDeclaration("Orientation", _knownTypeTable[0x02a3]); // Track
            _knownPropertyTable[0x007f] = new PropertyDeclaration("Value", _knownTypeTable[0x02a3]); // Track
            _knownPropertyTable[0x0080] = new PropertyDeclaration("ViewportSize", _knownTypeTable[0x02a3]); // Track
            _knownPropertyTable[0x0081] = new PropertyDeclaration("Children", _knownTypeTable[0x02a7]); // Transform3DGroup
            _knownPropertyTable[0x0082] = new PropertyDeclaration("Children", _knownTypeTable[0x02aa]); // TransformGroup
            _knownPropertyTable[0x0083] = new PropertyDeclaration("ClipToBounds", _knownTypeTable[0x02b6]); // UIElement
            _knownPropertyTable[0x0084] = new PropertyDeclaration("Focusable", _knownTypeTable[0x02b6]); // UIElement
            _knownPropertyTable[0x0085] = new PropertyDeclaration("IsEnabled", _knownTypeTable[0x02b6]); // UIElement
            _knownPropertyTable[0x0086] = new PropertyDeclaration("RenderTransform", _knownTypeTable[0x02b6]); // UIElement
            _knownPropertyTable[0x0087] = new PropertyDeclaration("Visibility", _knownTypeTable[0x02b6]); // UIElement
            _knownPropertyTable[0x0088] = new PropertyDeclaration("Children", _knownTypeTable[0x02d9]); // Viewport3D
            _knownPropertyTable[0x008a] = new PropertyDeclaration("Child", _knownTypeTable[0x0002]); // AdornedElementPlaceholder
            _knownPropertyTable[0x008b] = new PropertyDeclaration("Child", _knownTypeTable[0x0004]); // AdornerDecorator
            _knownPropertyTable[0x008c] = new PropertyDeclaration("Blocks", _knownTypeTable[0x0008]); // AnchoredBlock
            _knownPropertyTable[0x008d] = new PropertyDeclaration("Items", _knownTypeTable[0x000e]); // ArrayExtension
            _knownPropertyTable[0x008e] = new PropertyDeclaration("Child", _knownTypeTable[0x0025]); // BlockUIContainer
            _knownPropertyTable[0x008f] = new PropertyDeclaration("Inlines", _knownTypeTable[0x0029]); // Bold
            _knownPropertyTable[0x0090] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x002d]); // BooleanAnimationUsingKeyFrames
            _knownPropertyTable[0x0091] = new PropertyDeclaration("Child", _knownTypeTable[0x0032]); // Border
            _knownPropertyTable[0x0092] = new PropertyDeclaration("Child", _knownTypeTable[0x0036]); // BulletDecorator
            _knownPropertyTable[0x0093] = new PropertyDeclaration("Content", _knownTypeTable[0x0037]); // Button
            _knownPropertyTable[0x0094] = new PropertyDeclaration("Content", _knownTypeTable[0x0038]); // ButtonBase
            _knownPropertyTable[0x0095] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x003c]); // ByteAnimationUsingKeyFrames
            _knownPropertyTable[0x0096] = new PropertyDeclaration("Children", _knownTypeTable[0x0042]); // Canvas
            _knownPropertyTable[0x0097] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x0045]); // CharAnimationUsingKeyFrames
            _knownPropertyTable[0x0098] = new PropertyDeclaration("Content", _knownTypeTable[0x004a]); // CheckBox
            _knownPropertyTable[0x0099] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x0054]); // ColorAnimationUsingKeyFrames
            _knownPropertyTable[0x009a] = new PropertyDeclaration("Items", _knownTypeTable[0x005c]); // ComboBox
            _knownPropertyTable[0x009b] = new PropertyDeclaration("Content", _knownTypeTable[0x005d]); // ComboBoxItem
            _knownPropertyTable[0x009c] = new PropertyDeclaration("Items", _knownTypeTable[0x0069]); // ContextMenu
            _knownPropertyTable[0x009d] = new PropertyDeclaration("VisualTree", _knownTypeTable[0x006c]); // ControlTemplate
            _knownPropertyTable[0x009e] = new PropertyDeclaration("VisualTree", _knownTypeTable[0x0078]); // DataTemplate
            _knownPropertyTable[0x009f] = new PropertyDeclaration("Setters", _knownTypeTable[0x007a]); // DataTrigger
            _knownPropertyTable[0x00a0] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x0081]); // DecimalAnimationUsingKeyFrames
            _knownPropertyTable[0x00a1] = new PropertyDeclaration("Child", _knownTypeTable[0x0085]); // Decorator
            _knownPropertyTable[0x00a2] = new PropertyDeclaration("Children", _knownTypeTable[0x00a3]); // DockPanel
            _knownPropertyTable[0x00a3] = new PropertyDeclaration("Document", _knownTypeTable[0x00a6]); // DocumentViewer
            _knownPropertyTable[0x00a4] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x00ab]); // DoubleAnimationUsingKeyFrames
            _knownPropertyTable[0x00a5] = new PropertyDeclaration("Actions", _knownTypeTable[0x00c6]); // EventTrigger
            _knownPropertyTable[0x00a6] = new PropertyDeclaration("Content", _knownTypeTable[0x00c7]); // Expander
            _knownPropertyTable[0x00a7] = new PropertyDeclaration("Blocks", _knownTypeTable[0x00ca]); // Figure
            _knownPropertyTable[0x00a8] = new PropertyDeclaration("Pages", _knownTypeTable[0x00cd]); // FixedDocument
            _knownPropertyTable[0x00a9] = new PropertyDeclaration("References", _knownTypeTable[0x00ce]); // FixedDocumentSequence
            _knownPropertyTable[0x00aa] = new PropertyDeclaration("Children", _knownTypeTable[0x00cf]); // FixedPage
            _knownPropertyTable[0x00ab] = new PropertyDeclaration("Blocks", _knownTypeTable[0x00d0]); // Floater
            _knownPropertyTable[0x00ac] = new PropertyDeclaration("Blocks", _knownTypeTable[0x00d1]); // FlowDocument
            _knownPropertyTable[0x00ad] = new PropertyDeclaration("Document", _knownTypeTable[0x00d2]); // FlowDocumentPageViewer
            _knownPropertyTable[0x00ae] = new PropertyDeclaration("VisualTree", _knownTypeTable[0x00e7]); // FrameworkTemplate
            _knownPropertyTable[0x00af] = new PropertyDeclaration("Children", _knownTypeTable[0x00fe]); // Grid
            _knownPropertyTable[0x00b0] = new PropertyDeclaration("Columns", _knownTypeTable[0x0102]); // GridView
            _knownPropertyTable[0x00b1] = new PropertyDeclaration("Content", _knownTypeTable[0x0104]); // GridViewColumnHeader
            _knownPropertyTable[0x00b2] = new PropertyDeclaration("Content", _knownTypeTable[0x0108]); // GroupBox
            _knownPropertyTable[0x00b3] = new PropertyDeclaration("Content", _knownTypeTable[0x0109]); // GroupItem
            _knownPropertyTable[0x00b4] = new PropertyDeclaration("Content", _knownTypeTable[0x010d]); // HeaderedContentControl
            _knownPropertyTable[0x00b5] = new PropertyDeclaration("Items", _knownTypeTable[0x010e]); // HeaderedItemsControl
            _knownPropertyTable[0x00b6] = new PropertyDeclaration("VisualTree", _knownTypeTable[0x010f]); // HierarchicalDataTemplate
            _knownPropertyTable[0x00b7] = new PropertyDeclaration("Inlines", _knownTypeTable[0x0111]); // Hyperlink
            _knownPropertyTable[0x00b8] = new PropertyDeclaration("Children", _knownTypeTable[0x0120]); // InkCanvas
            _knownPropertyTable[0x00b9] = new PropertyDeclaration("Child", _knownTypeTable[0x0121]); // InkPresenter
            _knownPropertyTable[0x00ba] = new PropertyDeclaration("Child", _knownTypeTable[0x0124]); // InlineUIContainer
            _knownPropertyTable[0x00bb] = new PropertyDeclaration("NameValue", _knownTypeTable[0x012c]); // InputScopeName
            _knownPropertyTable[0x00bc] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x0131]); // Int16AnimationUsingKeyFrames
            _knownPropertyTable[0x00bd] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x0138]); // Int32AnimationUsingKeyFrames
            _knownPropertyTable[0x00be] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x0143]); // Int64AnimationUsingKeyFrames
            _knownPropertyTable[0x00bf] = new PropertyDeclaration("Inlines", _knownTypeTable[0x0147]); // Italic
            _knownPropertyTable[0x00c0] = new PropertyDeclaration("Items", _knownTypeTable[0x0149]); // ItemsControl
            _knownPropertyTable[0x00c1] = new PropertyDeclaration("VisualTree", _knownTypeTable[0x014a]); // ItemsPanelTemplate
            _knownPropertyTable[0x00c2] = new PropertyDeclaration("Content", _knownTypeTable[0x015a]); // Label
            _knownPropertyTable[0x00c3] = new PropertyDeclaration("GradientStops", _knownTypeTable[0x0166]); // LinearGradientBrush
            _knownPropertyTable[0x00c4] = new PropertyDeclaration("ListItems", _knownTypeTable[0x0174]); // List
            _knownPropertyTable[0x00c5] = new PropertyDeclaration("Items", _knownTypeTable[0x0175]); // ListBox
            _knownPropertyTable[0x00c6] = new PropertyDeclaration("Content", _knownTypeTable[0x0176]); // ListBoxItem
            _knownPropertyTable[0x00c7] = new PropertyDeclaration("Blocks", _knownTypeTable[0x0178]); // ListItem
            _knownPropertyTable[0x00c8] = new PropertyDeclaration("Items", _knownTypeTable[0x0179]); // ListView
            _knownPropertyTable[0x00c9] = new PropertyDeclaration("Content", _knownTypeTable[0x017a]); // ListViewItem
            _knownPropertyTable[0x00ca] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x0185]); // MatrixAnimationUsingKeyFrames
            _knownPropertyTable[0x00cb] = new PropertyDeclaration("Items", _knownTypeTable[0x0191]); // Menu
            _knownPropertyTable[0x00cc] = new PropertyDeclaration("Items", _knownTypeTable[0x0192]); // MenuBase
            _knownPropertyTable[0x00cd] = new PropertyDeclaration("Items", _knownTypeTable[0x0193]); // MenuItem
            _knownPropertyTable[0x00ce] = new PropertyDeclaration("Children", _knownTypeTable[0x0199]); // ModelVisual3D
            _knownPropertyTable[0x00cf] = new PropertyDeclaration("Bindings", _knownTypeTable[0x01a0]); // MultiBinding
            _knownPropertyTable[0x00d0] = new PropertyDeclaration("Setters", _knownTypeTable[0x01a2]); // MultiDataTrigger
            _knownPropertyTable[0x00d1] = new PropertyDeclaration("Setters", _knownTypeTable[0x01a3]); // MultiTrigger
            _knownPropertyTable[0x00d2] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x01ac]); // ObjectAnimationUsingKeyFrames
            _knownPropertyTable[0x00d3] = new PropertyDeclaration("Child", _knownTypeTable[0x01b3]); // PageContent
            _knownPropertyTable[0x00d4] = new PropertyDeclaration("Content", _knownTypeTable[0x01b4]); // PageFunctionBase
            _knownPropertyTable[0x00d5] = new PropertyDeclaration("Children", _knownTypeTable[0x01b5]); // Panel
            _knownPropertyTable[0x00d6] = new PropertyDeclaration("Inlines", _knownTypeTable[0x01b6]); // Paragraph
            _knownPropertyTable[0x00d7] = new PropertyDeclaration("Children", _knownTypeTable[0x01b7]); // ParallelTimeline
            _knownPropertyTable[0x00d8] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x01cc]); // Point3DAnimationUsingKeyFrames
            _knownPropertyTable[0x00d9] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x01d6]); // PointAnimationUsingKeyFrames
            _knownPropertyTable[0x00da] = new PropertyDeclaration("Bindings", _knownTypeTable[0x01e7]); // PriorityBinding
            _knownPropertyTable[0x00db] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x01f1]); // QuaternionAnimationUsingKeyFrames
            _knownPropertyTable[0x00dc] = new PropertyDeclaration("GradientStops", _knownTypeTable[0x01f6]); // RadialGradientBrush
            _knownPropertyTable[0x00dd] = new PropertyDeclaration("Content", _knownTypeTable[0x01f7]); // RadioButton
            _knownPropertyTable[0x00de] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x01fe]); // RectAnimationUsingKeyFrames
            _knownPropertyTable[0x00df] = new PropertyDeclaration("Content", _knownTypeTable[0x020a]); // RepeatButton
            _knownPropertyTable[0x00e0] = new PropertyDeclaration("Document", _knownTypeTable[0x020f]); // RichTextBox
            _knownPropertyTable[0x00e1] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x0215]); // Rotation3DAnimationUsingKeyFrames
            _knownPropertyTable[0x00e2] = new PropertyDeclaration("Text", _knownTypeTable[0x021e]); // Run
            _knownPropertyTable[0x00e3] = new PropertyDeclaration("Content", _knownTypeTable[0x0226]); // ScrollViewer
            _knownPropertyTable[0x00e4] = new PropertyDeclaration("Blocks", _knownTypeTable[0x0227]); // Section
            _knownPropertyTable[0x00e5] = new PropertyDeclaration("Items", _knownTypeTable[0x0229]); // Selector
            _knownPropertyTable[0x00e6] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x0232]); // SingleAnimationUsingKeyFrames
            _knownPropertyTable[0x00e7] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x023b]); // SizeAnimationUsingKeyFrames
            _knownPropertyTable[0x00e8] = new PropertyDeclaration("Inlines", _knownTypeTable[0x0244]); // Span
            _knownPropertyTable[0x00e9] = new PropertyDeclaration("Children", _knownTypeTable[0x0259]); // StackPanel
            _knownPropertyTable[0x00ea] = new PropertyDeclaration("Items", _knownTypeTable[0x025c]); // StatusBar
            _knownPropertyTable[0x00eb] = new PropertyDeclaration("Content", _knownTypeTable[0x025d]); // StatusBarItem
            _knownPropertyTable[0x00ec] = new PropertyDeclaration("Children", _knownTypeTable[0x0260]); // Storyboard
            _knownPropertyTable[0x00ed] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x0266]); // StringAnimationUsingKeyFrames
            _knownPropertyTable[0x00ee] = new PropertyDeclaration("Setters", _knownTypeTable[0x026c]); // Style
            _knownPropertyTable[0x00ef] = new PropertyDeclaration("Items", _knownTypeTable[0x026f]); // TabControl
            _knownPropertyTable[0x00f0] = new PropertyDeclaration("Content", _knownTypeTable[0x0270]); // TabItem
            _knownPropertyTable[0x00f1] = new PropertyDeclaration("Children", _knownTypeTable[0x0271]); // TabPanel
            _knownPropertyTable[0x00f2] = new PropertyDeclaration("RowGroups", _knownTypeTable[0x0272]); // Table
            _knownPropertyTable[0x00f3] = new PropertyDeclaration("Blocks", _knownTypeTable[0x0273]); // TableCell
            _knownPropertyTable[0x00f4] = new PropertyDeclaration("Cells", _knownTypeTable[0x0275]); // TableRow
            _knownPropertyTable[0x00f5] = new PropertyDeclaration("Rows", _knownTypeTable[0x0276]); // TableRowGroup
            _knownPropertyTable[0x00f6] = new PropertyDeclaration("Inlines", _knownTypeTable[0x027e]); // TextBlock
            _knownPropertyTable[0x00f7] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x028e]); // ThicknessAnimationUsingKeyFrames
            _knownPropertyTable[0x00f8] = new PropertyDeclaration("Content", _knownTypeTable[0x029c]); // ToggleButton
            _knownPropertyTable[0x00f9] = new PropertyDeclaration("Items", _knownTypeTable[0x029d]); // ToolBar
            _knownPropertyTable[0x00fa] = new PropertyDeclaration("Children", _knownTypeTable[0x029e]); // ToolBarOverflowPanel
            _knownPropertyTable[0x00fb] = new PropertyDeclaration("Children", _knownTypeTable[0x029f]); // ToolBarPanel
            _knownPropertyTable[0x00fc] = new PropertyDeclaration("ToolBars", _knownTypeTable[0x02a0]); // ToolBarTray
            _knownPropertyTable[0x00fd] = new PropertyDeclaration("Content", _knownTypeTable[0x02a1]); // ToolTip
            _knownPropertyTable[0x00fe] = new PropertyDeclaration("Items", _knownTypeTable[0x02ae]); // TreeView
            _knownPropertyTable[0x00ff] = new PropertyDeclaration("Items", _knownTypeTable[0x02af]); // TreeViewItem
            _knownPropertyTable[0x0100] = new PropertyDeclaration("Setters", _knownTypeTable[0x02b0]); // Trigger
            _knownPropertyTable[0x0101] = new PropertyDeclaration("Inlines", _knownTypeTable[0x02be]); // Underline
            _knownPropertyTable[0x0102] = new PropertyDeclaration("Children", _knownTypeTable[0x02bf]); // UniformGrid
            _knownPropertyTable[0x0103] = new PropertyDeclaration("Content", _knownTypeTable[0x02c2]); // UserControl
            _knownPropertyTable[0x0104] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x02c8]); // Vector3DAnimationUsingKeyFrames
            _knownPropertyTable[0x0105] = new PropertyDeclaration("KeyFrames", _knownTypeTable[0x02d0]); // VectorAnimationUsingKeyFrames
            _knownPropertyTable[0x0106] = new PropertyDeclaration("Child", _knownTypeTable[0x02d8]); // Viewbox
            _knownPropertyTable[0x0107] = new PropertyDeclaration("Children", _knownTypeTable[0x02da]); // Viewport3DVisual
            _knownPropertyTable[0x0108] = new PropertyDeclaration("Children", _knownTypeTable[0x02db]); // VirtualizingPanel
            _knownPropertyTable[0x0109] = new PropertyDeclaration("Children", _knownTypeTable[0x02dc]); // VirtualizingStackPanel
            _knownPropertyTable[0x010a] = new PropertyDeclaration("Content", _knownTypeTable[0x02e3]); // Window
            _knownPropertyTable[0x010b] = new PropertyDeclaration("Children", _knownTypeTable[0x02e6]); // WrapPanel
            _knownPropertyTable[0x010c] = new PropertyDeclaration("XmlSerializer", _knownTypeTable[0x02f2]); // XmlDataProvider

            _knownResourceTable.Add(0x1, new ResourceName("ActiveBorderBrush"));
            _knownResourceTable.Add(0x1f, new ResourceName("ActiveBorderColor"));
            _knownResourceTable.Add(0x2, new ResourceName("ActiveCaptionBrush"));
            _knownResourceTable.Add(0x20, new ResourceName("ActiveCaptionColor"));
            _knownResourceTable.Add(0x3, new ResourceName("ActiveCaptionTextBrush"));
            _knownResourceTable.Add(0x21, new ResourceName("ActiveCaptionTextColor"));
            _knownResourceTable.Add(0x4, new ResourceName("AppWorkspaceBrush"));
            _knownResourceTable.Add(0x22, new ResourceName("AppWorkspaceColor"));
            _knownResourceTable.Add(0xc6, new ResourceName("Border"));
            _knownResourceTable.Add(0xca, new ResourceName("BorderWidth"));
            _knownResourceTable.Add(0x40, new ResourceName("CaptionFontFamily"));
            _knownResourceTable.Add(0x3f, new ResourceName("CaptionFontSize"));
            _knownResourceTable.Add(0x41, new ResourceName("CaptionFontStyle"));
            _knownResourceTable.Add(0x43, new ResourceName("CaptionFontTextDecorations"));
            _knownResourceTable.Add(0x42, new ResourceName("CaptionFontWeight"));
            _knownResourceTable.Add(0xce, new ResourceName("CaptionHeight"));
            _knownResourceTable.Add(0xcd, new ResourceName("CaptionWidth"));
            _knownResourceTable.Add(0xc7, new ResourceName("CaretWidth"));
            _knownResourceTable.Add(0xba, new ResourceName("ClientAreaAnimation"));
            _knownResourceTable.Add(0xb9, new ResourceName("ComboBoxAnimation"));
            _knownResourceTable.Add(0xd2, new ResourceName("ComboBoxPopupAnimation"));
            _knownResourceTable.Add(0x5, new ResourceName("ControlBrush"));
            _knownResourceTable.Add(0x23, new ResourceName("ControlColor"));
            _knownResourceTable.Add(0x6, new ResourceName("ControlDarkBrush"));
            _knownResourceTable.Add(0x24, new ResourceName("ControlDarkColor"));
            _knownResourceTable.Add(0x7, new ResourceName("ControlDarkDarkBrush"));
            _knownResourceTable.Add(0x25, new ResourceName("ControlDarkDarkColor"));
            _knownResourceTable.Add(0x8, new ResourceName("ControlLightBrush"));
            _knownResourceTable.Add(0x26, new ResourceName("ControlLightColor"));
            _knownResourceTable.Add(0x9, new ResourceName("ControlLightLightBrush"));
            _knownResourceTable.Add(0x27, new ResourceName("ControlLightLightColor"));
            _knownResourceTable.Add(0xa, new ResourceName("ControlTextBrush"));
            _knownResourceTable.Add(0x28, new ResourceName("ControlTextColor"));
            _knownResourceTable.Add(0x62, new ResourceName("CursorHeight"));
            _knownResourceTable.Add(0xbb, new ResourceName("CursorShadow"));
            _knownResourceTable.Add(0x61, new ResourceName("CursorWidth"));
            _knownResourceTable.Add(0xb, new ResourceName("DesktopBrush"));
            _knownResourceTable.Add(0x29, new ResourceName("DesktopColor"));
            _knownResourceTable.Add(0xc9, new ResourceName("DragFullWindows"));
            _knownResourceTable.Add(0xa7, new ResourceName("DropShadow"));
            _knownResourceTable.Add(0x65, new ResourceName("FixedFrameHorizontalBorderHeight"));
            _knownResourceTable.Add(0x66, new ResourceName("FixedFrameVerticalBorderWidth"));
            _knownResourceTable.Add(0xa8, new ResourceName("FlatMenu"));
            _knownResourceTable.Add(0xa5, new ResourceName("FocusBorderHeight"));
            _knownResourceTable.Add(0xa4, new ResourceName("FocusBorderWidth"));
            _knownResourceTable.Add(0x67, new ResourceName("FocusHorizontalBorderHeight"));
            _knownResourceTable.Add(0x68, new ResourceName("FocusVerticalBorderWidth"));
            _knownResourceTable.Add(0xd7, new ResourceName("FocusVisualStyle"));
            _knownResourceTable.Add(0xc8, new ResourceName("ForegroundFlashCount"));
            _knownResourceTable.Add(0x6a, new ResourceName("FullPrimaryScreenHeight"));
            _knownResourceTable.Add(0x69, new ResourceName("FullPrimaryScreenWidth"));
            _knownResourceTable.Add(0xc, new ResourceName("GradientActiveCaptionBrush"));
            _knownResourceTable.Add(0x2a, new ResourceName("GradientActiveCaptionColor"));
            _knownResourceTable.Add(0xbc, new ResourceName("GradientCaptions"));
            _knownResourceTable.Add(0xd, new ResourceName("GradientInactiveCaptionBrush"));
            _knownResourceTable.Add(0x2b, new ResourceName("GradientInactiveCaptionColor"));
            _knownResourceTable.Add(0xe, new ResourceName("GrayTextBrush"));
            _knownResourceTable.Add(0x2c, new ResourceName("GrayTextColor"));
            _knownResourceTable.Add(0xde, new ResourceName("GridViewItemContainerStyle"));
            _knownResourceTable.Add(0xdc, new ResourceName("GridViewScrollViewerStyle"));
            _knownResourceTable.Add(0xdd, new ResourceName("GridViewStyle"));
            _knownResourceTable.Add(0xa6, new ResourceName("HighContrast"));
            _knownResourceTable.Add(0xf, new ResourceName("HighlightBrush"));
            _knownResourceTable.Add(0x2d, new ResourceName("HighlightColor"));
            _knownResourceTable.Add(0x10, new ResourceName("HighlightTextBrush"));
            _knownResourceTable.Add(0x2e, new ResourceName("HighlightTextColor"));
            _knownResourceTable.Add(0x6b, new ResourceName("HorizontalScrollBarButtonWidth"));
            _knownResourceTable.Add(0x6c, new ResourceName("HorizontalScrollBarHeight"));
            _knownResourceTable.Add(0x6d, new ResourceName("HorizontalScrollBarThumbWidth"));
            _knownResourceTable.Add(0x11, new ResourceName("HotTrackBrush"));
            _knownResourceTable.Add(0x2f, new ResourceName("HotTrackColor"));
            _knownResourceTable.Add(0xbd, new ResourceName("HotTracking"));
            _knownResourceTable.Add(0x59, new ResourceName("IconFontFamily"));
            _knownResourceTable.Add(0x58, new ResourceName("IconFontSize"));
            _knownResourceTable.Add(0x5a, new ResourceName("IconFontStyle"));
            _knownResourceTable.Add(0x5c, new ResourceName("IconFontTextDecorations"));
            _knownResourceTable.Add(0x5b, new ResourceName("IconFontWeight"));
            _knownResourceTable.Add(0x71, new ResourceName("IconGridHeight"));
            _knownResourceTable.Add(0x70, new ResourceName("IconGridWidth"));
            _knownResourceTable.Add(0x6f, new ResourceName("IconHeight"));
            _knownResourceTable.Add(0xaa, new ResourceName("IconHorizontalSpacing"));
            _knownResourceTable.Add(0xac, new ResourceName("IconTitleWrap"));
            _knownResourceTable.Add(0xab, new ResourceName("IconVerticalSpacing"));
            _knownResourceTable.Add(0x6e, new ResourceName("IconWidth"));
            _knownResourceTable.Add(0x12, new ResourceName("InactiveBorderBrush"));
            _knownResourceTable.Add(0x30, new ResourceName("InactiveBorderColor"));
            _knownResourceTable.Add(0x13, new ResourceName("InactiveCaptionBrush"));
            _knownResourceTable.Add(0x31, new ResourceName("InactiveCaptionColor"));
            _knownResourceTable.Add(0x14, new ResourceName("InactiveCaptionTextBrush"));
            _knownResourceTable.Add(0x32, new ResourceName("InactiveCaptionTextColor"));
            _knownResourceTable.Add(0x15, new ResourceName("InfoBrush"));
            _knownResourceTable.Add(0x33, new ResourceName("InfoColor"));
            _knownResourceTable.Add(0x16, new ResourceName("InfoTextBrush"));
            _knownResourceTable.Add(0x34, new ResourceName("InfoTextColor"));
            _knownResourceTable.Add(0x3d, new ResourceName("InternalSystemColorsEnd"));
            _knownResourceTable.Add(0x0, new ResourceName("InternalSystemColorsStart"));
            _knownResourceTable.Add(0x5d, new ResourceName("InternalSystemFontsEnd"));
            _knownResourceTable.Add(0x3e, new ResourceName("InternalSystemFontsStart"));
            _knownResourceTable.Add(0xda, new ResourceName("InternalSystemParametersEnd"));
            _knownResourceTable.Add(0x5e, new ResourceName("InternalSystemParametersStart"));
            _knownResourceTable.Add(0xe8, new ResourceName("InternalSystemThemeStylesEnd"));
            _knownResourceTable.Add(0xd6, new ResourceName("InternalSystemThemeStylesStart"));
            _knownResourceTable.Add(0x95, new ResourceName("IsImmEnabled"));
            _knownResourceTable.Add(0x96, new ResourceName("IsMediaCenter"));
            _knownResourceTable.Add(0x97, new ResourceName("IsMenuDropRightAligned"));
            _knownResourceTable.Add(0x98, new ResourceName("IsMiddleEastEnabled"));
            _knownResourceTable.Add(0x99, new ResourceName("IsMousePresent"));
            _knownResourceTable.Add(0x9a, new ResourceName("IsMouseWheelPresent"));
            _knownResourceTable.Add(0x9b, new ResourceName("IsPenWindows"));
            _knownResourceTable.Add(0x9c, new ResourceName("IsRemotelyControlled"));
            _knownResourceTable.Add(0x9d, new ResourceName("IsRemoteSession"));
            _knownResourceTable.Add(0x9f, new ResourceName("IsSlowMachine"));
            _knownResourceTable.Add(0xa1, new ResourceName("IsTabletPC"));
            _knownResourceTable.Add(0x91, new ResourceName("KanjiWindowHeight"));
            _knownResourceTable.Add(0xad, new ResourceName("KeyboardCues"));
            _knownResourceTable.Add(0xae, new ResourceName("KeyboardDelay"));
            _knownResourceTable.Add(0xaf, new ResourceName("KeyboardPreference"));
            _knownResourceTable.Add(0xb0, new ResourceName("KeyboardSpeed"));
            _knownResourceTable.Add(0xbe, new ResourceName("ListBoxSmoothScrolling"));
            _knownResourceTable.Add(0x73, new ResourceName("MaximizedPrimaryScreenHeight"));
            _knownResourceTable.Add(0x72, new ResourceName("MaximizedPrimaryScreenWidth"));
            _knownResourceTable.Add(0x75, new ResourceName("MaximumWindowTrackHeight"));
            _knownResourceTable.Add(0x74, new ResourceName("MaximumWindowTrackWidth"));
            _knownResourceTable.Add(0xbf, new ResourceName("MenuAnimation"));
            _knownResourceTable.Add(0x18, new ResourceName("MenuBarBrush"));
            _knownResourceTable.Add(0x36, new ResourceName("MenuBarColor"));
            _knownResourceTable.Add(0x92, new ResourceName("MenuBarHeight"));
            _knownResourceTable.Add(0x17, new ResourceName("MenuBrush"));
            _knownResourceTable.Add(0x79, new ResourceName("MenuButtonHeight"));
            _knownResourceTable.Add(0x78, new ResourceName("MenuButtonWidth"));
            _knownResourceTable.Add(0x77, new ResourceName("MenuCheckmarkHeight"));
            _knownResourceTable.Add(0x76, new ResourceName("MenuCheckmarkWidth"));
            _knownResourceTable.Add(0x35, new ResourceName("MenuColor"));
            _knownResourceTable.Add(0xb6, new ResourceName("MenuDropAlignment"));
            _knownResourceTable.Add(0xb7, new ResourceName("MenuFade"));
            _knownResourceTable.Add(0x4a, new ResourceName("MenuFontFamily"));
            _knownResourceTable.Add(0x49, new ResourceName("MenuFontSize"));
            _knownResourceTable.Add(0x4b, new ResourceName("MenuFontStyle"));
            _knownResourceTable.Add(0x4d, new ResourceName("MenuFontTextDecorations"));
            _knownResourceTable.Add(0x4c, new ResourceName("MenuFontWeight"));
            _knownResourceTable.Add(0xd1, new ResourceName("MenuHeight"));
            _knownResourceTable.Add(0x19, new ResourceName("MenuHighlightBrush"));
            _knownResourceTable.Add(0x37, new ResourceName("MenuHighlightColor"));
            _knownResourceTable.Add(0xdb, new ResourceName("MenuItemSeparatorStyle"));
            _knownResourceTable.Add(0xd3, new ResourceName("MenuPopupAnimation"));
            _knownResourceTable.Add(0xb8, new ResourceName("MenuShowDelay"));
            _knownResourceTable.Add(0x1a, new ResourceName("MenuTextBrush"));
            _knownResourceTable.Add(0x38, new ResourceName("MenuTextColor"));
            _knownResourceTable.Add(0xd0, new ResourceName("MenuWidth"));
            _knownResourceTable.Add(0x54, new ResourceName("MessageFontFamily"));
            _knownResourceTable.Add(0x53, new ResourceName("MessageFontSize"));
            _knownResourceTable.Add(0x55, new ResourceName("MessageFontStyle"));
            _knownResourceTable.Add(0x57, new ResourceName("MessageFontTextDecorations"));
            _knownResourceTable.Add(0x56, new ResourceName("MessageFontWeight"));
            _knownResourceTable.Add(0xc5, new ResourceName("MinimizeAnimation"));
            _knownResourceTable.Add(0x7f, new ResourceName("MinimizedGridHeight"));
            _knownResourceTable.Add(0x7e, new ResourceName("MinimizedGridWidth"));
            _knownResourceTable.Add(0x7d, new ResourceName("MinimizedWindowHeight"));
            _knownResourceTable.Add(0x7c, new ResourceName("MinimizedWindowWidth"));
            _knownResourceTable.Add(0x7b, new ResourceName("MinimumWindowHeight"));
            _knownResourceTable.Add(0x81, new ResourceName("MinimumWindowTrackHeight"));
            _knownResourceTable.Add(0x80, new ResourceName("MinimumWindowTrackWidth"));
            _knownResourceTable.Add(0x7a, new ResourceName("MinimumWindowWidth"));
            _knownResourceTable.Add(0xb4, new ResourceName("MouseHoverHeight"));
            _knownResourceTable.Add(0xb3, new ResourceName("MouseHoverTime"));
            _knownResourceTable.Add(0xb5, new ResourceName("MouseHoverWidth"));
            _knownResourceTable.Add(0xd8, new ResourceName("NavigationChromeDownLevelStyle"));
            _knownResourceTable.Add(0xd9, new ResourceName("NavigationChromeStyle"));
            _knownResourceTable.Add(0xd5, new ResourceName("PowerLineStatus"));
            _knownResourceTable.Add(0x83, new ResourceName("PrimaryScreenHeight"));
            _knownResourceTable.Add(0x82, new ResourceName("PrimaryScreenWidth"));
            _knownResourceTable.Add(0x86, new ResourceName("ResizeFrameHorizontalBorderHeight"));
            _knownResourceTable.Add(0x87, new ResourceName("ResizeFrameVerticalBorderWidth"));
            _knownResourceTable.Add(0x1b, new ResourceName("ScrollBarBrush"));
            _knownResourceTable.Add(0x39, new ResourceName("ScrollBarColor"));
            _knownResourceTable.Add(0xcc, new ResourceName("ScrollHeight"));
            _knownResourceTable.Add(0xcb, new ResourceName("ScrollWidth"));
            _knownResourceTable.Add(0xc0, new ResourceName("SelectionFade"));
            _knownResourceTable.Add(0x9e, new ResourceName("ShowSounds"));
            _knownResourceTable.Add(0x45, new ResourceName("SmallCaptionFontFamily"));
            _knownResourceTable.Add(0x44, new ResourceName("SmallCaptionFontSize"));
            _knownResourceTable.Add(0x46, new ResourceName("SmallCaptionFontStyle"));
            _knownResourceTable.Add(0x48, new ResourceName("SmallCaptionFontTextDecorations"));
            _knownResourceTable.Add(0x47, new ResourceName("SmallCaptionFontWeight"));
            _knownResourceTable.Add(0x93, new ResourceName("SmallCaptionHeight"));
            _knownResourceTable.Add(0xcf, new ResourceName("SmallCaptionWidth"));
            _knownResourceTable.Add(0x89, new ResourceName("SmallIconHeight"));
            _knownResourceTable.Add(0x88, new ResourceName("SmallIconWidth"));
            _knownResourceTable.Add(0x8b, new ResourceName("SmallWindowCaptionButtonHeight"));
            _knownResourceTable.Add(0x8a, new ResourceName("SmallWindowCaptionButtonWidth"));
            _knownResourceTable.Add(0xb1, new ResourceName("SnapToDefaultButton"));
            _knownResourceTable.Add(0xdf, new ResourceName("StatusBarSeparatorStyle"));
            _knownResourceTable.Add(0x4f, new ResourceName("StatusFontFamily"));
            _knownResourceTable.Add(0x4e, new ResourceName("StatusFontSize"));
            _knownResourceTable.Add(0x50, new ResourceName("StatusFontStyle"));
            _knownResourceTable.Add(0x52, new ResourceName("StatusFontTextDecorations"));
            _knownResourceTable.Add(0x51, new ResourceName("StatusFontWeight"));
            _knownResourceTable.Add(0xc1, new ResourceName("StylusHotTracking"));
            _knownResourceTable.Add(0xa0, new ResourceName("SwapButtons"));
            _knownResourceTable.Add(0x63, new ResourceName("ThickHorizontalBorderHeight"));
            _knownResourceTable.Add(0x64, new ResourceName("ThickVerticalBorderWidth"));
            _knownResourceTable.Add(0x5f, new ResourceName("ThinHorizontalBorderHeight"));
            _knownResourceTable.Add(0x60, new ResourceName("ThinVerticalBorderWidth"));
            _knownResourceTable.Add(0xe0, new ResourceName("ToolBarButtonStyle"));
            _knownResourceTable.Add(0xe3, new ResourceName("ToolBarCheckBoxStyle"));
            _knownResourceTable.Add(0xe5, new ResourceName("ToolBarComboBoxStyle"));
            _knownResourceTable.Add(0xe7, new ResourceName("ToolBarMenuStyle"));
            _knownResourceTable.Add(0xe4, new ResourceName("ToolBarRadioButtonStyle"));
            _knownResourceTable.Add(0xe2, new ResourceName("ToolBarSeparatorStyle"));
            _knownResourceTable.Add(0xe6, new ResourceName("ToolBarTextBoxStyle"));
            _knownResourceTable.Add(0xe1, new ResourceName("ToolBarToggleButtonStyle"));
            _knownResourceTable.Add(0xc2, new ResourceName("ToolTipAnimation"));
            _knownResourceTable.Add(0xc3, new ResourceName("ToolTipFade"));
            _knownResourceTable.Add(0xd4, new ResourceName("ToolTipPopupAnimation"));
            _knownResourceTable.Add(0xc4, new ResourceName("UIEffects"));
            _knownResourceTable.Add(0x8f, new ResourceName("VerticalScrollBarButtonHeight"));
            _knownResourceTable.Add(0x94, new ResourceName("VerticalScrollBarThumbHeight"));
            _knownResourceTable.Add(0x8e, new ResourceName("VerticalScrollBarWidth"));
            _knownResourceTable.Add(0x8d, new ResourceName("VirtualScreenHeight"));
            _knownResourceTable.Add(0xa2, new ResourceName("VirtualScreenLeft"));
            _knownResourceTable.Add(0xa3, new ResourceName("VirtualScreenTop"));
            _knownResourceTable.Add(0x8c, new ResourceName("VirtualScreenWidth"));
            _knownResourceTable.Add(0xb2, new ResourceName("WheelScrollLines"));
            _knownResourceTable.Add(0x1c, new ResourceName("WindowBrush"));
            _knownResourceTable.Add(0x85, new ResourceName("WindowCaptionButtonHeight"));
            _knownResourceTable.Add(0x84, new ResourceName("WindowCaptionButtonWidth"));
            _knownResourceTable.Add(0x90, new ResourceName("WindowCaptionHeight"));
            _knownResourceTable.Add(0x3a, new ResourceName("WindowColor"));
            _knownResourceTable.Add(0x1d, new ResourceName("WindowFrameBrush"));
            _knownResourceTable.Add(0x3b, new ResourceName("WindowFrameColor"));
            _knownResourceTable.Add(0x1e, new ResourceName("WindowTextBrush"));
            _knownResourceTable.Add(0x3c, new ResourceName("WindowTextColor"));
            _knownResourceTable.Add(0xa9, new ResourceName("WorkArea"));
        }

        #endregion // Private methods

        #region Nested types

        private enum BamlRecordType : byte
        {
            ClrEvent = 0x13,
            Comment = 0x17,
            AssemblyInfo = 0x1c,
            AttributeInfo = 0x1f,
            ConstructorParametersStart = 0x2a,
            ConstructorParametersEnd = 0x2b,
            ConstructorParameterType = 0x2c,
            ConnectionId = 0x2d,
            ContentProperty = 0x2e,
            DefAttribute = 0x19,
            DefAttributeKeyString = 0x26,
            DefAttributeKeyType = 0x27,
            DeferableContentStart = 0x25,
            DefTag = 0x18,
            DocumentEnd = 0x2,
            DocumentStart = 0x1,
            ElementEnd = 0x4,
            ElementStart = 0x3,
            EndAttributes = 0x1a,
            KeyElementEnd = 0x29,
            KeyElementStart = 0x28,
            LastRecordType = 0x39,
            LineNumberAndPosition = 0x35,
            LinePosition = 0x36,
            LiteralContent = 0xf,
            NamedElementStart = 0x2f,
            OptimizedStaticResource = 0x37,
            PIMapping = 0x1b,
            PresentationOptionsAttribute = 0x34,
            ProcessingInstruction = 0x16,
            Property = 0x5,
            PropertyArrayEnd = 0xa,
            PropertyArrayStart = 0x9,
            PropertyComplexEnd = 0x8,
            PropertyComplexStart = 0x7,
            PropertyCustom = 0x6,
            PropertyDictionaryEnd = 0xe,
            PropertyDictionaryStart = 0xd,
            PropertyListEnd = 0xc,
            PropertyListStart = 0xb,
            PropertyStringReference = 0x21,
            PropertyTypeReference = 0x22,
            PropertyWithConverter = 0x24,
            PropertyWithExtension = 0x23,
            PropertyWithStaticResourceId = 0x38,
            RoutedEvent = 0x12,
            StaticResourceEnd = 0x31,
            StaticResourceId = 0x32,
            StaticResourceStart = 0x30,
            StringInfo = 0x20,
            Text = 0x10,
            TextWithConverter = 0x11,
            TextWithId = 0x33,
            TypeInfo = 0x1d,
            TypeSerializerInfo = 0x1e,
            Unknown = 0x0,
            XmlAttribute = 0x15,
            XmlnsProperty = 0x14
        }

        private enum BamlAttributeUsage : short
        {
            Default = 0x0,
            RuntimeName = 0x3,
            XmlLang = 0x1,
            XmlSpace = 0x2
        }

        private class BamlBinaryReader : BinaryReader
        {
            public BamlBinaryReader(Stream stream)
                : base(stream)
            {
            }

            public virtual double ReadCompressedDouble()
            {
                byte leadingByte = this.ReadByte();
                switch (leadingByte)
                {
                    case 0x01:
                        return 0;

                    case 0x02:
                        return 1;

                    case 0x03:
                        return -1;

                    case 0x04:
                        double value = this.ReadInt32();
                        return (value * 1E-06);

                    case 0x05:
                        return this.ReadDouble();
                }

                throw new NotSupportedException();
            }

            public int ReadCompressedInt32()
            {
                return base.Read7BitEncodedInt();
            }
        }

        private class Element
        {
            private TypeDeclaration typeDeclaration;
            private PropertyCollection properties = new PropertyCollection();
            private IList arguments = new ArrayList();

            public TypeDeclaration TypeDeclaration
            {
                get
                {
                    return this.typeDeclaration;
                }

                set
                {
                    this.typeDeclaration = value;
                }
            }

            public PropertyCollection Properties
            {
                get
                {
                    return this.properties;
                }
            }

            public IList Arguments
            {
                get
                {
                    return this.arguments;
                }
            }

            public override string ToString()
            {
                /*
                using (StringWriter stringWriter = new StringWriter())
                {
                    using (IndentationTextWriter indentationTextWriter = new IndentationTextWriter(stringWriter))
                    {
                        WriteElement(this, indentationTextWriter);
                    }

                    return stringWriter.ToString();
                }
                */

                return "<" + this.TypeDeclaration.ToString() + ">";
            }
        }

        internal class TypeDeclaration
        {
            private string name;
            private string namespaceName;
            private string assembly;
            private string xmlPrefix;

            public TypeDeclaration(string name)
            {
                this.name = name;
                this.namespaceName = string.Empty;
                this.assembly = string.Empty;
            }

            public TypeDeclaration(string name, string namespaceName, string assembly)
            {
                this.name = name;
                this.namespaceName = namespaceName;
                this.assembly = assembly;
            }

            public TypeDeclaration Copy(string xmlPrefix)
            {
                TypeDeclaration copy = new TypeDeclaration(this.name, this.namespaceName, this.assembly);
                copy.xmlPrefix = xmlPrefix;
                return copy;
            }

            public string AssemblyQualifiedName
            {
                get
                {
                    var sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(Namespace))
                    {
                        sb.Append(Namespace).Append(".");
                    }

                    sb.Append(Name);

                    if (!string.IsNullOrEmpty(Assembly))
                    {
                        sb.Append(", ").Append(Assembly);
                    }
                    return sb.ToString();
                }
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            public string Namespace
            {
                get
                {
                    return this.namespaceName;
                }
            }

            public string Assembly
            {
                get
                {
                    return this.assembly;
                }
            }

            public string XmlPrefix
            {
                get { return this.xmlPrefix; }
            }

            public override string ToString()
            {
                if (null == this.xmlPrefix || 0 == this.xmlPrefix.Length)
                    return this.Name;

                return this.xmlPrefix + ":" + this.Name;
            }
        }

        internal class TypeDeclarationComparer : IEqualityComparer<TypeDeclaration>
        {
            /// <summary>
            /// Equals type definitions.
            /// </summary>
            /// <param name="ref1">TD1.</param>
            /// <param name="ref2">TD2.</param>
            /// <returns>Result.</returns>
            public bool Equals(TypeDeclaration ref1, TypeDeclaration ref2)
            {
                if (ReferenceEquals(ref1, ref2))
                {
                    return true;
                }
                if ((ref1 != null && ref2 == null) || (ref1 == null && ref2 != null))
                {
                    return false;
                }

                var result = (string.Compare(ref1.Assembly, ref2.Assembly, StringComparison.OrdinalIgnoreCase) == 0)
                    && (string.Compare(ref1.Name, ref2.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    && (string.Compare(ref1.Namespace, ref2.Namespace, StringComparison.OrdinalIgnoreCase) == 0)
                    && (string.Compare(ref1.XmlPrefix, ref2.XmlPrefix, StringComparison.OrdinalIgnoreCase) == 0);

                return result;
            }

            /// <summary>
            /// Gets hash code.
            /// </summary>
            /// <param name="a">Type definition.</param>
            /// <returns>Hash code.</returns>
            public int GetHashCode(TypeDeclaration a)
            {
                // Check whether the object is null.
                if (Object.ReferenceEquals(a, null)) return 0;

                int hashAssembly = string.IsNullOrEmpty(a.Assembly) ? 0 : a.Assembly.GetHashCode();
                int hashName = string.IsNullOrEmpty(a.Name) ? 0 : a.Name.GetHashCode();
                int hashNamespace = string.IsNullOrEmpty(a.Namespace) ? 0 : a.Namespace.GetHashCode();
                int hashXmlPrefix = string.IsNullOrEmpty(a.XmlPrefix) ? 0 : a.XmlPrefix.GetHashCode();

                return hashAssembly ^ hashName ^ hashNamespace ^ hashXmlPrefix;
            }
        }

        private enum PropertyType
        {
            Value,
            Content,
            Declaration,
            List,
            Dictionary,
            Complex,
            Namespace
        }

        private class Property
        {
            private PropertyType propertyType;
            private PropertyDeclaration propertyDeclaration;
            private object value;

            public Property(PropertyType propertyType)
            {
                this.propertyType = propertyType;
            }

            public PropertyType PropertyType
            {
                get
                {
                    return this.propertyType;
                }
            }

            public PropertyDeclaration PropertyDeclaration
            {
                get
                {
                    return this.propertyDeclaration;
                }

                set
                {
                    this.propertyDeclaration = value;
                }
            }

            public object Value
            {
                get
                {
                    return this.value;
                }

                set
                {
                    this.value = value;
                }
            }

            public override string ToString()
            {
                /*
                using (StringWriter stringWriter = new StringWriter())
                {
                    using (IndentationTextWriter indentationTextWriter = new IndentationTextWriter(stringWriter))
                    {
                        indentationTextWriter.Write(this.PropertyDeclaration.Name);
                        indentationTextWriter.Write("=");
                        indentationTextWriter.WriteLine();
                        WritePropertyValue(this, indentationTextWriter);
                    }

                    return stringWriter.ToString();
                }
                */

                return this.PropertyDeclaration.Name;
            }
        }

        private class PropertyDeclaration
        {
            private string name;
            private TypeDeclaration declaringType;

            public PropertyDeclaration(string name)
            {
                this.name = name;
                this.declaringType = null;
            }

            public PropertyDeclaration(string name, TypeDeclaration declaringType)
            {
                this.name = name;
                this.declaringType = declaringType;
            }

            public TypeDeclaration DeclaringType
            {
                get
                {
                    return this.declaringType;
                }
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            public override string ToString()
            {
                if ((this.DeclaringType != null) && (this.DeclaringType.Name == "XmlNamespace") && (this.DeclaringType.Namespace == null) && (this.DeclaringType.Assembly == null))
                {
                    if ((this.Name == null) || (this.Name.Length == 0))
                    {
                        return "xmlns";
                    }

                    return "xmlns:" + this.Name;
                }

                return this.Name;
            }
        }

        private class PropertyCollection : IEnumerable
        {
            private ArrayList list = new ArrayList();

            public void Add(Property value)
            {
                this.list.Add(value);
            }

            public void Remove(Property value)
            {
                this.list.Remove(value);
            }

            public int Count
            {
                get
                {
                    return this.list.Count;
                }
            }

            public IEnumerator GetEnumerator()
            {
                return this.list.GetEnumerator();
            }

            public Property this[int index]
            {
                get
                {
                    return (Property)this.list[index];
                }
            }
        }

        private class ResourceName
        {
            private string name;

            public ResourceName(string name)
            {
                this.name = name;
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            public override string ToString()
            {
                return this.Name;
            }
        }

        private class NamespaceManager
        {
            private HybridDictionary table = new HybridDictionary();
            private HybridDictionary reverseTable = new HybridDictionary();
            private Stack mappingStack = new Stack();

            internal NamespaceManager()
            {
            }

            public void AddNamespaceMapping(string xmlNamespace, string clrNamespace, string assembly)
            {
                ClrNamespace ns = new ClrNamespace(clrNamespace, assembly);
                table[xmlNamespace] = ns;
                reverseTable[ns] = xmlNamespace;
            }

            internal string GetXmlNamespace(TypeDeclaration type)
            {
                ClrNamespace ns = new ClrNamespace(type.Namespace, type.Assembly);
                return (string)reverseTable[ns];
            }

            internal void OnElementStart()
            {
                ElementEntry entry = new ElementEntry();
                this.mappingStack.Push(entry);
            }

            internal void OnElementEnd()
            {
                this.mappingStack.Pop();
            }

            internal void AddMapping(string prefix, string xmlNamespace)
            {
                ElementEntry element = (ElementEntry)this.mappingStack.Peek();
                element.MappingTable[xmlNamespace] = prefix;
            }

            internal string GetPrefix(string xmlNamespace)
            {
                foreach (ElementEntry element in this.mappingStack)
                {
                    if (element.HasMappingTable)
                    {
                        if (element.MappingTable.Contains(xmlNamespace))
                            return (string)element.MappingTable[xmlNamespace];
                    }
                }

                return null;
            }

            private class ElementEntry
            {
                private HybridDictionary mappingTable;

                internal bool HasMappingTable
                {
                    get { return null != this.mappingTable; }
                }

                internal HybridDictionary MappingTable
                {
                    get
                    {
                        if (null == this.mappingTable)
                            this.mappingTable = new HybridDictionary();

                        return this.mappingTable;
                    }
                }
            }
        }

        internal struct ClrNamespace
        {
            public string Namespace;
            public string Assembly;

            public ClrNamespace(string clrNamespace, string assembly)
            {
                this.Namespace = clrNamespace;
                this.Assembly = assembly;
            }
        }

        internal class KnownColors
        {
            private static readonly Hashtable colorTable;

            static KnownColors()
            {
                colorTable = new Hashtable();
                colorTable[0xFFF0F8FF] = "AliceBlue";
                colorTable[0xFFFAEBD7] = "AntiqueWhite";
                colorTable[0xFF00FFFF] = "Aqua";
                colorTable[0xFF7FFFD4] = "Aquamarine";
                colorTable[0xFFF0FFFF] = "Azure";
                colorTable[0xFFF5F5DC] = "Beige";
                colorTable[0xFFFFE4C4] = "Bisque";
                colorTable[0xFF000000] = "Black";
                colorTable[0xFFFFEBCD] = "BlanchedAlmond";
                colorTable[0xFF0000FF] = "Blue";
                colorTable[0xFF8A2BE2] = "BlueViolet";
                colorTable[0xFFA52A2A] = "Brown";
                colorTable[0xFFDEB887] = "BurlyWood";
                colorTable[0xFF5F9EA0] = "CadetBlue";
                colorTable[0xFF7FFF00] = "Chartreuse";
                colorTable[0xFFD2691E] = "Chocolate";
                colorTable[0xFFFF7F50] = "Coral";
                colorTable[0xFF6495ED] = "CornflowerBlue";
                colorTable[0xFFFFF8DC] = "Cornsilk";
                colorTable[0xFFDC143C] = "Crimson";
                colorTable[0xFF00FFFF] = "Cyan";
                colorTable[0xFF00008B] = "DarkBlue";
                colorTable[0xFF008B8B] = "DarkCyan";
                colorTable[0xFFB8860B] = "DarkGoldenrod";
                colorTable[0xFFA9A9A9] = "DarkGray";
                colorTable[0xFF006400] = "DarkGreen";
                colorTable[0xFFBDB76B] = "DarkKhaki";
                colorTable[0xFF8B008B] = "DarkMagenta";
                colorTable[0xFF556B2F] = "DarkOliveGreen";
                colorTable[0xFFFF8C00] = "DarkOrange";
                colorTable[0xFF9932CC] = "DarkOrchid";
                colorTable[0xFF8B0000] = "DarkRed";
                colorTable[0xFFE9967A] = "DarkSalmon";
                colorTable[0xFF8FBC8F] = "DarkSeaGreen";
                colorTable[0xFF483D8B] = "DarkSlateBlue";
                colorTable[0xFF2F4F4F] = "DarkSlateGray";
                colorTable[0xFF00CED1] = "DarkTurquoise";
                colorTable[0xFF9400D3] = "DarkViolet";
                colorTable[0xFFFF1493] = "DeepPink";
                colorTable[0xFF00BFFF] = "DeepSkyBlue";
                colorTable[0xFF696969] = "DimGray";
                colorTable[0xFF1E90FF] = "DodgerBlue";
                colorTable[0xFFB22222] = "Firebrick";
                colorTable[0xFFFFFAF0] = "FloralWhite";
                colorTable[0xFF228B22] = "ForestGreen";
                colorTable[0xFFFF00FF] = "Fuchsia";
                colorTable[0xFFDCDCDC] = "Gainsboro";
                colorTable[0xFFF8F8FF] = "GhostWhite";
                colorTable[0xFFFFD700] = "Gold";
                colorTable[0xFFDAA520] = "Goldenrod";
                colorTable[0xFF808080] = "Gray";
                colorTable[0xFF008000] = "Green";
                colorTable[0xFFADFF2F] = "GreenYellow";
                colorTable[0xFFF0FFF0] = "Honeydew";
                colorTable[0xFFFF69B4] = "HotPink";
                colorTable[0xFFCD5C5C] = "IndianRed";
                colorTable[0xFF4B0082] = "Indigo";
                colorTable[0xFFFFFFF0] = "Ivory";
                colorTable[0xFFF0E68C] = "Khaki";
                colorTable[0xFFE6E6FA] = "Lavender";
                colorTable[0xFFFFF0F5] = "LavenderBlush";
                colorTable[0xFF7CFC00] = "LawnGreen";
                colorTable[0xFFFFFACD] = "LemonChiffon";
                colorTable[0xFFADD8E6] = "LightBlue";
                colorTable[0xFFF08080] = "LightCoral";
                colorTable[0xFFE0FFFF] = "LightCyan";
                colorTable[0xFFFAFAD2] = "LightGoldenrodYellow";
                colorTable[0xFFD3D3D3] = "LightGray";
                colorTable[0xFF90EE90] = "LightGreen";
                colorTable[0xFFFFB6C1] = "LightPink";
                colorTable[0xFFFFA07A] = "LightSalmon";
                colorTable[0xFF20B2AA] = "LightSeaGreen";
                colorTable[0xFF87CEFA] = "LightSkyBlue";
                colorTable[0xFF778899] = "LightSlateGray";
                colorTable[0xFFB0C4DE] = "LightSteelBlue";
                colorTable[0xFFFFFFE0] = "LightYellow";
                colorTable[0xFF00FF00] = "Lime";
                colorTable[0xFF32CD32] = "LimeGreen";
                colorTable[0xFFFAF0E6] = "Linen";
                colorTable[0xFFFF00FF] = "Magenta";
                colorTable[0xFF800000] = "Maroon";
                colorTable[0xFF66CDAA] = "MediumAquamarine";
                colorTable[0xFF0000CD] = "MediumBlue";
                colorTable[0xFFBA55D3] = "MediumOrchid";
                colorTable[0xFF9370DB] = "MediumPurple";
                colorTable[0xFF3CB371] = "MediumSeaGreen";
                colorTable[0xFF7B68EE] = "MediumSlateBlue";
                colorTable[0xFF00FA9A] = "MediumSpringGreen";
                colorTable[0xFF48D1CC] = "MediumTurquoise";
                colorTable[0xFFC71585] = "MediumVioletRed";
                colorTable[0xFF191970] = "MidnightBlue";
                colorTable[0xFFF5FFFA] = "MintCream";
                colorTable[0xFFFFE4E1] = "MistyRose";
                colorTable[0xFFFFE4B5] = "Moccasin";
                colorTable[0xFFFFDEAD] = "NavajoWhite";
                colorTable[0xFF000080] = "Navy";
                colorTable[0xFFFDF5E6] = "OldLace";
                colorTable[0xFF808000] = "Olive";
                colorTable[0xFF6B8E23] = "OliveDrab";
                colorTable[0xFFFFA500] = "Orange";
                colorTable[0xFFFF4500] = "OrangeRed";
                colorTable[0xFFDA70D6] = "Orchid";
                colorTable[0xFFEEE8AA] = "PaleGoldenrod";
                colorTable[0xFF98FB98] = "PaleGreen";
                colorTable[0xFFAFEEEE] = "PaleTurquoise";
                colorTable[0xFFDB7093] = "PaleVioletRed";
                colorTable[0xFFFFEFD5] = "PapayaWhip";
                colorTable[0xFFFFDAB9] = "PeachPuff";
                colorTable[0xFFCD853F] = "Peru";
                colorTable[0xFFFFC0CB] = "Pink";
                colorTable[0xFFDDA0DD] = "Plum";
                colorTable[0xFFB0E0E6] = "PowderBlue";
                colorTable[0xFF800080] = "Purple";
                colorTable[0xFFFF0000] = "Red";
                colorTable[0xFFBC8F8F] = "RosyBrown";
                colorTable[0xFF4169E1] = "RoyalBlue";
                colorTable[0xFF8B4513] = "SaddleBrown";
                colorTable[0xFFFA8072] = "Salmon";
                colorTable[0xFFF4A460] = "SandyBrown";
                colorTable[0xFF2E8B57] = "SeaGreen";
                colorTable[0xFFFFF5EE] = "SeaShell";
                colorTable[0xFFA0522D] = "Sienna";
                colorTable[0xFFC0C0C0] = "Silver";
                colorTable[0xFF87CEEB] = "SkyBlue";
                colorTable[0xFF6A5ACD] = "SlateBlue";
                colorTable[0xFF708090] = "SlateGray";
                colorTable[0xFFFFFAFA] = "Snow";
                colorTable[0xFF00FF7F] = "SpringGreen";
                colorTable[0xFF4682B4] = "SteelBlue";
                colorTable[0xFFD2B48C] = "Tan";
                colorTable[0xFF008080] = "Teal";
                colorTable[0xFFD8BFD8] = "Thistle";
                colorTable[0xFFFF6347] = "Tomato";
                colorTable[0x00FFFFFF] = "Transparent";
                colorTable[0xFF40E0D0] = "Turquoise";
                colorTable[0xFFEE82EE] = "Violet";
                colorTable[0xFFF5DEB3] = "Wheat";
                colorTable[0xFFFFFFFF] = "White";
                colorTable[0xFFF5F5F5] = "WhiteSmoke";
                colorTable[0xFFFFFF00] = "Yellow";
                colorTable[0xFF9ACD32] = "YellowGreen";
            }

            internal static string KnownColorFromUInt(UInt32 argb)
            {
                Debug.Assert(colorTable.Contains(argb));
                return (string)colorTable[argb];
            }
        }

        private class PathDataParser
        {
            internal static object ParseStreamGeometry(BamlBinaryReader reader)
            {
                StringBuilder sb = new StringBuilder();
                bool shouldClose = false;
                char lastChar = '\0';

                while (true)
                {
                    byte b = reader.ReadByte();
                    bool bit1 = (b & 0x10) == 0x10;
                    bool bit2 = (b & 0x20) == 0x20;
                    bool bit3 = (b & 0x40) == 0x40;
                    bool bit4 = (b & 0x80) == 0x80;

                    switch (b & 0xF)
                    {
                        case 0x0: //Begin
                            {
                                shouldClose = bit2;

                                AddPathCommand('M', ref lastChar, sb);
                                AddPathPoint(reader, sb, bit3, bit4);
                                break;
                            }
                        case 0x1: //LineTo
                            {
                                AddPathCommand('L', ref lastChar, sb);
                                AddPathPoint(reader, sb, bit3, bit4);
                                break;
                            }
                        case 0x2: //QuadraticBezierTo
                            {
                                AddPathCommand('Q', ref lastChar, sb);
                                AddPathPoint(reader, sb, bit3, bit4);
                                AddPathPoint(reader, sb);
                                break;
                            }
                        case 0x3: //BezierTo
                            {
                                AddPathCommand('C', ref lastChar, sb);
                                AddPathPoint(reader, sb, bit3, bit4);
                                AddPathPoint(reader, sb);
                                AddPathPoint(reader, sb);
                                break;
                            }
                        case 0x4: //PolyLineTo
                            {
                                bool isStroked = bit1;
                                bool isSmooth = bit2;
                                AddPathCommand('L', ref lastChar, sb);
                                int count = reader.ReadInt32();

                                for (int i = 0; i < count; i++)
                                    AddPathPoint(reader, sb);
                                break;
                            }
                        case 0x5: //PolyQuadraticBezierTo
                            {
                                AddPathCommand('Q', ref lastChar, sb);
                                int count = reader.ReadInt32();
                                System.Diagnostics.Debug.Assert(count % 2 == 0);
                                for (int i = 0; i < count; i++)
                                    AddPathPoint(reader, sb);
                                break;
                            }
                        case 0x6: //PolyBezierTo
                            {
                                AddPathCommand('C', ref lastChar, sb);
                                int count = reader.ReadInt32();
                                System.Diagnostics.Debug.Assert(count % 3 == 0);
                                for (int i = 0; i < count; i++)
                                    AddPathPoint(reader, sb);
                                break;
                            }
                        case 0x7: //ArcTo
                            {
                                double endPtX = ReadPathDouble(reader, bit3);
                                double endPtY = ReadPathDouble(reader, bit4);
                                byte arcInfo = reader.ReadByte();
                                bool isLarge = (arcInfo & 0xF) != 0;
                                bool clockWise = (arcInfo & 0xF0) != 0;
                                double sizeX = reader.ReadCompressedDouble();
                                double sizeY = reader.ReadCompressedDouble();
                                double angle = reader.ReadCompressedDouble();
                                sb.AppendFormat("A {0},{1} {2} {3} {4} {5},{6} ", sizeX, sizeY, angle, isLarge ? 1 : 0, clockWise ? 1 : 0, endPtX, endPtY);
                                lastChar = 'A';
                                break;
                            }
                        case 0x8: //Closed
                            {
                                if (shouldClose)
                                {
                                    sb.Append("Z");
                                }
                                else if (sb.Length > 0)
                                {
                                    // trim off the ending space
                                    sb.Remove(sb.Length - 1, 0);
                                }

                                return sb.ToString();
                            }
                        case 0x9: //FillRule
                            {
                                sb.Insert(0, bit1 ? "F1 " : "F0 ");
                                lastChar = 'F';
                                break;
                            }
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            private static void AddPathCommand(char commandChar, ref char lastCommandChar, StringBuilder sb)
            {
                if (commandChar != lastCommandChar)
                {
                    lastCommandChar = commandChar;
                    sb.Append(commandChar);
                    sb.Append(' ');
                }
            }

            private static void AddPathPoint(BamlBinaryReader reader, StringBuilder sb, bool flag1, bool flag2)
            {
                sb.AppendFormat("{0},{1} ", ReadPathDouble(reader, flag1), ReadPathDouble(reader, flag2));
            }

            private static void AddPathPoint(BamlBinaryReader reader, StringBuilder sb)
            {
                sb.AppendFormat("{0},{1} ", reader.ReadCompressedDouble(), reader.ReadCompressedDouble());
            }

            private static double ReadPathDouble(BamlBinaryReader reader, bool isInt)
            {
                if (isInt)
                    return reader.ReadInt32() * 1E-06;

                return reader.ReadCompressedDouble();
            }
        }

        #endregion // Nested types
    }
}
