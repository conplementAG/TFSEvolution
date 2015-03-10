#region License and Terms

// /***************************************************************************
// Copyright (c) 2015 Conplement AG
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  
// ***************************************************************************/

#endregion

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TFSDataAccessPortable;
using TFSExpert.Common;
using TFSExpert.Navigation;
using TFSExpert.ViewModels;

namespace TFSExpert.Views
{
    /// <summary>
    ///     A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Taskboard : Page
    {
        private NavigationHelper _navigationHelper;
        private ObservableDictionary _defaultViewModel = new ObservableDictionary();
        private double _rotationValue;
        private WorkItem _draggingItem;
        private TaskState _newDraggingState;
        private List<Pointer> _registeredMultiTouchPoints = new List<Pointer>();
        private Sprint _sprintPreviewCopy = new Sprint();

        private DispatcherTimer HealthPreviewHideDelay = new DispatcherTimer();

        /// <summary>
        ///     This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return _defaultViewModel; }
        }

        /// <summary>
        ///     NavigationHelper is used on each page to aid in navigation and
        ///     process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        private CompositeTransform _scaleEditableTransform;


        public Taskboard()
        {
            InitializeComponent();
            _navigationHelper = new NavigationHelper(this);
            _navigationHelper.LoadState += navigationHelper_LoadState;
            _navigationHelper.SaveState += navigationHelper_SaveState;

            _scaleEditableTransform = new CompositeTransform {ScaleX = 1, ScaleY = 1};
            EditableWork.RenderTransform = _scaleEditableTransform;

            EditableWork.ManipulationDelta += EditableWork_ManipulationDelta;
            EditableWork.NewRemainingWorkChanged += EditableWork_NewRemainingWorkChanged;

            //setup delaytimer
            HealthPreviewHideDelay.Interval = new TimeSpan(0, 0, 1);
            HealthPreviewHideDelay.Tick += HealthPreviewHideDelay_Tick;

            ((App) Application.Current).DataSource.TaskboardItemsLoaded += DataSource_TaskboardItemsLoaded;
            ((App) Application.Current).DataSource.SprintDataLoaded += DataSource_SprintDataLoaded;
            ((App) Application.Current).DataSource.LoadSprintData();
        }

        private void EditableWork_NewRemainingWorkChanged()
        {
            try
            {
                _sprintPreviewCopy.SetCurrentRemainingWorkOfTask(EditableWork.OldRemainingWork,
                    EditableWork.NewRemainingWork);
                BurndownPreview.DataContext = _sprintPreviewCopy;
            }
            catch (Exception)
            {
            }
        }

        private ObservableCollection<BurnDownPointData> createBurnDownPointDeepCopy(
            IEnumerable<BurnDownPointData> origData)
        {
            ObservableCollection<BurnDownPointData> resultCollection = new ObservableCollection<BurnDownPointData>();
            foreach (BurnDownPointData oldItem in origData)
            {
                BurnDownPointData newItem = new BurnDownPointData
                {
                    ActualTrend = oldItem.ActualTrend,
                    Date = oldItem.Date,
                    IdealTrend = oldItem.IdealTrend,
                    RemainingWork = oldItem.RemainingWork
                };

                resultCollection.Add(newItem);
            }
            return resultCollection;
        }


        private void DataSource_SprintDataLoaded(bool success, Sprint sprint)
        {
            try
            {
                Sprint sprintOrigData = ((App) Application.Current).DataSource.Sprint;

                //to do --> Hasvalue
                _sprintPreviewCopy = new Sprint();
                _sprintPreviewCopy.SetSprintData(sprintOrigData.SprintName, sprintOrigData.StartDateTime.Value,
                    sprintOrigData.EndDateTime.Value, sprintOrigData.SprintPath);
                _sprintPreviewCopy.BurndownData = createBurnDownPointDeepCopy(sprintOrigData.BurndownData);

                SprintStatusPreview.DataContext = _sprintPreviewCopy;

                BurndownPreview.DataContext = null;
                BurndownPreview.DataContext = _sprintPreviewCopy;
            }
            catch (Exception)
            {
            }
        }

        private void DataSource_TaskboardItemsLoaded(bool success, ObservableCollection<WorkItem> results)
        {
            //set eventhandler to PBI for listening to the state-changes in all collections of the pbi
            AttachEventhandlersToTasks();
        }

        private void AttachEventhandlersToTasks()
        {
            List<WorkItem> thisTaskboardData =
                ((App) Application.Current).DataSource.WorkItemCollection.Where(x => x.WorkItemType == WorkItemType.Task)
                    .ToList();
            foreach (WorkItem item in thisTaskboardData)
            {
                item.DoneStateChanged += Item_DoneStateChanged;
                item.WorkItemStateChanging += item_TaskItemStateChanging;
            }
        }

