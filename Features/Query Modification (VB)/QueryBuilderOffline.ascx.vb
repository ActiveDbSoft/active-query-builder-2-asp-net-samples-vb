Imports System.Collections.Generic
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Xml
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Control

Public Partial Class QueryBuilderOffline
	Inherits System.Web.UI.UserControl

	Protected Sub Page_Load(sender As Object, e As EventArgs)
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		tbSQL.Text = QueryBuilderControl1.PlainTextSQLBuilder.SQL
		' prepare parsed names
		CustomersName = queryBuilder.SQLContext.ParseQualifiedName("Northwind.dbo.Customers")
		CustomersAlias = queryBuilder.SQLContext.ParseIdentifier("c")
		OrdersName = queryBuilder.SQLContext.ParseQualifiedName("Northwind.dbo.Orders")
		OrdersAlias = queryBuilder.SQLContext.ParseIdentifier("o")
		JoinFieldName = queryBuilder.SQLContext.ParseQualifiedName("CustomerId")
		CompanyNameFieldName = queryBuilder.SQLContext.ParseQualifiedName("CompanyName")
		OrderDateFieldName = queryBuilder.SQLContext.ParseQualifiedName("OrderDate")
	End Sub

	Public Sub SleepModeChanged(sender As Object, e As EventArgs)
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		If queryBuilder.SleepMode Then StatusBar1.Message.Error("Unsupported SQL statement.")
	End Sub

