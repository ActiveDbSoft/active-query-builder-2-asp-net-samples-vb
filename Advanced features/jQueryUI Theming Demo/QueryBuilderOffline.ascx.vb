Imports System.Configuration
Imports System.IO
Imports System.Xml
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Control

Public Partial Class QueryBuilderOffline
	Inherits System.Web.UI.UserControl
	Protected Sub Page_Load(sender As Object, e As EventArgs)
		If Not Page.IsPostBack Then
			SQLEditor1.SQL = "Select o.OrderID," & vbCr & vbLf & "  c.CustomerID," & vbCr & vbLf & "  s.ShipperID," & vbCr & vbLf & "  o.ShipCity" & vbCr & vbLf & "From Orders o" & vbCr & vbLf & "  Inner Join Customers c On o.CustomerID = c.CustomerID" & vbCr & vbLf & "  Inner Join Shippers s On s.ShipperID = o.OrderID" & vbCr & vbLf & "Where o.ShipCity = 'A'"
		End If
	End Sub

	Protected Sub SleepModeChanged(sender As Object, e As EventArgs)
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		If queryBuilder.SleepMode Then
			StatusBar1.Message.[Error]("Unsupported SQL statement.")
		End If
	End Sub

	Public Sub QueryBuilderControl1_Init(sender As Object, e As EventArgs)
		' Get instance of QueryBuilder
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
		queryBuilder.BehaviorOptions.AllowSleepMode = True
		queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

		queryBuilder.OfflineMode = True
		' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/XmlMetaData] key
		Try
			Dim path__1 = ConfigurationManager.AppSettings("XmlMetaData")
			Dim xml = Path.Combine(Server.MapPath(""), path__1)
			queryBuilder.MetadataContainer.ImportFromXML(xml)
		Catch ex As Exception
			Dim message As String = "Can't load metadata from XML."
			Logger.[Error](message, ex)
			StatusBar1.Message.[Error](message & " Check log.txt for details.")
		End Try


		' Initialization of the Metadata Structure object that's
		' responsible for representation of metadata in a tree-like form
		Try
			' Clears and loads the first level of the metadata structure tree 
			queryBuilder.MetadataStructure.Refresh()
			StatusBar1.Message.Information("Metadata loaded")
		Catch ex As Exception
			Dim message As String = "Error loading metadata from the database." & "Check the 'configuration\connectionStrings' key in the [web.config] file."
			Logger.[Error](message, ex)
			StatusBar1.Message.[Error](message & " Check log.txt for details.")
		End Try

	End Sub

End Class
