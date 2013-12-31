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
using System.Threading;
using System.Linq;

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

  public class AndroidManifestMerge : Task
  {

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public AndroidManifestMerge ()
      : base (new ResourceManager ("AndroidPlusPlus.MsBuild.DeployTasks.Properties.Resources", Assembly.GetExecutingAssembly ()))
    {
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [Required]
    public ITaskItem [] Manifests { get; set; }

    [Output]
    public ITaskItem MergedManifest { get; set; }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public override bool Execute ()
    {
      try
      {
        if (Manifests.Length == 0)
        {
          Log.LogError ("No input 'Manifests' entries specified.");

          return false;
        }

        // 
        // Evaluate the primary manifest in the provided list. There should be only one manifest with an <application> node.
        // 

        MergedManifest = null;

        foreach (ITaskItem item in Manifests)
        {
          try
          {
            AndroidManifestDocument itemManifest = new AndroidManifestDocument ();

            itemManifest.Load (item.GetMetadata ("FullPath"));

            if (itemManifest.IsApplication)
            {
              if (MergedManifest == null)
              {
                MergedManifest = new TaskItem (item.ItemSpec);

                item.CopyMetadataTo (MergedManifest);
              }
              else
              {
                Log.LogError ("Found multiple manifests which define an <application> node");

                break;
              }
            }
          }
          catch (Exception e)
          {
            Log.LogErrorFromException (e, true);

            break;
          }
        }

        // 
        // Process other 'library' manifests merging required metadata.
        // 

        if (MergedManifest != null)
        {
          foreach (ITaskItem item in Manifests)
          {
            if (item.GetMetadata ("FullPath") != MergedManifest.GetMetadata ("FullPath"))
            {
              AndroidManifestDocument itemManifest = new AndroidManifestDocument ();

              itemManifest.Load (item.GetMetadata ("FullPath"));

              string existingExtraPackages = MergedManifest.GetMetadata ("ExtraPackages");

              MergedManifest.SetMetadata ("ExtraPackages", existingExtraPackages + ((existingExtraPackages.Length > 0) ? ":" : "") + itemManifest.Package);
            }
          }
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogErrorFromException (e, true);
      }

      return false;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void MergeXmlManifests ()
    {

    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /*public override bool Execute ()
    {
      if (Manifests.Length == 0)
      {
        Log.LogError ("No input 'Manifests' entries specified.");

        return false;
      }

      // 
      // Sort manifest elements so that the ApplicationManifest task is first in the list. Remove duplicates.
      // 

      Dictionary<string, ITaskItem> sortedManifests = new Dictionary<string, ITaskItem> ();

      string applicationManifestFullPath = ApplicationManifest.GetMetadata ("FullPath");

      foreach (ITaskItem manifest in Manifests)
      {
        string manifestFullPath = manifest.GetMetadata ("FullPath");

        if (manifestFullPath == applicationManifestFullPath)
        {
          sortedManifests.Add (manifestFullPath, manifest);

          break;
        }
      }

      if (sortedManifests.Count == 0)
      {
        Log.LogError ("Input 'Manifest' list does not contain a reference matching 'ApplicationManifest'");

        return false;
      }

      foreach (ITaskItem manifest in Manifests)
      {
        string manifestFullPath = manifest.GetMetadata ("FullPath");

        if (!sortedManifests.ContainsKey (manifestFullPath))
        {
          sortedManifests.Add (manifestFullPath, manifest);
        }
      }

      // 
      // Manually compound the sorted list and export to array.
      // 

      List<ITaskItem> outputManifestList = new List<ITaskItem> ();

      foreach (KeyValuePair<string, ITaskItem> sortedKeyPair in sortedManifests)
      {
        ITaskItem manifestItem = new TaskItem (sortedKeyPair.Key);

        sortedKeyPair.Value.CopyMetadataTo (manifestItem);

        outputManifestList.Add (manifestItem);
      }

      SortedManifests = outputManifestList.ToArray ();

      return true;
    }*/

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

