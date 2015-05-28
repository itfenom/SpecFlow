﻿using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using TechTalk.SpecFlow.Specs.Drivers.Templates;

namespace TechTalk.SpecFlow.Specs.Drivers
{
    public class SpecFlowConfigurationDriver
    {
        private const string DefaultProviderName = "NUnit";
        private readonly XDocument parsedConfiguration;

        public string UnitTestProviderName { get; private set; }

        public XElement SpecFlowConfigurationElement
        {
            get { return parsedConfiguration.XPathSelectElement("/configuration/specFlow"); }
        }

        public SpecFlowConfigurationDriver(TemplateManager templateManager)
        {
            UnitTestProviderName = DefaultProviderName;
            parsedConfiguration = templateManager.LoadXmlTemplate("App.config");
        }

        private XElement EnsureGeneratorElement()
        {
            var generatorElement = parsedConfiguration.XPathSelectElement("/configuration/specFlow/generator");
            if (generatorElement == null)
            {
                generatorElement = new XElement("generator");
                SpecFlowConfigurationElement.Add(generatorElement);
            }
            return generatorElement;
        }

        public void SetSpecFlowConfigurationContent(string specFlowConfigurationContent)
        {
            SpecFlowConfigurationElement.ReplaceWith(XElement.Parse(specFlowConfigurationContent));
            var unitTestProviderElement = SpecFlowConfigurationElement.XPathSelectElement("unitTestProvider");
            UnitTestProviderName = unitTestProviderElement == null
                                       ? DefaultProviderName
                                       : (unitTestProviderElement.Attribute("name") ?? new XAttribute("name", DefaultProviderName)).Value;
        }

        public void SetUnitTestProvider(string name)
        {
            UnitTestProviderName = name;
            if (name == "NUnit3")
                name = "NUnit";
            SpecFlowConfigurationElement.Add(new XElement("unitTestProvider", new XAttribute("name", name)));
        }

        public void AddRuntimeDependencyCustomization(string typeName, string interfaceName)
        {
            SpecFlowConfigurationElement.Add(
                new XElement("runtime",
                    new XElement("dependencies",
                        new XElement("register", new XAttribute("type", typeName), new XAttribute("as", interfaceName)))
                    ));
/*
                    <runtime>  
                    <dependencies>
                      <register type=""{0}"" as=""{1}"" name=""myprovider""/>
                    </dependencies>
                  </runtime>

 */ 
        }

        public void SaveConfigurationTo(string path)
        {
            parsedConfiguration.Save(path);
        }

        public IEnumerable<string> GetAdditionalReferences()
        {
            switch (UnitTestProviderName.ToLower())
            {
                case "nunit":
                    yield return @"NUnit\lib\nunit.framework.dll";
                    break;
                case "nunit3":
                    yield return @"NUnit3-Runner\bin\nunit.framework.dll";
                    break;
                case "mbunit.3":
                    yield return @"mbUnit3\mbUnit.dll";
                    yield return @"mbUnit3\gallio.dll";
                    break;
                case "xunit":
                    yield return @"xUnit\lib\xUnit.dll";
                    yield return @"xUnit.extensions\lib\xunit.extensions.dll";
                    break;
            }
        }

        public void SetRowTest(bool enabled)
        {
            var generatorElement = EnsureGeneratorElement();
            generatorElement.SetAttributeValue("allowRowTests", enabled.ToString(CultureInfo.InvariantCulture).ToLower());
        }
    }
}
