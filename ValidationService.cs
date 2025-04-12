using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiralStair_4
{
    /// <summary>
    /// Performs calculations and IRC code validation for the spiral stair data.
    /// </summary>
    public class ValidationService
    {
        // Tolerance for floating-point comparisons
        private const double Tolerance = 0.0001;

        /// <summary>
        /// Calculates derived parameters and validates the stair data against code requirements.
        /// </summary>
        /// <param name="stairData">The StairData object to validate and populate.</param>
        /// <returns>True if the data is code compliant (excluding midlanding requirement), false otherwise.</returns>
        public bool ValidateAndCalculate(StairData stairData)
        {
            if (stairData == null)
            {
                throw new ArgumentNullException(nameof(stairData), "StairData cannot be null.");
            }

            stairData.ValidationIssues.Clear();
            stairData.IsCodeCompliant = true; // Assume true initially
            stairData.RequiresMidlanding = false; // Reset flags
            // Reset calculated values that depend on inputs
            stairData.Headroom = null;

            // 1. Calculate Basic Parameters
            CalculateRiserHeightAndCount(stairData);
            CalculateTreadAngle(stairData);
            CalculateClearWidth(stairData);
            CalculateWalklineDepth(stairData);
            CalculateTopLandingDimensions(stairData); // Calculate top landing width

            // 2. Check for Midlanding Requirement FIRST
            // This is a requirement, not strictly a violation of the *other* parameters yet.
            CheckMidlandingRequirement(stairData);

            // 3. Check IRC Violations (These affect IsCodeCompliant flag)
            CheckRiserHeightViolation(stairData);
            CheckClearWidthViolation(stairData);
            CheckWalklineDepthViolation(stairData);
            CalculateAndCheckHeadroom(stairData); // Headroom depends on other calculated values

            // Final compliance status depends only on violations, not the midlanding requirement itself
            // The IsCodeCompliant flag is already being set to false within the check methods.

            return stairData.IsCodeCompliant;
        }

        // --- Calculation Helpers ---

        private void CalculateRiserHeightAndCount(StairData stairData)
        {
            if (stairData.OverallHeight <= 0)
            {
                stairData.NumberOfRisers = 0;
                stairData.RiserHeight = 0;
                stairData.NumberOfTreads = 0;
                return; // Cannot calculate further
            }

            // Start with a reasonable guess, slightly less than max height
            double idealRiserHeight = StairData.MaxRiserHeight - 0.5;
            int numRisersEstimate = (int)Math.Ceiling(stairData.OverallHeight / idealRiserHeight);

            if (numRisersEstimate <= 0) numRisersEstimate = 1; // Need at least one riser

            double calculatedRiserHeight = stairData.OverallHeight / numRisersEstimate;

            // Adjust number of risers if calculated height exceeds max
            // Loop limiter prevents infinite loop in edge cases
            int loopLimiter = 0;
            while (calculatedRiserHeight > (StairData.MaxRiserHeight + Tolerance) && loopLimiter < 100)
            {
                numRisersEstimate++;
                calculatedRiserHeight = stairData.OverallHeight / numRisersEstimate;
                loopLimiter++;
            }

            stairData.NumberOfRisers = numRisersEstimate;
            stairData.RiserHeight = calculatedRiserHeight;
            // Number of treads is typically one less than risers, but ensure non-negative
            stairData.NumberOfTreads = Math.Max(0, stairData.NumberOfRisers - 1);
        }

        private void CalculateTreadAngle(StairData stairData)
        {
            // Avoid division by zero if there are no treads or zero rotation
            if (stairData.NumberOfTreads > 0 && Math.Abs(stairData.TotalRotation) > Tolerance)
            {
                stairData.TreadAngle = stairData.TotalRotation / stairData.NumberOfTreads;
                stairData.TreadAngleRadians = stairData.TreadAngle * (Math.PI / 180.0);
            }
            else
            {
                stairData.TreadAngle = 0;
                stairData.TreadAngleRadians = 0;
            }
        }

        private void CalculateClearWidth(StairData stairData)
        {
            double outerRadius = stairData.OutsideDiameter / 2.0;
            double poleRadius = stairData.CenterPoleDiameter / 2.0;
            // Per IRC R311.7.10.1: Clear width measured below handrail height
            // Handrail projection allowance (typical, might need refinement)
            double handrailClearance = 1.5; // Typical handrail radius/projection allowance

            if (outerRadius > poleRadius + handrailClearance)
            {
                // Clear width is space between pole and inner edge of required width boundary (often near handrail)
                stairData.ClearWidth = outerRadius - poleRadius - handrailClearance;
            }
            else
            {
                stairData.ClearWidth = 0; // Not physically possible or zero width
            }
        }

        private void CalculateWalklineDepth(StairData stairData)
        {
            // Walkline defined 12" from the narrow edge (pole surface) per IRC R311.7.10.2
            stairData.WalklineRadius = (stairData.CenterPoleDiameter / 2.0) + StairData.WalklineOffsetFromPole;

            // Ensure walkline radius doesn't exceed the outer radius (physically impossible)
            double outerRadius = stairData.OutsideDiameter / 2.0;
            if (stairData.WalklineRadius >= outerRadius)
            {
                 // Walkline is at or beyond the outer edge, depth is effectively the full tread depth at that point.
                 // This scenario usually indicates a very small diameter stair where the walkline concept breaks down
                 // or overlaps with clear width checks. We'll calculate based on outer radius for consistency,
                 // but the ClearWidth check is more critical here.
                 stairData.TreadDepthAtWalkline = outerRadius * Math.Abs(stairData.TreadAngleRadians);
                 // Optionally add a warning if walkline is outside?
            }
            else if (stairData.WalklineRadius > 0 && Math.Abs(stairData.TreadAngleRadians) > Tolerance)
            {
                // Calculate arc length at the walkline radius
                stairData.TreadDepthAtWalkline = stairData.WalklineRadius * Math.Abs(stairData.TreadAngleRadians);
            }
            else
            {
                stairData.TreadDepthAtWalkline = 0; // No angle or invalid radius
            }
        }

        private void CalculateAndCheckHeadroom(StairData stairData)
        {
            // Headroom calculation requires valid tread angle and riser height
            if (Math.Abs(stairData.TreadAngle) < Tolerance || stairData.RiserHeight <= 0 || stairData.NumberOfTreads <= 0)
            {
                stairData.Headroom = null; // Cannot calculate
                // Don't add a violation if we can't calculate it, other violations likely exist.
                return;
            }

            // Calculate treads needed for a full 360-degree rotation
            double treadsPerRevolution = 360.0 / Math.Abs(stairData.TreadAngle);

            // Calculate the vertical rise achieved in one full revolution
            double verticalRisePerRevolution = treadsPerRevolution * stairData.RiserHeight;

            // Headroom is the vertical rise minus the thickness of the tread/structure above
            stairData.Headroom = verticalRisePerRevolution - stairData.TreadThickness;

            // Check against minimum headroom requirement (IRC R311.7.2)
            if (stairData.Headroom < (StairData.MinHeadroom - Tolerance))
            {
                stairData.IsCodeCompliant = false;
                stairData.ValidationIssues.Add($"Headroom violation: Calculated {stairData.Headroom.Value:F2}\" (Min required: {StairData.MinHeadroom:F2}\"). Suggest increasing Total Rotation or Outside Diameter, or decreasing Tread Thickness.");
            }
        }

        private void CalculateTopLandingDimensions(StairData stairData)
        {
             // Top landing width is essentially the clear space from pole to outer edge at the top
             double outerRadius = stairData.OutsideDiameter / 2.0;
             double poleRadius = stairData.CenterPoleDiameter / 2.0;
             if (outerRadius > poleRadius)
             {
                 stairData.TopLandingWidth = outerRadius - poleRadius;
             }
             else
             {
                 stairData.TopLandingWidth = 0; // Invalid input diameters
             }
             // TopLandingLength and TopLandingThickness have defaults in StairData for now.
        }


        // --- Violation Check Helpers ---

        private void CheckMidlandingRequirement(StairData stairData)
        {
            // IRC R311.7.3: Max 12 feet (144 inches) vertical rise between floor levels or landings.
            // Note: Some interpretations use 147" (12' 3"). Using constant from StairData.
            if (stairData.OverallHeight > (StairData.MaxVerticalRiseNoLanding + Tolerance))
            {
                stairData.RequiresMidlanding = true;
                // This is a requirement, add to issues but doesn't make IsCodeCompliant false *yet*
                // The user must choose to proceed (and select position) or go back.
                stairData.ValidationIssues.Add($"Midlanding Required: Overall height ({stairData.OverallHeight:F2}\") exceeds max vertical rise ({StairData.MaxVerticalRiseNoLanding:F2}\") allowed between landings/floors (IRC R311.7.3).");
            }
        }

        private void CheckRiserHeightViolation(StairData stairData)
        {
            // IRC R311.7.10.1: Max riser height 9 1/2 inches
            if (stairData.RiserHeight > (StairData.MaxRiserHeight + Tolerance))
            {
                stairData.IsCodeCompliant = false;
                stairData.ValidationIssues.Add($"Riser height violation: Calculated {stairData.RiserHeight:F3}\" (Max allowed: {StairData.MaxRiserHeight:F2}\" per IRC R311.7.10.1). Suggest increasing Overall Height slightly or check input.");
            }
            // Also check for excessively small risers (though less common issue)
            // IRC doesn't specify minimum for spiral, but general stair min is 4" (R311.7.5.1) - apply cautiously
            if (stairData.RiserHeight < (4.0 - Tolerance) && stairData.RiserHeight > Tolerance) // Avoid check if 0
            {
                 stairData.IsCodeCompliant = false; // Technically non-compliant if applying general rule
                 stairData.ValidationIssues.Add($"Riser height warning: Calculated {stairData.RiserHeight:F3}\" is less than the general minimum of 4\". Consider adjusting Overall Height.");
            }
        }

        private void CheckClearWidthViolation(StairData stairData)
        {
            // IRC R311.7.10.1: Min clear width 26 inches
            if (stairData.ClearWidth < (StairData.MinClearWidth - Tolerance))
            {
                stairData.IsCodeCompliant = false;
                stairData.ValidationIssues.Add($"Clear width violation: Calculated {stairData.ClearWidth:F3}\" (Min required: {StairData.MinClearWidth:F2}\" per IRC R311.7.10.1). Suggest increasing Outside Diameter or decreasing Center Pole Diameter.");
            }
        }

        private void CheckWalklineDepthViolation(StairData stairData)
        {
            // IRC R311.7.10.2: Min tread depth 6 3/4 inches at the 12" walkline
            if (stairData.TreadDepthAtWalkline < (StairData.MinTreadDepthWalkline - Tolerance))
            {
                stairData.IsCodeCompliant = false;
                stairData.ValidationIssues.Add($"Walkline depth violation: Calculated {stairData.TreadDepthAtWalkline:F3}\" (Min required: {StairData.MinTreadDepthWalkline:F2}\" per IRC R311.7.10.2). Suggest increasing Total Rotation or Center Pole Diameter.");
            }
        }

        // --- Suggestion Generation ---

        /// <summary>
        /// Generates user-friendly suggestions based on the detected validation issues.
        /// </summary>
        /// <param name="stairData">The validated StairData object.</param>
        /// <returns>A string containing formatted suggestions.</returns>
        public string GenerateSuggestions(StairData stairData)
        {
            if (stairData?.ValidationIssues == null || !stairData.ValidationIssues.Any())
            {
                return "No validation issues found.";
            }

            var suggestions = new StringBuilder("Suggestions:\n");
            bool suggestionAdded = false;

            if (stairData.ValidationIssues.Any(s => s.Contains("Clear width violation")))
            {
                suggestions.AppendLine("- To increase Clear Width: Increase Outside Diameter or select a smaller standard Center Pole Diameter.");
                suggestionAdded = true;
            }
            if (stairData.ValidationIssues.Any(s => s.Contains("Walkline depth violation")))
            {
                suggestions.AppendLine("- To increase Walkline Depth: Increase Total Rotation or select a larger standard Center Pole Diameter.");
                suggestionAdded = true;
            }
            if (stairData.ValidationIssues.Any(s => s.Contains("Headroom violation")))
            {
                suggestions.AppendLine("- To increase Headroom: Increase Total Rotation or Outside Diameter.");
                suggestionAdded = true;
            }
            if (stairData.ValidationIssues.Any(s => s.Contains("Riser height violation")))
            {
                suggestions.AppendLine("- To fix Riser Height: Adjust Overall Height slightly (often a small change is enough).");
                suggestionAdded = true;
            }
             if (stairData.ValidationIssues.Any(s => s.Contains("Riser height warning"))) // For small risers
            {
                suggestions.AppendLine("- To increase small Riser Height: Decrease Overall Height or check inputs.");
                suggestionAdded = true;
            }

            if (!suggestionAdded && !stairData.RequiresMidlanding) // Only show if no other suggestions and no midlanding needed
            {
                 return "No specific suggestions for the detected warnings.";
            }
             else if (!suggestionAdded && stairData.RequiresMidlanding)
            {
                 // If only midlanding is required, no other suggestions needed here.
                 return string.Empty; // Return empty, the form handles the midlanding prompt explicitly.
            }


            return suggestions.ToString();
        }
    }
}