Imports System.Configuration
Imports System.IO
Imports ActiveDatabaseSoftware.ActiveQueryBuilder

Public Partial Class QueryBuilderToggleUseAltNames
	Inherits System.Web.UI.UserControl
	Public Sub QueryBuilderControl1_Init(sender As Object, e As EventArgs)
		' Get instance of QueryBuilder
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder

		queryBuilder.SyntaxProvider = New DB2SyntaxProvider()

		queryBuilder.BehaviorOptions.UseAltNames = True

		queryBuilder.OfflineMode = True

		Dim path__1 = ConfigurationManager.AppSettings("XmlMetaData")
		Dim xml = Path.Combine(Server.MapPath(""), path__1)
		queryBuilder.MetadataContainer.ImportFromXML(xml)
	End Sub

	Protected Sub ToggleOnClick(sender As Object, e As EventArgs)
		QueryBuilderControl1.QueryBuilder.BehaviorOptions.UseAltNames = Not QueryBuilderControl1.QueryBuilder.BehaviorOptions.UseAltNames
		QueryBuilderControl1.PlainTextSQLBuilder.UseAltNames = Not QueryBuilderControl1.PlainTextSQLBuilder.UseAltNames
		QueryBuilderControl1.QueryBuilder.MetadataStructure.Refresh()
	End Sub
End Class
