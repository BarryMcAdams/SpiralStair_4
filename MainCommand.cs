using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Windows.Forms; // Required for DialogResult and Application.ShowModalDialog

// Note: Ensure all other project files (StairData, Forms, Services) are compiled correctly.

namespace SpiralStair_4
{
    /// <summary>
    /// Main entry point for the Spiral Stair Generator AutoCAD command.
    /// </summary>
    public class MainCommand
    {
        /// <summary>
        /// Executes the spiral stair generation process. Registered as an AutoCAD command.
        /// </summary>
        [CommandMethod("GenerateSpiralStair", CommandFlags.Modal)]
        public static void ExecuteGenerateSpiralStair()
        {
            Document acadDoc = Application.DocumentManager.MdiActiveDocument;
            if (acadDoc == null)
            {
                Application.ShowAlertDialog("No active AutoCAD document found. Please open a drawing.");
                return;
            }
            Database acadDb = acadDoc.Database;
            Editor acadEditor = acadDoc.Editor;
            StairData stairData = null; // To hold the data across steps

            try
            {
                acadEditor.WriteMessage("\n--- Launching Spiral Stair Generator ---");

                // --- Step 1: Get User Input ---
                acadEditor.WriteMessage("\nOpening input form...");
                SpiralStairForm inputForm = new SpiralStairForm();
                // Use Application.ShowModalDialog for AutoCAD context
                DialogResult formResult = Application.ShowModalDialog(inputForm);

                if (formResult != DialogResult.OK)
                {
                    acadEditor.WriteMessage("\n*Cancel* Stair generation cancelled by user during input.");
                    return;
                }

                stairData = inputForm.GetStairData();
                if (stairData == null)
                {
                    // This shouldn't happen if DialogResult is OK, but check defensively
                    acadEditor.WriteMessage("\n*Error* Failed to retrieve data from input form. Aborting.");
                    Application.ShowAlertDialog("An internal error occurred retrieving input data.");
                    return;
                }
                acadEditor.WriteMessage("\nInput received successfully.");

                // --- Step 2: Validate and Calculate ---
                acadEditor.WriteMessage("\nValidating parameters and calculating dimensions...");
                ValidationService validationService = new ValidationService();
                validationService.ValidateAndCalculate(stairData); // Populates stairData with calculated values and issues

                acadEditor.WriteMessage($"\nValidation complete. Issues found: {stairData.ValidationIssues.Count}. Midlanding Required: {stairData.RequiresMidlanding}.");

                // --- Step 3: Handle Violations / Midlanding Prompt ---
                // Prompt if there are compliance issues OR if midlanding is required
                if (stairData.ValidationIssues.Count > 0 || stairData.RequiresMidlanding)
                {
                    // Only show non-midlanding issues in the prompt title if they exist
                    bool hasComplianceIssues = stairData.ValidationIssues.Any(s => !s.StartsWith("Midlanding Required", StringComparison.OrdinalIgnoreCase));
                    string promptTitle = stairData.RequiresMidlanding ? "Midlanding Required" : "Input Warnings";
                    if (hasComplianceIssues && stairData.RequiresMidlanding) promptTitle += " & Warnings"; // Use actual ampersand for display text
                    else if (hasComplianceIssues) promptTitle = "Input Violations / Warnings";


                    acadEditor.WriteMessage($"\n{promptTitle}. Displaying prompt...");
                    string suggestions = validationService.GenerateSuggestions(stairData);
                    ViolationPromptForm promptForm = new ViolationPromptForm(stairData.ValidationIssues, suggestions, stairData.RequiresMidlanding, stairData.NumberOfTreads);
                    DialogResult promptDialogResult = Application.ShowModalDialog(promptForm);

                    // Process user action from the prompt form
                    switch (promptForm.UserAction)
                    {
                        case "Cancel":
                            acadEditor.WriteMessage("\n*Cancel* Stair generation cancelled by user at violation prompt.");
                            return; // Exit command

                        case "GoBack":
                            acadEditor.WriteMessage("\nUser chose to go back. Restarting input process...");
                            // Simple way to restart is to call the command again.
                            // Be cautious with deep recursion if this could happen many times.
                            ExecuteGenerateSpiralStair();
                            return; // Exit the current execution path

                        case "Proceed":
                            if (stairData.RequiresMidlanding)
                            {
                                // Retrieve the selected index (0-based)
                                stairData.MidlandingPositionIndex = promptForm.SelectedMidlandingIndex;
                                if (!stairData.MidlandingPositionIndex.HasValue)
                                {
                                    acadEditor.WriteMessage("\n*Error* Midlanding required but position not selected or invalid. Aborting.");
                                    Application.ShowAlertDialog("Midlanding position was not correctly selected.");
                                    return;
                                }
                                acadEditor.WriteMessage($"\nProceeding with midlanding replacing Tread #{stairData.MidlandingPositionIndex.Value + 1} (0-based index: {stairData.MidlandingPositionIndex.Value}).");
                            }
                            else
                            {
                                acadEditor.WriteMessage("\nProceeding with generation despite warnings...");
                            }
                            break; // Continue to geometry generation

                        default:
                            acadEditor.WriteMessage("\n*Error* Unknown action received from violation prompt. Aborting.");
                            Application.ShowAlertDialog("An internal error occurred processing the violation prompt response.");
                            return; // Exit command
                    }
                }
                else
                {
                    acadEditor.WriteMessage("\nValidation successful. No warnings or midlanding requirement.");
                }

                // --- Step 4: Generate Geometry ---
                // Lock the document for geometry creation
                using (DocumentLock docLock = acadDoc.LockDocument())
                {
                    acadEditor.WriteMessage("\nGenerating stair geometry (this may take a moment)...");
                    GeometryGenerator geometryGenerator = new GeometryGenerator(acadDoc);
                    bool geometrySuccess = geometryGenerator.GenerateStairGeometry(stairData);

                    if (!geometrySuccess)
                    {
                        acadEditor.WriteMessage("\n*Error* Geometry generation failed. Operation cancelled and rolled back by transaction.");
                        Application.ShowAlertDialog("Failed to generate the stair geometry. Check command line for details.");
                        // Transaction is aborted within GenerateStairGeometry on failure
                        return; // Exit command
                    }
                    acadEditor.WriteMessage("\nGeometry generation successful.");
                    acadEditor.Regen(); // Regenerate display to show new geometry
                } // DocumentLock released here

                // --- Step 5: Generate Report ---
                acadEditor.WriteMessage("\nGenerating final report...");
                ReportGenerator reportGenerator = new ReportGenerator(acadDoc);
                reportGenerator.GenerateReport(stairData); // Handles MessageBox, Table, CSV prompt

                acadEditor.WriteMessage("\n--- Spiral Stair Generator finished successfully ---");

            }
            catch (System.Exception ex)
            {
                // Catch-all for unexpected errors during the process
                string errorMsg = $"\n--- CRITICAL ERROR in Spiral Stair Generator ---" +
                                  $"\nMessage: {ex.Message}" +
                                  $"\nSource: {ex.Source}" +
                                  $"\nTrace: {ex.StackTrace}";
                acadEditor.WriteMessage(errorMsg);
                System.Diagnostics.Debug.WriteLine($"SpiralStair Error: {ex.ToString()}"); // For debugging
                Application.ShowAlertDialog($"An unexpected error occurred:\n{ex.Message}\n\nSee command line or debug output for details.");
            }
            finally
            {
                // Optional: Any cleanup needed regardless of success/failure
                acadEditor.WriteMessage("\n--- Exiting Spiral Stair Generator command ---");
            }
        }
    }
}