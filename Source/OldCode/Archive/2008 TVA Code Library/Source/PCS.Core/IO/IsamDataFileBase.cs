﻿//*******************************************************************************************************
//  IsamDataFileBase.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/08/2007 - Pinal C. Patel
//       Original version of source code generated
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter
//  09/19/2008 - James R Carroll
//       Converted to C#.
//  10/28/2008 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using PCS.Configuration;
using PCS.Parsing;

namespace PCS.IO
{
    /// <summary>
    /// An abstract class that represents an ISAM (Indexed Sequential Access Method) file.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of the records the file contains. This <see cref="Type"/> must implement the <see cref="IBinaryDataProvider"/> interface.</typeparam>
    /// <remarks>
    /// For more information on ISAM files see http://en.wikipedia.org/wiki/ISAM.
    /// </remarks>
    /// <example>
    /// This example shows a sample implementation of <see cref="IsamDataFileBase{T}"/>:
    /// <code>
    /// using System;
    /// using System.Text;
    /// using PCS;
    /// using PCS.IO;
    /// using PCS.Parsing;
    /// 
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // Create a few test records.
    ///         TestIsamFileRecord r1 = new TestIsamFileRecord(1);
    ///         r1.Name = "TestRecord1";
    ///         r1.Value = double.MinValue;
    ///         r1.Description = "Test record with minimum double value";
    ///         TestIsamFileRecord r2 = new TestIsamFileRecord(2);
    ///         r2.Name = "TestRecord2";
    ///         r2.Value = double.MaxValue;
    ///         r2.Description = "Test record with maximum double value";
    /// 
    ///         // Open ISAM file.
    ///         TestIsamFile testFile = new TestIsamFile();
    ///         testFile.FileName = "TestIsamFile.dat";
    ///         testFile.Open();
    /// 
    ///         // Write test records.
    ///         testFile.Write(r1.ID, r1);
    ///         testFile.Write(r2.ID, r2);
    /// 
    ///         // Read test records.
    ///         Console.WriteLine(testFile.Read(1));
    ///         Console.WriteLine(testFile.Read(2));
    /// 
    ///         // Close ISAM file.
    ///         testFile.Close();
    /// 
    ///         Console.ReadLine();
    ///     }
    /// }
    /// 
    /// class TestIsamFile : IsamDataFileBase<![CDATA[<]]>TestIsamFileRecord<![CDATA[>]]>
    /// {
    ///     /// <summary>
    ///     /// Size of a single file record.
    ///     /// </summary>
    ///     protected override int GetRecordSize()
    ///     {
    ///         return TestIsamFileRecord.RecordLength;
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Creates a new empty file record.
    ///     /// </summary>
    ///     protected override TestIsamFileRecord CreateNewRecord(int id)
    ///     {
    ///         return new TestIsamFileRecord(id);
    ///     }
    /// }
    /// 
    /// class TestIsamFileRecord : IBinaryDataProvider, IBinaryDataConsumer
    /// {
    ///     private int m_id;
    ///     private string m_name;                  // 20  * 1 =  20
    ///     private double m_value;                 // 1   * 8 =   8
    ///     private string m_description;           // 100 * 1 = 100
    ///     public const int RecordLength = 128;    // Total   = 128
    /// 
    ///     public TestIsamFileRecord(int recordID)
    ///     {
    ///         m_id = recordID;
    ///         Name = string.Empty;
    ///         Value = double.NaN;
    ///         Description = string.Empty;
    ///     }
    /// 
    ///     /// <summary>
    ///     /// ID of the record.
    ///     /// </summary>
    ///     public int ID
    ///     {
    ///         get { return m_id; }
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Name of the record.
    ///     /// </summary>
    ///     public string Name
    ///     {
    ///         get { return m_name; }
    ///         set { m_name = value.TruncateRight(20).PadRight(20); }
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Value of the record.
    ///     /// </summary>
    ///     public double Value
    ///     {
    ///         get { return m_value; }
    ///         set { m_value = value; }
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Description of the record.
    ///     /// </summary>
    ///     public string Description
    ///     {
    ///         get { return m_description; }
    ///         set { m_description = value.TruncateRight(100).PadRight(100); }
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Serialized record length.
    ///     /// </summary>
    ///     public int BinaryLength
    ///     {
    ///         get { return RecordLength; }
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Serialized record data.
    ///     /// </summary>
    ///     public byte[] BinaryImage
    ///     {
    ///         get
    ///         {
    ///             // Serialize TestIsamFileRecord into byte array.
    ///             byte[] image = new byte[BinaryLength];
    ///             Buffer.BlockCopy(Encoding.ASCII.GetBytes(Name), 0, image, 0, 20);
    ///             Buffer.BlockCopy(BitConverter.GetBytes(Value), 0, image, 20, 8);
    ///             Buffer.BlockCopy(Encoding.ASCII.GetBytes(Description), 0, image, 28, 100);
    /// 
    ///             return image;
    ///         }
    ///     }
    /// 
    ///     /// <summary>
    ///     /// Deserializes the record.
    ///     /// </summary>
    ///     public int Initialize(byte[] binaryImage, int startIndex)
    ///     {
    ///         // Deserialize byte array into TestIsamFileRecord.
    ///         Name = Encoding.ASCII.GetString(binaryImage, startIndex, 20);
    ///         Value = BitConverter.ToDouble(binaryImage, startIndex + 20);
    ///         Description = Encoding.ASCII.GetString(binaryImage, startIndex + 28, 100);
    /// 
    ///         return RecordLength;
    ///     }
    /// 
    ///     /// <summary>
    ///     /// String representation of the record.
    ///     /// </summary>
    ///     public override string ToString()
    ///     {
    ///         return string.Format("Name: {0}, Value: {1}, Description: {2}", Name, Value, Description);
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class IsamDataFileBase<T> : Component, ISupportLifecycle, ISupportInitialize, IPersistSettings where T : IBinaryDataProvider, IBinaryDataConsumer
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the required extension of the <see cref="FileName"/>.
        /// </summary>
        public const string Extension = ".dat";

