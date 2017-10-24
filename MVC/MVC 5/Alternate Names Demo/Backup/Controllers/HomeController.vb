Imports System.IO
Imports System.Configuration
Imports System.Web.Mvc
Imports System.Web.UI
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Mvc.Filters
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class HomeController
		Inherits Controller
		<Initialize> _
		Public Function Index() As ActionResult
			Return View()
		End Function
	End Class

	Public Class Initialize
		Inherits InitializeQueryBuilderAttribute
		Private Class ExchangeClass
			Public SQL As String
			Public AlternateSQL As String
		End Class

		Protected Overrides Sub Init(filterContext As ActionExecutingContext, item As SessionStoreItem)
			' Get instance of QueryBuilder
			Dim queryBuilder As QueryBuilder = item.QueryBuilder

			queryBuilder.BehaviorOptions.UseAltNames = False
			item.PlainTextSQLBuilder.UseAltNames = False

			' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
			queryBuilder.BehaviorOptions.AllowSleepMode = True
			queryBuilder.SyntaxProvider = New DB2SyntaxProvider()

			AddHandler queryBuilder.SQLUpdated, AddressOf OnSQLUpdated

			queryBuilder.OfflineMode = True
			' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/XmlMetaData] key
			Try
				Dim path__1 = ConfigurationManager.AppSettings("XmlMetaData")
				Dim xml = Path.Combine(filterContext.HttpContext.Server.MapPath("~"), path__1)
				queryBuilder.MetadataContainer.ImportFromXML(xml)

				queryBuilder.MetadataStructure.Refresh()
				item.Message.Information("Metadata loaded")
			Catch ex As Exception
				Dim message As String = "Error loading metadata from the database." & "Check the 'configuration\connectionStrings' key in the [web.config] file."
				Logger.[Error](message, ex)
				item.Message.[Error](message & " Check log.txt for details.")
			End Try

			queryBuilder.SQL = "Select ""Employees"".""Employee ID"", ""Employees"".""First Name"", ""Employees"".""Last Name"", ""Employee Photos"".""Photo Image"", ""Employee Resumes"".Resume From ""Employee Photos"" Inner Join" & vbCr & vbLf & vbTab & vbTab & vbTab & """Employees"" On ""Employee Photos"".""Employee ID"" = ""Employees"".""Employee ID"" Inner Join" & vbCr & vbLf & vbTab & vbTab & vbTab & """Employee Resumes"" On ""Employee Resumes"".""Employee ID"" = ""Employees"".""Employee ID"""
		End Sub

		Public Sub OnSQLUpdated(sender As Object, e As EventArgs)
			Dim queryBuilder As QueryBuilder = SessionStore.Current.QueryBuilder
			Dim plainTextSQLBuilder As New PlainTextSQLBuilder() With { _
				Key .QueryBuilder = queryBuilder, _
				Key .KeywordFormat = KeywordFormat.UpperCase, _
				Key .UseAltNames = False _
			}
			Dim plainTextSQLBuilderWithAltNames As New PlainTextSQLBuilder() With { _
				Key .QueryBuilder = queryBuilder, _
				Key .KeywordFormat = KeywordFormat.UpperCase, _
				Key .UseAltNames = True _
			}

			SessionStore.Current.Exchange.Data = New ExchangeClass() With { _
				Key .SQL = plainTextSQLBuilder.SQL, _
				Key .AlternateSQL = plainTextSQLBuilderWithAltNames.SQL _
			}
		End Sub
	End Class
End Namespace
