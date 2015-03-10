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

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TFSDataAccessPortable;
using TFSExpert.Common;

namespace TFSExpert.ViewModels
{
    public class Sprint : BindableBase
    {
        #region sprint meta data

        private DateTime? _sprintStart;
        private DateTime? _sprintEnd;

        public string SprintPath { get; set; }
        public string SprintName { get; set; }

        public string StartDate
        {
            get { return _sprintStart.HasValue ? _sprintStart.Value.Date.ToString("d") : string.Empty; }
        }

        public string EndDate
        {
            get { return _sprintEnd.HasValue ? _sprintEnd.Value.Date.ToString("d") : string.Empty; }
        }

        public DateTime? StartDateTime
        {
            get
            {
                if (_sprintStart.HasValue)
                {
                    return _sprintStart.Value;
                }

                return null;
            }
        }

        public DateTime? EndDateTime
        {
            get
            {
                if (_sprintEnd.HasValue)
                {
                    return _sprintEnd.Value;
                }

                return null;
            }
        }

        public void SetSprintData(SprintData data)
        {
            SprintName = data.Name;
            _sprintStart = data.StartDate;
            _sprintEnd = data.EndDate;
            SprintPath = data.Path;
            OnPropertyChanged("SprintName");
            OnPropertyChanged("StartDate");
            OnPropertyChanged("EndDate");
            OnPropertyChanged("SprintPath");
        }

        public void SetSprintData(string sprintName, DateTime sprintStart, DateTime sprintEnd, string sprintPath)
        {
            SprintName = sprintName;
            _sprintStart = sprintStart;
            _sprintEnd = sprintEnd;
            SprintPath = sprintPath;
            OnPropertyChanged("SprintName");
            OnPropertyChanged("StartDate");
            OnPropertyChanged("EndDate");
            OnPropertyChanged("SprintPath");
        }

        #endregion

        #region sprint status data

        public double? ApproxTotalWorkOfSprint
        {
            get
            {
                if (BurndownData != null && BurndownData.Count > 0)
                {
                    return BurndownData.First().IdealTrend;
                }

                return null;
            }
        }

        public double? CurrentRemainingWork
        {
            get
            {
                if (BurndownData != null && BurndownData.Count > 0)
                {
                    var dataToday = BurndownData.FirstOrDefault(x => x.Date.Date == DateTime.Today.Date);
                    // current sprint can be in the past --> no value for today
                    if (dataToday == null)
                    {
                        BurnDownPointData burndown = BurndownData.Last();
                        if (burndown != null && burndown.RemainingWork.HasValue)
                        {
                            return Math.Round(BurndownData.Last().RemainingWork.Value);
                        }
                    }
                    else
                    {
                        if (dataToday.RemainingWork.HasValue)
                        {
                            return Math.Round(dataToday.RemainingWork.Value);
                        }
                    }
                }
                return 0.0;
            }
        }

        public string CurrentRemainingWorkPercentString
        {
            get
            {
                if (CurrentRemainingWorkPercent == null)
                {
                    return string.Empty;
                }

                if (Double.IsNaN(CurrentRemainingWorkPercent.Value))
                {
                    return "0 %";
                }

                return string.Format("{0:0.##} %", CurrentRemainingWorkPercent);
            }
        }

        public double? CurrentRemainingWorkPercent
        {
            get
            {
                if (CurrentRemainingWork != null && ApproxTotalWorkOfSprint != null)
                {
                    return CurrentRemainingWork*100/ApproxTotalWorkOfSprint;
                }
                return null;
            }
        }

        public double? CurrentIdealTrend
        {
            get
            {
                if (BurndownData == null || BurndownData.Count <= 0)
                {
                    return null;
                }

                var dataToday = BurndownData.FirstOrDefault(x => x.Date.Date == DateTime.Today.Date);
                // current sprint can be in the past --> no value for today
                if (dataToday == null)
                {
                    BurnDownPointData burndown = BurndownData.Last();
                    if (burndown != null)
                    {
                        return Math.Round(BurndownData.Last().IdealTrend);
                    }
                }
                else
                {
                    return Math.Round(dataToday.IdealTrend);
                }
                return null;
            }
        }