        /// <summary>
        /// Specifies the default value for the <see cref="FileName"/> property.
        /// </summary>
        public const string DefaultFileName = "IsamDataFile" + Extension;

        /// <summary>
        /// Specifies the default value for the <see cref="AutoSaveInterval"/> property.
        /// </summary>
        public const int DefaultAutoSaveInterval = -1;

        /// <summary>
        /// Specifes the default value for the <see cref="MinimumRecordCount"/> property.
        /// </summary>
        public const int DefaultMinimumRecordCount = 0;

        /// <summary>
        /// Specifies the default value for the <see cref="LoadOnOpen"/> property.
        /// </summary>
        public const bool DefaultLoadOnOpen = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SaveOnClose"/> property.
        /// </summary>
        public const bool DefaultSaveOnClose = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ReloadOnModify"/> property.
        /// </summary>
        public const bool DefaultReloadOnModify = false;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "IsamDataFile";

        // Events

        /// <summary>
        /// Occurs when data is being read from disk into memory.
        /// </summary>
        [Description("Occurs when data is being read from disk into memory.")]
        public event EventHandler DataLoading;

        /// <summary>
        /// Occurs when data has been read from disk into memory.
        /// </summary>
        [Description("Occurs when data has been read from disk into memory.")]
        public event EventHandler DataLoaded;

        /// <summary>
        /// Occurs when data is being saved from memory onto disk.
        /// </summary>
        [Description("Occurs when data is being saved from memory onto disk.")]
        public event EventHandler DataSaving;

        /// <summary>
        /// Occurs when data has been saved from memory onto disk.
        /// </summary>
        [Description("Occurs when data has been saved from memory onto disk.")]
        public event EventHandler DataSaved;

        /// <summary>
        /// Occurs when file data on the disk is modified.
        /// </summary>
        [Description("Occurs when file data on the disk is modified.")]
        public event EventHandler FileModified;

