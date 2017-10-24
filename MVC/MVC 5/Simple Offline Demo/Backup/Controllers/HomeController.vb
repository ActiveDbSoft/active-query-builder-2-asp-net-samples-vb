Imports System.IO
Imports System.Configuration
Imports System.Web.Mvc
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Mvc.Filters
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class HomeController
		Inherits Controller
		<QueryBuilderInit> _
		Public Function Index() As ActionResult
			Return View()
		End Function
	End Class

	Friend Class QueryBuilderInitAttribute
		Inherits InitializeQueryBuilderAttribute
		Protected Overrides Sub Init(filterContext As ActionExecutingContext, item As SessionStoreItem)
			' Get instance of the QueryBuilder object
			Dim queryBuilder = item.QueryBuilder

			' Create an instance of the proper syntax provider for your database server.
			queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

			' Denies metadata loading requests from the metadata provider
			queryBuilder.OfflineMode = True

			Dim path__1 = ConfigurationManager.AppSettings("XmlMetaData")
			Dim xml = Path.Combine(filterContext.HttpContext.Server.MapPath("~"), path__1)
			queryBuilder.MetadataContainer.ImportFromXML(xml)

			' Initialization of the Metadata Structure object that's
			' responsible for representation of metadata in a tree-like form
			Try
				' Clears and loads the first level of metadata structure tree
				queryBuilder.MetadataStructure.Refresh()
			Catch ex As Exception
				Logger.[Error]("Error loading metadata", ex)
			End Try
		End Sub
	End Class
End Namespace
