Imports System.Configuration
Imports System.IO
Imports System.Xml
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server


Partial Public Class QueryStructure
	Inherits System.Web.UI.UserControl

	Public Sub Page_Load(sender As Object, e As EventArgs)
		If Not Page.IsPostBack Then
			SQLEditor1.SQL = "Select o.OrderID, c.CustomerID, s.ShipperID, o.ShipCity From Orders o Inner Join Customers c On o.Customer_ID = c.ID Inner Join Shippers s On s.ID = o.Shipper_ID Where o.ShipCity = 'A'"
		End If
	End Sub

	Public Sub QueryBuilderControl1_OnSQLUpdated(sender As Object, e As EventArgs, clientdata AS ClientData)
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		If queryBuilder Is Nothing Then
			Return
		End If
		Dim data = New ExchangeClass()

		data.Statistics = GetQueryStatistic(queryBuilder.QueryStatistics)
		data.SubQueries = DumpSubQueries(queryBuilder)
		data.QueryStructure = DumpQueryStructureInfo(queryBuilder.ActiveSubQuery)
		data.UnionSubQuery = New UnionSubQueryExchangeClass()

		data.UnionSubQuery.SelectedExpressions = DumpSelectedExpressionsInfoFromUnionSubQuery(queryBuilder.ActiveSubQuery.ActiveUnionSubquery)
		data.UnionSubQuery.DataSources = DumpDataSourcesInfoFromUnionSubQuery(queryBuilder.ActiveSubQuery.ActiveUnionSubquery)


		data.UnionSubQuery.Links = DumpLinksInfoFromUnionSubQuery(queryBuilder.ActiveSubQuery.ActiveUnionSubquery)
		data.UnionSubQuery.Where = GetWhereInfo(queryBuilder.ActiveSubQuery.ActiveUnionSubquery)
		'            data.UnionSubQuery.Where

		SessionStore.Current.Exchange.Data = data
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
		' Pass loaded XML to QueryBuilder component
		Try
			queryBuilder.MetadataContainer.ImportFromXML(Page.Server.MapPath("/Northwind.xml"))
			queryBuilder.MetadataStructure.Refresh()
            StatusBar1.Message.Information("Metadata loaded")
		Catch ex As Exception
			Dim message As String = 
                "Error loading metadata from the database." +
                "Check the 'configuration\\connectionStrings' key in the [web.config] file."
			Logger.[Error](message, ex)
			StatusBar1.Message.Error(message & " Check log.txt for details.")
		End Try
		queryBuilder.SQL = SQLEditor1.ControlSQL
	End Sub

	Private Function LoadXml(xmlFileName As String) As XmlDocument
		Dim doc As XmlDocument = New XmlDocument()
		Try
			doc.Load(xmlFileName)
		Catch ex As Exception
			Dim message As String
			If TypeOf ex Is FileNotFoundException Then
				message = String.Format("Can't find XML file '{0}'.", xmlFileName)
				Logger.[Error](message, ex)
				StatusBar1.Message.Error(message & " Check log.txt for details.")
			ElseIf TypeOf ex Is IOException Then
				message = String.Format("Can't read XML file '{0}'.", xmlFileName)
				Logger.[Error](message, ex)
				StatusBar1.Message.Error(message & " Check log.txt for details.")
			ElseIf TypeOf ex Is XmlException Then
				message = String.Format("Can't parse XML file '{0}'.", xmlFileName)
				Logger.[Error](message, ex)
				StatusBar1.Message.Error(message & " Check log.txt for details.")
			Else
				message = String.Format("Unknown error during load XML file '{0}'", xmlFileName)
				Logger.[Error](message, ex)
				StatusBar1.Message.Error(message & " Check log.txt for details.")
			End If
			doc = Nothing
		End Try
		Return doc
	End Function
End Class