        // Fields
        private string m_fileName;
        private int m_autoSaveInterval;
        private int m_minimumRecordCount;
        private bool m_loadOnOpen;
        private bool m_saveOnClose;
        private bool m_reloadOnModify;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private List<T> m_fileRecords;
        private byte[] m_recordBuffer;
        private FileStream m_fileStream;
        private ManualResetEvent m_loadWaitHandle;
        private ManualResetEvent m_saveWaitHandle;
        private System.Timers.Timer m_autoSaveTimer;
        private FileSystemWatcher m_fileSystemWatcher;
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="IsamDataFileBase{T}"/> class.
        /// </summary>
        public IsamDataFileBase()
        {
            m_fileName = DefaultFileName;
            m_autoSaveInterval = DefaultAutoSaveInterval;
            m_minimumRecordCount = DefaultMinimumRecordCount;
            m_loadOnOpen = DefaultLoadOnOpen;
            m_saveOnClose = DefaultSaveOnClose;
            m_reloadOnModify = DefaultReloadOnModify;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_loadWaitHandle = new ManualResetEvent(true);
            m_saveWaitHandle = new ManualResetEvent(true);

            m_autoSaveTimer = new System.Timers.Timer();
            m_autoSaveTimer.Elapsed += m_autoSaveTimer_Elapsed;
            m_fileSystemWatcher = new FileSystemWatcher();
            m_fileSystemWatcher.Changed += FileSystemWatcher_Changed;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <remarks>
        /// Changing the <see cref="FileName"/> when the file is open will cause the file to be re-opend.
        /// </remarks>
        [Category("Settings"),
        DefaultValue(DefaultFileName),
        Description("Name of the file.")]
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                if (string.Compare(Path.GetExtension(value), Extension, true) != 0)
                    throw new ArgumentException(string.Format("Name must have an extension of {0}.", Extension));

                m_fileName = value;
                if (IsOpen)
                {
                    Close();
                    Open();
                }
            }
        }

        /// <summary>
        /// Gets or sets the interval in milliseconds at which the records loaded in memory are to be persisted to disk.
        /// </summary>
        /// <remarks>
        /// <see cref="AutoSaveInterval"/> will be effective only if records have been loaded in memory either manually 
        /// by calling the <see cref="Load()"/> method or automatically by settings <see cref="LoadOnOpen"/> to true.
        /// </remarks>
        [Category("Settings"),
        DefaultValue(DefaultAutoSaveInterval),
        Description("Interval in milliseconds at which the records loaded in memory are to be persisted to disk.")]
        public int AutoSaveInterval
        {
            get
            {
                return m_autoSaveInterval;
            }
            set
            {
                m_autoSaveInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of records that the file will have when it is opened.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultMinimumRecordCount),
        Description("Minimum number of records that the file will have when it is opened.")]
        public int MinimumRecordCount
        {
            get
            {
                return m_minimumRecordCount;
            }
            set
            {
                m_minimumRecordCount = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether records are to be loaded automatically in memory when 
        /// the file is opened.
        /// </summary>
        [Category("Behavior"),
        DefaultValue(DefaultLoadOnOpen),
        Description("Indicates whether records are to be loaded automatically in memory when the file is opened.")]
        public bool LoadOnOpen
        {
            get
            {
                return m_loadOnOpen;
            }
            set
            {
                m_loadOnOpen = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether records loaded in memory are to be persisted to disk 
        /// when the file is closed.
        /// </summary>
        /// <remarks>
        /// <see cref="SaveOnClose"/> will be effective only if records have been loaded in memory either manually 
        /// by calling the <see cref="Load()"/> method or automatically by settings <see cref="LoadOnOpen"/> to true.
        /// </remarks>
        [Category("Behavior"),
        DefaultValue(DefaultSaveOnClose),
        Description("Indicates whether records loaded in memory are to be persisted to disk when the file is closed.")]
        public bool SaveOnClose
        {
            get
            {
                return m_saveOnClose;
            }
            set
            {
                m_saveOnClose = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether records loaded in memory are to be re-loaded when the 
        /// file is modified on disk.
        /// </summary>
        /// <remarks>
        /// <see cref="ReloadOnModify"/> will be effective only if records have been loaded in memory either manually 
        /// by calling the <see cref="Load()"/> method or automatically by settings <see cref="LoadOnOpen"/> to true.
        /// </remarks>
        [Category("Behavior"),
        DefaultValue(DefaultReloadOnModify),
        Description("Indicates whether records loaded in memory are to be re-loaded when the file is modified on disk.")]
        public bool ReloadOnModify
        {
            get
            {
                return m_reloadOnModify;
            }
            set
            {
                m_reloadOnModify = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the file settings are to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the file settings are to be saved to the config file.")]
        public bool PersistSettings
        {
            get
            {
                return m_persistSettings;
            }
            set
            {
                m_persistSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the category under which the file settings are to be saved to the config file if the 
        /// <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is null or empty string.</exception>
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the file settings are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw (new ArgumentNullException());

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the file is currently enabled.
        /// </summary>
        /// <remarks>
        /// <see cref="Enabled"/> property is not be set by user-code directly.
        /// </remarks>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the file is open.
        /// </summary>
        [Browsable(false)]
        public bool IsOpen
        {
            get
            {
                return (m_fileStream != null);
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the file data on disk is corrupt.
        /// </summary>
        [Browsable(false)]
        public bool IsCorrupt
        {
            get
            {
                if (IsOpen)
                {
                    long fileLength;

                    lock (m_fileStream)
                    {
                        fileLength = m_fileStream.Length;
                    }

                    return (fileLength % m_recordBuffer.Length != 0);
                }
                else
                {
                    throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
                }
            }
        }

        /// <summary>
        /// Gets the approximate memory consumption (in KB) of the file.
        /// </summary>
        /// <remarks>
        /// <see cref="MemoryUsage"/> will be zero (0) unless records are loaded in memory.
        /// </remarks>
        [Browsable(false)]
        public long MemoryUsage
        {
            get
            {
                return RecordsInMemory * m_recordBuffer.Length / 1024;
            }
        }

        /// <summary>
        /// Gets the number of file records on the disk.
        /// </summary>
        [Browsable(false)]
        public int RecordsOnDisk
        {
            get
            {
                if (IsOpen)
                {
                    long fileLength;

                    lock (m_fileStream)
                    {
                        fileLength = m_fileStream.Length;
                    }

                    return (int)(fileLength / (long)m_recordBuffer.Length);
                }
                else
                {
                    throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
                }
            }
        }

        /// <summary>
        /// Gets the number of file records loaded in memory.
        /// </summary>
        [Browsable(false)]
        public int RecordsInMemory
        {
            get
            {
                int recordCount = 0;

                if (m_fileRecords != null)
                {
                    lock (m_fileRecords)
                    {
                        recordCount = m_fileRecords.Count;
                    }
                }

                return recordCount;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Opens the file.
        /// </summary>
        public void Open()
        {
            if (!IsOpen)
            {
                // Initialize if uninitialized.
                Initialize();

                // Make the file path absolute if it is relative.
                m_fileName = FilePath.GetAbsolutePath(m_fileName);

                // Create the file directory if it does not exist.
                if (!Directory.Exists(FilePath.GetDirectoryName(m_fileName)))
                    Directory.CreateDirectory(FilePath.GetDirectoryName(m_fileName));

                // Open if file exists, or create it if it doesn't.
                m_fileStream = new FileStream(m_fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                // Load records into memory if specified to do so.
                if (m_loadOnOpen) Load();

                // Makes sure that we have the minimum number of records specified.
                for (int i = RecordsOnDisk + 1; i <= m_minimumRecordCount; i++)
                {
                    Write(i, CreateNewRecord(i));
                }

                if (m_reloadOnModify)
                {
                    // Watch for any modifications made to the file on disk.
                    m_fileSystemWatcher.Path = FilePath.GetDirectoryName(m_fileName);
                    m_fileSystemWatcher.Filter = FilePath.GetFileName(m_fileName);
                    m_fileSystemWatcher.EnableRaisingEvents = true;
                }

                if (m_autoSaveInterval > 0)
                {
                    // Start timer for saving records loaded in memory automatically.
                    m_autoSaveTimer.Interval = m_autoSaveInterval;
                    m_autoSaveTimer.Start();
                }
            }
        }

        /// <summary>
        /// Closes the file.
        /// </summary>
        public void Close()
        {
            if (IsOpen)
            {
                // Stop the timer if it is ticking.
                m_autoSaveTimer.Stop();

                // Stop monitoring for changes to the file.
                m_fileSystemWatcher.EnableRaisingEvents = false;

                // Save records back to the file if specified.
                if (m_saveOnClose) Save();

                // Close the file stream used for file I/O.
                if (m_fileStream != null)
                {
                    lock (m_fileStream)
                    {
                        m_fileStream.Dispose();
                    }
                }
                m_fileStream = null;

                // Clear the records loaded in memory.
                if (m_fileRecords != null)
                {
                    lock (m_fileRecords)
                    {
                        m_fileRecords.Clear();
                    }
                }
                m_fileRecords = null;
            }
        }

        /// <summary>
        /// Loads records from disk into memory.
        /// </summary>
        public void Load()
        {
            if (IsOpen)
            {
                // Waits for any pending request to save records before completing.
                m_saveWaitHandle.WaitOne();

                // Waits for any prior request to load records before completing.
                m_loadWaitHandle.WaitOne();
                m_loadWaitHandle.Reset();

                try
                {
                    OnDataLoading(EventArgs.Empty);

                    if (m_fileRecords == null)
                        m_fileRecords = new List<T>();

                    List<T> records = new List<T>(ReadFromDisk());
                    lock (m_fileRecords)
                    {
                        m_fileRecords.Clear();
                        m_fileRecords.InsertRange(0, records);
                    }

                    OnDataLoaded(EventArgs.Empty);
                }
                finally
                {
                    m_loadWaitHandle.Set();
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Initializes the file.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the file is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();                             // Load settings from the config file.
                m_recordBuffer = new byte[GetRecordSize()]; // Create buffer for reading records.
                m_initialized = true;                       // Initialize only once.
            }
        }

        /// <summary>
        /// Saves records loaded in memory to disk.
        /// </summary>
        public void Save()
        {
            if (IsOpen)
            {
                // Waits for any pending request to save records before completing.
                m_saveWaitHandle.WaitOne();

                // Waits for any prior request to load records before completing.
                m_loadWaitHandle.WaitOne();
                m_saveWaitHandle.Reset();

                try
                {
                    OnDataSaving(EventArgs.Empty);

                    // Saves (persists) records to the file, if present in memory.
                    if (m_fileRecords != null)
                    {
                        lock (m_fileRecords)
                        {
                            WriteToDisk(m_fileRecords);
                        }
                    }

                    OnDataSaved(EventArgs.Empty);
                }
                finally
                {
                    m_saveWaitHandle.Set();
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Saves settings of the file to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings["FileName", true].Update(m_fileName, "Name of the file including its path.");
                settings["AutoSaveInterval", true].Update(m_autoSaveInterval, "Interval in milliseconds at which the file records loaded in memory are to be saved automatically to disk. Use -1 to disable automatic saving.");
                settings["MinimumRecordCount", true].Update(m_minimumRecordCount, "Minimum number of records that the file must have when it is opened.");
                settings["LoadOnOpen", true].Update(m_loadOnOpen, "True if file records are to be loaded in memory when opened; otherwise False.");
                settings["SaveOnClose", true].Update(m_saveOnClose, "True if file records loaded in memory are to be saved to disk when file is closed; otherwise False.");
                settings["ReloadOnModify", true].Update(m_reloadOnModify, "True if file records loaded in memory are to be re-loaded when file is modified on disk; otherwise False.");
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings of the file from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                FileName = settings["FileName", true].ValueAs(m_fileName);
                AutoSaveInterval = settings["AutoSaveInterval", true].ValueAs(m_autoSaveInterval);
                MinimumRecordCount = settings["MinimumRecordCount", true].ValueAs(m_minimumRecordCount);
                LoadOnOpen = settings["LoadOnOpen", true].ValueAs(m_loadOnOpen);
                SaveOnClose = settings["SaveOnClose", true].ValueAs(m_saveOnClose);
                ReloadOnModify = settings["ReloadOnModify", true].ValueAs(m_reloadOnModify);
            }
        }

        /// <summary>
        /// Performs necessary operations before the file properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the file is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void BeginInit()
        {
            // Nothing needs to be done before component is initialized.
        }

        /// <summary>
        /// Performs necessary operations after the file properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the file is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void EndInit()
        {
            if (!DesignMode)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Writes specified records to disk if records were not loaded in memory otherwise updates the records in memory.
        /// </summary>
        /// <param name="records">Records to be written.</param>
        /// <remarks>
        /// This operation will causes existing records to be deleted and replaced with the ones specified.
        /// </remarks>
        public virtual void Write(List<T> records)
        {
            if (IsOpen)
            {
                if (m_fileRecords == null)
                {
                    WriteToDisk(records);
                }
                else
                {
                    lock (m_fileRecords)
                    {
                        m_fileRecords.Clear();
                        m_fileRecords.InsertRange(0, records);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Writes specified record to disk if records were not loaded in memory otherwise updates the record in memory.
        /// </summary>
        /// <param name="recordID">ID of the record to be written.</param>
        /// <param name="record">Record to be written.</param>
        public virtual void Write(int recordID, T record)
        {
            if (IsOpen)
            {
                if (record != null)
                {
                    if (m_fileRecords == null)
                    {
                        // We're writing directly to the file.
                        WriteToDisk(recordID, record);
                    }
                    else
                    {
                        // We're updating the in-memory record list.
                        int lastRecordID = RecordsInMemory;

                        if (recordID > lastRecordID)
                        {
                            if (recordID > lastRecordID + 1)
                            {
                                for (int i = lastRecordID + 1; i <= recordID - 1; i++)
                                {
                                    Write(i, CreateNewRecord(i));
                                }
                            }

                            lock (m_fileRecords)
                            {
                                m_fileRecords.Add(record);
                            }
                        }
                        else
                        {
                            // Updates the existing record with the new one.
                            lock (m_fileRecords)
                            {
                                m_fileRecords[recordID - 1] = record;
                            }
                        }
                    }
                }
                else
                {
                    throw new ArgumentNullException("record");
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Reads file records from disk if records were not loaded in memory otherwise returns the records in memory.
        /// </summary>
        /// <returns>Records of the file.</returns>
        public virtual List<T> Read()
        {
            if (IsOpen)
            {
                List<T> records = new List<T>();

                if (m_fileRecords == null)
                {
                    // Reads persisted records if no records are in memory.
                    records.InsertRange(0, ReadFromDisk());
                }
                else
                {
                    // Reads records in memory.
                    lock (m_fileRecords)
                    {
                        records.InsertRange(0, m_fileRecords);
                    }
                }

                return records;
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Reads specified file record from disk if records were not loaded in memory otherwise returns the record in memory.
        /// </summary>
        /// <param name="recordID">ID of the record to be read.</param>
        /// <returns>Record with the specified ID if it exists; otherwise null.</returns>
        public virtual T Read(int recordID)
        {
            if (IsOpen)
            {
                T record = default(T);

                if (recordID > 0)
                {
                    // ID of the requested record is valid.
                    if (m_fileRecords == null && recordID <= RecordsOnDisk)
                    {
                        // Reads the requested record exists in the file.
                        record = ReadFromDisk(recordID);
                    }
                    else if ((m_fileRecords != null) && recordID <= RecordsInMemory)
                    {
                        // Uses the requested record from memory.
                        lock (m_fileRecords)
                        {
                            record = m_fileRecords[recordID - 1];
                        }
                    }
                }

                return record;
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, m_fileName));
            }
        }

        /// <summary>
        /// Raises the <see cref="FileModified"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnFileModified(EventArgs e)
        {
            if (FileModified != null)
                FileModified(this, e);
        }

        /// <summary>
        /// Raises the <see cref="DataLoading"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnDataLoading(EventArgs e)
        {
            if (DataLoading != null)
                DataLoading(this, e);
        }

        /// <summary>
        /// Raises the <see cref="DataLoaded"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnDataLoaded(EventArgs e)
        {
            if (DataLoaded != null)
                DataLoaded(this, e);
        }

        /// <summary>
        /// Raises the <see cref="DataSaving"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnDataSaving(EventArgs e)
        {
            if (DataSaving != null)
                DataSaving(this, e);
        }

        /// <summary>
        /// Raises the <see cref="DataSaved"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnDataSaved(EventArgs e)
        {
            if (DataSaved != null)
                DataSaved(this, e);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the file and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    SaveSettings();
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        if (m_loadWaitHandle != null) 
                            m_loadWaitHandle.Close();

                        if (m_saveWaitHandle != null) 
                            m_saveWaitHandle.Close();

                        if (m_autoSaveTimer != null)
                            m_autoSaveTimer.Dispose();

                        if (m_fileSystemWatcher != null)
                            m_fileSystemWatcher.Dispose();
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets the size of a record (in bytes).
        /// </summary>
        /// <returns>Size of a record in bytes.</returns>
        protected abstract int GetRecordSize();

        /// <summary>
        /// When overridden in a derived class, returns a new empty record.
        /// </summary>
        /// <param name="id">ID of the new record.</param>
        /// <returns>New empty record.</returns>
        protected abstract T CreateNewRecord(int id);

        /// <summary>
        /// Writes records to disk.
        /// </summary>
        /// <param name="records">Records to be written to disk.</param>
        private void WriteToDisk(List<T> records)
        {
            // Write all records to disk.
            for (int i = 1; i <= records.Count; i++)
            {
                WriteToDisk(i, records[i - 1]);
            }

            // Discard previously existing records that were not written.
            lock (m_fileStream)
            {
                m_fileStream.SetLength(records.Count * m_recordBuffer.Length);
            }
        }

        /// <summary>
        /// Writes single record to disk.
        /// </summary>
        /// <param name="recordID">ID of the record to be written to disk.</param>
        /// <param name="record">Record to be written to disk.</param>
        private void WriteToDisk(int recordID, T record)
        {
            lock (m_fileStream)
            {
                m_fileStream.Seek((recordID - 1) * record.BinaryLength, SeekOrigin.Begin);
                m_fileStream.Write(record.BinaryImage, 0, record.BinaryLength);
            }
        }

        /// <summary>
        /// Reads all records from disk.
        /// </summary>
        /// <returns>Records from disk.</returns>
        private List<T> ReadFromDisk()
        {
            List<T> records = new List<T>();
            int recordCount = RecordsOnDisk;

            for (int i = 1; i <= recordCount; i++)
            {
                records.Add(ReadFromDisk(i));
            }

            return records;
        }

        /// <summary>
        /// Read single record from disk.
        /// </summary>
        /// <param name="recordID">ID of the record to be read.</param>
        /// <returns>Record from the disk.</returns>
        private T ReadFromDisk(int recordID)
        {
            lock (m_fileStream)
            {
                m_fileStream.Seek((recordID - 1) * m_recordBuffer.Length, SeekOrigin.Begin);
                m_fileStream.Read(m_recordBuffer, 0, m_recordBuffer.Length);
            }

            T newRecord = CreateNewRecord(recordID);
            newRecord.Initialize(m_recordBuffer, 0);

            return newRecord;
        }

        private void m_autoSaveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Automatically save records to disk if loaded in memory.
            if ((m_fileRecords != null) && IsOpen) Save();
        }

        private void FileSystemWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            OnFileModified(EventArgs.Empty);

            // Reload records if they have been loaded in memory and reloading is enabled.
            if ((m_fileRecords != null) && m_reloadOnModify) Load();
        }

        #endregion
    }
}
