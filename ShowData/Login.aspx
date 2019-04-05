<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ShowData.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <style>
        #Image1{
            vertical-align: top;
        }
        #form1{
            margin-top: 150px;
        }
        td{
            height:35px;
            line-height:35px;
        }
        .input{
                height: 20px;
    line-height: 20px;
    border-left: none;
    border-right: none;
    border-top: none;
    border-bottom: 1px solid #000;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <table width="50%" border="0" align="center" width="400px">
  <tr>
    <td width="50%" align="right">用户名：</td>
    <td>
        <asp:TextBox ID="TextBox1" runat="server" CssClass="input"></asp:TextBox>      </td>
  </tr>
  <tr>
    <td align="right">密码：</td>
    <td>
        <asp:TextBox ID="TextBox2" runat="server" TextMode="Password" CssClass="input"></asp:TextBox>      </td>
  </tr>
  <tr>
    <td align="right">验证码：</td>
    <td>
        <asp:TextBox ID="TextBox3" runat="server" CssClass="input"></asp:TextBox>      
        <asp:Image ID="Image1" runat="server" Height="25px" Width="80px" ImageUrl="~/GetVerifyCode.ashx" onclick="javascript:this.src='GetVerifyCode.ashx?tm='+Math.random()" />
        <%--<img src="GetVerifyCode.ashx" alt="点击刷新" runat="server" height="25px" width="80px"  onclick="javascript:this.src='GetVerifyCode.ashx?tm='+Math.random()"/>--%>
      </td>
  </tr>
  <tr>
      <td colspan="2" align="center">       
          <asp:Button ID="Button1" runat="server" Text="登陆" OnClick="Button1_Click" style="width: 70px; height: 25px" />        
        
          <asp:Button ID="Button2" runat="server" Text="清空" style="width: 70px; height: 25px" OnClick="Button2_Click" />        

           </td>
      </tr>
  <tr>
      <td colspan="2" align="center">            
           <asp:Label ID="Label1" runat="server" ForeColor="Red" Font-Size="12px"></asp:Label>
           </td>
      </tr>      
</table>

    </div>
    </form>
</body>
</html>
