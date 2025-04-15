using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors; // Required for Color
using System;
using System.Collections.Generic; // Required for List
using Autodesk.AutoCAD.EditorInput; // Required for Editor


// Note: Error handling within these utilities is minimal for brevity,
// relying on the calling methods (like GeometryGenerator) to manage transactions and top-level errors.
// Consider adding more robust checks (e.g., null checks for db, transaction) in a production environment.

namespace SpiralStair_4
{
    /// <summary>
    /// Provides static utility functions for common AutoCAD interactions.
    /// </summary>
    public static class AcadUtils
    {
        /// <summary>
        /// Sets the active database units to Decimal with precision 4 and insertion scale to Inches.
        /// </summary>
        /// <param name="db">The database to modify.</param>
        public static void SetUnitsToDecimalInches(Database db)
        {
            if (db == null) return;
            try
            {
                Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("LUNITS", 4); // Decimal units (4)
                Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("LUPREC", 4); // 4 decimal places
                db.Insunits = UnitsValue.Inches; // Insertion scale units
            }
            catch (System.Exception ex)
            {
                LogWarning($"Failed to set database units: {ex.Message}");
                // Depending on severity, might re-throw or just log
            }
        }

        /// <summary>
        /// Ensures a layer with the specified name exists in the database. Creates it if not found.
        /// </summary>
        /// <param name="tr">The active transaction.</param>
        /// <param name="db">The database containing the LayerTable.</param>
        /// <param name="layerName">The name of the layer to ensure exists.</param>
        /// <param name="colorIndex">Optional ACI color index for the new layer (default is 7/White).</param>
        /// <param name="lineWeight">Optional LineWeight for the new layer (default is 0.30mm).</param>
        public static void EnsureLayerExists(Transaction tr, Database db, string layerName, short colorIndex = 7, LineWeight lineWeight = LineWeight.LineWeight030)
        {
            if (tr == null || db == null || string.IsNullOrWhiteSpace(layerName)) return;

            LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
            if (lt == null)
            {
                LogWarning($"Could not open LayerTable.");
                return;
            }

            if (!lt.Has(layerName))
            {
                LayerTableRecord ltr = null;
                try
                {
                    lt.UpgradeOpen(); // Need write access to add layer
                    ltr = new LayerTableRecord
                    {
                        Name = layerName,
                        Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, colorIndex),
                        LineWeight = lineWeight
                    };

                    // Set Linetype to "Continuous" if available, otherwise default
                    LinetypeTable ltt = tr.GetObject(db.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                    if (ltt != null)
                    {
                        ltr.LinetypeObjectId = ltt.Has("Continuous") ? ltt["Continuous"] : db.Celtype;
                    }
                    else
                    {
                         LogWarning($"Could not open LinetypeTable. Using default linetype for layer '{layerName}'.");
                         ltr.LinetypeObjectId = db.Celtype; // Fallback
                    }


                    lt.Add(ltr);
                    tr.AddNewlyCreatedDBObject(ltr, true);
                    // DowngradeOpen happens automatically when transaction commits/aborts
                }
                catch (System.Exception ex)
                {
                    LogWarning($"Failed to create layer '{layerName}': {ex.Message}");
                    // Clean up partially created object if necessary (though transaction abort should handle it)
                    if (ltr != null && !ltr.IsDisposed && !ltr.IsWriteEnabled) ltr.Dispose();
                }
            }
        }

        /// <summary>
        /// Ensures a Registered Application name exists in the database. Creates it if not found.
        /// </summary>
        /// <param name="tr">The active transaction.</param>
        /// <param name="db">The database containing the RegAppTable.</param>
        /// <param name="appName">The name of the Registered Application.</param>
        public static void EnsureRegAppExists(Transaction tr, Database db, string appName)
        {
            if (tr == null || db == null || string.IsNullOrWhiteSpace(appName)) return;

            RegAppTable rat = tr.GetObject(db.RegAppTableId, OpenMode.ForRead) as RegAppTable;
             if (rat == null)
            {
                LogWarning($"Could not open RegAppTable.");
                return;
            }

            if (!rat.Has(appName))
            {
                RegAppTableRecord ratr = null;
                try
                {
                    rat.UpgradeOpen();
                    ratr = new RegAppTableRecord { Name = appName };
                    rat.Add(ratr);
                    tr.AddNewlyCreatedDBObject(ratr, true);
                }
                catch (System.Exception ex)
                {
                    LogWarning($"Failed to create RegApp '{appName}': {ex.Message}");
                     if (ratr != null && !ratr.IsDisposed && !ratr.IsWriteEnabled) ratr.Dispose();
                }
            }
        }

