<%@ Control Language="C#" AutoEventWireup="true"  CodeBehind="BtnSadminDeleteUser.ascx.cs" Inherits="CMSWebParts_WTE_PRISM_mindpathcare_DeleteSadminUser_BtnSadminDeleteUser" %>

<%@ Register Src="~/CMSAdminControls/UI/UniControls/UniButton.ascx" TagName="UniButton" TagPrefix="cms" %>
<!--<cms:UniButton ID="btnElem" runat="server" />-->

<style>
.modal {
  display: none;
  position: fixed; 
  padding-top: 50px;
  left: 0; 
  top: 0;
  width: 100%;
  height: 100%; 
  background-color: rgb(0, 0, 0);
  background-color: rgba(0, 0, 0, 0.5);
}
.modal-content {
  position: relative; 
  text-align: center;
  background-color: white;
  padding: 2.5rem; 
  margin: auto; 
  width: 75%;  
  -webkit-animation-name: animatetop;
  -webkit-animation-duration: 0.4s;
  animation-name: animatetop;
  animation-duration: 0.4s
}
.close-btn {
  float: right; 
  color: lightgray; 
  font-size: 24px;  
  font-weight: bold;
}
.close-btn:hover {
  color: darkgray;
}
@-webkit-keyframes animatetop {
  from {top:-300px; opacity:0} 
  to {top:0; opacity:1}
}
@keyframes animatetop {
  from {top:-300px; opacity:0}
  to {top:0; opacity:1}
}
</style>

<input type="button" onclick="openModal();return false;" value="Delete User" class="button" style="float:right;margin-right:8%" />

<div class="modal">
  <div class="modal-content">
    <span class="close-btn">&times;</span>
    <p>Are you sure? please confirm deletion</p>
    <input type="button" class="button" onclick="deleteUser();return false;" value="Delete" />
    <input type="button" class="button" onclick="closeModal();return false;" value="Cancel" />
  </div>
</div>

<cms:CMSButton ID="btnRegister" runat="server"  style="float:right;margin-right:8%;display:none" CssClass="button" />


<script>
    let modalBtn = document.getElementById("modal-btn")
    let modal = document.querySelector(".modal")
    let closeBtn = document.querySelector(".close-btn")
    let realDeleteBtn = document.getElementById('<%= btnRegister.ClientID %>')


    closeBtn.onclick = function () {
        modal.style.display = "none"
    }
    window.onclick = function (e) {
        if (e.target == modal) {
            modal.style.display = "none"
        }
    }

    function openModal() {
        modal.style.display = "block"
    }

    function closeModal() {
        modal.style.display = "none"
    }

    function deleteUser() {
        modal.style.display = "none"
        realDeleteBtn.click();
    }
</script>