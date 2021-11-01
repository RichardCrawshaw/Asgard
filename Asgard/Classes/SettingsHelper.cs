using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Asgard
{
    internal class SettingsHelper
    {
        #region Fields

        private readonly Settings settings = new();

        #endregion

        #region Properties

        public ISettings Settings => this.settings;

        #endregion

        #region Constructors

        public SettingsHelper() { }

        #endregion

        #region Methods

        public bool Load(string source, SettingsOptionsEnum options)
        {
            switch (options)
            {
                case SettingsOptionsEnum.None:
                    throw new InvalidOperationException("A source must be specified.");

                case SettingsOptionsEnum.File:
                    var file = new FileInfo(source);
                    return Load(file);
                case SettingsOptionsEnum.Text:
                    return Load(source);

                default:
                    throw new InvalidOperationException("Unrecognised source option.");
            }
        }

        #endregion

        #region Support routines

        private List<Type> GetSettingsNodeTypes()
        {
            var assembly = Assembly.GetAssembly(GetType());
            var results =
                assembly.GetTypes()
                    .Where(t => t.BaseType == typeof(Settings.SettingsNode<>))
                    .ToList();
            return results;
        }

        private static List<(PropertyInfo Property, SettingsNodePropertyAttribute Attribute)> GetSettingsNodePropertyAttributes(Type type)
        {
            var results=
                type.GetProperties()
                    .Select(p => (Property: p, Attribute: p.GetCustomAttribute<SettingsNodePropertyAttribute>(true)))
                    .Where(n => n.Attribute is not null)
                    .ToList();
            return results;
        }

        private static string GetNodeName(Type type)
        {
            var property = type.GetProperty("NodeName", BindingFlags.Public | BindingFlags.Static);
            if (property is null) return null;
            var nodeName = (string)property.GetValue(type);
            return nodeName;
        }

        private bool Load(FileInfo file)
        {
            if (!Read(file, out var text)) return false;
            return Load(text);
        }

        private bool Load(string text)
        {
            var xDoc = XDocument.Parse(text);
            var doc = new XPathDocument(xDoc.CreateReader());

            var rootName = Asgard.Settings.NodeName;
            var settingsNodeTypes = GetSettingsNodeTypes();
            foreach(var type in settingsNodeTypes)
            {
                var noteName = GetNodeName(type);
                var items = GetSettingsNodePropertyAttributes(type);

                var constructor = type.GetConstructor(Array.Empty<Type>());
                if (constructor is null) continue;

                var settingNode = (ISettings.ISettingsNode)
                    constructor.Invoke(Array.Empty<object>());

                foreach(var item in items)
                {
                    var path = $"/{rootName}/{noteName}/{item.Attribute.Path}";
                    var xml = doc.CreateNavigator();
                    var i = xml.Select(path);
                    if (i.MoveNext())
                    {
                        item.Property.SetValue(settingNode, i.Current.ValueAs(item.Property.PropertyType));
                    }
                }

                this.settings.Add(settingNode);
            }

            return true;
        }

        private static bool Read(FileInfo file, out string text)
        {
            using var reader = file.OpenText();
            text = reader.ReadToEnd();

            return true;
        }

        #endregion
    }
}