        /// <summary>
        /// Appends a newly created DBObject (Entity or other) to the Model Space of the database.
        /// </summary>
        /// <param name="tr">The active transaction.</param>
        /// <param name="db">The database to add the entity to.</param>
        /// <param name="obj">The DBObject (e.g., Entity) to append.</param>
        public static void AppendEntityToModelSpace(Transaction tr, Database db, DBObject obj)
        {
            if (tr == null || db == null || obj == null || obj.IsDisposed) return;

            try
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                 if (bt == null) { LogWarning("Could not open BlockTable."); return; }

                BlockTableRecord ms = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                 if (ms == null) { LogWarning("Could not open ModelSpace BlockTableRecord."); return; }

                if (obj is Entity entity) // Common case
                {
                    ms.AppendEntity(entity);
                    tr.AddNewlyCreatedDBObject(entity, true);
                }
                else // Handle other DBObject types if necessary, though less common for direct model space addition
                {
                     LogWarning($"Attempted to add non-Entity DBObject of type {obj.GetType().Name} directly to ModelSpace. This might not be standard practice.");
                     // For some objects, adding might still work or require different handling.
                     // Example: Adding a Dictionary might involve adding it to the Named Object Dictionary instead.
                     // If this is needed, add specific handling here. For now, we focus on Entities.
                     // ms.AppendEntity(obj); // This line would likely cause a cast error if obj is not an Entity.
                     // tr.AddNewlyCreatedDBObject(obj, true);
                }
            }
            catch (System.Exception ex)
            {
                LogWarning($"Failed to append object {obj.GetType().Name} (Handle: {obj.Handle}) to ModelSpace: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the Layer property of an Entity.
        /// </summary>
        /// <param name="entity">The entity to modify.</param>
        /// <param name="layerName">The name of the layer to assign.</param>
        public static void SetObjectLayer(Entity entity, string layerName)
        {
            if (entity == null || entity.IsDisposed || string.IsNullOrWhiteSpace(layerName)) return;

            try
            {
                // Check if the entity is write-enabled, upgrade if necessary (though typically done by caller)
                if (!entity.IsWriteEnabled)
                {
                    // This indicates a potential issue in the calling code's transaction management.
                    // Upgrading here might mask the root cause. Logging a warning is appropriate.
                    LogWarning($"Attempting to set layer on an entity '{entity.GetType().Name}' (Handle: {entity.Handle}) that was not opened for write.");
                    // entity.UpgradeOpen(); // Avoid doing this here; caller should manage open state.
                    return; // Exit if not write-enabled, as setting layer will fail.
                }
                entity.Layer = layerName;
            }
            catch (System.Exception ex) // Catch potential errors like layer not existing (though EnsureLayer should prevent this)
            {
                LogWarning($"Failed to set layer '{layerName}' for entity {entity.GetType().Name} (Handle: {entity.Handle}): {ex.Message}");
            }
        }

        /// <summary>
        /// Adds Extended Entity Data (XData) to a DBObject.
        /// </summary>
        /// <param name="tr">The active transaction (needed to verify RegApp exists).</param>
        /// <param name="obj">The DBObject to add XData to.</param>
        /// <param name="appName">The Registered Application name for the XData.</param>
        /// <param name="dataPairs">A list of key-value string pairs to store as XData.</param>
        public static void AddXData(Transaction tr, DBObject obj, string appName, List<(string Key, string Value)> dataPairs)
        {
            if (tr == null || obj == null || obj.IsDisposed || string.IsNullOrWhiteSpace(appName) || dataPairs == null || dataPairs.Count == 0) return;

            // Ensure the RegApp exists (important!)
             RegAppTable rat = tr.GetObject(obj.Database.RegAppTableId, OpenMode.ForRead) as RegAppTable;
             if (rat == null || !rat.Has(appName))
             {
                 LogWarning($"Cannot add XData. RegApp '{appName}' does not exist. Ensure it's created first using EnsureRegAppExists.");
                 return;
             }


            try
            {
                 // Check if the object is write-enabled
                if (!obj.IsWriteEnabled)
                {
                    LogWarning($"Attempting to add XData to an object '{obj.GetType().Name}' (Handle: {obj.Handle}) that was not opened for write.");
                    return; // Exit if not write-enabled
                }

                var resultBufferList = new List<TypedValue>
                {
                    // Start with the RegApp name
                    new TypedValue((int)DxfCode.ExtendedDataRegAppName, appName)
                };

                // Add the key-value pairs as strings
                foreach (var pair in dataPairs)
                {
                    if (!string.IsNullOrEmpty(pair.Key) && pair.Value != null)
                    {
                        // Store as "Key=Value" string for simplicity in this example.
                        // Other DxfCode types (Integer, Double, Point, etc.) can be used for more structured data.
                        resultBufferList.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, $"{pair.Key}={pair.Value}"));
                    }
                }

                // Only add XData if there's more than just the app name
                if (resultBufferList.Count > 1)
                {
                    using (ResultBuffer rb = new ResultBuffer(resultBufferList.ToArray()))
                    {
                        obj.XData = rb;
                    }
                }
                else
                {
                     LogWarning($"No valid data pairs provided for XData application '{appName}'.");
                }
            }
            catch (System.Exception ex)
            {
                LogWarning($"Failed to add XData for app '{appName}' to object {obj.GetType().Name} (Handle: {obj.Handle}): {ex.Message}");
            }
        }

        /// <summary>
        /// Logs a warning message to the AutoCAD command line.
        /// </summary>
        /// <param name="message">The message to log.</param>
        private static void LogWarning(string message)
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"\nWARNING (AcadUtils): {message}");
            System.Diagnostics.Debug.WriteLine($"WARNING (AcadUtils): {message}"); // Also write to debug output
        }

         /// <summary>
        /// Logs an error message to the AutoCAD command line.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogError(string message) // Made public for potential use elsewhere
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"\nERROR (AcadUtils): {message}");
             System.Diagnostics.Debug.WriteLine($"ERROR (AcadUtils): {message}"); // Also write to debug output
        }
    }
}