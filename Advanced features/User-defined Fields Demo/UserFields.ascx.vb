Imports System.IO
Imports System.Configuration
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server
Imports Logger = ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server.Logger

Public Partial Class UserFields
	Inherits System.Web.UI.UserControl
	Protected Sub Page_Load(sender As Object, e As EventArgs)
		If Not Page.IsPostBack Then
			SQLEditor1.SQL = "Select o.OrderID, o.DetailsCount From Orders o"
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
		queryBuilder.OfflineMode = True

		queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

		Try
			Dim path__1 = ConfigurationManager.AppSettings("XmlMetaData")
			Dim xml = Path.Combine(Server.MapPath(""), path__1)
			queryBuilder.MetadataContainer.ImportFromXML(xml)

			queryBuilder.MetadataStructure.Refresh()
			StatusBar1.Message.Information("Metadata loaded")
		Catch ex As Exception
			Dim message As String = "Error loading metadata from the file."
			Logger.[Error](message, ex)
			StatusBar1.Message.[Error](message & " Check log.txt for details.")
		End Try

		AddUserFields(queryBuilder)
	End Sub

	Private Sub AddUserFields(queryBuilder As QueryBuilder)
		Dim order As MetadataObject = queryBuilder.MetadataContainer.FindItem(Of MetadataObject)("Orders")
		order.AddUserField("DetailsCount", "(select count(*) from [Order Details] od where od.OrderId = Orders.OrderId)")
	End Sub

	Protected Sub QueryBuilderControl1_OnSQLUpdated(sender As Object, e As EventArgs, clientdata As ClientData)
		SessionStore.Current.Exchange.Data = SessionStore.Current.UserObjectsSQL
	End Sub
End Class
