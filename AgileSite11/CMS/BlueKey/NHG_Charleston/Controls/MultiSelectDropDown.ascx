<%@ Control Language="c#" AutoEventWireup="false" Codebehind="MultiSelectDropDown.ascx.cs" Inherits="NHG_C.MultiSelectDropDown" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>

	<script language="javascript">
			function SelectedIndexChanged(ctlId)
			{
			    //ctlId = ctlId.replace("_", "$");
   				var control = document.getElementById(ctlId+'DDList'); 
				var strSelText='';
				for(var i = 0; i < control.length; i ++)
				{ 
					if(control.options[i].selected)
					{
						strSelText +=control.options[i].text + ',';
					}
				}
				if (strSelText.length>0)
					strSelText=strSelText.substring(0,strSelText.length-1);
				var ddLabel = document.getElementById(ctlId+"DDLabel"); 
				ddLabel.innerHTML = strSelText;
				ddLabel.innerText  = strSelText;
				ddLabel.title = strSelText;
			}

			function OpenListBox(ctlId)
			{
			    ctlId = ctlId.replace("$", "_");
			    console.log(ctlId);
				var lstBox = document.getElementById(ctlId+"DDList");
				if (lstBox.style.visibility == "visible")				
				{ CloseListBox(ctlId) ; }
				else
				{
					lstBox.style.visibility = "visible"; 
					lstBox.style.height="100px";
				}
			}

			function CloseListBox(ctlId)
			{
				var panel = document.getElementById(ctlId+"Panel2");
				var tabl = document.getElementById(ctlId+"Table2");
				var lstBox = document.getElementById(ctlId+"DDList");
				lstBox.style.visibility = "hidden"; 
				lstBox.style.height="0px";
				panel.style.height=tabl.style.height;
			}
	</script>
	<asp:panel id="Panel2" Height="1px" runat="server" Width="160px" BackColor="White">
		<table id="Table2" style="TABLE-LAYOUT: fixed; HEIGHT: 24px" cellSpacing="0" borderColorDark="#cccccc"
			cellPadding="0" width="100%" borderColorLight="#7eb3e3" border="1" runat="server">
			<tr id="rowDD" style="HEIGHT: 15px" runat="server">
				<td noWrap>
					<asp:Label id="DDLabel" style="CURSOR: default" runat="server" Width="134px" ToolTip="" Font-Size="Smaller"
						Font-Names="Arial" BorderColor="Transparent" BorderStyle="None" height="15px"></asp:Label></td>
				<td id="colDDImage" style="PADDING-RIGHT: 0px; PADDING-LEFT: 0px; PADDING-BOTTOM: 0px; PADDING-TOP: 0px; BACKGROUND-COLOR: #7eb3e3"
					width="20" background="Image/DDImage.bmp" runat="server"></td>
			</tr>
		</table>
		<div style="Z-INDEX: 9999; POSITION: absolute">
			<asp:ListBox id="DDList" Height="72px" runat="server" Width="100%" SelectionMode="Multiple"></asp:ListBox>
		</div>
	</asp:panel>
