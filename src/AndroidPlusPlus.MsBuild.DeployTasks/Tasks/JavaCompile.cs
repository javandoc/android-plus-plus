﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Reflection;
using System.Resources;

using Microsoft.Build.Framework;
using Microsoft.Win32;
using Microsoft.Build.Utilities;

using AndroidPlusPlus.MsBuild.Common;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace AndroidPlusPlus.MsBuild.DeployTasks
{

  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  public class JavaCompile : TrackedOutOfDateToolTask, ITask
  {

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    List<string> m_outputClassPackages;

    List<ITaskItem> m_outputClassSourceFiles;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public JavaCompile ()
      : base (new ResourceManager ("AndroidPlusPlus.MsBuild.DeployTasks.Properties.Resources", Assembly.GetExecutingAssembly ()))
    {
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [Output]
    public ITaskItem [] OutputClassPaths { get; set; }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    protected override bool Setup ()
    {
      if (base.Setup ())
      {
        m_outputClassPackages = new List<string> ();

        m_outputClassSourceFiles = new List<ITaskItem> ();

        return true;
      }

      return false;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    protected override int TrackedExecuteTool (string pathToTool, string responseFileCommands, string commandLineCommands)
    {
      int retCode = -1;

      try
      {
        m_outputClassPackages.Clear ();

        m_outputClassSourceFiles.Clear ();

        retCode = base.TrackedExecuteTool (pathToTool, responseFileCommands, commandLineCommands);
      }
      catch (Exception e)
      {
        Log.LogErrorFromException (e, true);

        retCode = -1;
      }
      finally
      {
        if (retCode == 0)
        {
          // 
          // Export listing of compiled .class outputs and the default class path.
          // 

          string defaultClassPath = Sources [0].GetMetadata ("ClassOutputDirectory");

          ITaskItem defaultClassPathItem = new TaskItem (defaultClassPath);

          defaultClassPathItem.SetMetadata ("ClassPaths", defaultClassPath);

          OutputClassPaths = new ITaskItem [] { defaultClassPathItem };

          OutputFiles = m_outputClassSourceFiles.ToArray ();
        }
      }

      return retCode;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    protected override void LogEventsFromTextOutput (string singleLine, MessageImportance messageImportance)
    {
      try
      {
        if (!string.IsNullOrWhiteSpace (singleLine))
        {
          // 
          // Intercept output class filenames and package addresses
          // .
          // e.g.
          //  [checking com.example.nativemedia.MyRenderer]
          //  [wrote AndroidMT\Debug\bin\classes\com\example\nativemedia\MyRenderer.class]
          // 

          string sanitisedOutput = singleLine.Trim (new char [] { ' ', '[', ']' });

          if (sanitisedOutput.StartsWith ("checking "))
          {
            string packageAddressWithClassName = sanitisedOutput.Substring ("checking ".Length);

            string packageAddressWithoutClass = packageAddressWithClassName.Substring (0, packageAddressWithClassName.LastIndexOf ('.'));

            if (!m_outputClassPackages.Contains (packageAddressWithoutClass))
            {
              m_outputClassPackages.Add (packageAddressWithoutClass);
            }
          }
          else if (singleLine.StartsWith ("[wrote "))
          {
            string classFileOutput = singleLine.Trim (new char [] { ' ', '[', ']' }).Substring ("wrote ".Length);

            string classFilePath = Path.GetFullPath (classFileOutput);

            ITaskItem classFileItem = new TaskItem (classFilePath);

            classFileItem.SetMetadata ("ClassOutputDirectory", Sources [0].GetMetadata ("ClassOutputDirectory"));

            m_outputClassSourceFiles.Add (classFileItem);
          }
        }
      }
      catch (Exception e)
      {
        Log.LogErrorFromException (e, true);
      }
      finally
      {
        if (Sources [0].GetMetadata ("Verbose") == "true")
        {
          base.LogEventsFromTextOutput (singleLine, messageImportance);
        }
      }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    protected override string GenerateCommandLineFromProps (ITaskItem source)
    {
      // 
      // Build a commandline based on parsing switches from the registered property sheet, and any additional flags.
      // 

      StringBuilder builder = new StringBuilder (GccUtilities.CommandLineLength);

      try
      {
        if (source == null)
        {
          throw new ArgumentNullException ();
        }

        builder.Append (m_parsedProperties.Parse (source));

        builder.Append (" -verbose ");
      }
      catch (Exception e)
      {
        Log.LogErrorFromException (e, true);
      }

      return builder.ToString ();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    protected override string ToolName
    {
      get
      {
        return "JavaCompile";
      }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
