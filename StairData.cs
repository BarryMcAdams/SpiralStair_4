using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiralStair_4
{
    /// <summary>
    /// Holds all input and calculated data for the spiral stair.
    /// </summary>
    public class StairData
    {
        // --- User Inputs ---
        public double CenterPoleDiameter { get; set; } // Inches
        public double OverallHeight { get; set; }      // Finished Floor to Finished Floor (Inches)
        public double OutsideDiameter { get; set; }    // Inches
        public double TotalRotation { get; set; }      // Degrees
        public string Handedness { get; set; }         // "Clockwise" or "CounterClockwise" (Future use)
        public int? MidlandingPositionIndex { get; set; } // Nullable, 0-based index of tread replaced by midlanding

        // --- Calculated Values ---
        public double RiserHeight { get; set; }
        public int NumberOfRisers { get; set; }
        public int NumberOfTreads { get; set; }
        public double TreadAngle { get; set; }         // Degrees
        public double TreadAngleRadians { get; set; }  // Radians
        public double ClearWidth { get; set; }
        public double WalklineRadius { get; set; }
        public double TreadDepthAtWalkline { get; set; }
        public double? Headroom { get; set; }          // Nullable, calculated if possible
        public bool RequiresMidlanding { get; set; }
        public double TreadThickness { get; set; } = 1.5; // Default, could be user input later
        public double TopLandingWidth { get; set; }    // Calculated based on TreadWidth at outer edge
        public double TopLandingLength { get; set; } = 50.0; // Fixed for now
        public double TopLandingThickness { get; set; } = 1.5; // Matches tread thickness

        // --- Validation Status ---
        public List<string> ValidationIssues { get; private set; } // Stores violation messages
        public bool IsCodeCompliant { get; set; }       // Overall compliance status

        // --- Constants ---
        // Note: Consider making these configurable or part of a settings class later.
        public static readonly List<double> StandardPoleSizes = new List<double> { 3.0, 3.5, 4.0, 4.5, 5.0, 5.563, 6.0, 6.625, 8.0, 8.625, 10.75, 12.75 };
        public const double MinClearWidth = 26.0;
        // public const double MaxWalklineRadius = 25.5; // Note: Re-verify IRC source. ClearWidth check is primary. Using ClearWidth instead.
        public const double MinTreadDepthWalkline = 6.75;
        public const double MaxRiserHeight = 9.5;
        public const double MinHeadroom = 78.0;
        public const double MaxVerticalRiseNoLanding = 147.0;
        public const double WalklineOffsetFromPole = 12.0; // Standard distance from pole surface for walkline calculation

        // --- Constructor ---
        public StairData()
        {
            ValidationIssues = new List<string>();
            IsCodeCompliant = true; // Assume compliant until proven otherwise
            RequiresMidlanding = false;
            MidlandingPositionIndex = null;
            Handedness = "Clockwise"; // Default, though not used in generation logic yet
        }
    }
}