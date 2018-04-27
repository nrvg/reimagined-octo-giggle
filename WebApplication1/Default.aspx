<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication1._Default" %>
<%@ Import Namespace="Dinos" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager runat="server" ID="scriptManager" AsyncPostBackTimeout="300" />
        <asp:UpdatePanel runat="server" ID="updPanel" UpdateMode="Conditional">
            <ContentTemplate>
                <h1><%= moveNumber %></h1>
                <table cellpadding="2px">
                    <tr>
                        <td><%= me.Name %></td>
                        <td><%= op.Name %></td>
                    </tr>
                    <tr>
                        <td>
                            <%if(me.Dinos.Count > 0) { %>
                            <div style="background-color:<%=me.Dinos[0].Color %>">
                                <%=me.Dinos[0].ToString() %>
                            </div>
                            <% } %>
                            <%if(me.Dinos.Count > 1) { %>
                            <div style="background-color:<%=me.Dinos[1].Color %>">
                                <%=me.Dinos[1].ToString() %>
                                <asp:LinkButton runat="server" ID="btnSwap1" OnClick="btnSwap1_Click" Text="swap"/>
                            </div>
                            <% } %>
                            <%if(me.Dinos.Count > 2) { %>
                            <div style="background-color:<%=me.Dinos[2].Color %>">
                                <%=me.Dinos[2].ToString() %>
                                <asp:LinkButton runat="server" ID="btnSwap2" OnClick="btnSwap2_Click" Text="swap"/>
                            </div>
                            <% } %>
                        </td>
                        <td>
                            <%if(op.Dinos.Count > 0) { %>
                            <div style="background-color:<%=op.Dinos[0].Color %>">
                                <%=op.Dinos[0].ToString() %>
                            </div>
                            <% } %>
                            <%if(op.Dinos.Count > 1) { %>
                            <div style="background-color:<%=op.Dinos[1].Color %>">
                                <%=op.Dinos[1].ToString() %>
                            </div>
                            <% } %>
                            <%if(op.Dinos.Count > 2) { %>
                            <div style="background-color:<%=op.Dinos[2].Color %>">
                                <%=op.Dinos[2].ToString() %>
                            </div>
                            <% } %>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            moves: <%= GetMovesLeft() %> / <%= Player.nextSeq(moveNumber) %>
                            <asp:Button runat="server" Text="S+" ID="btnSave" OnClick="btnSave_Click" style="margin-left:10px;"/><asp:Label runat="server" ID="save"/>
                            <asp:Button runat="server" Text="D+" ID="btnDef" OnClick="btnDef_Click" style="margin-left:10px;" /><asp:Label runat="server" ID="def" />
                            <asp:Button runat="server" Text="A+" ID="btnAtt" OnClick="btnAtt_Click" style="margin-left:10px;" /><asp:Label runat="server" ID="att" />
                        </td>
                        <td>
                            <asp:Label runat="server" ID="lbAi">
                            </asp:Label>
                        </td>
                    </tr>
<%--                    <tr>
                        <td>
                            <asp:LinkButton runat="server" ID="go" OnClick="go_Click" Text="go" />
                        </td>
                    </tr>--%>
                </table>
                <asp:UpdateProgress runat="server" ID="updateProgress" DisplayAfter="70">
                <progresstemplate>
						<div style="position:absolute; top:0;left:300px; background-color:red;">
                            <span>Laadimine...</span>
						</div>
					</progresstemplate>
            </asp:UpdateProgress>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    </form>
</body>
</html>
