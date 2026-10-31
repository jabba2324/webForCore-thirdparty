<%@ Page Language="C#" %>
<%@ Register TagPrefix="ajaxToolkit" Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" %>
<!DOCTYPE html>
<html>
<head runat="server"><title>AjaxControlToolkit smoke test</title></head>
<body>
    <form runat="server">
        <asp:ScriptManager runat="server" />
        <asp:TextBox ID="Date" runat="server" />
        <ajaxToolkit:CalendarExtender runat="server" TargetControlID="Date" Format="yyyy-MM-dd" />
    </form>
</body>
</html>
