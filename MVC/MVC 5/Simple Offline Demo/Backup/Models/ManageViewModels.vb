Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports Microsoft.AspNet.Identity
Imports Microsoft.Owin.Security

Namespace Models
	Public Class IndexViewModel
		Public Property HasPassword() As Boolean
			Get
				Return m_HasPassword
			End Get
			Set
				m_HasPassword = Value
			End Set
		End Property
		Private m_HasPassword As Boolean
		Public Property Logins() As IList(Of UserLoginInfo)
			Get
				Return m_Logins
			End Get
			Set
				m_Logins = Value
			End Set
		End Property
		Private m_Logins As IList(Of UserLoginInfo)
		Public Property PhoneNumber() As String
			Get
				Return m_PhoneNumber
			End Get
			Set
				m_PhoneNumber = Value
			End Set
		End Property
		Private m_PhoneNumber As String
		Public Property TwoFactor() As Boolean
			Get
				Return m_TwoFactor
			End Get
			Set
				m_TwoFactor = Value
			End Set
		End Property
		Private m_TwoFactor As Boolean
		Public Property BrowserRemembered() As Boolean
			Get
				Return m_BrowserRemembered
			End Get
			Set
				m_BrowserRemembered = Value
			End Set
		End Property
		Private m_BrowserRemembered As Boolean
	End Class

	Public Class ManageLoginsViewModel
		Public Property CurrentLogins() As IList(Of UserLoginInfo)
			Get
				Return m_CurrentLogins
			End Get
			Set
				m_CurrentLogins = Value
			End Set
		End Property
		Private m_CurrentLogins As IList(Of UserLoginInfo)
		Public Property OtherLogins() As IList(Of AuthenticationDescription)
			Get
				Return m_OtherLogins
			End Get
			Set
				m_OtherLogins = Value
			End Set
		End Property
		Private m_OtherLogins As IList(Of AuthenticationDescription)
	End Class

	Public Class FactorViewModel
		Public Property Purpose() As String
			Get
				Return m_Purpose
			End Get
			Set
				m_Purpose = Value
			End Set
		End Property
		Private m_Purpose As String
	End Class

	Public Class SetPasswordViewModel
		<Required> _
		<StringLength(100, ErrorMessage := "The {0} must be at least {2} characters long.", MinimumLength := 6)> _
		<DataType(DataType.Password)> _
		<Display(Name := "New password")> _
		Public Property NewPassword() As String
			Get
				Return m_NewPassword
			End Get
			Set
				m_NewPassword = Value
			End Set
		End Property
		Private m_NewPassword As String

		<DataType(DataType.Password)> _
		<Display(Name := "Confirm new password")> _
		<Compare("NewPassword", ErrorMessage := "The new password and confirmation password do not match.")> _
		Public Property ConfirmPassword() As String
			Get
				Return m_ConfirmPassword
			End Get
			Set
				m_ConfirmPassword = Value
			End Set
		End Property
		Private m_ConfirmPassword As String
	End Class

	Public Class ChangePasswordViewModel
		<Required> _
		<DataType(DataType.Password)> _
		<Display(Name := "Current password")> _
		Public Property OldPassword() As String
			Get
				Return m_OldPassword
			End Get
			Set
				m_OldPassword = Value
			End Set
		End Property
		Private m_OldPassword As String

		<Required> _
		<StringLength(100, ErrorMessage := "The {0} must be at least {2} characters long.", MinimumLength := 6)> _
		<DataType(DataType.Password)> _
		<Display(Name := "New password")> _
		Public Property NewPassword() As String
			Get
				Return m_NewPassword
			End Get
			Set
				m_NewPassword = Value
			End Set
		End Property
		Private m_NewPassword As String

		<DataType(DataType.Password)> _
		<Display(Name := "Confirm new password")> _
		<Compare("NewPassword", ErrorMessage := "The new password and confirmation password do not match.")> _
		Public Property ConfirmPassword() As String
			Get
				Return m_ConfirmPassword
			End Get
			Set
				m_ConfirmPassword = Value
			End Set
		End Property
		Private m_ConfirmPassword As String
	End Class

	Public Class AddPhoneNumberViewModel
		<Required> _
		<Phone> _
		<Display(Name := "Phone Number")> _
		Public Property Number() As String
			Get
				Return m_Number
			End Get
			Set
				m_Number = Value
			End Set
		End Property
		Private m_Number As String
	End Class

	Public Class VerifyPhoneNumberViewModel
		<Required> _
		<Display(Name := "Code")> _
		Public Property Code() As String
			Get
				Return m_Code
			End Get
			Set
				m_Code = Value
			End Set
		End Property
		Private m_Code As String

		<Required> _
		<Phone> _
		<Display(Name := "Phone Number")> _
		Public Property PhoneNumber() As String
			Get
				Return m_PhoneNumber
			End Get
			Set
				m_PhoneNumber = Value
			End Set
		End Property
		Private m_PhoneNumber As String
	End Class

	Public Class ConfigureTwoFactorViewModel
		Public Property SelectedProvider() As String
			Get
				Return m_SelectedProvider
			End Get
			Set
				m_SelectedProvider = Value
			End Set
		End Property
		Private m_SelectedProvider As String
		Public Property Providers() As ICollection(Of System.Web.Mvc.SelectListItem)
			Get
				Return m_Providers
			End Get
			Set
				m_Providers = Value
			End Set
		End Property
		Private m_Providers As ICollection(Of System.Web.Mvc.SelectListItem)
	End Class
End Namespace
