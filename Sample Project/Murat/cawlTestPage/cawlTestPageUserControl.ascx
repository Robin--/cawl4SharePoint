﻿<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="cawlTestPageUserControl.ascx.cs" Inherits="Murat.cawlTestPage.cawlTestPageUserControl" %>


<p>
    <br />
    <asp:Button ID="Button4" runat="server" onclick="Button4_Click" 
        Text="Get list items" />
&nbsp;
    <asp:Button ID="Button1" runat="server" onclick="Button1_Click" Text="Insert" 
        style="height: 26px" />
&nbsp;
    <asp:Button ID="Button2" runat="server" onclick="Button2_Click" Text="Update" />
&nbsp;&nbsp;
    <asp:Button ID="Button3" runat="server" onclick="Button3_Click" Text="Delete" />
    &nbsp;</p>


<p>
    <br />
    Caml Query:
    <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
</p>


<asp:GridView ID="GridView1" runat="server">
</asp:GridView>

<asp:Label ID="Label2" runat="server" Text="Label"></asp:Label>


