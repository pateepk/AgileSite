using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Class visualizing AB conversion rate interval.
    /// </summary>
    public sealed class ABConversionRateIntervalVisualizer : WebControl
    {
        #region "Variables"

        /// <summary>
        /// The difference between the minimum lower bound and the maximum upper bound values for the test.
        /// </summary>
        private readonly double mRange;


        /// <summary>
        /// The minimum lower bound of the conversion rate intervals for the test.
        /// </summary>
        private readonly double mMin;     


        /// <summary>
        /// Conversion rate interval for the challenger variant.
        /// </summary>
        private readonly ABConversionRateInterval mChallengerInterval;


        /// <summary>
        /// Conversion rate interval for the original variant.
        /// </summary>
        private readonly ABConversionRateInterval mOriginalInterval;

        #endregion


        #region "Constants"

        /// <summary>
        /// Color of the horizontal line (axis).
        /// </summary>
        private readonly string HORIZONTAL_COLOR = ColorTranslator.ToHtml(Color.Black);


        /// <summary>
        /// Color of the vertical border lines.
        /// </summary>
        private readonly string BORDER_COLOR = ColorTranslator.ToHtml(Color.Black);


        /// <summary>
        /// Color of the rectangle representing the overlap with the original variant.
        /// </summary>
        private readonly string OVERLAP_COLOR = ColorTranslator.ToHtml(Color.Gray);


        /// <summary>
        /// Color of the rectangle representing that the variant is losing to the original variant.
        /// </summary>
        private readonly string NEGATIVE_COLOR = ColorTranslator.ToHtml(Color.Red);


        /// <summary>
        /// Color of the rectangle representing that the variant is outperforming the original variant.
        /// </summary>
        private readonly string POSITIVE_COLOR = ColorTranslator.ToHtml(Color.Green);


        /// <summary>
        /// Color of the line displaying variant's mean value.
        /// </summary>
        private readonly string VERTICAL_COLOR = ColorTranslator.ToHtml(Color.Black);

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="min">Minimum lower bound of all conversion rate intervals for the test</param>
        /// <param name="range">Range of all conversion rate intervals for the test</param>
        /// <param name="challengerInterval">Variant conversion rate interval</param>
        /// <param name="originalInterval">Original conversion rate interval</param>
        public ABConversionRateIntervalVisualizer(double min, double range, ABConversionRateInterval challengerInterval, ABConversionRateInterval originalInterval)
        {
            mMin = min;
            mRange = range;

            // If range is zero create artificial range for the visualizer
            if (mRange.Equals(0))
            {
                mMin = 0;
                mRange = 1;
            }
            
            mChallengerInterval = challengerInterval;
            mOriginalInterval = originalInterval;
        }


        /// <summary>
        /// Renders the conversion rate interval visualizer.
        /// </summary>
        /// <param name="writer">The HtmlTextWriter object that receives the content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            // Main wrapper for the conversion rate interval info
            var mainWrapper = GetMainWrapper();
            var picture = GetPictureLayout();
            var textWrapper = GetTextWrapper();
            mainWrapper.Controls.Add(textWrapper);
            mainWrapper.Controls.Add(picture);
            mainWrapper.RenderControl(writer);
        }

        #endregion


        #region "Layout panels"

        /// <summary>
        /// Gets the main wrapper for the conversion rate interval info.
        /// </summary>
        private Panel GetMainWrapper()
        {
            var mainWrapper = new Panel();
            mainWrapper.Style[HtmlTextWriterStyle.Width] = "22em";
            mainWrapper.Style[HtmlTextWriterStyle.Height] = "2.5em";
            return mainWrapper;
        }


        /// <summary>
        /// Gets the wrapper for the textual representation of the conversion rate interval.
        /// </summary>
        private Panel GetTextWrapper()
        {
            // Get the text wrapper
            var textWrapper = new Panel();
            textWrapper.Style[HtmlTextWriterStyle.Width] = "40%";
            textWrapper.Style[HtmlTextWriterStyle.MarginRight] = "1em";
            textWrapper.Style["line-height"] = "2.5em";
            textWrapper.Style["float"] = "left";

            // Get the text content
            var textContent = new Literal();
            textContent.Text = mChallengerInterval.ToString();
            textWrapper.Controls.Add(textContent);

            return textWrapper;
        }


        /// <summary>
        /// Gets the layout for the picture representation of the conversion rate interval.
        /// </summary>
        private Panel GetPictureLayout()
        {
            // Wrapper for the interval picture
            var pictureWrapper = GetPictureWrapper();

            var leftBorder = GetLeftBorder();
            pictureWrapper.Controls.Add(leftBorder);

            var rightBorder = GetRightBorder();
            pictureWrapper.Controls.Add(rightBorder);

            var axis = GetHorizontalLine();
            pictureWrapper.Controls.Add(axis);

            // Wrapper for the interval rectangles
            var intervalWrapper = GetIntervalWrapper();
            pictureWrapper.Controls.Add(intervalWrapper);

            // Create panels visualizing the conversion rate interval
            CreateIntervalPanels(intervalWrapper);

            return pictureWrapper;
        }


        /// <summary>
        /// Gets the wrapper for the picture representation of the conversion rate interval.
        /// </summary>
        private Panel GetPictureWrapper()
        {
            var pictureWrapper = new Panel();
            pictureWrapper.Style[HtmlTextWriterStyle.Position] = "relative";
            pictureWrapper.Style[HtmlTextWriterStyle.Width] = "53%";
            pictureWrapper.Style[HtmlTextWriterStyle.Height] = "100%";
            pictureWrapper.Style["float"] = "left";
            return pictureWrapper;
        }
   

        /// <summary>
        /// Gets the left border of the interval visualizer.
        /// </summary>
        private Panel GetLeftBorder()
        {
            var leftBorder = new Panel();
            leftBorder.Style[HtmlTextWriterStyle.Position] = "absolute";
            leftBorder.Style[HtmlTextWriterStyle.Top] = "30%";
            leftBorder.Style["right"] = "95%";
            leftBorder.Style[HtmlTextWriterStyle.Width] = "1px";
            leftBorder.Style[HtmlTextWriterStyle.Height] = "40%";
            leftBorder.Style[HtmlTextWriterStyle.BackgroundColor] = BORDER_COLOR;
            return leftBorder;
        }


        /// <summary>
        /// Gets the right border of the interval visualizer.
        /// </summary>
        private Panel GetRightBorder()
        {
            var rightBorder = new Panel();
            rightBorder.Style[HtmlTextWriterStyle.Position] = "absolute";
            rightBorder.Style[HtmlTextWriterStyle.Top] = "30%";
            rightBorder.Style[HtmlTextWriterStyle.Left] = "95%";
            rightBorder.Style[HtmlTextWriterStyle.Width] = "1px";
            rightBorder.Style[HtmlTextWriterStyle.Height] = "40%";
            rightBorder.Style[HtmlTextWriterStyle.BackgroundColor] = BORDER_COLOR;
            return rightBorder;
        }


        /// <summary>
        /// Gets the horizontal line (axis).
        /// </summary>
        private Panel GetHorizontalLine()
        {
            var axis = new Panel();
            axis.Style[HtmlTextWriterStyle.Position] = "absolute";
            axis.Style[HtmlTextWriterStyle.Top] = "50%";
            axis.Style[HtmlTextWriterStyle.Left] = "5%";
            axis.Style[HtmlTextWriterStyle.Width] = "90%";
            axis.Style[HtmlTextWriterStyle.Height] = "1px";
            axis.Style[HtmlTextWriterStyle.BackgroundColor] = HORIZONTAL_COLOR;
            return axis;
        }


        /// <summary>
        /// Gets the wrapper for the conversion rate interval panels. 
        /// </summary>
        private Panel GetIntervalWrapper()
        {
            var rangeWrapper = new Panel();
            rangeWrapper.Style[HtmlTextWriterStyle.Position] = "relative";
            rangeWrapper.Style[HtmlTextWriterStyle.Top] = "25%";
            rangeWrapper.Style[HtmlTextWriterStyle.Left] = "10%";
            rangeWrapper.Style[HtmlTextWriterStyle.Width] = "80%";
            rangeWrapper.Style[HtmlTextWriterStyle.Height] = "50%";
            return rangeWrapper;
        }

        #endregion


        #region "Interval panels"

        /// <summary>
        /// Creates panels visualizing the conversion rate interval.
        /// </summary>
        /// <param name="intervalWrapper">Conversion rate interval wrapper</param>
        private void CreateIntervalPanels(Panel intervalWrapper)
        {
            // Calculate positions and widths of interval panels

            // Get position and size of the imaginary rectangle representing the whole conversion rate interval
            double left = (mChallengerInterval.ConversionRateLowerBound - mMin) / mRange * 100;
            double width = (mChallengerInterval.ConversionRateUpperBound - mChallengerInterval.ConversionRateLowerBound) / mRange * 100;
            double right = left + width;

            // Get position and size of the rectangle representing the original variant
            double controlLeft = (mOriginalInterval.ConversionRateLowerBound - mMin) / mRange * 100;
            double controlWidth = (mOriginalInterval.ConversionRateUpperBound - mOriginalInterval.ConversionRateLowerBound) / mRange * 100;
            double controlRight = controlLeft + controlWidth;

            // Get position and width of the rectangle representing the overlap with the original variant
            double overlapLeft = Math.Max(left, controlLeft);
            double overlapRight = Math.Min(right, controlRight);
            double overlapWidth = overlapRight - overlapLeft;

            double center = (left + right) / 2;

            // Create rectangles for conversion rate interval visualization

            // Display overlap with the original variant
            var overlapRectangle = GetOverlapRectangle(overlapLeft, overlapWidth);
            intervalWrapper.Controls.Add(overlapRectangle);

            // If the variant can be worse than the original variant then display negative result 
            if (left < controlLeft)
            {
                var negativeRectangle = GetNegativeRectangle(left, right, controlLeft);
                intervalWrapper.Controls.Add(negativeRectangle);
            }

            // If the variant can be better than the original variant then display positive result 
            if (right > controlRight)
            {
                var positiveRectangle = GetPositiveRectangle(controlRight, left, right);
                intervalWrapper.Controls.Add(positiveRectangle);
            }

            // Display the mean value as a vertical line
            var middleLine = GetVerticalLine(center);
            intervalWrapper.Controls.Add(middleLine);
        }


        /// <summary>
        /// Gets neutral (overlap) portion of the conversion rate rectangle.
        /// </summary>
        /// <param name="overlapLeft">Position of the left side of the rectangle</param>
        /// <param name="overlapWidth">Width of the rectangle</param>
        private Panel GetOverlapRectangle(double overlapLeft, double overlapWidth)
        {
            var overlapRectangle = new Panel();
            overlapRectangle.Style[HtmlTextWriterStyle.Position] = "absolute";
            overlapRectangle.Style[HtmlTextWriterStyle.Left] = overlapLeft + "%";
            overlapRectangle.Style[HtmlTextWriterStyle.Width] = overlapWidth + "%";
            overlapRectangle.Style[HtmlTextWriterStyle.Height] = "100%";
            overlapRectangle.Style[HtmlTextWriterStyle.BackgroundColor] = OVERLAP_COLOR;
            overlapRectangle.Style["opacity"] = "0.9";
            return overlapRectangle;
        }


        /// <summary>
        /// Gets positive portion of the conversion rate rectangle.
        /// </summary>
        /// <param name="originalRight">Position of the right side of the original's rectangle</param>
        /// <param name="left">Position of the left side of the rectangle</param>
        /// <param name="right">Position of the right side of the rectangle</param>
        private Panel GetPositiveRectangle(double originalRight, double left, double right)
        {
            // Get position and size of the rectangle representing that the variant is outperforming the control variant
            double positiveLeft = Math.Max(originalRight, left);
            double positiveWidth = right - Math.Max(originalRight, left);

            var positiveRectangle = new Panel();
            positiveRectangle.Style[HtmlTextWriterStyle.Position] = "absolute";
            positiveRectangle.Style[HtmlTextWriterStyle.Left] = positiveLeft + "%";
            positiveRectangle.Style[HtmlTextWriterStyle.Width] = positiveWidth + "%";
            positiveRectangle.Style[HtmlTextWriterStyle.Height] = "100%";
            positiveRectangle.Style[HtmlTextWriterStyle.BackgroundColor] = POSITIVE_COLOR;
            positiveRectangle.Style["opacity"] = "0.9";
            return positiveRectangle;
        }


        /// <summary>
        /// Gets negative portion of the conversion rate rectangle.
        /// </summary>
        /// <param name="left">Position of the left side of the rectangle</param>
        /// <param name="right">Position of the right side of the rectangle</param>
        /// <param name="originalLeft">Position of the left side of the original's rectangle</param>
        private Panel GetNegativeRectangle(double left, double right, double originalLeft)
        {
            // Get position and size of the rectangle representing that the variant is losing to the original variant
            double negativeLeft = left;
            double negativeWidth = Math.Min(right, originalLeft) - negativeLeft;

            var negativeRectangle = new Panel();
            negativeRectangle.Style[HtmlTextWriterStyle.Position] = "absolute";
            negativeRectangle.Style[HtmlTextWriterStyle.Left] = negativeLeft + "%";
            negativeRectangle.Style[HtmlTextWriterStyle.Width] = negativeWidth + "%";
            negativeRectangle.Style[HtmlTextWriterStyle.Height] = "100%";
            negativeRectangle.Style[HtmlTextWriterStyle.BackgroundColor] = NEGATIVE_COLOR;
            negativeRectangle.Style["opacity"] = "0.9";
            return negativeRectangle;
        }


        /// <summary>
        /// Gets line displaying the variant's mean value.
        /// </summary>
        /// <param name="center">Center of the conversion rate interval</param>
        private Panel GetVerticalLine(double center)
        {
            var middleLine = new Panel();
            middleLine.Style[HtmlTextWriterStyle.Position] = "absolute";
            middleLine.Style[HtmlTextWriterStyle.Left] = center + "%";
            middleLine.Style[HtmlTextWriterStyle.Width] = "1px";
            middleLine.Style[HtmlTextWriterStyle.Height] = "100%";
            middleLine.Style[HtmlTextWriterStyle.BackgroundColor] = VERTICAL_COLOR;
            return middleLine;
        }

        #endregion
    }
}
