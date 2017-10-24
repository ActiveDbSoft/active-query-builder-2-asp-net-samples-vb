Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports Logger = ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server.Logger

Public Partial Class QueryBuilderChangeConnection
	Inherits System.Web.UI.UserControl
	Public Sub QueryBuilderControl1_Init(sender As Object, e As EventArgs)
		' Get instance of QueryBuilder
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		queryBuilder.SyntaxProvider = New MSAccessSyntaxProvider()
		queryBuilder.MetadataProvider = New OLEDBMetadataProvider()
	End Sub

	Protected Sub FirstOnClick(sender As Object, e As EventArgs)
		SetConnection("Nwind.mdb")
	End Sub

	Protected Sub SecondOnClick(sender As Object, e As EventArgs)
		SetConnection("demo.mdb")
	End Sub

	Private Sub SetConnection(dbName As String)
		Dim queryBuilder = QueryBuilderControl1.QueryBuilder

		queryBuilder.MetadataProvider.Connection = CreateConnection(dbName)

		Try
			queryBuilder.MetadataContainer.Clear()
			queryBuilder.MetadataStructure.Refresh()
			StatusBar1.Message.Information("Metadata loaded")
		Catch ex As Exception
			Dim message As String = "Error loading metadata from the database."
			Logger.[Error](message, ex)
			StatusBar1.Message.[Error](message & " Check log.txt for details.")
		End Try
	End Sub

	Private Function CreateConnection(dbname As String) As IDbConnection
		'var provider = "Microsoft.ACE.OLEDB.12.0";
		Dim provider = "Microsoft.Jet.OLEDB.4.0"
		Dim path__1 = "..\..\Sample databases\" & dbname

		Dim xml = Path.Combine(Server.MapPath(""), path__1)
		Dim connectionString = String.Format("Provider={0};Data Source={1};Persist Security Info=False;", provider, xml)
		Return New OleDbConnection(connectionString)
	End Function
End Class
