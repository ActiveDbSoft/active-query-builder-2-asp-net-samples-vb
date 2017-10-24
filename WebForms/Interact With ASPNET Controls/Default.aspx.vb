Imports System.Collections.Generic
Imports System.IO
Imports System.Web.UI
Imports System.Xml
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server

Public NotInheritable Class PresetHelper
	Private Sub New()
	End Sub
	Public Class Preset
		Public XmlMetaData As String
		Public SQL As String
	End Class

	Public Shared CurrentPresetNumber As Integer = 0
	Public Shared ReadOnly Property CurrentPreset() As Preset
        Get
            If Presets.Count = 0 Then
                Presets.Add(New Preset() With { _
    .XmlMetaData = "/Northwind.xml", _
    .SQL = "Select o.OrderID, c.CustomerID As a1, c.ContactName" & vbCr & vbLf & "From Orders o Inner Join" & vbCr & vbLf & "  Customers c On o.CustomerID = c.CustomerID Inner Join" & vbCr & vbLf & "  Shippers On Shippers.ShipperID = c.Country Inner Join" & vbCr & vbLf & "  Region On Shippers.CompanyName = Region.RegionID" & vbCr & vbLf & "Where o.ShipCity = 'A'" _
    })
                Presets.Add(New Preset() With { _
    .XmlMetaData = "/db2_sample_with_alt_names.xml", _
    .SQL = "Select Employees.[Employee ID], Employees.[First Name]," & vbCr & vbLf & "  [Employee Photos].[Photo Image], [Employee Resumes].Resume" & vbCr & vbLf & "From [Employee Photos] Inner Join" & vbCr & vbLf & "  Employees On Employees.[Employee ID] = [Employee Photos].[Employee ID]" & vbCr & vbLf & "  Inner Join" & vbCr & vbLf & "  [Employee Resumes] On Employees.[Employee ID] =" & vbCr & vbLf & "    [Employee Resumes].[Employee ID]" _
    })

            End If

            Return Presets(CurrentPresetNumber)
        End Get
	End Property

	Public Shared Sub SwitchPreset()
		If System.Threading.Interlocked.Increment(CurrentPresetNumber) >= Presets.Count Then
			CurrentPresetNumber = 0
		End If
	End Sub

	Public Shared Sub ResetPreset()
		CurrentPresetNumber = 0
	End Sub


    Public Shared Presets As List(Of Preset) = New List(Of Preset)

End Class

Public Partial Class InteractWitnAspNetControls
	Inherits Page
	Public Sub Page_Load(sender As Object, e As EventArgs)
		If Not Page.IsPostBack Then
			SQLEditor1.SQL = "Select o.OrderID, c.CustomerID, s.ShipperID, o.ShipCity From Orders o Inner Join Customers c On o.Customer_ID = c.ID Inner Join Shippers s On s.ID = o.Shipper_ID Where o.ShipCity = 'A'"
			PresetHelper.ResetPreset()
		End If
		label.Text = "Preset #" & (PresetHelper.CurrentPresetNumber + 1)
	End Sub

	Public Sub SleepModeChanged(sender As Object, e As EventArgs)
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		If queryBuilder.SleepMode Then StatusBar1.Message.Error("Unsupported SQL statement.")
	End Sub

Public Sub QueryBuilderControl1_Init(sender As Object, e As EventArgs)
		' Get instance of QueryBuilder
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
		queryBuilder.BehaviorOptions.AllowSleepMode = True
		queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

		queryBuilder.OfflineMode = True
		' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/XmlMetaData] key
        ' Check what XML was loaded
        ' Pass loaded XML to QueryBuilder component
        Try
	queryBuilder.SQL = PresetHelper.CurrentPreset.SQL
            Dim sql = queryBuilder.SQL
            queryBuilder.SQL = String.Empty
            queryBuilder.MetadataContainer.ImportFromXML(Server.MapPath(PresetHelper.CurrentPreset.XmlMetaData))
            queryBuilder.MetadataStructure.Refresh()
            queryBuilder.SQL = sql
            StatusBar1.Message.Information("Metadata loaded")
        Catch ex As Exception
            Dim message As String = 
                "Error loading metadata from the database." +
                "Check the 'configuration\\connectionStrings' key in the [web.config] file."
			Logger.[Error](message, ex)
			StatusBar1.Message.Error(message & " Check log.txt for details.")
        End Try
    End Sub

    

	Protected Sub button_Click(sender As Object, e As EventArgs)
		PresetHelper.SwitchPreset()
		label.Text = "Preset #" & (PresetHelper.CurrentPresetNumber + 1)
	End Sub
End Class
