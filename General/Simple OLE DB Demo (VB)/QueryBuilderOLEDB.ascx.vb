Imports System.Configuration
Imports System.Data.OleDb
Imports System.IO
Imports System.Xml
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Control
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server

Partial Public Class QueryBuilderOLEDB
    Inherits System.Web.UI.UserControl
	Public Sub Page_Load(sender As Object, e As EventArgs)
		If Not Page.IsPostBack Then
			SQLEditor1.SQL = "Select o.OrderID, c.CustomerID, s.ShipperID, o.ShipCity From Orders o Inner Join Customers c On o.Customer_ID = c.ID Inner Join Shippers s On s.ID = o.Shipper_ID Where o.ShipCity = 'A'"
		End If
	End Sub
    Public Sub SleepModeChanged(sender As Object, e As EventArgs)
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		If queryBuilder.SleepMode Then StatusBar1.Message.Error("Unsupported SQL statement.")
	End Sub

Public Sub QueryBuilderControl1_Init(ByVal sender As Object, ByVal e As EventArgs)

        ' Get instance of QueryBuilder
        Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
        queryBuilder.OfflineMode = False
        ' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
		queryBuilder.BehaviorOptions.AllowSleepMode = True
		queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

        ' you may load metadata from the database connection using live database connection and metadata provider
        Dim connection As New OleDbConnection()
        connection.ConnectionString = ConfigurationManager.ConnectionStrings("Northwind").ConnectionString
        If String.IsNullOrEmpty(connection.ConnectionString) Then
            Dim message As String = "Can't find in [web.config] key <configuration>/<connectionStrings><add key=""YourDB"" connectionString=""..."">!"
            Logger.[Error](message)
            StatusBar1.Message.Error(message & " Check log.txt for details.")
            queryBuilder.OfflineMode = True
            Return
        End If

        Try
            Dim metadataProvider As New OLEDBMetadataProvider()
            metadataProvider.Connection = connection
            queryBuilder.MetadataProvider = metadataProvider
            queryBuilder.MetadataStructure.Refresh()
            StatusBar1.Message.Information("Metadata loaded")
        Catch ex As Exception
            Dim message As String = 
                "Error loading metadata from the database." +
                "Check the 'configuration\\connectionStrings' key in the [web.config] file."
			Logger.[Error](message, ex)
			StatusBar1.Message.Error(message & " Check log.txt for details.")
            Return
        End Try

       
    End Sub

End Class