        public string CurrentIdealTrendPercentString
        {
            get
            {
                if (CurrentIdealTrendPercent == null)
                {
                    return string.Empty;
                }

                if (Double.IsNaN(CurrentIdealTrendPercent.Value))
                {
                    return "0 %";
                }

                return string.Format("{0:0.##} %", CurrentIdealTrendPercent);
            }
        }

        public double? CurrentIdealTrendPercent
        {
            get
            {
                if (CurrentIdealTrend != null && ApproxTotalWorkOfSprint != null)
                {
                    return CurrentIdealTrend*100/ApproxTotalWorkOfSprint;
                }
                return null;
            }
        }

        public double? CurrentWorkDone
        {
            get
            {
                if (ApproxTotalWorkOfSprint.HasValue && CurrentRemainingWork.HasValue)
                {
                    return Math.Round(ApproxTotalWorkOfSprint.Value - CurrentRemainingWork.Value);
                }
                return null;
            }
        }

        public double? CurrentWorkDonePercent
        {
            get
            {
                if (CurrentRemainingWorkPercent.HasValue)
                {
                    return 100 - CurrentRemainingWorkPercent.Value;
                }
                return null;
            }
        }

        public double? CurrentIdealDone
        {
            get
            {
                if (ApproxTotalWorkOfSprint.HasValue && CurrentIdealTrend.HasValue)
                {
                    return Math.Round(ApproxTotalWorkOfSprint.Value - CurrentIdealTrend.Value);
                }
                return null;
            }
        }

        public double? CurrentIdealDonePercent
        {
            get
            {
                if (CurrentIdealTrendPercent.HasValue)
                {
                    return 100 - CurrentIdealTrendPercent.Value;
                }
                return null;
            }
        }

        public double? CurrentStatusPercent
        {
            get
            {
                if (CurrentWorkDonePercent.HasValue && CurrentIdealDonePercent.HasValue)
                {
                    return CurrentWorkDonePercent - CurrentIdealDonePercent;
                }
                return null;
            }
        }

        public string CurrentStatusPercentString
        {
            get
            {
                if (!CurrentStatusPercent.HasValue)
                {
                    return string.Empty;
                }

                if (Double.IsNaN(CurrentStatusPercent.Value))
                {
                    return "0 %";
                }

                return string.Format("{0:0} %", CurrentStatusPercent);
            }
        }

        #endregion

        #region burndown data

        private bool _bIsBurndownDataLoaded;
        private ObservableCollection<BurnDownPointData> _burndownData;

        public ObservableCollection<BurnDownPointData> BurndownData
        {
            get { return _burndownData; }
            set
            {
                _burndownData = value;
                OnPropertyChanged("BurndownData");
                if (value != null)
                {
                    IsBurndownDataLoaded = true;
                }
            }
        }

        public bool IsBurndownDataLoaded
        {
            get { return _bIsBurndownDataLoaded; }
            private set
            {
                _bIsBurndownDataLoaded = value;
                FirePropertyChangedForStatusData();
            }
        }

        internal void SetCurrentRemainingWorkOfTask(double oldRemainingWork, double newRemainingWork)
        {
            BurnDownPointData dataToday = BurndownData.FirstOrDefault(n => n.Date == DateTime.Today);
            dataToday.RemainingWork = dataToday.RemainingWork - oldRemainingWork + newRemainingWork;

            CalculateTrendlines();

            RefreshBurndownData();
        }

        #endregion

        public static Sprint Empty
        {
            get { return new Sprint(); }
        }

        internal void Reset()
        {
            IsBurndownDataLoaded = false;
            if (BurndownData != null)
            {
                BurndownData.Clear();
            }
            SprintName = string.Empty;
            _sprintStart = null;
            _sprintEnd = null;

            OnPropertyChanged("BurndownData");
            FirePropertyChangedForStatusData();

            OnPropertyChanged("SprintName");
            OnPropertyChanged("StartDate");
            OnPropertyChanged("EndDate");
        }

