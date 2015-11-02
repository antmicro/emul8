//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Emul8.Bootstrap
{
    public class CustomProject : Project
    {
        public CustomProject(string name, string path, string startupObject, IEnumerable<Project> references) : base(name, path)
        {
            GUID = Guid.NewGuid();
            StartupObject = startupObject;
            References = references;
            Target = "Any CPU";
        }

        public void GenerateMainClass(string path)
        {
            var stream = typeof(CustomProject).Assembly.GetManifestResourceStream("Emul8.Bootstrap.Projects.MainClassTemplate.cstemplate");
            using(var streamReader = new StreamReader(stream))
            {
                File.WriteAllText(path, string.Format(
                    streamReader.ReadToEnd(),
                    GeneratedProjectMainClassName,
                    StartupObject,
                    typeof(CustomProject).Assembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company,
                    typeof(CustomProject).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright));
            }
        }

        public void Save(string path)
        {
            var xnamespace = (XNamespace)"http://schemas.microsoft.com/developer/msbuild/2003";
            var referencedProjectsNode = new XElement(xnamespace + "ItemGroup");
            var forcedOutputNode = new XElement(xnamespace + "ItemGroup");
            foreach(var reference in References)
            {
                referencedProjectsNode.Add(
                    new XElement(xnamespace + "ProjectReference",
                        new XAttribute("Include", reference.Path),
                        new XElement(xnamespace + "Project", string.Format("{{{0}}}", reference.GUID.ToString().ToUpper())),
                        new XElement(xnamespace + "Name", reference.Name)));

                if (reference.HasForcedOutput)
                {
                    forcedOutputNode.Add(
                        new XElement(xnamespace + "ForcedOutput", new XAttribute("Include", reference.Path)));
                }
            }

            var xml = new XDocument(
              new XElement(xnamespace + "Project",
                  new XAttribute("DefaultTargets", "Build"),
                  new XAttribute("ToolsVersion", "4.0"),
                  new XElement(xnamespace + "PropertyGroup",
                      new XElement(xnamespace + "Configuration",
                          new XAttribute("Condition", " '$(Configuration)' == '' "),
                          "Debug"),
                        new XElement(xnamespace + "Platform",
                            new XAttribute("Condition", " '$(Platform)' == '' "),
                                "AnyCPU"),
                        new XElement(xnamespace + "ProjectGuid", string.Format("{{{0}}}", GUID.ToString().ToUpper())),
                        new XElement(xnamespace + "OutputType", "Exe"),
                        new XElement(xnamespace + "ProjectInfo", new XAttribute("Skip", "true")),
                        new XElement(xnamespace + "AssemblyName", Name),
                        new XElement(xnamespace + "TargetFrameworkVersion", "v4.5"),
                        new XElement(xnamespace + "StartupObject", string.Format("Emul8.{0}", GeneratedProjectMainClassName))),
                   new XElement(xnamespace + "PropertyGroup",
                        new XAttribute("Condition", " '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "),
                        new XElement(xnamespace + "DebugSymbols", "true"),
                        new XElement(xnamespace + "DebugType", "full"),
                        new XElement(xnamespace + "Optimize", "false"),
                        new XElement(xnamespace + "OutputPath", @"Debug"),
                        new XElement(xnamespace + "DefineConstants", "DEBUG;"),
                        new XElement(xnamespace + "ErrorReport", "prompt"),
                        new XElement(xnamespace + "WarningLevel", "4"),
                        new XElement(xnamespace + "Externalconsole", "true")),
                    new XElement(xnamespace + "PropertyGroup",
                        new XAttribute("Condition", " '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "),
                        new XElement(xnamespace + "Optimize", "true"),
                        new XElement(xnamespace + "OutputPath", @"Release"),
                        new XElement(xnamespace + "ErrorReport", "prompt"),
                        new XElement(xnamespace + "Externalconsole", "true")),
                    new XElement(xnamespace + "ItemGroup",
                        new XElement(xnamespace + "Reference", new XAttribute("Include", "System"))),
                    new XElement(xnamespace + "ItemGroup",
                        new XElement(xnamespace + "Compile", new XAttribute("Include", string.Format("{0}.cs", GeneratedProjectMainClassName)))),
                    new XElement(xnamespace + "Import", new XAttribute("Project", @"$(MSBuildBinPath)\Microsoft.CSharp.targets")),
                    referencedProjectsNode,
                    forcedOutputNode,
                    new XElement(xnamespace + "Target", new XAttribute("AfterTargets", "ResolveAssemblyReferences"),
                        new XElement(xnamespace + "MSBuild",
                            new XAttribute("Projects", "@(ForcedOutput)"),
                            new XAttribute("Targets", "GetForcedOutput"),
                            new XElement(xnamespace + "Output",
                                new XAttribute("TaskParameter", "TargetOutputs"),
                                new XAttribute("ItemName", "ReferenceCopyLocalPaths"))))
                ));

            xml.Save(path);
        }
    }
}

