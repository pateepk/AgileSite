<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TN_QuizForm.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.TrainingNetwork.TN_QuizForm" %>

<div id="divMain" class="form-horizontal" runat="server">
    <asp:Label ID="lblQuizName" runat="server"></asp:Label>
    <asp:HiddenField ID="hfAdminQuizItemID" runat="server" Value='0' />
    <asp:Repeater ID="rptQAForm" runat="server" OnItemDataBound="rptQAForm_ItemDataBound">
        <ItemTemplate>
            <asp:HiddenField ID="hfQuestionNumber" runat="server" Value='<%#Eval("QuestionNumber") %>' />
            <div id="divQuestionItem" class="form-group" runat="server">
                <div class="editing-form-label-cell">
                    <asp:Label ID="lblQuestion" runat="server" Text='<%#Eval("QuestionText") %>' />
                </div>
                <div>
                    <asp:RequiredFieldValidator runat="server" ID="rfvRblAnswer" Display="Dynamic" CssClass="Error"
                        ControlToValidate="rblAnswers" ErrorMessage="Please answer this question"></asp:RequiredFieldValidator>
                </div>
                <br />
                <div class="editing-form-value-cell">
                    <asp:RadioButtonList ID="rblAnswers" runat="server" AutoPostBack="false"></asp:RadioButtonList>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <asp:Label ID="lblMsg" runat="server"></asp:Label>
    <div id="divButton" class="form-group" runat="server">
        <cms:FormSubmitButton ID="btnSubmit" runat="server" Text="Submit" ResourceString="" OnClick="btnSubmit_Click"/>
        <cms:FormSubmitButton ID="btnCancel" runat="server" Text="Cancel" Resourcestring="" OnClick="btnCancel_Click" Visible="false"/>
    </div>
</div>