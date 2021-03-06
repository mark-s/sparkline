﻿// Copyright 2011 Jimmy Schementi
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Schementi.Controls {

    /// <summary>
    /// Interaction logic for Sparkline.xaml
    /// </summary>
    public class Sparkline : ContentControl {
#if !SILVERLIGHT
        static Sparkline() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Sparkline),
                new FrameworkPropertyMetadata(typeof(Sparkline)));

        }
#endif

        #region Dependency Properties
        #region Points
        public static DependencyProperty TimeSeriesProperty = DependencyProperty.Register(
            "TimeSeries",
            typeof(TimeSeries),
            typeof(Sparkline),
            new PropertyMetadata(new TimeSeries(), OnTimeSeriesPropertyChanged));

        public TimeSeries TimeSeries {
            get { return (TimeSeries)GetValue(TimeSeriesProperty); }
            set { SetValue(TimeSeriesProperty, value); }
        }
        #endregion

        #region StrokeThickness
        public static DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness",
            typeof(double),
            typeof(Sparkline),
            new PropertyMetadata(0.5));

        public double StrokeThickness {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        #endregion

        #region LineMargin
        public static DependencyProperty LineMarginProperty = DependencyProperty.Register(
            "LineMargin",
            typeof(Thickness),
            typeof(Sparkline),
            new PropertyMetadata(new Thickness(0)));

        public Thickness LineMargin {
            get { return (Thickness)GetValue(LineMarginProperty); }
            set { SetValue(LineMarginProperty, value); }
        }
        #endregion

        #region PointFill
        public static DependencyProperty PointFillProperty = DependencyProperty.Register(
            "PointFill",
            typeof(Brush),
            typeof(Sparkline),
            new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public Brush PointFill {
            get { return (Brush)GetValue(PointFillProperty); }
            set { SetValue(PointFillProperty, value); }
        }
        #endregion

        #region PointRadius
        public static DependencyProperty PointRadiusProperty = DependencyProperty.Register(
            "PointRadius",
            typeof(double),
            typeof(Sparkline),
            new PropertyMetadata(0.0));

        public double PointRadius {
            get { return (double)GetValue(PointRadiusProperty); }
            set { SetValue(PointRadiusProperty, value); }
        }
        #endregion

        #region HighWaterMark
        public static DependencyProperty HighWaterMarkProperty = DependencyProperty.Register(
            "HighWaterMark",
            typeof(double?),
            typeof(Sparkline),
            new PropertyMetadata(null));

        public double? HighWaterMark {
            get { return (double?)GetValue(HighWaterMarkProperty); }
            set { SetValue(HighWaterMarkProperty, value); }
        }
        #endregion

        #region LowWaterMark
        public static DependencyProperty LowWaterMarkProperty = DependencyProperty.Register(
            "LowWaterMark",
            typeof(double?),
            typeof(Sparkline),
            new PropertyMetadata(null));

        public double? LowWaterMark {
            get { return (double?)GetValue(LowWaterMarkProperty); }
            set { SetValue(LowWaterMarkProperty, value); }
        }
        #endregion

        #region LatestLevel
        public static DependencyProperty LatestLevelProperty = DependencyProperty.Register(
            "LatestLevel",
            typeof(double?),
            typeof(Sparkline),
            new PropertyMetadata(null));

        public double? LatestLevel {
            get { return (double?)GetValue(LatestLevelProperty); }
            set { SetValue(LatestLevelProperty, value); }
        }
        #endregion

        #region ShowWatermarks
        public static DependencyProperty ShowWatermarksProperty = DependencyProperty.Register(
            "ShowWatermarks",
            typeof(bool),
            typeof(Sparkline),
            new PropertyMetadata(false, OnShowWatermarksPropertyChanged));

        private static void OnShowWatermarksPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((Sparkline)d).OnShowWatermarksPropertyChanged();
        }

        private void OnShowWatermarksPropertyChanged() {
            if (ShowWatermarks) {
                _lowwatermark = new Rectangle {
                    Fill = new SolidColorBrush(Colors.Red),
                    Opacity = 0.5,
                    Height = StrokeThickness,
                    VerticalAlignment = VerticalAlignment.Top,
                    UseLayoutRounding = false,
                };
                BindingOperations.SetBinding(_lowwatermark, MarginProperty,
                                             new Binding("LowWaterMark") { Source = this, Converter = new YCoordinateToThicknessConverter() });
                _canvas.Children.Insert(0, _lowwatermark);

                _highwatermark = new Rectangle {
                    Fill = new SolidColorBrush(Colors.Green),
                    Opacity = 0.5,
                    Height = StrokeThickness,
                    VerticalAlignment = VerticalAlignment.Top,
                    UseLayoutRounding = false,
                };
                BindingOperations.SetBinding(_highwatermark, MarginProperty,
                                             new Binding("HighWaterMark") { Source = this, Converter = new YCoordinateToThicknessConverter() });
                _canvas.Children.Insert(0, _highwatermark);
            } else {
                if (_lowwatermark != null) {
                    _canvas.Children.Remove(_lowwatermark);
                    _lowwatermark = null;
                }

                if (_highwatermark != null) {
                    _canvas.Children.Remove(_highwatermark);
                    _highwatermark = null;
                }
            }
        }

        public bool ShowWatermarks {
            get { return (bool)GetValue(ShowWatermarksProperty); }
            set { SetValue(ShowWatermarksProperty, value); }
        }
        #endregion

        #region ShowLatestLevel
        public static DependencyProperty ShowLatestLevelProperty = DependencyProperty.Register(
            "ShowLatestLevel",
            typeof(bool),
            typeof(Sparkline),
            new PropertyMetadata(false, OnShowLatestLevelPropertyChanged));

        private static void OnShowLatestLevelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((Sparkline)d).OnShowLatestLevelPropertyChanged();
        }

        private void OnShowLatestLevelPropertyChanged() {
            if (ShowLatestLevel) {
                _latestLevel = new Rectangle {
                    Fill = new SolidColorBrush(Colors.White),
                    Opacity = 0.5,
                    Height = StrokeThickness,
                    VerticalAlignment = VerticalAlignment.Top,
                    UseLayoutRounding = false,
                };
                BindingOperations.SetBinding(_latestLevel, MarginProperty,
                                             new Binding("LatestLevel") { Source = this, Converter = new YCoordinateToThicknessConverter() });
                _canvas.Children.Insert(0, _latestLevel);
            } else {
                if (_latestLevel != null) {
                    _canvas.Children.Remove(_latestLevel);
                    _latestLevel = null;
                }
            }
        }

        public bool ShowLatestLevel {
            get { return (bool)GetValue(ShowLatestLevelProperty); }
            set { SetValue(ShowLatestLevelProperty, value); }
        }
        #endregion

        #region MinYRange
        public static DependencyProperty MinYRangeProperty = DependencyProperty.Register(
            "MinYRange",
            typeof(double),
            typeof(Sparkline),
            new PropertyMetadata(25.0));

        public double MinYRange {
            get { return (double)GetValue(MinYRangeProperty); }
            set { SetValue(MinYRangeProperty, value); }
        }
        #endregion


        #endregion

        #region Events
        public class TimeValueAddedEventArgs : EventArgs {
            public Point Point { get; set; }
            public Panel Panel { get; set; }
            public TimeValue TimeValue { get; set; }
        }

        public delegate void TimeValueAddedHandler(Sparkline obj, TimeValueAddedEventArgs eventArgs);

        public event TimeValueAddedHandler TimeValueAdded;

        protected void OnTimeValueAdded(Point po, Panel pa, TimeValue timeValue) {
            var handler = TimeValueAdded;
            if (handler != null) {
                handler(this, new TimeValueAddedEventArgs { Point = po, Panel = pa, TimeValue = timeValue });
            }
        }
        #endregion

        #region Fields
        private int _nextXValue;

        private const int XWidth = 2;

        private Grid _canvas;
        private Polyline _polyline;

        private Rectangle _highwatermark;
        private Rectangle _lowwatermark;
        private Rectangle _latestLevel;
        #endregion

        #region Public API
        public Sparkline() {
            DefaultStyleKey = typeof (Sparkline);
            TimeSeries = new TimeSeries();
        }

        public string AddTimeValue(double value, DateTime? time = null) {
            return TimeSeries.AddTimeValue(value, time);
        }

        public Action ScrollToRightEnd { private get; set; }
        #endregion

        #region Implementation

        public override void OnApplyTemplate() {
            InitializePolyline();
            base.OnApplyTemplate();
        }

        private void InitializePolyline() {
            _canvas = GetTemplateChild("Canvas") as Grid;
            _polyline = GetTemplateChild("Polyline") as Polyline;
            if (_canvas == null) throw new Exception("Sparkline: \"Canvas\" element not found in custom template.");
            if (_polyline == null) throw new Exception("Sparkline: \"Polyline\" element not found in custom template.");

            if (Foreground == null) Foreground = new SolidColorBrush(Colors.Black);

            BindingOperations.SetBinding(_polyline, Shape.StrokeProperty,
                                         new Binding("Foreground") { Mode = BindingMode.TwoWay, Source = this });
            BindingOperations.SetBinding(_polyline, Shape.StrokeThicknessProperty,
                                         new Binding("StrokeThickness") { Mode = BindingMode.TwoWay, Source = this });
            BindingOperations.SetBinding(_polyline, MarginProperty,
                                         new Binding("LineMargin") { Mode = BindingMode.TwoWay, Source = this });
        }

        private static void OnTimeSeriesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((Sparkline)d).OnTimeSeriesPropertyChanged(e);
        }

        private void OnTimeSeriesPropertyChanged(DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null) ((TimeSeries)e.OldValue).CollectionChanged -= TimeSeriesCollectionChanged;
            if (e.NewValue != null) ((TimeSeries)e.NewValue).CollectionChanged += TimeSeriesCollectionChanged;
        }

        private void TimeSeriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (var timeValue in e.NewItems.OfType<TimeValue>()) DrawTimeValue(timeValue);
                    break;
                default:
                    ResetTimeSeries();
                    break;
            }
        }

        private void ResetTimeSeries() {
            _both = _lower = _higher = false;
            _canvas.Children.Clear();
            _canvas.Children.Add(_polyline);
            if (ShowWatermarks) {
                _canvas.Children.Add(_lowwatermark);
                _canvas.Children.Add(_highwatermark);
            }
            if (ShowLatestLevel) {
                _canvas.Children.Add(_latestLevel);
            }
            _canvas.Height = double.NaN;
            _nextXValue = 0;
            foreach (var timeValue in TimeSeries) {
                DrawTimeValue(timeValue);
            }
        }

        private void DrawTimeValue(TimeValue newTimeValue) {
            var point = GetPoint(newTimeValue);
            AddPoint(point);
            OnTimeValueAdded(point, _canvas, newTimeValue);
            if (ScrollToRightEnd != null) ScrollToRightEnd();
        }

        private void AddPoint(Point point) {
            _polyline.Points.Add(point);
            SetWatermarks(point.Y);
            SetLatestLevel(point.Y);
            SetCanvasHeight(point.Y);
            SetContainerHeight(point.Y);
            if (PointRadius > 0.0)
                _canvas.Children.Add(DrawDot(point));
            _nextXValue++;
        }

        private Path DrawDot(Point center) {
            var path = new Path();
            var circle = new EllipseGeometry { Center = center, RadiusX = PointRadius, RadiusY = PointRadius };
            path.Fill = PointFill;
            path.Data = circle;
            return path;
        }

        private Point GetPoint(TimeValue timeValue) {
            return new Point(_nextXValue * XWidth, timeValue.Value);
        }

        private void SetWatermarks(double y) {
            if (LowWaterMark == null)
                LowWaterMark = y;
            if (HighWaterMark == null)
                HighWaterMark = y;

            if (y > HighWaterMark) HighWaterMark = y;
            else if (y < LowWaterMark) LowWaterMark = y;
        }

        private void SetLatestLevel(double y) {
            LatestLevel = y;
        }

        private double _lowMargin;
        private double _height;
        private bool _lower;
        private bool _higher;
        private bool _both;

        private void SetCanvasHeight(double y) {
            if (TimeSeries.Count < 2) {
                _canvas.Height = _height = y + MinYRange;
                _lowMargin = -y + MinYRange;
                _canvas.Margin = new Thickness(0, 0, 0, _lowMargin);
                return;
            }
            if (!_both && LowWaterMark != null && LowWaterMark < _lowMargin * -1) {
                _lower = true;
                _lowMargin = -LowWaterMark.Value;
                _canvas.Margin = new Thickness(0, 0, 0, _lowMargin);
                _both = _lower && _higher;
            }
            if (!_both && HighWaterMark != null && HighWaterMark > _height) {
                _higher = true;
                _canvas.Height = HighWaterMark.Value;
                _both = _lower && _higher;
            }
            if (!_both) return;
            _canvas.Height = double.NaN;
            _canvas.Margin = new Thickness(0, 0, 0, -(LowWaterMark ?? 0));
        }

        private void SetContainerHeight(double y) {
            FrameworkElement canvas = _canvas;
            var lineChart = this.Parent as LineChart;
            if (lineChart != null) {
                canvas = lineChart;
            }
        }

        //private DoubleAnimation _animation;
        //
        //private void StartGraphAnimation() {
        //    if (_animation != null)
        //        return;

        //    //Console.WriteLine(string.Format("{0} Sim Time: {1}", DateTime.Now, AverageSimulatedTimePerSecond));
        //    _animation = new DoubleAnimation();
        //    _animation.FillBehavior = FillBehavior.Stop;
        //    _animation.From = 0.0;
        //    _animation.To = DurationInPoints * -2;
        //    _animation.Completed += new EventHandler(Animation_Completed);
        //    _animation.Duration = TimeSpan.FromMilliseconds(Duration.TotalMilliseconds * 2);

        //    GraphLine.BeginAnimation(Canvas.LeftProperty, _animation, HandoffBehavior.SnapshotAndReplace);
        //}

        //private void EndGraphAnimation() {
        //    if (_animation == null)
        //        return;

        //    _animation.Completed -= new EventHandler(Animation_Completed);
        //    _animation = null;

        //    // Stop the old animation by overriding it with a new animation.
        //    DoubleAnimation resetAnimation = new DoubleAnimation();
        //    resetAnimation.To = 0.0;
        //    resetAnimation.Duration = TimeSpan.Zero;
        //    GraphLine.BeginAnimation(Canvas.LeftProperty, _animation, HandoffBehavior.SnapshotAndReplace);
        //}

        //void Animation_Completed(object sender, EventArgs e) {
        //    EndGraphAnimation();
        //    //Redraw();
        //}

        #endregion
    }

    public class YCoordinateToThicknessConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return new Thickness(0, (double?)value ?? 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
