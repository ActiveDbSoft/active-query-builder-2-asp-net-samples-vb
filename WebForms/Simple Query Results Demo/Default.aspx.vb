Imports System.Configuration
Imports System.Data
Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server.Models

Partial Public Class [Default]
	Inherits System.Web.UI.Page
	Public Sub SleepModeChanged(sender As Object, e As EventArgs)
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		If queryBuilder.SleepMode Then SessionStore.Current.Message.Error("Unsupported SQL statement.")
	End Sub

Public Sub QueryBuilderControl1_Init(sender As Object, e As EventArgs)
		' Get instance of QueryBuilder
		Dim queryBuilder As QueryBuilder = (QueryBuilderControl1).QueryBuilder
		queryBuilder.OfflineMode = False
		' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
		queryBuilder.BehaviorOptions.AllowSleepMode = True
		queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

		' you may load metadata from the database connection using live database connection and metadata provider
		Dim connection = New SqlConnection(ConfigurationManager.ConnectionStrings("Northwind").ConnectionString)

		If String.IsNullOrEmpty(connection.ConnectionString) Then
			Dim message As String = "Can't find in [web.config] key <configuration>/<connectionStrings><add key=""YourDB"" connectionString=""..."">!"
			Logger.[Error](message)
			queryBuilder.OfflineMode = True
			Return
		End If

		Try
			Dim metadataProvider = New MSSQLMetadataProvider()
			metadataProvider.Connection = connection
			queryBuilder.MetadataProvider = metadataProvider
		Catch ex As Exception
			Dim message As String = "Can't setup metadata provider!"
			Logger.[Error](message, ex)
			Return
		End Try

		' Initialization of the Metadata Structure object that's
		' responsible for representation of metadata in a tree-like form
		Try
			' Clears and loads the first level of the metadata structure tree 
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

	Protected Sub Page_Load(sender As Object, e As EventArgs)
		If Not Page.IsPostBack Then
			SQLEditor1.SQL = "Select top 10 * From Orders Where ID = @ID"
		End If
	End Sub

	Protected Sub Button1_Click(sender As Object, e As EventArgs)
		Dim queryBuilder1 As QueryBuilder = (QueryBuilderControl1).QueryBuilder
		Dim sqlBuilder As PlainTextSQLBuilder = (QueryBuilderControl1).PlainTextSQLBuilder

		dataGridView1.DataSource = Nothing

		If queryBuilder1.MetadataProvider Is Nothing Then
			Return
		End If

		Dim cmd As SqlCommand = DirectCast(queryBuilder1.MetadataProvider.Connection.CreateCommand(), SqlCommand)
		cmd.CommandTimeout = 30
		cmd.CommandText = queryBuilder1.SQL

		Try
			For Each paramDto As QueryParamDto In SessionStore.Current.ClientQueryParams
				cmd.Parameters.Add(paramDto.Name, paramDto.Value)
			Next

			Dim adapter = New SqlDataAdapter(cmd)
			Dim dataset = New DataSet()
			adapter.Fill(dataset, "QueryResult")
			dataGridView1.DataSource = dataset.Tables("QueryResult")
			dataGridView1.DataBind()
		Catch ex As Exception
			Dim message As String = "SQL query error"
			Logger.[Error](message, ex)
		End Try
	End Sub
End Class

