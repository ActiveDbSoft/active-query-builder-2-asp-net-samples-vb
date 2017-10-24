Imports System.Configuration
Imports System.Web.Mvc
Imports System.IO
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Mvc.Filters
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class HomeController
		Inherits Controller
		<QueryBuilderInit> _
		Public Function Index() As ActionResult
			ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application."

			Return View()
		End Function
	End Class

	Friend Class QueryBuilderInitAttribute
		Inherits InitializeQueryBuilderAttribute
		Protected Overrides Sub Init(filterContext As ActionExecutingContext, item As SessionStoreItem)
			' Get instance of QueryBuilder
			Dim queryBuilder As QueryBuilder = item.QueryBuilder
			queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()
			queryBuilder.OfflineMode = True

			' Load MetaData from XML document
			Try
				Dim path__1 = ConfigurationManager.AppSettings("XmlMetaData")
				Dim xml = Path.Combine(filterContext.HttpContext.Server.MapPath("~"), path__1)
				queryBuilder.MetadataContainer.ImportFromXML(xml)

				queryBuilder.MetadataStructure.Refresh()
			Catch ex As Exception
				Dim message As String = "Can't load metadata from XML."
				Logger.[Error](message, ex)
				item.Message.[Error](message & " Check log.txt for details.")
			End Try
		End Sub
	End Class
End Namespace
