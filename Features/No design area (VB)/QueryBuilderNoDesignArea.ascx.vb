Imports System.Configuration
Imports System.IO
Imports System.Xml
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Control
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server

Public Partial Class QueryBuilderNoDesignArea
	Inherits System.Web.UI.UserControl
	Public Sub Page_Load(sender As Object, e As EventArgs)
		If Not Page.IsPostBack Then
			SQLEditor1.SQL = "Select o.OrderID, c.CustomerID, s.ShipperID, o.ShipCity From Orders o Inner Join   Customers c On o.Customer_ID = c.ID Inner Join  Shippers s On s.ID = o.Shipper_ID Where o.ShipCity = 'A'"
		End If
	End Sub

    Public Sub SleepModeChanged(sender As Object, e As EventArgs)
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		If queryBuilder.SleepMode Then StatusBar1.Message.Error("Unsupported SQL statement.")
	End Sub

Public Sub QueryBuilderControl1_Init(ByVal sender As Object, ByVal e As EventArgs)
        ' Get instance of QueryBuilder
        Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
        ' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
		queryBuilder.BehaviorOptions.AllowSleepMode = True
		queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

        queryBuilder.OfflineMode = True
		queryBuilder.BehaviorOptions.DeleteUnusedObjects = True
		queryBuilder.BehaviorOptions.AddLinkedObjects = True
        ' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/XmlMetaData] key
        ' Pass loaded XML to QueryBuilder component
        Try
            queryBuilder.MetadataContainer.ImportFromXML(Page.Server.MapPath(ConfigurationManager.AppSettings("XmlMetaData")))
            queryBuilder.MetadataStructure.Refresh()
            StatusBar1.Message.Information("Metadata loaded")
        Catch ex As Exception
            Dim message As String = 
                "Error loading metadata from the database." +
                "Check the 'configuration\\connectionStrings' key in the [web.config] file."
			Logger.[Error](message, ex)
			StatusBar1.Message.Error(message & " Check log.txt for details.")
        End Try
        
    End Sub

    
End Class
