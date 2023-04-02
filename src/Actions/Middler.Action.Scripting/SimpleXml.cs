using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using doob.Reflectensions.ExtensionMethods;

namespace doob.Middler.Action.Scripting
{

    public class SimpleXml
    {
        internal XDocument _document;
        internal XmlNamespaceManager _namespaceManager = new XmlNamespaceManager(new NameTable());

        public SimpleXml()
        {
            _document = new XDocument();
        }

        public SimpleXml(string xml)
        {
            _document = XDocument.Parse(xml);
        }

        public SimpleXml(Stream xml)
        {
            _document = XDocument.Load(xml);
        }

        public SimpleXml(SimpleXmlElement element)
        {
            _document = XDocument.Parse(element.ToString());
        }

        public SimpleXml(XDocument element)
        {
            _document = element;
        }


        public SimpleXmlElement[] SelectElements(string xpath)
        {

            var elements = _document.XPathSelectElements(xpath, _namespaceManager);
            var list = new List<SimpleXmlElement>();
            foreach (var xElement in elements)
            {
                list.Add(new SimpleXmlElement(xElement).AddNameSpaceManager(_namespaceManager));
            }
            return list.ToArray();
        }

        public SimpleXmlElement[] SelectElements()
        {

            return _document.Elements().Select(xElement =>
                new SimpleXmlElement(xElement).AddNameSpaceManager(_namespaceManager)).ToArray();

        }

        public SimpleXmlElement SelectElement(string xpath)
        {
            var el = _document.XPathSelectElement(xpath, _namespaceManager);
            if (el == null)
                return null;

            return new SimpleXmlElement(el).AddNameSpaceManager(_namespaceManager);
        }

        public string GetAttribute(string name)
        {
            return new SimpleXmlElement(_document.Root).GetAttribute(name);
        }

        public Dictionary<string, string> GetAttributes()
        {
            return new SimpleXmlElement(_document.Root).GetAttributes();
        }

        public SimpleXml RemoveComments()
        {
            _document.DescendantNodes().OfType<XComment>().Remove();
            return this;
        }

        public override string ToString()
        {
            return _document.ToString();
        }


        public SimpleXml AddNameSpace(string prefix, string uri)
        {
            _namespaceManager.AddNamespace(prefix, uri);
            return this;
        }

        public SimpleXml AddNameSpaceManager(XmlNamespaceManager namespaceManager)
        {
            this._namespaceManager = namespaceManager;
            return this;
        }


        public static SimpleXml Parse(string value)
        {
            return new SimpleXml(value);
        }

        public static bool TryParse(string value, out SimpleXml simpleXml)
        {
            try
            {
                simpleXml = Parse(value);
                return true;
            }
            catch
            {
                simpleXml = null;
                return false;
            }
        }

        public static SimpleXml Parse(SimpleXmlElement element)
        {
            return new SimpleXml(element);
        }

        public static implicit operator XDocument(SimpleXml simpleXml)
        {
            return simpleXml._document;
        }
    }

    public class SimpleXmlElement
    {


        internal XElement _element;
        internal XmlNamespaceManager _namespaceManager = new XmlNamespaceManager(new NameTable());

        public string Name => _element.Name.LocalName;

        public SimpleXmlElement(string xml)
        {
            _element = XElement.Parse(xml);
        }
        public SimpleXmlElement(XElement element)
        {
            _element = element;
        }

        public string GetValue()
        {
            return _element?.Value;
        }

        public bool? GetValueAsBoolean()
        {
            var el = _element?.Value;
            if (el == null)
                return null;

            return el.Reflect().To<bool>();
        }

        public SimpleXmlElement[] SelectElements()
        {

            return _element.Elements().Select(el => new SimpleXmlElement(el).AddNameSpaceManager(_namespaceManager)).ToArray();

        }

        public SimpleXmlElement[] SelectElements(string xpath)
        {
            var elements = _element.XPathSelectElements(xpath, _namespaceManager);
            var list = new List<SimpleXmlElement>();
            foreach (var xElement in elements)
            {
                list.Add(new SimpleXmlElement(xElement).AddNameSpaceManager(_namespaceManager));
            }
            return list.ToArray();
        }

        public SimpleXmlElement SelectElement(string xpath)
        {
            var el = _element.XPathSelectElement(xpath, _namespaceManager);
            if (el == null)
                return null;

            return new SimpleXmlElement(el).AddNameSpaceManager(_namespaceManager);
        }

        public string GetAttribute(string name)
        {
            var attr = _element.Attribute(name);
            return attr?.Value;
        }

        public Dictionary<string, string> GetAttributes()
        {

            return _element.Attributes()
                .ToDictionary(at => at.Name.LocalName, at => at.Value);
        }

        public SimpleXmlElement RemoveComments()
        {
            _element.DescendantNodes().OfType<XComment>().Remove();
            return this;
        }

        public SimpleXml ToSimpleXml()
        {
            return new SimpleXml(this).AddNameSpaceManager(_namespaceManager);
        }

        public override string ToString()
        {
            return _element?.ToString();
        }


        public SimpleXmlElement AddNameSpace(string prefix, string uri)
        {
            _namespaceManager.AddNamespace(prefix, uri);
            return this;
        }

        public SimpleXmlElement AddNameSpaceManager(XmlNamespaceManager namespaceManager)
        {
            this._namespaceManager = namespaceManager;
            return this;
        }


        public static SimpleXmlElement Parse(string value)
        {
            return new SimpleXmlElement(value);
        }


        public static implicit operator XDocument(SimpleXmlElement simpleXmlElement)
        {
            return simpleXmlElement.ToSimpleXml()._document;
        }
    }
}
