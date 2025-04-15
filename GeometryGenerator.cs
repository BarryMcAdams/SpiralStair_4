using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq; // Used for checking treadSolids

// Note: Assumes AcadUtils is in the same namespace or accessible.

namespace SpiralStair_4
{
    /// <summary>
    /// Creates the AutoCAD 3D solid geometry for the spiral stair.
    /// </summary>
    public class GeometryGenerator
    {
        private const string AppName = "SpiralStairGenerator_V1"; // RegApp name for XData
        private readonly Document _acadDoc;
        private readonly Database _acadDb;
        private readonly Editor _acadEditor;

        /// <summary>
        /// Initializes a new instance of the GeometryGenerator class.
        /// </summary>
        /// <param name="doc">The active AutoCAD document.</param>
        public GeometryGenerator(Document doc)
        {
            _acadDoc = doc ?? throw new ArgumentNullException(nameof(doc));
            _acadDb = _acadDoc.Database;
            _acadEditor = _acadDoc.Editor;
        }

        /// <summary>
        /// Generates the complete 3D geometry for the spiral stair based on the provided data.
        /// </summary>
        /// <param name="stairData">The calculated and validated stair data.</param>
        /// <returns>True if geometry generation was successful, false otherwise.</returns>
        public bool GenerateStairGeometry(StairData stairData)
        {
            if (stairData == null)
            {
                LogError("GenerateStairGeometry called with null StairData.");
                return false;
            }

            using (Transaction tr = _acadDb.TransactionManager.StartTransaction())
            {
                try
                {
                    // Prepare Database (Units, Layers, RegApp)
                    AcadUtils.SetUnitsToDecimalInches(_acadDb);
                    AcadUtils.EnsureLayerExists(tr, _acadDb, "Stair-Pole");
                    AcadUtils.EnsureLayerExists(tr, _acadDb, "Stair-Treads");
                    AcadUtils.EnsureLayerExists(tr, _acadDb, "Stair-Landing-Mid");
                    AcadUtils.EnsureLayerExists(tr, _acadDb, "Stair-Landing-Top");
                    AcadUtils.EnsureRegAppExists(tr, _acadDb, AppName);

                    // Create Center Pole
                    Solid3d poleSolid = CreateCenterPole(stairData);
                    if (poleSolid == null)
                    {
                        throw new InvalidOperationException("Failed to create center pole geometry.");
                    }
                    AcadUtils.AppendEntityToModelSpace(tr, _acadDb, poleSolid);
                    AcadUtils.SetObjectLayer(poleSolid, "Stair-Pole");
                    AcadUtils.AddXData(tr, poleSolid, AppName, new List<(string, string)> { ("ObjectType", "Pole"), ("Version", "1.0") });

                    // Create Treads and Midlanding (if applicable)
                    List<Solid3d> treadSolids = CreateTreadsAndMidlanding(tr, stairData);
                    if (treadSolids == null) // Check for null list itself, indicating a failure in the method
                    {
                         throw new InvalidOperationException("Failed during treads/midlanding geometry creation process.");
                    }
                    // Append valid solids even if some failed within CreateTreadsAndMidlanding
                    foreach (Solid3d solid in treadSolids.Where(s => s != null && !s.IsDisposed))
                    {
                        AcadUtils.AppendEntityToModelSpace(tr, _acadDb, solid);
                        // Layer and XData are set within CreateTreadsAndMidlanding
                    }


                    // Create Top Landing
                    Solid3d topLandingSolid = CreateTopLanding(tr, stairData);
                    if (topLandingSolid == null)
                    {
                        throw new InvalidOperationException("Failed to create top landing geometry.");
                    }
                    AcadUtils.AppendEntityToModelSpace(tr, _acadDb, topLandingSolid);
                    // Layer and XData are set within CreateTopLanding


                    tr.Commit();
                    _acadEditor.WriteMessage($"\nSuccessfully generated {1 + treadSolids.Count + 1} solid objects.");
                    return true;
                }
                catch (System.Exception ex)
                {
                    LogError($"Geometry generation failed: {ex.Message}\n{ex.StackTrace}");
                    tr.Abort(); // Ensure transaction is aborted on any error
                    return false;
                }
            } // Transaction is disposed here
        }

