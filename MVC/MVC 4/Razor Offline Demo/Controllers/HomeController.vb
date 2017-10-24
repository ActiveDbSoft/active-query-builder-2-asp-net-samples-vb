Imports System.Web.Mvc
Imports System.IO
Imports System.Configuration
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

		Public Function About() As ActionResult
			ViewBag.Message = "Your app description page."

			Return View()
		End Function

		Public Function Contact() As ActionResult
			ViewBag.Message = "Your contact page."

			Return View()
		End Function
	End Class

	Friend Class QueryBuilderInitAttribute
		Inherits InitializeQueryBuilderAttribute
		Protected Overrides Sub Init(filterContext As ActionExecutingContext, item As SessionStoreItem)
			' Get instance of QueryBuilder
			Dim queryBuilder = item.QueryBuilder
			queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

			queryBuilder.OfflineMode = True
			' Load MetaData from XML document

			Try
				Dim path__1 = ConfigurationManager.AppSettings("XmlMetaData")
				Dim xml = Path.Combine(filterContext.HttpContext.Server.MapPath("~"), path__1)
				queryBuilder.MetadataContainer.ImportFromXML(xml)

				queryBuilder.MetadataStructure.Refresh()
			Catch ex As Exception
				Logger.[Error]("Can't load metadata from XML.", ex)
				SessionStore.Current.Message.[Error]("Can't load metadata from XML.")
			End Try
		End Sub
	End Class
End Namespace
