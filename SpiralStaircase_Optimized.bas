Option Explicit

Public gCenterPoleDia As Double
Public gOverallHeight As Double
Public gOutsideDia As Double
Public gRotationDeg As Double
Public gFormSubmitted As Boolean
Public gIsClockwise As Boolean

Sub CreateSpiralStaircase()
    Dim acadDoc As Object
    Dim centerPoleDia As Double
    Dim overallHeight As Double
    Dim outsideDia As Double
    Dim rotationDeg As Double
    Dim riserHeight As Double
    Dim numTreads As Integer
    Dim treadAngle As Double
    Dim i As Integer
    Dim centerPoint(0 To 2) As Double
    Dim walklineRadius As Double
    Dim walklineWidth As Double
    Dim clearWidth As Double
    Dim minRotationDeg As Double
    Dim minCenterPoleDia As Double
    Dim suggestedCenterPoleDia As Double
    Dim suggestedCenterPoleLabel As String
    Dim response As VbMsgBoxResult
    Dim proceed As Boolean
    Dim midlandingIndex As Integer
    Dim midlandingPrompt As String
    Dim currentAngle As Double
    Dim adjustChoice As String
    Dim availableDiameters As Variant
    Dim diameterLabels As Variant
    Dim direction As Integer
    Dim closestDiameter As Double
    Dim minDiff As Double
    Dim diameterList As String

    On Error GoTo ErrorHandler

    Set acadDoc = ThisDrawing.Application.ActiveDocument
    acadDoc.SetVariable "LUNITS", 2
    acadDoc.SetVariable "INSUNITS", 1
    MsgBox "Drawing units set to decimal inches for this script.", vbInformation

    availableDiameters = Array(3, 3.5, 4, 4.5, 5, 5.56, 6, 6.625, 8, 8.625, 10.75, 12.75)
    diameterLabels = Array("3 (tube)", "3.5 (tube)", "4 (tube)", "4.5 (tube)", "5 (tube)", "5.56 (5in. pipe)", _
                          "6 (tube)", "6.625 (6in. pipe)", "8 (tube)", "8.625 (8in. pipe)", "10.75 (10in. pipe)", "12.75 (12in. pipe)")

    Do
        proceed = False