        private void DetachEventhandlersFromTasks()
        {
            List<WorkItem> thisTaskboardData =
                ((App) Application.Current).DataSource.WorkItemCollection.Where(x => x.WorkItemType == WorkItemType.Task)
                    .ToList();
            foreach (WorkItem item in thisTaskboardData)
            {
                item.RemoveEventHandlers();
            }
        }

        private void item_TaskItemStateChanging(WorkItem sender, WorkItemState newState)
        {
            _draggingItem = sender;
            switch (newState)
            {
                case WorkItemState.ToDo:
                    _newDraggingState = TaskState.ToDo;
                    break;

                case WorkItemState.InProgress:
                    _newDraggingState = TaskState.InProgress;
                    break;

                case WorkItemState.Done:
                    _newDraggingState = TaskState.Done;
                    break;
            }


            VisualStateGroup dialogStateGroup = VisualStateManager.GetVisualStateGroups(MainGrid)[1];

            VisualState showDialogState = dialogStateGroup.States[0];
            showDialogState.Storyboard.Completed += Storyboard_Completed;
            VisualStateManager.GoToState(this, "ShowLoading", true);
        }

        private async void Storyboard_Completed(object sender, object e)
        {
            if (_draggingItem == null)
            {
                return;
            }

            if (_draggingItem.WorkItemState != WorkItemState.Done)
            {
                await _draggingItem.SetTaskStateAsync(_newDraggingState, null);
            }
            else
            {
                _draggingItem.ChangeStateFromDone(_newDraggingState, _draggingItem);
            }

            VisualStateManager.GoToState(this, "HideLoading", true);
            _draggingItem = null;
        }

        private void EditableWork_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            //edit the size
            _rotationValue += e.Delta.Rotation;
            double rotationToSizeValue;
            rotationToSizeValue = 1 + (_rotationValue/150);

            if (rotationToSizeValue > 0.6 &&
                rotationToSizeValue <= 1.6)
            {
                _scaleEditableTransform.ScaleX = rotationToSizeValue;
                _scaleEditableTransform.ScaleY = rotationToSizeValue;
            }

