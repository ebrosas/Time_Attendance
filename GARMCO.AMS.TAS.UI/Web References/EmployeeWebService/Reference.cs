﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace GARMCO.AMS.TAS.UI.EmployeeWebService {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1590.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="EmployeeSoap", Namespace="http://www.garmco.com/")]
    public partial class Employee : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback GetEmployeeByDomainNameOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetEmployeeByEmpNoOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetEmployeeListOperationCompleted;
        
        private System.Threading.SendOrPostCallback LoginOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public Employee() {
            this.Url = global::GARMCO.AMS.TAS.UI.Properties.Settings.Default.GARMCO_AMS_TAS_UI_EmployeeWebService_Employee;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event GetEmployeeByDomainNameCompletedEventHandler GetEmployeeByDomainNameCompleted;
        
        /// <remarks/>
        public event GetEmployeeByEmpNoCompletedEventHandler GetEmployeeByEmpNoCompleted;
        
        /// <remarks/>
        public event GetEmployeeListCompletedEventHandler GetEmployeeListCompleted;
        
        /// <remarks/>
        public event LoginCompletedEventHandler LoginCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.garmco.com/GetEmployeeByDomainName", RequestNamespace="http://www.garmco.com/", ResponseNamespace="http://www.garmco.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public EmployeeInfo GetEmployeeByDomainName(string loginName) {
            object[] results = this.Invoke("GetEmployeeByDomainName", new object[] {
                        loginName});
            return ((EmployeeInfo)(results[0]));
        }
        
        /// <remarks/>
        public void GetEmployeeByDomainNameAsync(string loginName) {
            this.GetEmployeeByDomainNameAsync(loginName, null);
        }
        
        /// <remarks/>
        public void GetEmployeeByDomainNameAsync(string loginName, object userState) {
            if ((this.GetEmployeeByDomainNameOperationCompleted == null)) {
                this.GetEmployeeByDomainNameOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetEmployeeByDomainNameOperationCompleted);
            }
            this.InvokeAsync("GetEmployeeByDomainName", new object[] {
                        loginName}, this.GetEmployeeByDomainNameOperationCompleted, userState);
        }
        
        private void OnGetEmployeeByDomainNameOperationCompleted(object arg) {
            if ((this.GetEmployeeByDomainNameCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetEmployeeByDomainNameCompleted(this, new GetEmployeeByDomainNameCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.garmco.com/GetEmployeeByEmpNo", RequestNamespace="http://www.garmco.com/", ResponseNamespace="http://www.garmco.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public EmployeeInfo GetEmployeeByEmpNo(string employeeNo) {
            object[] results = this.Invoke("GetEmployeeByEmpNo", new object[] {
                        employeeNo});
            return ((EmployeeInfo)(results[0]));
        }
        
        /// <remarks/>
        public void GetEmployeeByEmpNoAsync(string employeeNo) {
            this.GetEmployeeByEmpNoAsync(employeeNo, null);
        }
        
        /// <remarks/>
        public void GetEmployeeByEmpNoAsync(string employeeNo, object userState) {
            if ((this.GetEmployeeByEmpNoOperationCompleted == null)) {
                this.GetEmployeeByEmpNoOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetEmployeeByEmpNoOperationCompleted);
            }
            this.InvokeAsync("GetEmployeeByEmpNo", new object[] {
                        employeeNo}, this.GetEmployeeByEmpNoOperationCompleted, userState);
        }
        
        private void OnGetEmployeeByEmpNoOperationCompleted(object arg) {
            if ((this.GetEmployeeByEmpNoCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetEmployeeByEmpNoCompleted(this, new GetEmployeeByEmpNoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.garmco.com/GetEmployeeList", RequestNamespace="http://www.garmco.com/", ResponseNamespace="http://www.garmco.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public EmployeeInfo[] GetEmployeeList() {
            object[] results = this.Invoke("GetEmployeeList", new object[0]);
            return ((EmployeeInfo[])(results[0]));
        }
        
        /// <remarks/>
        public void GetEmployeeListAsync() {
            this.GetEmployeeListAsync(null);
        }
        
        /// <remarks/>
        public void GetEmployeeListAsync(object userState) {
            if ((this.GetEmployeeListOperationCompleted == null)) {
                this.GetEmployeeListOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetEmployeeListOperationCompleted);
            }
            this.InvokeAsync("GetEmployeeList", new object[0], this.GetEmployeeListOperationCompleted, userState);
        }
        
        private void OnGetEmployeeListOperationCompleted(object arg) {
            if ((this.GetEmployeeListCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetEmployeeListCompleted(this, new GetEmployeeListCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.garmco.com/Login", RequestNamespace="http://www.garmco.com/", ResponseNamespace="http://www.garmco.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public EmployeeInfo Login(string loginName, string password, ref int retError) {
            object[] results = this.Invoke("Login", new object[] {
                        loginName,
                        password,
                        retError});
            retError = ((int)(results[1]));
            return ((EmployeeInfo)(results[0]));
        }
        
        /// <remarks/>
        public void LoginAsync(string loginName, string password, int retError) {
            this.LoginAsync(loginName, password, retError, null);
        }
        
        /// <remarks/>
        public void LoginAsync(string loginName, string password, int retError, object userState) {
            if ((this.LoginOperationCompleted == null)) {
                this.LoginOperationCompleted = new System.Threading.SendOrPostCallback(this.OnLoginOperationCompleted);
            }
            this.InvokeAsync("Login", new object[] {
                        loginName,
                        password,
                        retError}, this.LoginOperationCompleted, userState);
        }
        
        private void OnLoginOperationCompleted(object arg) {
            if ((this.LoginCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.LoginCompleted(this, new LoginCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.garmco.com/")]
    public partial class EmployeeInfo {
        
        private string usernameField;
        
        private string fullNameField;
        
        private string emailField;
        
        private string employeeNoField;
        
        private string costCenterField;
        
        private string costCenterNameField;
        
        private string companyField;
        
        private string extensionNoField;
        
        private System.Nullable<System.DateTime> dateOfBirthField;
        
        private string genderField;
        
        private string destinationField;
        
        private int payGradeField;
        
        private string employeeClassField;
        
        private string ticketClassField;
        
        private string positionIDField;
        
        private string positionDescField;
        
        private int supervisorEmpNoField;
        
        private string supervisorEmpNameField;
        
        private string supervisorLeaveReasonField;
        
        private int superintendentEmpNoField;
        
        private string superintendentEmpNameField;
        
        private int managerEmpNoField;
        
        private string managerEmpNameField;
        
        /// <remarks/>
        public string Username {
            get {
                return this.usernameField;
            }
            set {
                this.usernameField = value;
            }
        }
        
        /// <remarks/>
        public string FullName {
            get {
                return this.fullNameField;
            }
            set {
                this.fullNameField = value;
            }
        }
        
        /// <remarks/>
        public string Email {
            get {
                return this.emailField;
            }
            set {
                this.emailField = value;
            }
        }
        
        /// <remarks/>
        public string EmployeeNo {
            get {
                return this.employeeNoField;
            }
            set {
                this.employeeNoField = value;
            }
        }
        
        /// <remarks/>
        public string CostCenter {
            get {
                return this.costCenterField;
            }
            set {
                this.costCenterField = value;
            }
        }
        
        /// <remarks/>
        public string CostCenterName {
            get {
                return this.costCenterNameField;
            }
            set {
                this.costCenterNameField = value;
            }
        }
        
        /// <remarks/>
        public string Company {
            get {
                return this.companyField;
            }
            set {
                this.companyField = value;
            }
        }
        
        /// <remarks/>
        public string ExtensionNo {
            get {
                return this.extensionNoField;
            }
            set {
                this.extensionNoField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public System.Nullable<System.DateTime> DateOfBirth {
            get {
                return this.dateOfBirthField;
            }
            set {
                this.dateOfBirthField = value;
            }
        }
        
        /// <remarks/>
        public string Gender {
            get {
                return this.genderField;
            }
            set {
                this.genderField = value;
            }
        }
        
        /// <remarks/>
        public string Destination {
            get {
                return this.destinationField;
            }
            set {
                this.destinationField = value;
            }
        }
        
        /// <remarks/>
        public int PayGrade {
            get {
                return this.payGradeField;
            }
            set {
                this.payGradeField = value;
            }
        }
        
        /// <remarks/>
        public string EmployeeClass {
            get {
                return this.employeeClassField;
            }
            set {
                this.employeeClassField = value;
            }
        }
        
        /// <remarks/>
        public string TicketClass {
            get {
                return this.ticketClassField;
            }
            set {
                this.ticketClassField = value;
            }
        }
        
        /// <remarks/>
        public string PositionID {
            get {
                return this.positionIDField;
            }
            set {
                this.positionIDField = value;
            }
        }
        
        /// <remarks/>
        public string PositionDesc {
            get {
                return this.positionDescField;
            }
            set {
                this.positionDescField = value;
            }
        }
        
        /// <remarks/>
        public int SupervisorEmpNo {
            get {
                return this.supervisorEmpNoField;
            }
            set {
                this.supervisorEmpNoField = value;
            }
        }
        
        /// <remarks/>
        public string SupervisorEmpName {
            get {
                return this.supervisorEmpNameField;
            }
            set {
                this.supervisorEmpNameField = value;
            }
        }
        
        /// <remarks/>
        public string SupervisorLeaveReason {
            get {
                return this.supervisorLeaveReasonField;
            }
            set {
                this.supervisorLeaveReasonField = value;
            }
        }
        
        /// <remarks/>
        public int SuperintendentEmpNo {
            get {
                return this.superintendentEmpNoField;
            }
            set {
                this.superintendentEmpNoField = value;
            }
        }
        
        /// <remarks/>
        public string SuperintendentEmpName {
            get {
                return this.superintendentEmpNameField;
            }
            set {
                this.superintendentEmpNameField = value;
            }
        }
        
        /// <remarks/>
        public int ManagerEmpNo {
            get {
                return this.managerEmpNoField;
            }
            set {
                this.managerEmpNoField = value;
            }
        }
        
        /// <remarks/>
        public string ManagerEmpName {
            get {
                return this.managerEmpNameField;
            }
            set {
                this.managerEmpNameField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1590.0")]
    public delegate void GetEmployeeByDomainNameCompletedEventHandler(object sender, GetEmployeeByDomainNameCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1590.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetEmployeeByDomainNameCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetEmployeeByDomainNameCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public EmployeeInfo Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((EmployeeInfo)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1590.0")]
    public delegate void GetEmployeeByEmpNoCompletedEventHandler(object sender, GetEmployeeByEmpNoCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1590.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetEmployeeByEmpNoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetEmployeeByEmpNoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public EmployeeInfo Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((EmployeeInfo)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1590.0")]
    public delegate void GetEmployeeListCompletedEventHandler(object sender, GetEmployeeListCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1590.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetEmployeeListCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetEmployeeListCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public EmployeeInfo[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((EmployeeInfo[])(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1590.0")]
    public delegate void LoginCompletedEventHandler(object sender, LoginCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1590.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class LoginCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal LoginCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public EmployeeInfo Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((EmployeeInfo)(this.results[0]));
            }
        }
        
        /// <remarks/>
        public int retError {
            get {
                this.RaiseExceptionIfNecessary();
                return ((int)(this.results[1]));
            }
        }
    }
}

#pragma warning restore 1591