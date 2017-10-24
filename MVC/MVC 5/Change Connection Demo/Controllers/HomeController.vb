Imports System.IO
Imports System.Net
Imports System.Data
Imports System.Data.OleDb
Imports System.Web.Mvc
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Mvc.Filters
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class HomeController
		Inherits Controller
		<Initilize> _
		Public Function Index() As ActionResult
			Return View()
		End Function

		<HttpPost> _
		Public Function ChangeConnection(name As String) As ActionResult
			Dim item = SessionStore.Current
			Dim queryBuilder = item.QueryBuilder

			' Initialization of the Metadata Structure object that's
			' responsible for representation of metadata in a tree-like form
			Try
				queryBuilder.MetadataProvider.Connection = CreateConnection(name)
				' Clears and loads the first level of metadata structure tree
				queryBuilder.MetadataContainer.Clear()
				queryBuilder.MetadataStructure.Refresh()
			Catch ex As Exception
				Logger.[Error]("Error loading metadata, connection failed", ex)
				Return New HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message)
			End Try
			Return New EmptyResult()
		End Function

		Private Function CreateConnection(dbname As String) As IDbConnection
			'var provider = "Microsoft.ACE.OLEDB.12.0";
			Dim provider = "Microsoft.Jet.OLEDB.4.0"
			Dim path__1 = "..\..\..\Sample databases\" & dbname
			Dim db = Path.Combine(Server.MapPath("~"), path__1)
			Dim connectionString = String.Format("Provider={0};Data Source={1};Persist Security Info=False;", provider, db)
			Return New OleDbConnection(connectionString)
		End Function
	End Class

	Public Class InitilizeAttribute
		Inherits InitializeQueryBuilderAttribute
		Protected Overrides Sub Init(filterContext As ActionExecutingContext, item As SessionStoreItem)

			' Get instance of the QueryBuilder object
			Dim queryBuilder = item.QueryBuilder

			' Create an instance of the proper syntax provider for your database server.
			queryBuilder.SyntaxProvider = New MSAccessSyntaxProvider()
			queryBuilder.MetadataProvider = New OLEDBMetadataProvider()
		End Sub
	End Class
End Namespace