        /// <summary>
        /// Creates the 3D solid for the center pole.
        /// </summary>
        private Solid3d CreateCenterPole(StairData stairData)
        {
            if (stairData.CenterPoleDiameter <= 0 || stairData.OverallHeight <= 0)
            {
                LogWarning("Cannot create pole with non-positive diameter or height.");
                return null;
            }

            double poleRadius = stairData.CenterPoleDiameter / 2.0;
            Point3d startPoint = Point3d.Origin; // Base at 0,0,0
            Point3d endPoint = new Point3d(0, 0, stairData.OverallHeight);
            Vector3d axisVector = endPoint - startPoint;

            try
            {
                Solid3d poleSolid = new Solid3d();
                // Create a simple cylinder (Frustum with equal radii)
                poleSolid.CreateFrustum(axisVector.Length, poleRadius, poleRadius, poleRadius);
                // Position it (already at origin, but good practice if base wasn't 0,0,0)
                poleSolid.TransformBy(Matrix3d.Displacement(startPoint - Point3d.Origin));
                return poleSolid;
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to create pole Solid3d: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates the 3D solids for all treads and the midlanding (if required).
        /// </summary>
        private List<Solid3d> CreateTreadsAndMidlanding(Transaction tr, StairData stairData)
        {
            var solids = new List<Solid3d>();
            if (stairData.NumberOfTreads <= 0)
            {
                LogWarning("No treads to generate.");
                return solids; // Return empty list, not null
            }

            double poleRadius = stairData.CenterPoleDiameter / 2.0;
            double outerRadius = stairData.OutsideDiameter / 2.0;
            double currentRotationRad = 0.0; // Start angle for the first tread
            double currentZ = stairData.RiserHeight; // Z level of the *top* surface of the first tread

            for (int i = 0; i < stairData.NumberOfTreads; i++)
            {
                int treadIndex = i; // 0-based index
                bool isMidlanding = stairData.RequiresMidlanding && stairData.MidlandingPositionIndex.HasValue && stairData.MidlandingPositionIndex.Value == treadIndex;

                Solid3d solidToAdd = null;
                string objectType = "Tread";
                string layerName = "Stair-Treads";
                double angleForThisStepRad = stairData.TreadAngleRadians; // Default angle

                if (isMidlanding)
                {
                    objectType = "MidLanding";
                    layerName = "Stair-Landing-Mid";
                    // Midlanding typically replaces a tread and occupies a larger angle, e.g., 90 degrees.
                    // This assumes the midlanding *replaces* the standard tread's rotation contribution.
                    angleForThisStepRad = 90.0 * (Math.PI / 180.0); // Example: 90-degree midlanding
                    LogWarning($"Generating Midlanding at index {treadIndex}, replacing standard tread angle with {angleForThisStepRad * 180.0 / Math.PI:F1} degrees.");
                }

                // Create the solid for this step (tread or midlanding)
                solidToAdd = CreateSingleSectorSolid(stairData.TreadThickness, currentZ - stairData.TreadThickness, currentRotationRad, angleForThisStepRad, poleRadius, outerRadius);

                if (solidToAdd != null && !solidToAdd.IsDisposed)
                {
                    AcadUtils.SetObjectLayer(solidToAdd, layerName);
                    AcadUtils.AddXData(tr, solidToAdd, AppName, new List<(string, string)> {
                        ("ObjectType", objectType),
                        ("Index", treadIndex.ToString()), // 0-based index
                        ("StartAngle", (currentRotationRad * 180.0 / Math.PI).ToString("F4")),
                        ("SweepAngle", (angleForThisStepRad * 180.0 / Math.PI).ToString("F4")),
                        ("ZLevel", (currentZ - stairData.TreadThickness).ToString("F4")) // Z of bottom surface
                    });
                    solids.Add(solidToAdd);
                }
                else
                {
                    LogWarning($"Failed to create geometry for {objectType} at index {treadIndex}. Skipping.");
                    // Continue to next tread even if one fails
                }

                // Update rotation and Z for the *next* step
                currentRotationRad += angleForThisStepRad; // Add the angle used by the *current* step
                currentZ += stairData.RiserHeight; // Move up one riser height for the top of the next tread
            }
            return solids;
        }


        /// <summary>
        /// Creates a single 3D solid representing a sector (tread or landing piece).
        /// Extrudes downwards from the specified Z level.
        /// </summary>
        private Solid3d CreateSingleSectorSolid(double thickness, double zLevelTop, double startAngleRad, double sweepAngleRad, double innerRadius, double outerRadius)
        {
            // Basic validation
            if (thickness <= 0 || outerRadius <= innerRadius || outerRadius <= 0 || innerRadius < 0)
            {
                LogError($"Invalid parameters for CreateSingleSectorSolid: thickness={thickness}, innerR={innerRadius}, outerR={outerRadius}");
                return null;
            }
             // Handle zero sweep angle - create nothing or log warning? For now, return null.
            if (Math.Abs(sweepAngleRad) < 1e-9)
            {
                 LogWarning("Attempted to create sector solid with zero sweep angle.");
                 return null;
            }


            Point3d p1, p2, p3, p4; // Points defining the base profile at Z=0
            DBObjectCollection curves = new DBObjectCollection();
            Autodesk.AutoCAD.DatabaseServices.Region sectorRegion = null;
            Solid3d sectorSolid = null;

            try
            {
                // Define corner points at Z=0
                double endAngleRad = startAngleRad + sweepAngleRad;
                p1 = new Point3d(innerRadius * Math.Cos(startAngleRad), innerRadius * Math.Sin(startAngleRad), 0); // Inner start
                p2 = new Point3d(outerRadius * Math.Cos(startAngleRad), outerRadius * Math.Sin(startAngleRad), 0); // Outer start
                p3 = new Point3d(outerRadius * Math.Cos(endAngleRad), outerRadius * Math.Sin(endAngleRad), 0);     // Outer end
                p4 = new Point3d(innerRadius * Math.Cos(endAngleRad), innerRadius * Math.Sin(endAngleRad), 0);     // Inner end

                // Create boundary curves
                curves.Add(new Line(p1, p2)); // Start radial line
                // Outer Arc - handle potential full circle? No, sweepAngle should be less than 360 for single tread
                curves.Add(new Arc(Point3d.Origin, outerRadius, startAngleRad, endAngleRad));
                curves.Add(new Line(p3, p4)); // End radial line
                 // Inner Arc - handle potential full circle? No.
                curves.Add(new Arc(Point3d.Origin, innerRadius, endAngleRad, startAngleRad)); // Note reversed angles for inner arc direction


                // Create Region from curves
                DBObjectCollection regions = Autodesk.AutoCAD.DatabaseServices.Region.CreateFromCurves(curves);
                if (regions == null || regions.Count == 0)
                {
                    throw new InvalidOperationException("Failed to create region from boundary curves.");
                }
                sectorRegion = regions[0] as Autodesk.AutoCAD.DatabaseServices.Region;
                if (sectorRegion == null)
                {
                     // Dispose potential other regions if needed, though usually only one is expected
                     foreach(DBObject obj in regions) obj.Dispose();
                    throw new InvalidOperationException("Created DBObject is not a Region.");
                }
                 // Dispose original curves now that region is made
                foreach (DBObject curve in curves) curve.Dispose();
                curves.Clear(); // Clear collection


                // Extrude the region to create the solid
                sectorSolid = new Solid3d();
                // Extrude downwards by thickness, so top surface is at zLevelTop
                sectorSolid.Extrude(sectorRegion, -thickness, 0); // Negative thickness for downward extrusion, taper angle 0 for straight sides

                // Move the solid to the correct Z level
                Matrix3d transform = Matrix3d.Displacement(new Vector3d(0, 0, zLevelTop));
                sectorSolid.TransformBy(transform);

                sectorRegion.Dispose(); // Dispose region after extrusion
                return sectorSolid;
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to create single sector solid: {ex.Message}");
                // Clean up intermediate objects if they exist and are not disposed
                sectorSolid?.Dispose();
                sectorRegion?.Dispose();
                foreach (DBObject curve in curves) curve?.Dispose(); // Dispose any remaining curves
                return null;
            }
        }

        /// <summary>
        /// Creates the 3D solid for the rectangular top landing.
        /// </summary>
        private Solid3d CreateTopLanding(Transaction tr, StairData stairData)
        {
            if (stairData.TopLandingLength <= 0 || stairData.TopLandingThickness <= 0 || stairData.OutsideDiameter <= stairData.CenterPoleDiameter)
            {
                LogError("Invalid dimensions for top landing.");
                return null;
            }

            // Recalculate width just in case (should match ValidationService)
            double poleRadius = stairData.CenterPoleDiameter / 2.0;
            double outerRadius = stairData.OutsideDiameter / 2.0;
            double landingWidth = outerRadius - poleRadius; // Width from pole face outwards
            if (landingWidth <= 0)
            {
                 LogError("Calculated top landing width is zero or negative.");
                 return null;
            }


            // Determine the final rotation angle where the stair ends
            double finalAngleRad = 0.0;
            if (stairData.NumberOfTreads > 0)
            {
                // Sum up angles considering the midlanding if it exists
                double accumulatedAngle = 0;
                for(int i=0; i < stairData.NumberOfTreads; i++)
                {
                    bool isMidlanding = stairData.RequiresMidlanding && stairData.MidlandingPositionIndex.HasValue && stairData.MidlandingPositionIndex.Value == i;
                    accumulatedAngle += isMidlanding ? (90.0 * Math.PI / 180.0) : stairData.TreadAngleRadians;
                }
                finalAngleRad = accumulatedAngle;
                // Alternative: Use TotalRotation if no midlanding? Check consistency.
                // If midlanding exists, TotalRotation input might not match the actual final angle.
                // Using the sum of actual step angles is more robust.
            }
             _acadEditor.WriteMessage($"\nTop landing calculated final angle: {finalAngleRad * 180.0 / Math.PI:F2} degrees.");


            Polyline polyline = null;
            Autodesk.AutoCAD.DatabaseServices.Region landingRegion = null;
            Solid3d landingSolid = null;

            try
            {
                // Create rectangular profile at XY plane, origin at the point touching the pole face
                // Points relative to the connection point on the pole face at Z=0, before rotation/translation
                Point2d p1 = new Point2d(0, 0); // Inner corner touching pole face
                Point2d p2 = new Point2d(stairData.TopLandingLength, 0); // Outer corner, same side
                Point2d p3 = new Point2d(stairData.TopLandingLength, landingWidth); // Outer corner, far side
                Point2d p4 = new Point2d(0, landingWidth); // Inner corner, far side

                polyline = new Polyline();
                polyline.AddVertexAt(0, p1, 0, 0, 0);
                polyline.AddVertexAt(1, p2, 0, 0, 0);
                polyline.AddVertexAt(2, p3, 0, 0, 0);
                polyline.AddVertexAt(3, p4, 0, 0, 0);
                polyline.Closed = true;

                // Create region from polyline
                DBObjectCollection curves = new DBObjectCollection { polyline };
                DBObjectCollection regions = Autodesk.AutoCAD.DatabaseServices.Region.CreateFromCurves(curves);
                 // Dispose polyline now region is made (or let using handle it if polyline declared in using)
                polyline.Dispose(); // Dispose explicitly here
                curves.Clear();

                if (regions == null || regions.Count == 0)
                {
                    throw new InvalidOperationException("Failed to create region for top landing.");
                }
                landingRegion = regions[0] as Autodesk.AutoCAD.DatabaseServices.Region;
                 if (landingRegion == null)
                {
                     foreach(DBObject obj in regions) obj.Dispose();
                    throw new InvalidOperationException("Created DBObject is not a Region for top landing.");
                }


                // Extrude the region (downwards, similar to treads)
                landingSolid = new Solid3d();
                landingSolid.Extrude(landingRegion, -stairData.TopLandingThickness, 0); // Taper angle 0 for straight sides
                landingRegion.Dispose(); // Dispose region after extrusion

                // --- Transformation ---
                // 1. Rotate the landing around Z-axis at origin to match the final stair angle
                Matrix3d rotationMatrix = Matrix3d.Rotation(finalAngleRad, Vector3d.ZAxis, Point3d.Origin);

                // 2. Translate the landing to the correct position:
                //    - The origin (p1) of the landing profile should be at the pole face (radius)
                //    - at the final angle
                //    - at the final height (OverallHeight - LandingThickness)
                Point3d connectionPointOnPole = new Point3d(
                    poleRadius * Math.Cos(finalAngleRad),
                    poleRadius * Math.Sin(finalAngleRad),
                    stairData.OverallHeight // Top surface Z level
                );
                Matrix3d translationMatrix = Matrix3d.Displacement(connectionPointOnPole - Point3d.Origin);

                // Apply transformations: Rotate first, then translate
                landingSolid.TransformBy(rotationMatrix);
                landingSolid.TransformBy(translationMatrix);


                // Set Layer and XData
                AcadUtils.SetObjectLayer(landingSolid, "Stair-Landing-Top");
                AcadUtils.AddXData(tr, landingSolid, AppName, new List<(string, string)> {
                    ("ObjectType", "TopLanding"),
                    ("ConnectAngle", (finalAngleRad * 180.0 / Math.PI).ToString("F4")),
                    ("ZLevel", (stairData.OverallHeight - stairData.TopLandingThickness).ToString("F4")) // Z of bottom surface
                });

                return landingSolid;
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to create top landing: {ex.Message}");
                landingSolid?.Dispose();
                landingRegion?.Dispose();
                polyline?.Dispose(); // Ensure polyline is disposed on error too
                return null;
            }
        }


        // --- Logging Helpers ---
        private void LogError(string message)
        {
            _acadEditor?.WriteMessage($"\nERROR (GeometryGenerator): {message}");
            System.Diagnostics.Debug.WriteLine($"ERROR (GeometryGenerator): {message}");
        }
        private void LogWarning(string message)
        {
            _acadEditor?.WriteMessage($"\nWARNING (GeometryGenerator): {message}");
            System.Diagnostics.Debug.WriteLine($"WARNING (GeometryGenerator): {message}");
        }
    }
}