            EditableWork.SetRotation(_rotationValue);
        }


        private void LoadData()
        {
            ((App) Application.Current).DataSource.TaskboardItemsLoaded += myDataSource_ItemsLoaded;
            ((App) Application.Current).DataSource.LoadTaskboardDataAsync();

            DefaultViewModel["CurrentTeamProject"] = Navigator.CurrentTeamProject;
        }

        private void myDataSource_ItemsLoaded(bool success, ObservableCollection<WorkItem> results)
        {
            _defaultViewModel["TaskBoardData"] = results;
        }

        /// <summary>
        ///     Populates the page with content passed during navigation. Any saved state is also
        ///     provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        ///     The source of the event; typically <see cref="NavigationHelper" />
        /// </param>
        /// <param name="e">
        ///     Event data that provides both the navigation parameter passed to
        ///     <see cref="Frame.Navigate(Type, Object)" /> when this page was initially requested and
        ///     a dictionary of state preserved by this page during an earlier
        ///     session. The state will be null the first time a page is visited.
        /// </param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        ///     Preserves state associated with this page in case the application is suspended or the
        ///     page is discarded from the navigation cache.  Values must conform to the serialization
        ///     requirements of <see cref="SuspensionManager.SessionState" />.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper" /></param>
        /// <param name="e">
        ///     Event data that provides an empty dictionary to be populated with
        ///     serializable state.
        /// </param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="GridCS.Common.NavigationHelper.LoadState" />
        /// and
        /// <see cref="GridCS.Common.NavigationHelper.SaveState" />
        /// .
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
            DetachEventhandlersFromTasks();
            ((App) Application.Current).DataSource.TaskboardItemsLoaded -= DataSource_TaskboardItemsLoaded;
            ((App) Application.Current).DataSource.SprintDataLoaded -= DataSource_SprintDataLoaded;
            EditableWork.NewRemainingWorkChanged -= EditableWork_NewRemainingWorkChanged;
            EditableWork.ManipulationDelta -= EditableWork_ManipulationDelta;
        }

        private void zoomedOutPBIs_Tapped(object sender, TappedRoutedEventArgs e)
        {
            WorkItem thisWorkItem = (e.OriginalSource as FrameworkElement).DataContext as WorkItem;
            if (thisWorkItem == null)
            {
                return;
            }

            int scrollIndex = 0;
            int counter = 0;
            foreach (WorkItem singleListItem in MainList.Items)
            {
                if (singleListItem.ID == thisWorkItem.ID)
                {
                    scrollIndex = counter;
                }
                counter++;
            }

            MainList.ScrollIntoView(MainList.Items[scrollIndex]);
        }

        #endregion

        private async void OnSwitchTeamClicked(object sender, RoutedEventArgs e)
        {
            FrameworkElement context = sender as FrameworkElement;

            if (context != null && context.DataContext != null)
            {
                Team selectedTeam = (Team) context.DataContext;
                Navigator.CurrentTeamProject.SwitchTeam(selectedTeam);
                await ((App) Application.Current).DataSource.LoadTaskboardDataAsync();

                TopAppBar.IsOpen = false;
                BottomAppBar.IsOpen = false;
            }
        }

        private async void OnReloadClicked(object sender, RoutedEventArgs e)
        {
            await ((App) Application.Current).DataSource.LoadTaskboardDataAsync();
        }

        private void ListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Load Sprint-Preview Data
            ((App) Application.Current).DataSource.LoadSprintData();

            WorkItem thisWorkItem = (e.OriginalSource as FrameworkElement).DataContext as WorkItem;
            if (thisWorkItem != null && thisWorkItem.WorkItemType == WorkItemType.Task)
            {
                EditableWork.DataContext = thisWorkItem;
                EditTaskFeatureGrid.DataContext = thisWorkItem;
                OldRemainingWork.Text = ((double) thisWorkItem.WorkRemaining).ToString();

                VisualStateManager.GoToState(this, "ShowDialog", true);
                _rotationValue = 0;
            }
        }

        private async void LoveItButton_Click(object sender, RoutedEventArgs e)
        {
            //save the new data to vso
            if (!EditableWork.IsDoneStateReset)
            {
                if (((WorkItem) EditableWork.DataContext).WorkRemaining != EditableWork.NewRemainingWork)
                {
                    await ((WorkItem) EditableWork.DataContext).SetRemainingWorkAsync(EditableWork.NewRemainingWork);
                }
            }
            else
            {
                //save new state and new remaining work to vso
                await
                    ((WorkItem) EditableWork.DataContext).SetTaskStateAsync(EditableWork.NewWorkItemState,
                        EditableWork.NewRemainingWork);
            }

            await ((WorkItem) EditableWork.DataContext).SetAssignedToAsync(EditableWork.NewAssignedTo);

            EditableWork.IsDoneStateReset = false;

            VisualStateManager.GoToState(this, "HideDialog", true);
        }

        private void LeaveItButton_Click(object sender, RoutedEventArgs e)
        {
            //leave the dialog without a change
            EditableWork.IsDoneStateReset = false;
            VisualStateManager.GoToState(this, "HideDialog", true);
        }

        private void Item_DoneStateChanged(TaskState newState, WorkItem changingItem)
        {
            OldRemainingWork.Text = ((double) changingItem.WorkRemaining).ToString();

            //set up edit-Workitem-Dialog and bring it up to view
            EditableWork.NewWorkItemState = newState;
            EditableWork.IsDoneStateReset = true;
            EditableWork.DataContext = changingItem;

            VisualStateManager.GoToState(this, "ShowDialog", true);
            _rotationValue = 0;
        }

        private void EditableWork_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // bring the pointer to a collection to know all the fingers
            HealthPreviewHideDelay.Stop();
            if (_registeredMultiTouchPoints.Count > 0)
            {
                EditableWork.ShowJogRaster();
                VisualStateManager.GoToState(this, "ShowHealthPreview", true);
            }
            _registeredMultiTouchPoints.Add(e.Pointer);
        }

        private void EditableWork_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //Delete Pointer from the pointercollection, finger is not longer there
            Pointer releasedPointer =
                _registeredMultiTouchPoints.FirstOrDefault(x => x.PointerId == e.Pointer.PointerId);

            if (releasedPointer != null)
            {
                _registeredMultiTouchPoints.Remove(releasedPointer);
            }

            if (_registeredMultiTouchPoints.Count < 1)
            {
                HealthPreviewHideDelay.Start();
            }
        }

        private void HealthPreviewHideDelay_Tick(object sender, object e)
        {
            HealthPreviewHideDelay.Stop();
            EditableWork.HideJogRaster();
            VisualStateManager.GoToState(this, "HideHealthPreview", true);
        }

        private void EditDialog_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Fallback if the Pointer-Collection will not work correctly. 
            //Hide the Data-Preview, and jump back to the Edit-Dialog-Main
            if (_registeredMultiTouchPoints.Count <= 0)
            {
                return;
            }

            _registeredMultiTouchPoints.Clear();
            EditableWork.HideJogRaster();
            VisualStateManager.GoToState(this, "HideHealthPreview", true);
        }
    }
}