<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ShowData.Default" %>

<%@ Register assembly="AspNetPager" namespace="Wuqi.Webdiyer" tagprefix="webdiyer" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>修改</title>
    <script src="laydate/laydate.js"></script>
    <script type="application/javascript">
		//function myrefresh(){ 
		//window.location.reload(); 
		//} 
        //setTimeout('myrefresh()',10000); //指定1秒刷新一次 

        //var _run;
        //function reload() {
        //    location.reload();
        //}

        //function stop() {
        //    clearInterval(_run);
        //}

        //function star() {
        //    clearInterval(_run);
        //    _run = setInterval("reload()", 60000);
        //}
        //_run = setInterval("reload()", 60000);

    </script>
    
    <style>

    a:link {
	text-decoration: none;
    color:#333333;
}
a:visited {
	text-decoration: none;
}
a:hover {
	text-decoration: none;
}
a:active {
	text-decoration: none;
}
body,td,th {
	font-family: 微软雅黑;
	font-size: 14px;
        padding: 0px;
    margin: 0px;
}
        td, th {
            height: 30px;
            line-height: 30px;
    padding-left: 10px;
    padding-right: 10px;
        }
        #AspNetPager1{
    height: 30px;
    line-height: 30px;
    /* padding-left: 20px; */
    float: right;
    /*padding-right: 40px;*/
        }
        #DropDownList1{vertical-align: text-top;}
</style>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
<%--    <meta http-equiv="refresh" content="10">--%>
</head>
<body>
    <form name="form1" id="form1" runat="server">
    <div>
    <div>
        <table border="0"  id="tdata">
  <tr>
      
    <td>上游订单号:</td>
    <td><asp:TextBox ID="tbOid" runat="server"></asp:TextBox></td>
    <td>用户名:</td>
    <td><asp:TextBox ID="tbUserName" runat="server"></asp:TextBox></td>
    <td><asp:DropDownList ID="DropDownList1" runat="server">
        <asp:ListItem Value="*">选择</asp:ListItem>
        <asp:ListItem Selected="True" Value="0">待处理</asp:ListItem>
        <asp:ListItem Value="2">已处理</asp:ListItem>
        <asp:ListItem Value="3">处理失败</asp:ListItem>
        </asp:DropDownList></td>      
    <td><asp:TextBox ID="tbstartTime" runat="server" ></asp:TextBox>
    </td>
    <td>日期:</td>
    <td><asp:TextBox ID="tbendTime" runat="server"  ></asp:TextBox></td>-
    <td><asp:Button ID="btnQuery" runat="server" Text="查询" OnClick="btnQuery_Click" Width="80px" /></td>

    <%--<td>自动刷新:</td>--%>

  </tr>
            
</table>

    </div>
    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False"  DataKeyNames="id"  OnRowDeleting="GridView1_RowDeleting" 
       OnRowEditing="GridView1_RowEditing"   OnRowCancelingEdit="GridView1_RowCancelingEdit"  OnRowUpdating="GridView1_RowUpdating" CellPadding="4" ForeColor="#333333" GridLines="None" Width="100%" OnRowDataBound="GridView1_RowDataBound">  
        <AlternatingRowStyle BackColor="White" />
<Columns>  

                <asp:BoundField DataField="id" HeaderText="编号" InsertVisible="False" ReadOnly="True"  HeaderStyle-Width="10%"  ItemStyle-HorizontalAlign="Center" >
<HeaderStyle Width="10%"></HeaderStyle>

<ItemStyle HorizontalAlign="Center"></ItemStyle>
                </asp:BoundField>
                <asp:BoundField DataField="oid" HeaderText="上游订单号"  ReadOnly="true" HeaderStyle-Width="30%"  ItemStyle-HorizontalAlign="Left" >
<HeaderStyle Width="30%"></HeaderStyle>

<ItemStyle HorizontalAlign="Left"></ItemStyle>
                </asp:BoundField>
                <asp:BoundField DataField="username" HeaderText="用户名"  HeaderStyle-Width="20%"  ItemStyle-HorizontalAlign="Left" >
<HeaderStyle Width="20%"></HeaderStyle>

<ItemStyle HorizontalAlign="Left"></ItemStyle>
                </asp:BoundField>
                <asp:BoundField DataField="deposit" HeaderText="存款"  ReadOnly="true" HeaderStyle-Width="10%"  ItemStyle-HorizontalAlign="Left" >
<HeaderStyle Width="10%"></HeaderStyle>

<ItemStyle HorizontalAlign="Left"></ItemStyle>
                </asp:BoundField>
                <asp:BoundField DataField="subtime" HeaderText="提交时间"  ReadOnly="true" HeaderStyle-Width="10%"  ItemStyle-HorizontalAlign="Left" >
<HeaderStyle Width="10%"></HeaderStyle>

<ItemStyle HorizontalAlign="Left"></ItemStyle>
                </asp:BoundField>
                <asp:TemplateField HeaderText="状态">
<%--                    <EditItemTemplate>
                        <asp:Label ID="Label1" runat="server" Text='<%# Eval("state") %>'></asp:Label>
                    </EditItemTemplate>--%>
                    <ItemTemplate>
                        <asp:Label ID="Label1" runat="server" Text='<%#formatStr( Eval("state").ToString()) %>'></asp:Label>
                    </ItemTemplate>
                    <HeaderStyle Width="10%" />
                    <ItemStyle HorizontalAlign="Center" />
                </asp:TemplateField>

    <asp:TemplateField HeaderText="操作" HeaderStyle-Width="10%"  ItemStyle-HorizontalAlign="Center" >
        
        <EditItemTemplate>
            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True" 
                CommandName="Update" Text="更新"></asp:LinkButton>
            <asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" 
                CommandName="Cancel" Text="取消"></asp:LinkButton>
        </EditItemTemplate>
        <ItemTemplate>
            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False"  CommandName="Edit" Text="编辑" ></asp:LinkButton>
            <asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False"  CommandName="Delete" OnClientClick="javascript:return confirm('确认要删除么?');"  Text="删除"></asp:LinkButton>
        </ItemTemplate>

<HeaderStyle Width="10%"></HeaderStyle>

<ItemStyle HorizontalAlign="Center"></ItemStyle>
    </asp:TemplateField>
</Columns>  
        <EditRowStyle BackColor="#7C6F57" />
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="#E3EAEB" />
        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#F8FAFA" />
        <SortedAscendingHeaderStyle BackColor="#246B61" />
        <SortedDescendingCellStyle BackColor="#D4DFE1" />
        <SortedDescendingHeaderStyle BackColor="#15524A" />
</asp:GridView> 
    <webdiyer:AspNetPager ID="AspNetPager1" runat="server" 
        CssClass=""         CurrentPageButtonClass="" FirstPageText="首页" 
        LastPageText="尾页"         NextPageText="后页" PrevPageText="前页" AlwaysShow="True" 
        NumericButtonCount="3"  
        onpagechanging="AspNetPager1_PageChanging1" CustomInfoClass="" Direction="LeftToRight">
    </webdiyer:AspNetPager>

    </div>
    </form>
    <script>
//执行一个laydate实例
laydate.render({
    elem: '#tbstartTime' //指定元素
    , type: 'datetime'
});
laydate.render({
    elem: '#tbendTime' //指定元素
    , type: 'datetime'
});
</script>
</body>
</html>