Public Sub QueryBuilderControl1_Init(sender As Object, e As EventArgs)
		' Get instance of QueryBuilder
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		If queryBuilder.SyntaxProvider Is Nothing Then
			' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
		queryBuilder.BehaviorOptions.AllowSleepMode = True
		queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()
		End If
		queryBuilder.OfflineMode = True
		queryBuilder.MetadataContainer.ImportFromXML(Page.Server.MapPath("Northwind.xml"))
		queryBuilder.MetadataStructure.Refresh()
            StatusBar1.Message.Information("Metadata loaded")
	End Sub

	Protected Sub btn1_Click(sender As Object, e As EventArgs)
		Dim queryBuilder1 = QueryBuilderControl1.QueryBuilder

		' prevent QueryBuilder to request metadata
		queryBuilder1.OfflineMode = True

		queryBuilder1.MetadataProvider = Nothing

		Dim metadataContainer As MetadataContainer = queryBuilder1.MetadataContainer
		metadataContainer.BeginUpdate()

		Try
			metadataContainer.Items.Clear()

			Dim schemaDbo As MetadataNamespace = metadataContainer.AddSchema("dbo")

			' prepare metadata for table "Orders"
			Dim orders As MetadataObject = schemaDbo.AddTable("Orders")
			' fields
			orders.AddField("OrderId")
			orders.AddField("CustomerId")

			' prepare metadata for table "Order Details"
			Dim orderDetails As MetadataObject = schemaDbo.AddTable("Order Details")
			' fields
			orderDetails.AddField("OrderId")
			orderDetails.AddField("ProductId")
			' foreign keys
			Dim foreignKey As MetadataForeignKey = orderDetails.AddForeignKey("OrderDetailsToOrders")

			Using referencedName As New MetadataQualifiedName()
				referencedName.Add("Orders")
				referencedName.Add("dbo")

				foreignKey.ReferencedObjectName = referencedName
			End Using

			foreignKey.Fields.Add("OrderId")
			foreignKey.ReferencedFields.Add("OrderId")
		Finally
			metadataContainer.EndUpdate()
		End Try

		queryBuilder1.MetadataStructure.Refresh()
            StatusBar1.Message.Information("Metadata loaded")
	End Sub

	Protected Sub btn2_Click(sender As Object, e As EventArgs)
		Dim queryBuilder1 As QueryBuilder = QueryBuilderControl1.QueryBuilder

		' allow QueryBuilder to request metadata
		queryBuilder1.OfflineMode = False

		queryBuilder1.MetadataProvider = Nothing
		queryBuilder1.MetadataContainer.Items.Clear()
		AddHandler queryBuilder1.MetadataContainer.ItemMetadataLoading, AddressOf way2ItemMetadataLoading
		queryBuilder1.MetadataStructure.Refresh()
	End Sub

	Private Sub way2ItemMetadataLoading(sender As Object, item As MetadataItem, types As MetadataType)
		Select Case item.Type
			Case MetadataType.Root
				If (types And MetadataType.Schema) > 0 Then
					item.AddSchema("dbo")
				End If
				Exit Select

			Case MetadataType.Schema
				If (item.Name = "dbo") AndAlso (types And MetadataType.Table) > 0 Then
					item.AddTable("Orders")
					item.AddTable("Order Details")
				End If
				Exit Select

			Case MetadataType.Table
				If item.Name = "Orders" Then
					If (types And MetadataType.Field) > 0 Then
						item.AddField("OrderId")
						item.AddField("CustomerId")
					End If
				ElseIf item.Name = "Order Details" Then
					If (types And MetadataType.Field) > 0 Then
						item.AddField("OrderId")
						item.AddField("ProductId")
					End If

					If (types And MetadataType.ForeignKey) > 0 Then
						Dim foreignKey As MetadataForeignKey = item.AddForeignKey("OrderDetailsToOrder")
						foreignKey.Fields.Add("OrderId")
						foreignKey.ReferencedFields.Add("OrderId")
						Using name As New MetadataQualifiedName()
							name.Add("Orders")
							name.Add("dbo")

							foreignKey.ReferencedObjectName = name
						End Using
					End If
				End If
				Exit Select
		End Select

		item.Items.SetLoaded(types, True)
	End Sub

	Private m_way1EventMetadataProvider As ActiveDatabaseSoftware.ActiveQueryBuilder.EventMetadataProvider
	Private ReadOnly Property Way1EventMetadataProvider() As ActiveDatabaseSoftware.ActiveQueryBuilder.EventMetadataProvider
		Get
			If m_way1EventMetadataProvider Is Nothing Then
				m_way1EventMetadataProvider = New EventMetadataProvider()
				AddHandler m_way1EventMetadataProvider.ExecSQL, New ActiveDatabaseSoftware.ActiveQueryBuilder.ExecSQLEventHandler(AddressOf Me.way3EventMetadataProvider_ExecSQL)
			End If
			Return m_way1EventMetadataProvider
		End Get
	End Property

	Private Sub way3EventMetadataProvider_ExecSQL(metadataProvider As BaseMetadataProvider, sql As String, schemaOnly As Boolean, ByRef dataReader As IDataReader)
		dataReader = Nothing

		If dbConnection IsNot Nothing Then
			Dim command As IDbCommand = dbConnection.CreateCommand()
			command.CommandText = sql
			dataReader = command.ExecuteReader()
		End If
	End Sub

	Private dbConnection As IDbConnection = New SqlConnection(ConfigurationManager.ConnectionStrings("AdventureWorks").ConnectionString)
	Protected Sub btn3_Click(sender As Object, e As EventArgs)
		Dim queryBuilder1 = QueryBuilderControl1.QueryBuilder

		If dbConnection IsNot Nothing Then
			Try
				dbConnection.Close()
				dbConnection.Open()

				' allow QueryBuilder to request metadata
				queryBuilder1.OfflineMode = False

				ResetQueryBuilderMetadata()

				queryBuilder1.MetadataProvider = Way1EventMetadataProvider
				queryBuilder1.MetadataStructure.Refresh()
            StatusBar1.Message.Information("Metadata loaded")
			Catch ex As System.Exception
				StatusBar1.Message.[Error](ex.Message)
			End Try
		Else
			StatusBar1.Message.[Error]("Please setup a database connection by clicking on the ""Connect"" menu item before testing this method.")
		End If
	End Sub

	Private Sub ResetQueryBuilderMetadata()
		Dim queryBuilder1 As QueryBuilder = QueryBuilderControl1.QueryBuilder

		queryBuilder1.MetadataProvider = Nothing
		queryBuilder1.MetadataContainer.Items.Clear()
		RemoveHandler queryBuilder1.MetadataContainer.ItemMetadataLoading, AddressOf way2ItemMetadataLoading
	End Sub

	Protected Sub btn4_Click(sender As Object, e As EventArgs)
		Dim queryBuilder1 = QueryBuilderControl1.QueryBuilder
		queryBuilder1.MetadataProvider = Nothing
		queryBuilder1.OfflineMode = True
		' prevent QueryBuilder to request metadata from connection
		Dim dataSet As New DataSet()

		' Load sample dataset created in the Visual Studio with Dataset Designer
		' and exported to XML using WriteXmlSchema() method.
		dataSet.ReadXmlSchema(Path.Combine(Server.MapPath("~/"), "StoredDataSetSchema.xml"))

		queryBuilder1.MetadataContainer.BeginUpdate()

		Try
			queryBuilder1.MetadataContainer.Items.Clear()

			' add tables
			For Each table As DataTable In dataSet.Tables
				' add new metadata table
				Dim metadataTable As MetadataObject = queryBuilder1.MetadataContainer.AddTable(table.TableName)

				' add metadata fields (columns)
				For Each column As DataColumn In table.Columns
					' create new field
					Dim metadataField As MetadataField = metadataTable.AddField(column.ColumnName)
					' setup field
					metadataField.FieldType = TypeToDbType(column.DataType)
					metadataField.Nullable = column.AllowDBNull
					metadataField.[ReadOnly] = column.[ReadOnly]

					If column.MaxLength <> -1 Then
						metadataField.Size = column.MaxLength
					End If

					' detect the field is primary key
					For Each pkColumn As DataColumn In table.PrimaryKey
						If column Is pkColumn Then
							metadataField.PrimaryKey = True
						End If
					Next
				Next

				' add relations
				For Each relation As DataRelation In table.ParentRelations
					' create new relation on the parent table
					Dim metadataRelation As MetadataForeignKey = metadataTable.AddForeignKey(relation.RelationName)

					' set referenced table
					Using referencedName As New MetadataQualifiedName()
						referencedName.Add(relation.ParentTable.TableName)

						metadataRelation.ReferencedObjectName = referencedName
					End Using

					' set referenced fields
					For Each parentColumn As DataColumn In relation.ParentColumns
						metadataRelation.ReferencedFields.Add(parentColumn.ColumnName)
					Next

					' set fields
					For Each childColumn As DataColumn In relation.ChildColumns
						metadataRelation.Fields.Add(childColumn.ColumnName)
					Next
				Next
			Next
		Finally
			queryBuilder1.MetadataContainer.EndUpdate()
		End Try

		queryBuilder1.MetadataStructure.Refresh()
            StatusBar1.Message.Information("Metadata loaded")
	End Sub

	Private Shared Function TypeToDbType(type As Type) As DbType
		If type Is GetType(System.String) Then
			Return DbType.[String]
		ElseIf type Is GetType(System.Int16) Then
			Return DbType.Int16
		ElseIf type Is GetType(System.Int32) Then
			Return DbType.Int32
		ElseIf type Is GetType(System.Int64) Then
			Return DbType.Int64
		ElseIf type Is GetType(System.UInt16) Then
			Return DbType.UInt16
		ElseIf type Is GetType(System.UInt32) Then
			Return DbType.UInt32
		ElseIf type Is GetType(System.UInt64) Then
			Return DbType.UInt64
		ElseIf type Is GetType(System.Boolean) Then
			Return DbType.[Boolean]
		ElseIf type Is GetType(System.Single) Then
			Return DbType.[Single]
		ElseIf type Is GetType(System.Double) Then
			Return DbType.[Double]
		ElseIf type Is GetType(System.Decimal) Then
			Return DbType.[Decimal]
		ElseIf type Is GetType(System.DateTime) Then
			Return DbType.DateTime
		ElseIf type Is GetType(System.TimeSpan) Then
			Return DbType.Time
		ElseIf type Is GetType(System.Byte) Then
			Return DbType.[Byte]
		ElseIf type Is GetType(System.SByte) Then
			Return DbType.[SByte]
		ElseIf type Is GetType(System.Char) Then
			Return DbType.[String]
		ElseIf type Is GetType(System.Byte()) Then
			Return DbType.Binary
		ElseIf type Is GetType(System.Guid) Then
			Return DbType.Guid
		Else
			Return DbType.[Object]
		End If
	End Function

	Protected Sub btnQueryCustomers_Click(sender As Object, e As EventArgs)
		QueryBuilderControl1.QueryBuilder.SQL = "select * from Northwind.dbo.Customers c"
	End Sub

	Protected Sub btnQueryOrders_Click(sender As Object, e As EventArgs)
		QueryBuilderControl1.QueryBuilder.SQL = "select * from Northwind.dbo.Orders o"
	End Sub

	Protected Sub btnQueryCustomersOrders_Click(sender As Object, e As EventArgs)
		QueryBuilderControl1.QueryBuilder.SQL = "select * from Northwind.dbo.Customers c, Northwind.dbo.Orders o"
	End Sub

	Private CustomersName As SQLQualifiedName
	Private CustomersAlias As AstTokenIdentifier
	Private OrdersName As SQLQualifiedName
	Private OrdersAlias As AstTokenIdentifier
	Private JoinFieldName As SQLQualifiedName
	Private CompanyNameFieldName As SQLQualifiedName
	Private OrderDateFieldName As SQLQualifiedName

	Private Customers As DataSource
	Private Orders As DataSource
	Private Shadows CompanyName As QueryColumnListItem
	Private OrderDate As QueryColumnListItem

	Protected Sub btnApply_Click(sender As Object, e As EventArgs)
		Dim queryBuilder = QueryBuilderControl1.QueryBuilder

		queryBuilder.BeginUpdate()

		Try
			' get the active SELECT
			Dim usq As UnionSubQuery = queryBuilder.Query.ActiveUnionSubquery

			'#Region "actualize stored references (if query is modified in GUI)"
			'#Region "actualize datasource references"
			' if user removed previously added datasources then clear their references
			If Customers IsNot Nothing AndAlso Not IsTablePresentInQuery(usq, Customers) Then
				' user removed this table in GUI
				Customers = Nothing
			End If

			If Orders IsNot Nothing AndAlso Not IsTablePresentInQuery(usq, Orders) Then
				' user removed this table in GUI
				Orders = Nothing
			End If
			'#End Region

			' clear CompanyName conditions
			If CompanyName IsNot Nothing Then
				' if user removed entire row OR cleared expression cell in GUI, clear the stored reference
				If Not IsQueryColumnListItemPresentInQuery(usq, CompanyName) Then
					CompanyName = Nothing
				End If
			End If

			' clear all condition cells for CompanyName row
			If CompanyName IsNot Nothing Then
				ClearConditionCells(usq, CompanyName)
			End If

			' clear OrderDate conditions
			If OrderDate IsNot Nothing Then
				' if user removed entire row OR cleared expression cell in GUI, clear the stored reference
				If Not IsQueryColumnListItemPresentInQuery(usq, OrderDate) Then
					OrderDate = Nothing
				End If
			End If

			' clear all condition cells for OrderDate row
			If OrderDate IsNot Nothing Then
				ClearConditionCells(usq, OrderDate)
			End If
			'#End Region

			'#Region "process Customers table"
			If cbCustomers.Checked OrElse cbCompanyName.Checked Then
				' if we have no previously added Customers table, try to find one already added by the user
				If Customers Is Nothing Then
					Customers = FindTableInQueryByName(usq, CustomersName)
				End If

				' there is no Customers table in query, add it
				If Customers Is Nothing Then
					Customers = usq.AddObject(CustomersName, CustomersAlias)
				End If

				'#Region "process CompanyName condition"
				If cbCompanyName.Enabled AndAlso cbCompanyName.Checked AndAlso Not [String].IsNullOrEmpty(tbCompanyName.Text) Then
					' if we have no previously added grid row for this condition, add it
					If CompanyName Is Nothing Then
						CompanyName = usq.QueryColumnList.AddField(Customers, CompanyNameFieldName.QualifiedNameWithoutQuotes)
						' do not append it to the select list, use this row for conditions only
						CompanyName.[Select] = False
					End If

					' write condition from edit box to all needed grid cells
					AddWhereCondition(usq.QueryColumnList, CompanyName, tbCompanyName.Text)
				Else
					' remove previously added grid row
					If CompanyName IsNot Nothing Then
						CompanyName.Dispose()
					End If

					CompanyName = Nothing
					'#End Region
				End If
			Else
				' remove previously added datasource
				If Customers IsNot Nothing Then
					Customers.Dispose()
				End If

				Customers = Nothing
			End If
			'#End Region

			'#Region "process Orders table"
			If cbOrders.Checked OrElse cbOrderDate.Checked Then
				' if we have no previosly added Orders table, try to find one already added by the user
				If Orders Is Nothing Then
					Orders = FindTableInQueryByName(usq, OrdersName)
				End If

				' there are no Orders table in query, add one
				If Orders Is Nothing Then
					Orders = usq.AddObject(OrdersName, OrdersAlias)
				End If

				'#Region "link between Orders and Customers"
				' we added Orders table,
				' check if we have Customers table too,
				' and if there are no joins between them, create such join
				Dim joinFieldNameStr As String = JoinFieldName.QualifiedNameWithoutQuotes
				If Customers IsNot Nothing AndAlso usq.FromClause.FindLink(Orders, joinFieldNameStr, Customers, joinFieldNameStr) Is Nothing AndAlso usq.FromClause.FindLink(Customers, joinFieldNameStr, Orders, joinFieldNameStr) Is Nothing Then
					usq.AddLink(Customers, JoinFieldName, Orders, JoinFieldName)
				End If
				'#End Region

				'#Region "process OrderDate condition"
				If cbOrderDate.Enabled AndAlso cbOrderDate.Checked AndAlso Not [String].IsNullOrEmpty(tbOrderDate.Text) Then
					' if we have no previously added grid row for this condition, add it
					If OrderDate Is Nothing Then
						OrderDate = usq.QueryColumnList.AddField(Orders, OrderDateFieldName.QualifiedNameWithoutQuotes)
						' do not append it to the select list, use this row for conditions only
						OrderDate.[Select] = False
					End If

					' write condition from edit box to all needed grid cells
					AddWhereCondition(usq.QueryColumnList, OrderDate, tbOrderDate.Text)
				Else
					' remove prviously added grid row
					If OrderDate IsNot Nothing Then
						OrderDate.Dispose()
					End If

					OrderDate = Nothing
					'#End Region
				End If
			Else
				If Orders IsNot Nothing Then
					Orders.Dispose()
					Orders = Nothing
				End If
				'#End Region
			End If
		Finally
			queryBuilder.EndUpdate()
		End Try

		tbSQL.Text = QueryBuilderControl1.PlainTextSQLBuilder.SQL
	End Sub

	Private Function IsTablePresentInQuery(unionSubQuery As UnionSubQuery, table As DataSource) As Boolean
		' collect the list of datasources used in FROM
		Dim dataSources As New List(Of DataSource)()
		unionSubQuery.FromClause.GetDatasources(dataSources)

		' check given table in list of all datasources
		Return dataSources.IndexOf(table) <> -1
	End Function

	Private Function IsQueryColumnListItemPresentInQuery(unionSubQuery As UnionSubQuery, item As QueryColumnListItem) As Boolean
		Return unionSubQuery.QueryColumnList.IndexOf(item) = -1 OrElse [String].IsNullOrEmpty(item.ExpressionString)
	End Function

	Private Sub ClearConditionCells(unionSubQuery As UnionSubQuery, item As QueryColumnListItem)
		For i As Integer = 0 To unionSubQuery.QueryColumnList.GetMaxConditionCount() - 1
			item.ConditionStrings(i) = ""
		Next
	End Sub

	Private Function FindTableInQueryByName(unionSubQuery As UnionSubQuery, name As SQLQualifiedName) As DataSource
		Dim foundDatasources As New List(Of DataSource)()
		unionSubQuery.FromClause.FindTablesByDBName(name, foundDatasources)

		' if found more than one tables with given name in the query, use the first one
		Return If(foundDatasources.Count > 0, foundDatasources(0), Nothing)
	End Function

	Private Sub AddWhereCondition(columnList As QueryColumnList, whereListItem As QueryColumnListItem, condition As String)
		Dim whereFound As Boolean = False

		' GetMaxConditionCount: returns the number of non-empty criteria columns in the grid.
		For i As Integer = 0 To columnList.GetMaxConditionCount() - 1
			' CollectCriteriaItemsWithWhereCondition:
			' This function returns the list of conditions that were found in
			' the i-th criteria column, applied to specific clause (WHERE or HAVING).
			' Thus, this function collects all conditions joined with AND
			' within one OR group (one grid column).
			Dim foundColumnItems As New List(Of QueryColumnListItem)()
			CollectCriteriaItemsWithWhereCondition(columnList, i, foundColumnItems)

			' if found some conditions in i-th column, append condition to i-th column
			If foundColumnItems.Count > 0 Then
				whereListItem.ConditionStrings(i) = condition
				whereFound = True
			End If
		Next

		' if there are no cells with "where" conditions, add condition to new column
		If Not whereFound Then
			whereListItem.ConditionStrings(columnList.GetMaxConditionCount()) = condition
		End If
	End Sub
	Private Sub CollectCriteriaItemsWithWhereCondition(criteriaList As QueryColumnList, columnIndex As Integer, result As IList(Of QueryColumnListItem))
		result.Clear()

		For i As Integer = 0 To criteriaList.Count - 1
			If criteriaList(i).ConditionType = ConditionType.Where AndAlso criteriaList(i).ConditionCount > columnIndex AndAlso criteriaList(i).GetASTCondition(columnIndex) IsNot Nothing Then
				result.Add(criteriaList(i))
			End If
		Next
	End Sub

	Protected Sub QueryBuilderControl1_SQLUpdated(sender As Object, e As EventArgs, clientdata AS ClientData)
		tbSQL.Text = QueryBuilderControl1.PlainTextSQLBuilder.SQL
	End Sub


End Class
