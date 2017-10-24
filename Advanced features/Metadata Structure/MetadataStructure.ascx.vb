Imports System.Configuration
Imports System.IO
Imports System.Xml
Imports ActiveDatabaseSoftware.ActiveQueryBuilder
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Control
Imports System.Web.UI
Imports ActiveDatabaseSoftware.ActiveQueryBuilder.Web.Server

Partial Public Class MetadataStructure
	Inherits System.Web.UI.UserControl

	Public Sub Page_Load(sender As Object, e As EventArgs)
		If Not Page.IsPostBack Then
			SQLEditor1.SQL = "SELECT Orders.OrderID, Orders.CustomerID, Orders.OrderDate, [Order Details].ProductID, [Order Details].UnitPrice, [Order Details].Quantity, [Order Details].Discount FROM Orders INNER JOIN [Order Details] ON Orders.OrderID = [Order Details].OrderID WHERE Orders.OrderID > 0 AND [Order Details].Discount > 0"
		End If
	End Sub

	Public Sub SleepModeChanged(sender As Object, e As EventArgs)
		Dim queryBuilder As QueryBuilder = QueryBuilderControl1.QueryBuilder
		If queryBuilder.SleepMode Then StatusBar1.Message.Error("Unsupported SQL statement.")
	End Sub

	Public Sub QueryBuilderControl1_Init(ByVal sender As Object, ByVal e As EventArgs)
		Dim control1 As QueryBuilderControl = QueryBuilderControl1

		' Get instance of QueryBuilder
		Dim queryBuilder As QueryBuilder = control1.QueryBuilder
		' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
		queryBuilder.BehaviorOptions.AllowSleepMode = True
		queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

		queryBuilder.OfflineMode = True
		' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/XmlMetaData] key
		' Pass loaded XML to QueryBuilder component
		Try
			queryBuilder.MetadataContainer.ImportFromXML(Page.Server.MapPath(ConfigurationManager.AppSettings("XmlMetaData")))
		Catch ex As Exception
			Dim message As String = "Can't load metadata from XML."
			Logger.[Error](message, ex)
			StatusBar1.Message.Error(message & " Check log.txt for details.")
		End Try

		' Initialization of the Metadata Structure object that's
		' responsible for representation of metadata in a tree-like form
		Try
			' Disable the automatic metadata structure creation
			queryBuilder.MetadataStructure.AllowChildAutoItems = False

			' queryBuilder.DatabaseSchemaTreeOptions.DefaultExpandLevel = 0;

			Dim filter As MetadataFilterItem

			' Create a top-level folder containing all objects
			Dim allObjects As New MetadataStructureItem()
			allObjects.Caption = "All objects"
			allObjects.ImageIndex = queryBuilder.MetadataStructure.Structure.Options.FolderImageIndex
			filter = allObjects.MetadataFilter.Add()
			filter.ObjectTypes = MetadataType.All
			queryBuilder.MetadataStructure.Items.Add(allObjects)

			' Create "Favorites" folder
			Dim favorites As New MetadataStructureItem()
			favorites.Caption = "Favorites"
			favorites.ImageIndex = queryBuilder.MetadataStructure.Structure.Options.FolderImageIndex
			queryBuilder.MetadataStructure.Items.Add(favorites)

			Dim metadataItem As MetadataItem
			Dim item As MetadataStructureItem

			' Add some metadata objects to "Favorites" folder
			metadataItem = queryBuilder.MetadataContainer.FindItem(Of MetadataItem)("Orders")
			item = New MetadataStructureItem()
			item.MetadataItem = metadataItem
			item.ImageIndex = queryBuilder.MetadataStructure.Structure.Options.UserTableImageIndex
			favorites.Items.Add(item)

			metadataItem = queryBuilder.MetadataContainer.FindItem(Of MetadataItem)("Order Details")
			item = New MetadataStructureItem()
			item.MetadataItem = metadataItem
			item.ImageIndex = queryBuilder.MetadataStructure.Structure.Options.UserTableImageIndex
			favorites.Items.Add(item)

			' Create folder with filter
			Dim filteredFolder As New MetadataStructureItem()
			' creates dynamic node
			filteredFolder.Caption = "Filtered by 'Prod%'"
			filteredFolder.ImageIndex = queryBuilder.MetadataStructure.Structure.Options.FolderImageIndex
			filter = filteredFolder.MetadataFilter.Add()
			filter.ObjectTypes = MetadataType.Table Or MetadataType.View
			filter.Object = "Prod%"
			queryBuilder.MetadataStructure.Items.Add(filteredFolder)


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

	Private Function LoadXml(ByVal xmlFileName As String) As XmlDocument
		Dim doc As New XmlDocument()
		Try
			doc.Load(xmlFileName)
		Catch ex As Exception
			Dim message As String
			Dim control1 As QueryBuilderControl = QueryBuilderControl1
			If TypeOf ex Is FileNotFoundException Then
				message = String.Format("Can't find XML file '{0}'.", xmlFileName)
				Logger.[Error](message, ex)
				StatusBar1.Message.Error(message & " Check log.txt for details.")
			ElseIf TypeOf ex Is IOException Then
				message = String.Format("Can't read XML file '{0}'.", xmlFileName)
				Logger.[Error](message, ex)
				StatusBar1.Message.Error(message & " Check log.txt for details.")
			ElseIf TypeOf ex Is XmlException Then
				message = String.Format("Can't parse XML file '{0}'.", xmlFileName)
				Logger.[Error](message, ex)
				StatusBar1.Message.Error(message & " Check log.txt for details.")
			Else
				message = String.Format("Unknown error during load XML file '{0}'", xmlFileName)
				Logger.[Error](message, ex)
				StatusBar1.Message.Error(message & " Check log.txt for details.")
			End If
			doc = Nothing
		End Try
		Return doc
	End Function
End Class
