Imports System.Collections
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Data
Imports System.Data.OleDb
Imports System.Dynamic
Imports System.IO
Imports System.Linq
Imports System.Web
Imports System.Web.Mvc
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.QueryTransformer
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Mvc.Filters
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server.Models

Namespace Controllers
	Public Class HomeController
		Inherits Controller
		Public Const PageSize As Integer = 10

		Public Function RefreshQueryResultPartial(Optional page As Integer = 1, Optional sort As String = "", Optional sortdir As String = "") As ActionResult
			ViewBag.CurrentPage = page
			Dim model As Object = Nothing

			Dim queryBuilder As QueryBuilder = SessionStore.Current.QueryBuilder
			Dim queryTransformer As QueryTransformer = SessionStore.Current.QueryTransformer

			queryTransformer.Sortings.Clear()
			If String.IsNullOrEmpty(sortdir) Then
				sortdir = "ASC"
			End If
			Dim column As OutputColumn = Nothing
			If Not String.IsNullOrEmpty(sort) Then
				column = queryTransformer.Columns.FirstOrDefault(Function(c) c.Name = sort AndAlso c.IsSupportSorting)
			End If
			If column Is Nothing Then
				column = queryTransformer.Columns.FirstOrDefault(Function(c) Not String.IsNullOrEmpty(c.Name) AndAlso c.IsSupportSorting)
			End If
			If column IsNot Nothing Then
				queryTransformer.OrderBy(column, sortdir = "ASC")
			End If

			queryTransformer.Take(PageSize.ToString()).Skip(((page - 1) * PageSize).ToString())

			If queryBuilder.MetadataProvider IsNot Nothing Then
				Dim cmd = DirectCast(queryBuilder.MetadataProvider.Connection.CreateCommand(), OleDbCommand)
				cmd.CommandTimeout = 30
				ViewBag.Sql = InlineAssignHelper(cmd.CommandText, queryTransformer.Sql)

				Try
					If cmd.Connection.State = ConnectionState.Open Then
						cmd.Connection.Close()
					End If

					For Each paramDto As QueryParamDto In SessionStore.Current.ClientQueryParams
						cmd.Parameters.AddWithValue(paramDto.Name, paramDto.Value)
					Next

					Dim adapter = New OleDbDataAdapter(cmd)
					Dim dataset = New DataSet()
					adapter.Fill(dataset, "QueryResult")
					model = ConvertToDictionary(dataset.Tables("QueryResult"))

					ViewBag.RowCount = GetRowCount(queryBuilder, queryTransformer)
				Catch ex As Exception
					Dim message As String = "Execute query error!"
					Logger.[Error](message, ex)
					ViewBag.Message = message & " " & ex.Message
				End Try
			End If

			Return PartialView("_QueryResultPartial", model)
		End Function

		Private Function GetRowCount(queryBuilder As QueryBuilder, queryTransformer As QueryTransformer) As Integer
			Try
				queryBuilder.MetadataProvider.Connection.Open()

				queryTransformer.ResultOffset = Nothing
				queryTransformer.ResultCount = Nothing
				Dim selectedColumn = New SelectedColumn(Nothing, "count(*)")
				queryTransformer.Aggregations.Add(selectedColumn, "cnt")
				Dim cmd = DirectCast(queryBuilder.MetadataProvider.Connection.CreateCommand(), OleDbCommand)
				cmd.CommandTimeout = 30
				cmd.CommandText = queryTransformer.Sql
				queryTransformer.Aggregations.Remove(selectedColumn)
				Return CInt(cmd.ExecuteScalar())
			Catch ex As Exception
				Dim message As String = "GetRowCount error!"
				Logger.[Error](message, ex)
				Throw
			End Try
		End Function

		Private Function ConvertToDictionary(dtObject As DataTable) As List(Of object)
			Dim columns As IEnumerable(Of DataColumn) = dtObject.Columns.Cast(Of DataColumn)()

			Dim dictionaryList As List(Of Dictionary(Of string, object)) = dtObject.AsEnumerable().[Select](Function(dataRow) columns.[Select](Function(column) New With { _
				.Column = column.ColumnName, _
				.Value = dataRow(column) _
			}).ToDictionary(Function(data) data.Column, Function(data) data.Value)).ToList()
            
			Dim result As List(Of object) = New List(Of object)()
			For Each emprow As IDictionary In dictionaryList
				Dim row As IDictionary(Of String, Object) = DirectCast(New ExpandoObject(), IDictionary(Of String, Object))

				For Each keyValuePair As KeyValuePair(Of String, Object) In DirectCast(emprow, Dictionary(Of String, Object))
					row.Add(keyValuePair)
				Next
				result.Add(row)
			Next
			Return result
		End Function

		<QueryBuilderInit> _
		Public Function Index() As ActionResult
			Return View()
		End Function
		Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
			target = value
			Return value
		End Function
	End Class

	Friend Class QueryBuilderInitAttribute
		Inherits InitializeQueryBuilderAttribute
		Protected Overrides Sub Init(filterContext As ActionExecutingContext, item As SessionStoreItem)
			' Get instance of QueryBuilder
			Dim queryBuilder = item.QueryBuilder
			queryBuilder.OfflineMode = False
			queryBuilder.SyntaxProvider = New MSAccessSyntaxProvider()

			Try
				' you may load metadata from the database connection using live database connection and metadata provider
				Dim connection = CreateConnection(filterContext.HttpContext.Server)
				queryBuilder.MetadataProvider = New OLEDBMetadataProvider() With { _
					.Connection = connection _
				}
			Catch ex As Exception
				Dim message As String = "Can't setup metadata provider!"
				Logger.[Error](message, ex)
				SessionStore.Current.Message.[Error](message)
				Return
			End Try

			Try
				queryBuilder.MetadataStructure.Refresh()
			Catch ex As Exception
				Dim message As String = "Error loading metadata from the database. Check [web.config] key <configuration>/<connectionStrings><add key=""YourDB"" connectionString=""..."">"
				Logger.[Error](message, ex)
				SessionStore.Current.Message.[Error](message)
				queryBuilder.OfflineMode = True
			End Try
		End Sub

		Private Function CreateConnection(server As HttpServerUtilityBase) As IDbConnection
			'var provider = "Microsoft.ACE.OLEDB.12.0";
			Dim provider = "Microsoft.Jet.OLEDB.4.0"
			Dim path__1 = ConfigurationManager.AppSettings("dbpath")
			Dim xml = Path.Combine(server.MapPath("~"), path__1)
			Dim connectionString = String.Format("Provider={0};Data Source={1};Persist Security Info=False;", provider, xml)
			Return New OleDbConnection(connectionString)
		End Function
	End Class
End Namespace
