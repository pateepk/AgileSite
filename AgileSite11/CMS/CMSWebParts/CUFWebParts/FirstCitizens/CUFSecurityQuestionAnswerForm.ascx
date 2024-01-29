<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CUFSecurityQuestionAnswerForm.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUFWebParts.FirstCitizens.CUFSecurityQuestionAnswerForm" %>

<div id="divMain" class="form-horizontal" runat="server">
    <asp:Repeater ID="rptQAForm" runat="server" OnItemDataBound="rptQAForm_ItemDataBound">
        <ItemTemplate>
            <div id="divQuestionItem" class="form-group" runat="server">
                <asp:HiddenField ID="hfQuestionID" runat="server" Value='<%#Eval("QuestionID") %>' />
                <asp:HiddenField ID="hfAnswerID" runat="server" Value='<%#Eval("AnswerID") %>' />
                <div class="editing-form-label-cell">
                    <asp:Label ID="lblSecurityQuestion" runat="server" Text='<%#Eval("QuestionText") %>' />
                </div>
                <br />
                <div class="editing-form-value-cell">
                    <asp:TextBox ID="txtSecurityAnswer" runat="server" Text='<%#Eval("Answer") %>' />
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <asp:Repeater ID="rptQADropDown" runat="server" OnItemDataBound="rptQADropDown_ItemDataBound">
        <ItemTemplate>
            <div id="divQuestionItem" class="form-group" runat="server">
                <asp:HiddenField ID="hfdQuestionGroupID" runat="server" Value='<%#Eval("GroupNumber") %>' />
                <div class="editing-form-value-cell-offset" style="padding-bottom: 5px">
                    <asp:DropDownList ID="ddldQuestionDropdown" AutoPostBack="false" runat="server">
                    </asp:DropDownList>
                </div>
                <div class="editing-form-value-cell">
                    <asp:TextBox ID="txtdSecurityAnswer" runat="server" Text='' />
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <div id="divButton" class="form-group" runat="server">
        <cms:FormSubmitButton ID="btnSubmit" runat="server" Text="Submit" ResourceString=""
            OnClick="btnSubmit_Click" />
        <cms:FormSubmitButton ID="btnCancel" runat="server" Text="Cancel" ResourceString=""
            OnClick="btnCancel_Click" Visible="false" />
    </div>
    <asp:Label ID="lblError" runat="server"></asp:Label>
</div>