RetryInput:
        gFormSubmitted = False
        UserForm1.show

        If Not gFormSubmitted Then
            MsgBox "Script aborted by user.", vbInformation
            Exit Sub
        End If

        centerPoleDia = gCenterPoleDia
        overallHeight = gOverallHeight
        outsideDia = gOutsideDia
        rotationDeg = gRotationDeg

        ' Input validation with limits
        Dim validDiameter As Boolean
        validDiameter = False
        For i = LBound(availableDiameters) To UBound(availableDiameters)
            If Abs(centerPoleDia - availableDiameters(i)) < 0.001 Then ' Floating-point tolerance
                validDiameter = True
                Exit For
            End If
        Next i
        If Not validDiameter Then
            minDiff = Abs(centerPoleDia - availableDiameters(0))
            closestDiameter = availableDiameters(0)
            For i = 1 To UBound(availableDiameters)
                If Abs(centerPoleDia - availableDiameters(i)) < minDiff Then
                    minDiff = Abs(centerPoleDia - availableDiameters(i))
                    closestDiameter = availableDiameters(i)
                End If
            Next i
            diameterList = ""
            For i = LBound(diameterLabels) To UBound(diameterLabels)
                diameterList = diameterList & diameterLabels(i) & vbCrLf
            Next i
            MsgBox "Center pole diameter " & centerPoleDia & " is not available." & vbCrLf & vbCrLf & _
                   "Using closest size: " & closestDiameter & " (" & diameterLabels(i - 1) & ")" & vbCrLf & vbCrLf & _
                   "Available diameters:" & vbCrLf & diameterList, vbInformation
            centerPoleDia = closestDiameter
        End If
        If overallHeight < 20 Or overallHeight > 300 Then
            MsgBox "Overall height must be between 20 and 300 inches.", vbExclamation
            GoTo RetryInput
        End If
        If outsideDia < centerPoleDia + 10 Or outsideDia > 120 Then
            MsgBox "Outside diameter must be between " & centerPoleDia + 10 & " and 120 inches.", vbExclamation
            GoTo RetryInput
        End If
        If rotationDeg < 90 Or rotationDeg > 1080 Then
            MsgBox "Total rotation must be between 90 and 1080 degrees.", vbExclamation
            GoTo RetryInput
        End If

        ' New code compliance checks
        numTreads = Ceiling(overallHeight / 9.5)
        walklineRadius = centerPoleDia / 2 + 12
        If walklineRadius > 24.5 Then
            response = MsgBox("Walkline Radius Violation:" & vbCrLf & vbCrLf & _
                              "Current walkline radius is " & Format(walklineRadius, "0.00") & " inches, exceeding the maximum of 24.5 inches." & vbCrLf & vbCrLf & _
                              "Suggestion: Reduce center pole diameter to " & Format((24.5 - 12) * 2, "0.00") & " inches or less." & vbCrLf & vbCrLf & _
                              "OK to proceed, Cancel to adjust.", vbOKCancel + vbExclamation, "Code Compliance Warning")
            If response = vbCancel Then GoTo RetryInput
        End If

        clearWidth = (outsideDia / 2) - (centerPoleDia / 2) - 1.5
        If clearWidth < 26 Then
            response = MsgBox("Clear Width Violation:" & vbCrLf & vbCrLf & _
                              "Current clear width is " & Format(clearWidth, "0.00") & " inches, less than the minimum of 26 inches." & vbCrLf & vbCrLf & _
                              "Suggestion: Increase outside diameter to " & Format((26 + centerPoleDia / 2 + 1.5) * 2, "0.00") & " inches or more." & vbCrLf & vbCrLf & _
                              "OK to proceed, Cancel to adjust.", vbOKCancel + vbExclamation, "Code Compliance Warning")
            If response = vbCancel Then GoTo RetryInput
        End If

        riserHeight = overallHeight / numTreads
        treadAngle = rotationDeg / numTreads ' Initial tread angle for walkline check
        midlandingIndex = -1 ' Default: no midlanding

        ' Step 1: Validate walkline width
        walklineWidth = walklineRadius * (Abs(treadAngle) * 3.14159 / 180)

        If walklineWidth < 6.75 Then
            minRotationDeg = 90 + (6.75 / walklineRadius) * (180 / 3.14159) * (numTreads - 1)
            If minRotationDeg < rotationDeg Then minRotationDeg = rotationDeg
            minCenterPoleDia = (6.75 * 180 / 3.14159 / Abs(treadAngle) - 12) * 2

            suggestedCenterPoleDia = 12.75
            suggestedCenterPoleLabel = "12.75 (12in. pipe)"
            For i = LBound(availableDiameters) To UBound(availableDiameters)
                If availableDiameters(i) >= minCenterPoleDia Then
                    suggestedCenterPoleDia = availableDiameters(i)
                    suggestedCenterPoleLabel = diameterLabels(i)
                    Exit For
                End If
            Next i
            response = MsgBox("Inputs causing violation:" & vbCrLf & _
                              "Center Pole Diameter: " & centerPoleDia & " inches" & vbCrLf & _
                              "Overall Height: " & overallHeight & " inches" & vbCrLf & _
                              "Outside Diameter: " & outsideDia & " inches" & vbCrLf & _
                              "Rotation Degree: " & rotationDeg & "°" & vbCrLf & vbCrLf & _
                              "Tread width at walkline (12"" from center pole edge) is " & Format(walklineWidth, "0.00") & _
                              " inches, which is less than the minimum 6.75 inches required." & vbCrLf & vbCrLf & _
                              "Choose an option:" & vbCrLf & _
                              "- Retry: Auto-adjust to meet code" & vbCrLf & _
                              "- Ignore: Proceed anyway" & vbCrLf & _
                              "- Abort: Cancel script", _
                              vbAbortRetryIgnore + vbExclamation, "International Building Code Violation")
            If response = vbRetry Then
                adjustChoice = InputBox("Choose adjustment to meet code:" & vbCrLf & vbCrLf & _
                                        "1. Increase Center Pole Diameter to " & suggestedCenterPoleLabel & " (next available size)" & vbCrLf & vbCrLf & _
                                        "2. Increase Rotation Degree to " & Format(minRotationDeg, "0") & "°" & vbCrLf & vbCrLf & _
                                        "Enter 1 or 2:", "Auto-Adjust Option")
                If adjustChoice = "" Then
                    MsgBox "Script aborted by user.", vbInformation
                    Exit Sub
                ElseIf adjustChoice = "1" Then
                    centerPoleDia = suggestedCenterPoleDia
                    walklineRadius = centerPoleDia / 2 + 12
                    treadAngle = rotationDeg / numTreads
                    walklineWidth = walklineRadius * (Abs(treadAngle) * 3.14159 / 180)
                    MsgBox "Adjusted Center Pole Diameter to " & suggestedCenterPoleLabel & " to meet code." & vbCrLf & _
                           "Walkline width is now " & Format(walklineWidth, "0.00") & " inches.", vbInformation
                ElseIf adjustChoice = "2" Then
                    rotationDeg = minRotationDeg
                    treadAngle = rotationDeg / numTreads
                    walklineWidth = walklineRadius * (Abs(treadAngle) * 3.14159 / 180)
                    MsgBox "Adjusted Rotation Degree to " & Format(minRotationDeg, "0") & "° to meet code." & vbCrLf & _
                           "Walkline width is now " & Format(walklineWidth, "0.00") & " inches.", vbInformation
                Else
                    MsgBox "Invalid choice. Script aborted.", vbExclamation
                    Exit Sub
                End If
            ElseIf response = vbIgnore Then
                ' Proceed without adjustment
            Else
                MsgBox "Script aborted by user.", vbInformation
                Exit Sub
            End If
        End If

        ' Step 2: Midlanding check after walkline is finalized
        If overallHeight > 147 Then
            response = MsgBox("Overall height exceeds 147 inches. A midlanding is required per R311.7.3 of the International Building Code (2020)." & vbCrLf & vbCrLf & _
                              "Current walkline width: " & Format(walklineWidth, "0.00") & " inches" & vbCrLf & vbCrLf & _
                              "Choose an option:" & vbCrLf & vbCrLf & _
                              "- Retry: Add a midlanding" & vbCrLf & vbCrLf & _
                              "- Ignore: Proceed without midlanding" & vbCrLf & vbCrLf & _
                              "- Abort: Cancel script", _
                              vbAbortRetryIgnore + vbExclamation, "Midlanding Required")
            If response = vbRetry Then
                midlandingPrompt = InputBox("Enter the tread number (1 to " & numTreads & ") for the midlanding:", _
                                            "Midlanding Position", Round(numTreads / 2))
                If midlandingPrompt = "" Then
                    MsgBox "Script aborted by user.", vbInformation
                    Exit Sub
                ElseIf Not IsNumeric(midlandingPrompt) Then
                    MsgBox "Please enter a valid tread number.", vbExclamation
                    Exit Sub
                Else
                    midlandingIndex = CInt(midlandingPrompt) - 1
                    If midlandingIndex < 0 Or midlandingIndex >= numTreads Then
                        MsgBox "Tread number must be between 1 and " & numTreads & ".", vbExclamation
                        Exit Sub
                    End If
                End If
                treadAngle = (rotationDeg - 90) / (numTreads - 1) ' Adjust for drawing only
            ElseIf response = vbIgnore Then
                midlandingIndex = -1
                treadAngle = rotationDeg / numTreads
            Else
                MsgBox "Script aborted by user.", vbInformation
                Exit Sub
            End If
        End If

        proceed = True ' Exit loop after all checks
    Loop Until proceed

    direction = IIf(gIsClockwise, 1, -1)
    currentAngle = 0

    centerPoint(0) = 0
    centerPoint(1) = 0
    centerPoint(2) = 0
    Dim pole As Object
    Set pole = acadDoc.ModelSpace.AddCylinder(centerPoint, centerPoleDia / 2, overallHeight)
    pole.color = 251
    Dim poleMove(0 To 3, 0 To 3) As Double
    poleMove(0, 0) = 1: poleMove(1, 1) = 1: poleMove(2, 2) = 1: poleMove(3, 3) = 1
    poleMove(2, 3) = overallHeight / 2
    pole.TransformBy poleMove

    Dim numRegularTreads As Integer
    If midlandingIndex >= 0 Then
        numRegularTreads = numTreads - 1 ' Exclude landing
        treadAngle = direction * Abs((rotationDeg - 90) / (numRegularTreads - 1))
    Else
        numRegularTreads = numTreads - 1 ' Exclude landing
        treadAngle = direction * Abs(rotationDeg / numRegularTreads)
    End If

    For i = 0 To numTreads - 1
        Dim treadHeight As Double
        treadHeight = riserHeight * (i + 1) - 0.25
        If treadHeight + 0.25 > overallHeight Then treadHeight = overallHeight - 0.25

        If i = numTreads - 1 Then
            Dim landingWidth As Double
            Dim landingLength As Double
            Dim startPt(0 To 2) As Double
            Dim endPt(0 To 2) As Double
            landingWidth = 50
            landingLength = outsideDia / 2
            startPt(0) = 0
            startPt(1) = 0
            startPt(2) = treadHeight

            Dim landingLines(0 To 3) As Object
            On Error Resume Next
            endPt(0) = landingLength * Cos(rotationDeg * 3.14159 / 180 * direction)
            endPt(1) = landingLength * Sin(rotationDeg * 3.14159 / 180 * direction)
            endPt(2) = treadHeight
            Set landingLines(0) = acadDoc.ModelSpace.AddLine(startPt, endPt)
            If Err.Number <> 0 Then MsgBox "Error on Line 0: " & Err.Description, vbCritical: Exit Sub

            startPt(0) = endPt(0)
            startPt(1) = endPt(1)
            startPt(2) = treadHeight
            endPt(0) = startPt(0) + landingWidth * Cos(rotationDeg * 3.14159 / 180 * direction + (3.14159 / 2) * direction)
            endPt(1) = startPt(1) + landingWidth * Sin(rotationDeg * 3.14159 / 180 * direction + (3.14159 / 2) * direction)
            endPt(2) = treadHeight
            Set landingLines(1) = acadDoc.ModelSpace.AddLine(startPt, endPt)
            If Err.Number <> 0 Then MsgBox "Error on Line 1: " & Err.Description, vbCritical: Exit Sub

            startPt(0) = endPt(0)
            startPt(1) = endPt(1)
            startPt(2) = treadHeight
            endPt(0) = startPt(0) - landingLength * Cos(rotationDeg * 3.14159 / 180 * direction)
            endPt(1) = startPt(1) - landingLength * Sin(rotationDeg * 3.14159 / 180 * direction)
            endPt(2) = treadHeight
            Set landingLines(2) = acadDoc.ModelSpace.AddLine(startPt, endPt)
            If Err.Number <> 0 Then MsgBox "Error on Line 2: " & Err.Description, vbCritical: Exit Sub

            startPt(0) = endPt(0)
            startPt(1) = endPt(1)
            startPt(2) = treadHeight
            endPt(0) = startPt(0) - landingWidth * Cos(rotationDeg * 3.14159 / 180 * direction + (3.14159 / 2) * direction)
            endPt(1) = startPt(1) - landingWidth * Sin(rotationDeg * 3.14159 / 180 * direction + (3.14159 / 2) * direction)
            endPt(2) = treadHeight
            Set landingLines(3) = acadDoc.ModelSpace.AddLine(startPt, endPt)
            If Err.Number <> 0 Then MsgBox "Error on Line 3: " & Err.Description, vbCritical: Exit Sub
            On Error GoTo ErrorHandler

            Dim landingRegion As Object
            Dim landingRegions As Variant
            landingRegions = acadDoc.ModelSpace.AddRegion(landingLines)
            If UBound(landingRegions) >= 0 Then
                Set landingRegion = landingRegions(0)
            Else
                MsgBox "Failed to create region for landing at Z=" & treadHeight, vbCritical
                Exit Sub
            End If

            Dim landing As Object
            Set landing = acadDoc.ModelSpace.AddExtrudedSolid(landingRegion, 0.25, 0)
            landing.color = 3

            landingLines(0).Delete
            landingLines(1).Delete
            landingLines(2).Delete
            landingLines(3).Delete
            landingRegion.Delete
        Else
            Dim endAngle As Double
            Dim innerRadius As Double
            Dim outerRadius As Double

            If i = midlandingIndex Then
                endAngle = currentAngle + (90 * 3.14159 / 180) * direction
                innerRadius = centerPoleDia / 2
                outerRadius = outsideDia / 2
            Else
                endAngle = currentAngle + (treadAngle * 3.14159 / 180)
                innerRadius = centerPoleDia / 2
                outerRadius = outsideDia / 2
            End If

            Dim startPoint(0 To 2) As Double
            Dim endPoint(0 To 2) As Double
            Dim arcCenter(0 To 2) As Double
            Dim entities(0 To 2) As Object

            arcCenter(0) = 0: arcCenter(1) = 0: arcCenter(2) = treadHeight

            startPoint(0) = 0
            startPoint(1) = 0
            startPoint(2) = treadHeight
            endPoint(0) = outerRadius * Cos(currentAngle)
            endPoint(1) = outerRadius * Sin(currentAngle)
            endPoint(2) = treadHeight
            Set entities(0) = acadDoc.ModelSpace.AddLine(startPoint, endPoint)

            If direction = 1 Then
                Set entities(1) = acadDoc.ModelSpace.AddArc(arcCenter, outerRadius, currentAngle, endAngle)
            Else
                Set entities(1) = acadDoc.ModelSpace.AddArc(arcCenter, outerRadius, endAngle, currentAngle)
            End If

            startPoint(0) = outerRadius * Cos(endAngle)
            startPoint(1) = outerRadius * Sin(endAngle)
            startPoint(2) = treadHeight
            endPoint(0) = 0
            endPoint(1) = 0
            endPoint(2) = treadHeight
            Set entities(2) = acadDoc.ModelSpace.AddLine(startPoint, endPoint)

            Dim regionObj As Object
            Dim regions As Variant
            regions = acadDoc.ModelSpace.AddRegion(entities)
            If UBound(regions) >= 0 Then
                Set regionObj = regions(0)
            Else
                MsgBox "Failed to create region for tread " & (i + 1) & " at Z=" & treadHeight, vbCritical
                Exit Sub
            End If

            Dim tread As Object
            Set tread = acadDoc.ModelSpace.AddExtrudedSolid(regionObj, 0.25, 0)
            If i = midlandingIndex Then
                tread.color = 1
            Else
                tread.color = 251
            End If

            entities(0).Delete
            entities(1).Delete
            entities(2).Delete
            regionObj.Delete

            currentAngle = endAngle
        End If
    Next i

    acadDoc.Regen acAllViewports
    acadDoc.Application.ZoomExtents

    Dim successMsg As String
    successMsg = "Spiral staircase created successfully!" & vbCrLf & vbCrLf
    successMsg = successMsg & "Stair Dimensions Outline:" & vbCrLf
    successMsg = successMsg & "- Center Pole Diameter: " & Format(centerPoleDia, "0.00") & " inches" & vbCrLf
    successMsg = successMsg & "- Overall Height: " & Format(overallHeight, "0.00") & " inches" & vbCrLf
    successMsg = successMsg & "- Outside Diameter: " & Format(outsideDia, "0.00") & " inches" & vbCrLf
    successMsg = successMsg & "- Total Rotation: " & Format(rotationDeg, "0") & "°" & vbCrLf
    successMsg = successMsg & "- Number of Treads: " & numTreads & vbCrLf
    successMsg = successMsg & "- Riser Height (top-to-top): " & Format(riserHeight, "0.00") & " inches" & vbCrLf
    successMsg = successMsg & "- Tread Angle: " & Format(treadAngle, "0.00") & "°" & vbCrLf
    successMsg = successMsg & "- Walkline Width: " & Format(walklineWidth, "0.00") & " inches" & vbCrLf
    successMsg = successMsg & "- Clear Width: " & Format(clearWidth, "0.00") & " inches" & vbCrLf
    successMsg = successMsg & IIf(midlandingIndex >= 0, "- Midlanding at Tread " & midlandingIndex + 1 & " (Z=" & Format(riserHeight * (midlandingIndex + 1), "0.00") & " inches)", "- No Midlanding") & vbCrLf & vbCrLf
    successMsg = successMsg & "So, whether you eat or drink, or whatever you do, do all to the glory of God.  1st Corinthians 10:31"

    MsgBox successMsg, vbInformation

    Dim textHeight As Double
    Dim tableX As Double
    Dim tableY As Double
    Dim textPos(0 To 2) As Double
    Dim textObj As Object

    textHeight = 2
    tableX = 100
    tableY = 50

    textPos(0) = tableX: textPos(1) = tableY: textPos(2) = 0
    Set textObj = acadDoc.ModelSpace.AddText("Center Pole Diameter:", textPos, textHeight)
    textPos(0) = tableX + 40
    Set textObj = acadDoc.ModelSpace.AddText(Format(centerPoleDia, "0.00") & " inches", textPos, textHeight)
    tableY = tableY - 8

    textPos(0) = tableX: textPos(1) = tableY: textPos(2) = 0
    Set textObj = acadDoc.ModelSpace.AddText("Overall Height:", textPos, textHeight)
    textPos(0) = tableX + 40
    Set textObj = acadDoc.ModelSpace.AddText(Format(overallHeight, "0.00") & " inches", textPos, textHeight)
    tableY = tableY - 8

    textPos(0) = tableX: textPos(1) = tableY: textPos(2) = 0
    Set textObj = acadDoc.ModelSpace.AddText("Outside Diameter:", textPos, textHeight)
    textPos(0) = tableX + 40
    Set textObj = acadDoc.ModelSpace.AddText(Format(outsideDia, "0.00") & " inches", textPos, textHeight)
    tableY = tableY - 8

    textPos(0) = tableX: textPos(1) = tableY: textPos(2) = 0
    Set textObj = acadDoc.ModelSpace.AddText("Total Rotation:", textPos, textHeight)
    textPos(0) = tableX + 40
    Set textObj = acadDoc.ModelSpace.AddText(Format(rotationDeg, "0") & "°", textPos, textHeight)
    tableY = tableY - 8

    textPos(0) = tableX: textPos(1) = tableY: textPos(2) = 0
    Set textObj = acadDoc.ModelSpace.AddText("Number of Treads:", textPos, textHeight)
    textPos(0) = tableX + 40
    Set textObj = acadDoc.ModelSpace.AddText(numTreads, textPos, textHeight)
    tableY = tableY - 8

    textPos(0) = tableX: textPos(1) = tableY: textPos(2) = 0
    Set textObj = acadDoc.ModelSpace.AddText("Riser Height (top-to-top):", textPos, textHeight)
    textPos(0) = tableX + 40
    Set textObj = acadDoc.ModelSpace.AddText(Format(riserHeight, "0.00") & " inches", textPos, textHeight)
    tableY = tableY - 8

    textPos(0) = tableX: textPos(1) = tableY: textPos(2) = 0
    Set textObj = acadDoc.ModelSpace.AddText("Tread Angle:", textPos, textHeight)
    textPos(0) = tableX + 40
    Set textObj = acadDoc.ModelSpace.AddText(Format(treadAngle, "0.00") & "°", textPos, textHeight)
    tableY = tableY - 8

    textPos(0) = tableX: textPos(1) = tableY: textPos(2) = 0
    Set textObj = acadDoc.ModelSpace.AddText("Walkline Width:", textPos, textHeight)
    textPos(0) = tableX + 40
    Set textObj = acadDoc.ModelSpace.AddText(Format(walklineWidth, "0.00") & " inches", textPos, textHeight)
    tableY = tableY - 8

    textPos(0) = tableX: textPos(1) = tableY: textPos(2) = 0
    Set textObj = acadDoc.ModelSpace.AddText("Clear Width:", textPos, textHeight)
    textPos(0) = tableX + 40
    Set textObj = acadDoc.ModelSpace.AddText(Format(clearWidth, "0.00") & " inches", textPos, textHeight)
    tableY = tableY - 8

    textPos(0) = tableX: textPos(1) = tableY: textPos(2) = 0
    Set textObj = acadDoc.ModelSpace.AddText("Midlanding:", textPos, textHeight)
    textPos(0) = tableX + 40
    If midlandingIndex >= 0 Then
        Set textObj = acadDoc.ModelSpace.AddText("Tread " & midlandingIndex + 1 & " (Z=" & Format(riserHeight * (midlandingIndex + 1), "0.00") & " inches)", textPos, textHeight)
    Else
        Set textObj = acadDoc.ModelSpace.AddText("No Midlanding", textPos, textHeight)
    End If
    tableY = tableY - 8

    textPos(0) = tableX: textPos(1) = tableY: textPos(2) = 0
    Set textObj = acadDoc.ModelSpace.AddText("Rotation Direction:", textPos, textHeight)
    textPos(0) = tableX + 40
    Set textObj = acadDoc.ModelSpace.AddText(IIf(gIsClockwise, "Right Hand - Up", "Left Hand - Up"), textPos, textHeight)

    acadDoc.Regen acAllViewports
    Exit Sub

ErrorHandler:
    If Err.Number <> 0 Then
        MsgBox "Error occurred: " & Err.Description & " (Error Number: " & Err.Number & ")", vbCritical
    End If
    Exit Sub
End Sub

Function Ceiling(value As Double) As Integer
    Ceiling = -Int(-value)
End Function