        private void FirePropertyChangedForStatusData()
        {
            OnPropertyChanged("IsBurndownDataLoaded");
            OnPropertyChanged("ApproxTotalWorkOfSprint");
            OnPropertyChanged("CurrentRemainingWork");
            OnPropertyChanged("CurrentRemainingWorkPercent");
            OnPropertyChanged("CurrentRemainingWorkPercentString");
            OnPropertyChanged("CurrentIdealTrend");
            OnPropertyChanged("CurrentIdealTrendPercent");
            OnPropertyChanged("CurrentIdealTrendPercentString");
            OnPropertyChanged("CurrentWorkDone");
            OnPropertyChanged("CurrentWorkDonePercent");
            OnPropertyChanged("CurrentIdealDone");
            OnPropertyChanged("CurrentIdealDonePercent");
            OnPropertyChanged("CurrentStatusPercent");
            OnPropertyChanged("CurrentStatusPercentString");
        }

        internal void RefreshBurndownData()
        {
            OnPropertyChanged("BurndownData");
            OnPropertyChanged("IsBurndownDataLoaded");
            OnPropertyChanged("ApproxTotalWorkOfSprint");
            OnPropertyChanged("CurrentRemainingWork");
            OnPropertyChanged("CurrentRemainingWorkPercent");
            OnPropertyChanged("CurrentRemainingWorkPercentString");
            OnPropertyChanged("CurrentIdealTrend");
            OnPropertyChanged("CurrentIdealTrendPercent");
            OnPropertyChanged("CurrentIdealTrendPercentString");
            OnPropertyChanged("CurrentWorkDone");
            OnPropertyChanged("CurrentWorkDonePercent");
            OnPropertyChanged("CurrentIdealDone");
            OnPropertyChanged("CurrentIdealDonePercent");
            OnPropertyChanged("CurrentStatusPercent");
            OnPropertyChanged("CurrentStatusPercentString");
        }

        internal void CalculateTrendlines()
        {
            var dataPoints = BurndownData.ToArray();

            if (!dataPoints.Any(dp => dp.RemainingWork.HasValue))
            {
                return;
            }
            double correction = 0;

            // Iterate over data points and calculate correction
            Parallel.For(1, dataPoints.Count(), i =>
            {
                // Read the remaining work for the current and the previous day.
                double currentWork = dataPoints[i].RemainingWork.HasValue ? dataPoints[i].RemainingWork.Value : 0.0;
                double previousWork = dataPoints[i - 1].RemainingWork.HasValue
                    ? dataPoints[i - 1].RemainingWork.Value
                    : 0.0;

                // Aggregate the data if needed.
                if (currentWork > previousWork)
                    correction += (currentWork - previousWork);
            });

            // Calculate the remaining work value for the first day.
            double firstValue = dataPoints[0].RemainingWork.HasValue
                ? dataPoints[0].RemainingWork.Value + correction
                : correction;

            // Get the iteration duration (in days).
            int lastPointIndex = dataPoints.Count() - 1;
            int iterationDays = (dataPoints[lastPointIndex].Date - dataPoints[0].Date).Days;

            // The value by which the ideal burndown value should decrease each day.
            double idealDelta = firstValue/iterationDays;

            // Set the ideal trend value for the first day.
            dataPoints[0].IdealTrend = firstValue;

            // Set the ideal trend values for the rest of the iteration.
            for (int i = 1; i <= lastPointIndex; i++)
                dataPoints[i].IdealTrend = dataPoints[i - 1].IdealTrend - idealDelta;

            // Calculate the remaining work for the last day we have data on.
            int indexOfLastElementWithData = Array.IndexOf(dataPoints, dataPoints.Last(dp => dp.RemainingWork.HasValue));
            double lastValue = dataPoints[indexOfLastElementWithData].RemainingWork.Value;

            // The value by which the actual burndown value should decrease each day.
            double actualDelta = (firstValue - lastValue)/indexOfLastElementWithData;

            // Set the actual trend value for the first day.
            dataPoints[0].ActualTrend = firstValue;

            // Set the actual trend values for the rest of the iteration.
            for (int i = 1; i <= lastPointIndex; i++)
                dataPoints[i].ActualTrend = dataPoints[i - 1].ActualTrend - actualDelta;
        }
    }
}