using System;
using System.Data;

using CMS;
using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.Helpers;
using CMS.Search.Azure;
using CMS.UIControls;

[assembly: RegisterCustomClass("SearchTaskAzureListControlExtender", typeof(SearchTaskAzureListControlExtender))]


/// <summary>
/// Azure Search task list control extender
/// </summary>
public class SearchTaskAzureListControlExtender : ControlExtender<UniGrid>
{
    private const string PROCESS = "processtasksazure";
    private const string REFRESH = "refreshazure";


    private HeaderAction mProcessTasksAction;
    private HeaderAction mRefreshAction;


    /// <summary>
    /// OnInit event handler.
    /// </summary>
    public override void OnInit()
    {
        Control.OnExternalDataBound += OnExternalDataBound;

        // Add header actions
        InitHeaderActions();

        // Header action execution occurs in RaisePostBackEvent after Load event and before PreRender. 
        // We need to enable header action and display message after the the action is executed.  
        Control.PreRender += (sender, args) => {

            //Enable header action
            if (mProcessTasksAction != null)
            {
                mProcessTasksAction.Enabled = (Control.RowsCount > 0);
                Control.HeaderActions.ReloadData();
            }

            // Displays message when task processor is running
            DisplayTaskRunningMessage();

            ScriptHelper.RegisterDialogScript(Control.Page);
        };
    }


    /// <summary>
    /// OnExternalDataBound event handler
    /// </summary>
    private object OnExternalDataBound(object sender, string sourceName, object parameter)
    {
        switch (sourceName.ToLowerInvariant())
        {
            case "result":
                return RenderResult(GetDataRowViewFromParameter(parameter));
            case "tasktype":
                string taskType = ValidationHelper.GetString(parameter, String.Empty);
                return Control.GetString($"smartsearch.tasktype.{taskType.ToLowerInvariant()}");
        }

        return null;
    }


    /// <summary>
    /// Return DataRowView from OnExternalDataBound parameter when unigrid column source is ##ALL##.
    /// </summary>
    /// <param name="parameter">OnExternalDataBound parameter</param>
    /// <returns>DataViewRow</returns>
    private DataRowView GetDataRowViewFromParameter(object parameter)
    {
        DataRowView row = null;
        if (parameter is System.Web.UI.WebControls.GridViewRow)
        {
            row = (DataRowView)((System.Web.UI.WebControls.GridViewRow)parameter).DataItem;
        }
        else if (parameter is DataRowView)
        {
            row = (DataRowView)parameter;
        }

        return row;
    }


    /// <summary>
    /// Returns the content of results column. Results column contains message "failed" with link to error message when search task execution has failed.
    /// </summary>
    /// <param name="row">Grid row</param>
    /// <returns>Content of result column</returns>
    private object RenderResult(DataRowView row)
    {
        if (row == null || String.IsNullOrEmpty(ValidationHelper.GetString(row["SearchTaskAzureErrorMessage"], String.Empty)))
        {
            return String.Empty;
        }

        // Check task ID, render failed message if task id cannot be obtained
        int taskID = ValidationHelper.GetInteger(row["SearchTaskAzureID"], 0);
        if (taskID == 0)
        {
            return ResHelper.GetString("smartsearch.resultfailed");
        }

        // Render failed message with link to modal dialog with full report
        string resultUrl = URLHelper.ResolveUrl("~/CMSModules/SmartSearch/SearchTaskAzure_Report.aspx?taskid=") + taskID;
        return $"<a target=\"_blank\" href=\"{resultUrl}\" onclick=\"modalDialog('{resultUrl}', 'taskresult', 700, 500); return false;\">{ResHelper.GetString("smartsearch.resultfailed")}</a>";
    }


    /// <summary>
    /// Adds header actions to page.
    /// </summary>
    private void InitHeaderActions()
    {
        // Add process tasks action
        mProcessTasksAction = new HeaderAction
        {
            Text = Control.GetString("smartsearch.task.processtasks"),
            CommandName = PROCESS
        };
        Control.AddHeaderAction(mProcessTasksAction);

        ComponentEvents.RequestEvents.RegisterForEvent(PROCESS, (sender, args) => SearchTaskExecutorUtils.ProcessSearchTasks());

        // Add refresh action
        mRefreshAction = new HeaderAction
        {
            Text = Control.GetString("general.refresh"),
            CommandName = REFRESH
        };
        Control.AddHeaderAction(mRefreshAction);
    }


    /// <summary>
    /// Displays info message when indexer thread is running.
    /// </summary>
    private void DisplayTaskRunningMessage()
    {
        if (SearchTaskExecutorUtils.IsSearchTaskProcessingRunning())
        {
            string message = ResHelper.GetString("smartsearch.taskprocessingrunning.azure");
            Control.ShowInformation(message);
        }
    }
}
