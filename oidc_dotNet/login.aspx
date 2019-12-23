<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="oidc_dotNet.login" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="~/style.css" rel="stylesheet" />
</head>
<body runat="server">
   
        <form id="form1" runat="server">
   
        <div class="container">
        <div class="loading"><div class="loader">
         
        </div>
            
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="AD Auth test" Visible="False" />
            
    <%=User.Identity.Name%>
         <asp:TextBox ID="TextBox1" runat="server" Height="152px" Width="918px" Visible="False"></asp:TextBox>

         

        </form>
    
</body>
</html>
