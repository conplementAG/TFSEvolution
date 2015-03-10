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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using TFSDataAccessPortable;
using TFSExpert.Navigation;
using TFSExpert.ViewModels;

namespace TFSExpert.Controls
{
    public sealed partial class EditableWorkItem : UserControl
    {
        public static DependencyProperty NewRemainingWorkProperty = DependencyProperty.Register("NewRemainingWork",
            typeof (double), typeof (EditableWorkItem),
            new PropertyMetadata(""));

        public double NewRemainingWork
        {
            get { return (double) GetValue(NewRemainingWorkProperty); }
            set { SetValue(NewRemainingWorkProperty, value); }
        }

        public static DependencyProperty OldRemainingWorkProperty = DependencyProperty.Register("OldRemainingWork",
            typeof (double), typeof (EditableWorkItem),
            new PropertyMetadata(""));

        public double OldRemainingWork
        {
            get { return (double) GetValue(OldRemainingWorkProperty); }
            set { SetValue(OldRemainingWorkProperty, value); }
        }

        public string NewAssignedTo { get; set; }

        public TaskState NewWorkItemState { get; set; }

        public bool IsDoneStateReset { get; set; }

        //some DependencyProps
        public static DependencyProperty WorkIDProperty = DependencyProperty.Register("WorkItemID", typeof (string),
            typeof (EditableWorkItem),
            new PropertyMetadata(""));

        public string WorkItemID
        {
            get { return (string) GetValue(WorkIDProperty); }
            set { SetValue(WorkIDProperty, value); }
        }

        private CompositeTransform Rotator;
        private CompositeTransform JogRasterRotator;


        public delegate void _NewRemainingWorkChanged();

        public event _NewRemainingWorkChanged NewRemainingWorkChanged;

        public EditableWorkItem()
        {
            InitializeComponent();
            try
            {
                Rotator = new CompositeTransform();
                JogRasterRotator = new CompositeTransform();

                MainShape.RenderTransform = Rotator;
                JogRaster.RenderTransform = JogRasterRotator;
                AssignedToList.ItemsSource = Navigator.CurrentTeamProject.CurrentTeam.TeamMembers;

                DataContextChanged += EditableWorkItem_DataContextChanged;
            }
            catch (Exception)
            {
            }
        }

        private void EditableWorkItem_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            //Changing teammember-selection

            WorkItem thisWorkItem = DataContext as WorkItem;

            //setting RemainingWork-Settings
            if (thisWorkItem != null)
            {
                //disable jogwheel if the state is done
                if (thisWorkItem.WorkItemState == WorkItemState.Done && !IsDoneStateReset)
                {
                    IsEnabled = false;
                }
                else
                {
                    IsEnabled = true;
                }

                if (thisWorkItem.WorkRemaining.HasValue)
                    OldRemainingWork = thisWorkItem.WorkRemaining.Value;
                else
                    OldRemainingWork = 0;

                if (!IsDoneStateReset)
                {
                    NewRemainingWork = (double) thisWorkItem.WorkRemaining;
                }
                else
                {
                    NewRemainingWork = 1;
                }
                if (AssignedToList.Items.Count > 0)
                {
                    int memberIndex = 0;
                    AssignedToList.SelectedIndex = 0;
                    foreach (TeamMember thisMember in AssignedToList.Items)
                    {
                        if (thisMember.Name == thisWorkItem.AssignedTo)
                        {
                            AssignedToList.SelectedIndex = memberIndex;
                        }
                        memberIndex++;
                    }
                }
            }
        }

        public void SetRotation(double newAngle)
        {
            Rotator.CenterX = 0.5;
            Rotator.CenterY = 0.5;
            JogRasterRotator.CenterX = 0.5;
            JogRasterRotator.CenterY = 0.5;

            Rotator.Rotation = newAngle;
            JogRasterRotator.Rotation = newAngle;

            //calculate new RemainingWork
            int calcValue = Convert.ToInt16(newAngle/10);
            double newValue = (double) ((WorkItem) DataContext).WorkRemaining + Convert.ToDouble(calcValue);

            if (newValue >= 1)
            {
                double oldValue = NewRemainingWork;
                //save the old value before setting the new value
                OldRemainingWork = NewRemainingWork;
                NewRemainingWork = (double) ((WorkItem) DataContext).WorkRemaining + Convert.ToDouble(calcValue);
                //fire event
                if (oldValue != NewRemainingWork)
                {
                    //jogFeedback.Play();
                    NewRemainingWorkChanged();
                }
            }
        }

        private void AssignedToList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssignedToList.SelectedIndex > 0)
            {
                TeamMember newSelection = AssignedToList.SelectedItem as TeamMember;
                NewAssignedTo = newSelection.Name;
            }
            else
            {
                NewAssignedTo = "";
            }
        }

        public void ShowJogRaster()
        {
            VisualStateManager.GoToState(this, "ShowRaster", true);
        }

        public void HideJogRaster()
        {
            VisualStateManager.GoToState(this, "HideRaster", true);
        }
    }
}