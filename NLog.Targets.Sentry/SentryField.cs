using NLog.Config;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NLog.Targets.Sentry
{
    /// <summary>
    /// A configuration item for Sentry target.
    /// </summary>
    [NLogConfigurationItem]
    public sealed class SentryField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SentryField"/> class.
        /// </summary>
        public SentryField()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SentryField"/> class.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="layout">The layout used to generate the value for the field.</param>
        public SentryField(string name, Layout layout)
            : this(name, layout, "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SentryField" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="layout">The layout.</param>
        /// <param name="bsonType">The bson type.</param>
        public SentryField(string name, Layout layout, string res)
        {
            Name = name;
            Layout = layout;
            Res = res ?? "";
        }

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        [RequiredParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout used to generate the value for the field.
        /// </summary>
        /// <value>
        /// The layout used to generate the value for the field.
        /// </value>
        [RequiredParameter]
        public Layout Layout { get; set; }

        /// <summary>
        /// get or ser result string of render layout
        /// </summary>
        /// <value>
        /// result string of render layout
        /// </value>
        [RequiredParameter]
        public string Res { get; set; }
    }
}
