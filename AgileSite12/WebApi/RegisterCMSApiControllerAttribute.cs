using System;

namespace CMS.WebApi
{
    /// <summary>
    /// Registers route given CMS API controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterCMSApiControllerAttribute : Attribute, IPreInitAttribute
    {
        /// <summary>
        /// Type of the registered API controller.
        /// </summary>
        public Type MarkedType { get; }


        /// <summary>
        /// Gets or sets value that defines if the target API controller requires read and write access to session-state values.
        /// Default value is <c>true</c>.
        /// </summary>
        public bool RequiresSessionState { get; set; } = true;


        /// <summary>
        /// Creates new instance of <see cref="RegisterCMSApiControllerAttribute"/>
        /// </summary>
        /// <param name="markedType"></param>
        public RegisterCMSApiControllerAttribute(Type markedType)
        {
            MarkedType = markedType;
        }


        /// <summary>
        /// Registers instance of this attribute to <see cref="HttpControllerRouteTable"/>.
        /// </summary>
        public void PreInit()
        {
            HttpControllerRouteTable.Instance.Register(MarkedType, RequiresSessionState);
        }
    }
}
