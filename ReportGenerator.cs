using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry; // Needed for Point3d in table insertion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms; // For MessageBox and SaveFileDialog/DialogResult
using System.IO; // For Path and File operations

// Note: Assumes AcadUtils is accessible.

namespace SpiralStair_4
{
    /// <summary>
    /// Handles final reporting of the generated stair data (MessageBox, Table, CSV).
    /// </summary>
    public class ReportGenerator
    {
        private readonly Document _acadDoc;
        private readonly Database _acadDb;
        private readonly Editor _acadEditor;

        /// <summary>
        /// Initializes a new instance of the ReportGenerator class.
        /// </summary>
        /// <param name="doc">The active AutoCAD document.</param>
        public ReportGenerator(Document doc)
        {
            _acadDoc = doc ?? throw new ArgumentNullException(nameof(doc));
            _acadDb = _acadDoc.Database;
            _acadEditor = _acadDoc.Editor;
        }

        /// <summary>
        /// Generates and displays the final report in various formats.
        /// </summary>
        /// <param name="stairData">The completed StairData object.</param>
        public void GenerateReport(StairData stairData)
        {
            if (stairData == null)
            {
                LogError("GenerateReport called with null StairData.");
                return;
            }

            try
            {
                string reportText = FormatReportText(stairData);
                ShowSuccessMessage(reportText); // Show MessageBox first

                // Attempt to create AutoCAD table
                bool tableSuccess = CreateReportTable(stairData);
                if (!tableSuccess)
                {
                    LogWarning("Failed to create AutoCAD report table in the drawing.");
                    // Don't stop the process, just warn the user.
                }

                // Prompt for and export CSV
                PromptAndExportCsv(stairData);
            }
            catch (System.Exception ex)
            {
                LogError($"An error occurred during report generation: {ex.Message}\n{ex.StackTrace}");
                // Show a generic error to the user as well
                 MessageBox.Show($"An unexpected error occurred while generating the report:\n{ex.Message}",
                                "Report Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Formats the stair data into a multi-line string for display.
        /// </summary>
        private string FormatReportText(StairData stairData)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Spiral Stair Generation Report");
            sb.AppendLine("==============================");
            sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("--- Inputs ---");
            sb.AppendLine($"Center Pole Diameter: {stairData.CenterPoleDiameter:F3}\"");
            sb.AppendLine($"Overall Height (FF-FF): {stairData.OverallHeight:F3}\"");
            sb.AppendLine($"Outside Diameter: {stairData.OutsideDiameter:F3}\"");
            sb.AppendLine($"Total Rotation: {stairData.TotalRotation:F1}°");
            // sb.AppendLine($"Handedness: {stairData.Handedness}"); // Optional, as it's not used yet
            sb.AppendLine();
            sb.AppendLine("--- Calculated Values ---");
            sb.AppendLine($"Riser Height: {stairData.RiserHeight:F3}\"");
            sb.AppendLine($"Number of Risers: {stairData.NumberOfRisers}");
            sb.AppendLine($"Number of Treads: {stairData.NumberOfTreads}");
            sb.AppendLine($"Tread Angle: {stairData.TreadAngle:F2}°");
            sb.AppendLine($"Clear Width: {stairData.ClearWidth:F3}\"");
            sb.AppendLine($"Walkline Radius: {stairData.WalklineRadius:F3}\"");
            sb.AppendLine($"Tread Depth @ Walkline: {stairData.TreadDepthAtWalkline:F3}\"");
            sb.AppendLine($"Calculated Headroom: {(stairData.Headroom.HasValue ? stairData.Headroom.Value.ToString("F2") + "\"" : "N/A")}");
            sb.AppendLine($"Midlanding Required: {(stairData.RequiresMidlanding ? "Yes" : "No")}");
            if (stairData.RequiresMidlanding)
            {
                sb.AppendLine($"Midlanding Position: Replaces Tread #{(stairData.MidlandingPositionIndex.HasValue ? (stairData.MidlandingPositionIndex.Value + 1).ToString() : "N/A")}"); // Display 1-based index
            }
            sb.AppendLine($"Top Landing Generated: Yes"); // Assuming it's always generated if process reaches here
            sb.AppendLine($"Top Landing Width: {stairData.TopLandingWidth:F3}\"");
            sb.AppendLine($"Top Landing Length: {stairData.TopLandingLength:F3}\"");
            sb.AppendLine();
            sb.AppendLine("--- Compliance Status ---");

            // Filter out the "Midlanding Required" message from the compliance list shown here
            var complianceIssues = stairData.ValidationIssues?.Where(s => !s.StartsWith("Midlanding Required", StringComparison.OrdinalIgnoreCase)).ToList()
                                   ?? new List<string>();

            if (complianceIssues.Count > 0)
            {
                sb.AppendLine("Status: Generated with Code Violations/Warnings");
                foreach (string issue in complianceIssues)
                {
                    sb.AppendLine($"- {issue}");
                }
            }
            else
            {
                sb.AppendLine("Status: Code Compliant (Based on checks performed)");
            }

            if (stairData.RequiresMidlanding)
            {
                sb.AppendLine("Note: Midlanding was required and generated as selected.");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Shows the report text in a standard Windows Forms MessageBox.
        /// </summary>
        private void ShowSuccessMessage(string reportText)
        {
             MessageBox.Show(reportText, "Stair Generation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Creates an AutoCAD Table entity containing the report data and prompts the user for insertion point.
        /// </summary>
        /// <returns>True if the table was successfully created and inserted, false otherwise.</returns>
        private bool CreateReportTable(StairData stairData)
        {
            using (Transaction tr = _acadDb.TransactionManager.StartTransaction())
            {
                Table table = null;
                try
                {
                    table = new Table();
                    table.SetDatabaseDefaults(_acadDb); // Use current DB settings

                    // Define table data (Parameter, Value)
                    var dataRows = new List<(string, string)>
                    {
                        ("Center Pole Dia:", $"{stairData.CenterPoleDiameter:F3}\""),
                        ("Overall Height:", $"{stairData.OverallHeight:F3}\""),
                        ("Outside Diameter:", $"{stairData.OutsideDiameter:F3}\""),
                        ("Total Rotation:", $"{stairData.TotalRotation:F1}°"),
                        ("Riser Height:", $"{stairData.RiserHeight:F3}\""),
                        ("# Risers:", stairData.NumberOfRisers.ToString()),
                        ("# Treads:", stairData.NumberOfTreads.ToString()),
                        ("Tread Angle:", $"{stairData.TreadAngle:F2}°"),
                        ("Clear Width:", $"{stairData.ClearWidth:F3}\""),
                        ("Walkline Depth:", $"{stairData.TreadDepthAtWalkline:F3}\""),
                        ("Headroom:", $"{(stairData.Headroom.HasValue ? stairData.Headroom.Value.ToString("F2") + "\"" : "N/A")}"),
                        ("Midlanding:", $"{(stairData.RequiresMidlanding ? "Yes (Tread #" + (stairData.MidlandingPositionIndex.HasValue ? (stairData.MidlandingPositionIndex.Value + 1).ToString() : "N/A") + ")" : "No")}"),
                        ("Compliance:", $"{(stairData.ValidationIssues?.Count(s => !s.StartsWith("Midlanding Required", StringComparison.OrdinalIgnoreCase)) == 0 ? "Pass" : "Warnings")}")
                    };

                    int numRows = dataRows.Count + 1; // +1 for title row
                    int numCols = 2;
                    table.SetSize(numRows, numCols);
                    table.SetRowHeight(0.25); // Adjust as needed
                    table.SetColumnWidth(3.0); // Adjust as needed

                    // Title Row
                    table.Cells[0, 0].Value = "Spiral Stair Report";
                    table.Cells[0, 0].Alignment = CellAlignment.MiddleCenter;
                    table.MergeCells(CellRange.Create(table, 0, 0, 0, numCols - 1)); // Merge title across columns

                    // Data Rows
                    for (int r = 0; r < dataRows.Count; r++)
                    {
                        int row = r + 1; // Start from row 1 (below title)
                        table.Cells[row, 0].Value = dataRows[r].Item1; // Parameter name
                        table.Cells[row, 0].Alignment = CellAlignment.MiddleLeft;
                        table.Cells[row, 1].Value = dataRows[r].Item2; // Value
                        table.Cells[row, 1].Alignment = CellAlignment.MiddleRight;
                    }

                    // Style adjustments (optional)
                    table.Cells.Style = "Standard"; // Use a predefined table style if desired
                    table.Cells[0,0].TextStyleId = _acadDb.Textstyle; // Use current text style for title
                    // You can set text heights, colors, etc. per cell or range

                    // Prompt for insertion point
                    var pointOptions = new PromptPointOptions("\nSelect insertion point for report table: ")
                    {
                        AllowNone = false // Require a point
                    };
                    PromptPointResult insertionPointResult = _acadEditor.GetPoint(pointOptions);

                    if (insertionPointResult.Status != PromptStatus.OK)
                    {
                        _acadEditor.WriteMessage("\n*Cancel* Table placement cancelled by user.");
                        tr.Abort(); // Abort transaction if user cancels placement
                        return false;
                    }

                    table.Position = insertionPointResult.Value; // Set table position

                    // Add table to ModelSpace
                    AcadUtils.AppendEntityToModelSpace(tr, _acadDb, table);

                    tr.Commit(); // Commit transaction if everything succeeded
                    _acadEditor.WriteMessage("\nReport table created successfully.");
                    return true;
                }
                catch (System.Exception ex)
                {
                    LogError($"Failed to create report table: {ex.Message}");
                    table?.Dispose(); // Dispose table if created but error occurred before commit
                    tr.Abort(); // Ensure transaction is aborted on error
                    return false;
                }
            } // Transaction disposed here
        }

        /// <summary>
        /// Prompts the user if they want to save the report data to a CSV file on their Desktop.
        /// </summary>
        private void PromptAndExportCsv(StairData stairData)
        {
            try
            {
                var result = MessageBox.Show("Save dimensional stats to CSV file on your Desktop?",
                                             "Export Report Data",
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Question,
                                             MessageBoxDefaultButton.Button2); // Default to No

                if (result == DialogResult.Yes)
                {
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    // Sanitize filename slightly (though timestamp usually makes it unique)
                    string safeAppName = "SpiralStairReport";
                    string fileName = $"{safeAppName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    string filePath = Path.Combine(desktopPath, fileName);

                    try
                    {
                        string csvContent = ConvertDataToCsv(stairData);
                        File.WriteAllText(filePath, csvContent, Encoding.UTF8); // Use UTF8 for broader compatibility

                        MessageBox.Show($"Report saved successfully to:\n{filePath}",
                                        "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _acadEditor.WriteMessage($"\nReport data exported to: {filePath}");
                    }
                    catch (IOException ioEx)
                    {
                        LogError($"Failed to write CSV file: {ioEx.Message}");
                        MessageBox.Show($"Could not save report to CSV (File I/O Error):\n{ioEx.Message}",
                                        "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (System.Exception ex) // Catch other potential errors during CSV generation/write
                    {
                        LogError($"Failed to export CSV: {ex.Message}");
                        MessageBox.Show($"Could not save report to CSV:\n{ex.Message}",
                                        "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                     _acadEditor.WriteMessage("\nCSV export skipped by user.");
                }
            }
             catch (System.Exception ex) // Catch errors related to MessageBox or Environment.GetFolderPath
            {
                 LogError($"Error during CSV export prompt phase: {ex.Message}");
                 MessageBox.Show($"An error occurred trying to prompt for CSV export:\n{ex.Message}",
                                 "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Converts the StairData object into a CSV formatted string.
        /// </summary>
        private string ConvertDataToCsv(StairData stairData)
        {
            var sb = new StringBuilder();

            // Header Row
            sb.AppendLine("Parameter,Value,Units");

            // Helper function to format CSV cells (handles commas, quotes)
            Func<object, string> FormatCsvCell = (value) => {
                if (value == null) return "";
                string stringValue = value.ToString();
                // If the value contains a comma, quote, or newline, enclose in double quotes and escape existing quotes
                if (stringValue.Contains(",") || stringValue.Contains("\"") || stringValue.Contains("\n"))
                {
                    return $"\"{stringValue.Replace("\"", "\"\"")}\"";
                }
                else
                {
                    return stringValue;
                }
            };

            // Data Rows
            sb.AppendLine($"Center Pole Diameter,{FormatCsvCell(stairData.CenterPoleDiameter)},inches");
            sb.AppendLine($"Overall Height,{FormatCsvCell(stairData.OverallHeight)},inches");
            sb.AppendLine($"Outside Diameter,{FormatCsvCell(stairData.OutsideDiameter)},inches");
            sb.AppendLine($"Total Rotation,{FormatCsvCell(stairData.TotalRotation)},degrees");
            sb.AppendLine($"Riser Height,{FormatCsvCell(stairData.RiserHeight)},inches");
            sb.AppendLine($"Number of Risers,{FormatCsvCell(stairData.NumberOfRisers)},");
            sb.AppendLine($"Number of Treads,{FormatCsvCell(stairData.NumberOfTreads)},");
            sb.AppendLine($"Tread Angle,{FormatCsvCell(stairData.TreadAngle)},degrees");
            sb.AppendLine($"Clear Width,{FormatCsvCell(stairData.ClearWidth)},inches");
            sb.AppendLine($"Walkline Radius,{FormatCsvCell(stairData.WalklineRadius)},inches");
            sb.AppendLine($"Tread Depth @ Walkline,{FormatCsvCell(stairData.TreadDepthAtWalkline)},inches");
            sb.AppendLine($"Headroom,{FormatCsvCell(stairData.Headroom.HasValue ? stairData.Headroom.Value.ToString("F2") : "N/A")},inches");
            sb.AppendLine($"Midlanding Required,{FormatCsvCell(stairData.RequiresMidlanding ? "Yes" : "No")},");
            if (stairData.RequiresMidlanding)
            {
                 sb.AppendLine($"Midlanding Position (0-based),{FormatCsvCell(stairData.MidlandingPositionIndex)},");
                 sb.AppendLine($"Midlanding Position (Replaces Tread #),{FormatCsvCell(stairData.MidlandingPositionIndex.HasValue ? (stairData.MidlandingPositionIndex.Value + 1).ToString() : "N/A")},");
            }
            sb.AppendLine($"Top Landing Width,{FormatCsvCell(stairData.TopLandingWidth)},inches");
            sb.AppendLine($"Top Landing Length,{FormatCsvCell(stairData.TopLandingLength)},inches");
            sb.AppendLine($"Tread Thickness,{FormatCsvCell(stairData.TreadThickness)},inches"); // Added thickness
            sb.AppendLine($"Top Landing Thickness,{FormatCsvCell(stairData.TopLandingThickness)},inches"); // Added thickness

            // Compliance Status
             var complianceIssues = stairData.ValidationIssues?.Where(s => !s.StartsWith("Midlanding Required", StringComparison.OrdinalIgnoreCase)).ToList() ?? new List<string>();
            sb.AppendLine($"Compliance Status,{FormatCsvCell(complianceIssues.Count == 0 ? "Pass" : "Warnings")},");
            if (complianceIssues.Count > 0)
            {
                // Join issues with a semicolon for a single cell, formatted using the helper
                sb.AppendLine($"Violations/Warnings,{FormatCsvCell(string.Join("; ", complianceIssues))},");
            }

            return sb.ToString();
        }


        // --- Logging Helpers ---
        private void LogError(string message)
        {
            _acadEditor?.WriteMessage($"\nERROR (ReportGenerator): {message}");
            System.Diagnostics.Debug.WriteLine($"ERROR (ReportGenerator): {message}");
        }
        private void LogWarning(string message)
        {
            _acadEditor?.WriteMessage($"\nWARNING (ReportGenerator): {message}");
            System.Diagnostics.Debug.WriteLine($"WARNING (ReportGenerator): {message}");
        }
    }
}