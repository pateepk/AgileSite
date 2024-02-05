using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

using CMS.DataEngine;
using CMS.IO;

namespace CMS.WorkflowEngine.Definitions
{
    /// <summary>
    /// Class to hold the workflow step definition.
    /// </summary>
    public class Step
    {
        #region "Variables"

        private SourcePoint mDefinitionPoint = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether timeout is enabled for the step.
        /// </summary>
        public bool TimeoutEnabled
        {
            get;
            set;
        }


        /// <summary>
        /// Timeout interval of the step
        /// </summary>
        public string TimeoutInterval
        {
            get;
            set;
        }


        /// <summary>
        /// GUID of target transition to use after timeout
        /// </summary>
        public Guid TimeoutTarget
        {
            get;
            set;
        }


        /// <summary>
        /// List of source points for transitions
        /// </summary>
        public List<SourcePoint> SourcePoints
        {
            get;
            set;
        }


        /// <summary>
        /// Source point with additional definitions
        /// </summary>
        public SourcePoint DefinitionPoint
        {
            get
            {
                if (mDefinitionPoint == null)
                {
                    return SourcePoints.FirstOrDefault();
                }

                return mDefinitionPoint;
            }
            set
            {
                mDefinitionPoint = value;
            }
        }


        /// <summary>
        /// Position of the step
        /// </summary>
        public Point Position
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public Step()
        {
            SourcePoints = new List<SourcePoint>();
            Position = new Point(-1, -1);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads data from string
        /// </summary>
        /// <param name="data">Xml data to load</param>
        /// <param name="stepType">Step type</param>
        public void LoadData(string data, WorkflowStepTypeEnum stepType)
        {
            bool loaded = false;
            if (!string.IsNullOrEmpty(data))
            {
                XmlSerializer deserializer = new XmlSerializer(GetType());
                using (StringReader reader = new StringReader(data))
                {
                    Step definition = (Step)deserializer.Deserialize(reader);
                    if (definition != null)
                    {
                        TimeoutEnabled = definition.TimeoutEnabled;
                        TimeoutInterval = definition.TimeoutInterval;
                        TimeoutTarget = definition.TimeoutTarget;
                        SourcePoints = definition.SourcePoints;
                        Position = definition.Position;
                        mDefinitionPoint = definition.mDefinitionPoint;

                        // Backward compatibility (RC issue - doubled definitions)
                        var sp = SourcePoints.FirstOrDefault();
                        if ((sp != null) && (mDefinitionPoint != null) && (sp.Guid == mDefinitionPoint.Guid))
                        {
                            switch (stepType)
                            {
                                case WorkflowStepTypeEnum.Multichoice:
                                case WorkflowStepTypeEnum.Userchoice:
                                    // Ensure separate definition point
                                    mDefinitionPoint = new SourcePoint();
                                    break;

                                default:
                                    // Clear definition point
                                    mDefinitionPoint = null;
                                    break;
                            }
                        }

                        loaded = true;
                    }

                    reader.Close();
                }
            }

            if (!loaded)
            {
                // Clear data
                Clear();
            }
        }


        /// <summary>
        /// Gets XML node representation
        /// </summary>
        public string GetData()
        {
            string xml = null;

            XmlAttributeOverrides overrides = null;
            // Do not include definition point if shared
            if (mDefinitionPoint == null)
            {
                XmlAttributes attrs = new XmlAttributes();
                attrs.XmlIgnore = true;
                overrides = new XmlAttributeOverrides();
                overrides.Add(GetType(), "DefinitionPoint", attrs);
            }

            XmlSerializer serializer = new XmlSerializer(GetType(), overrides);
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, this);
                writer.Flush();

                xml = writer.ToString();
                writer.Close();
            }

            return xml;
        }


        /// <summary>
        /// Clears data
        /// </summary>
        public void Clear()
        {
            TimeoutInterval = null;
            TimeoutTarget = Guid.Empty;
            Position = new Point();
            if (SourcePoints != null)
            {
                SourcePoints.Clear();
            }
        }

        #endregion
    }
}